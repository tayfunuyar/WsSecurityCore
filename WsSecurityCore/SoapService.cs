using System.Text;
using System.Xml;
using WsSecurityCore.Helper;
using WsSecurityCore.Models;
using System.Net.Http.Headers;
using WsSecurityCore.Configuration;
using WsSecurityCore.Exceptions;
using WsSecurityCore.Logging;
using System.Net.Sockets;
using System.Security.Cryptography;
using WsSecurityCore.Security;

namespace WsSecurityCore
{
    /// <summary>
    /// Main service for handling WS-Security SOAP requests
    /// </summary>
    public class SoapService : ISoapService
    {
        private readonly IXmlHelper _xmlHelper;
        private readonly HttpClient _httpClient;
        private readonly IWsLogger _logger;
        private readonly WsSoapOptions _options;

        public SoapService(IXmlHelper xmlHelper, HttpClient httpClient, IWsLogger logger, WsSoapOptions options)
        {
            _xmlHelper = xmlHelper;
            _httpClient = httpClient;
            _logger = logger;
            _options = options;
        }

        /// <summary>
        /// Sends a SOAP request with WS-Security headers
        /// </summary> 
        /// <param name="soapRequest">The SOAP request parameters</param>
        /// <returns>Response from the SOAP service</returns>
        public WsSoapResponse SendSoapRequest(WsSoapRequest soapRequest)
        {
            try
            {
                _logger.LogInformation($"Sending SOAP request to {soapRequest.EndpointUrl}");
                return SendSoapRequestAsync(soapRequest).GetAwaiter().GetResult();
            }
            catch (WsSoapValidationException ex)
            {
                _logger.LogError($"SOAP request validation failed: {ex.Message}", ex);

                if (_options.ThrowExceptions)
                    throw;

                return WsSoapResponse.Fail($"{Constants.ValidationFailed}: {ex.Message}");
            }
            catch (WsSoapException ex)
            {
                _logger.LogError($"SOAP request failed: {ex.Message}", ex);

                if (_options.ThrowExceptions)
                    throw;

                return WsSoapResponse.Fail($"{Constants.ErrorOccurred} {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in SOAP request: {ex.Message}", ex);

                if (_options.ThrowExceptions)
                    throw new WsSoapException($"Unexpected error in SOAP request: {ex.Message}", ex);

                return WsSoapResponse.Fail($"{Constants.ErrorOccurred} {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a SOAP request with WS-Security headers asynchronously
        /// </summary>
        /// <param name="soapRequest">The SOAP request parameters</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Response from the SOAP service</returns>
        public async Task<WsSoapResponse> SendSoapRequestAsync(WsSoapRequest soapRequest,
            CancellationToken cancellationToken = default)
        {
            ValidateRequestParameters(soapRequest);

            try
            {
                _logger.LogDebug("Creating SOAP envelope");
                string certId = $"uuid-{Guid.NewGuid()}-1";

                DateTime dateTime = DateTime.UtcNow;
                string currentTime = dateTime.ToString("o")[..23] + "Z";
                string expiryTime =
                    dateTime.AddMinutes(soapRequest.SecurityOptions.TimestampLifetimeMinutes).ToString("o")[..23] + "Z";

                string soapXml = CreateSoapEnvelope(soapRequest, certId, currentTime, expiryTime);
                XmlDocument soapEnvelope = new();
                soapEnvelope.LoadXml(soapXml);

             
                if (soapRequest.SecurityOptions.Mode == SecurityMode.MessageSigning ||
                    soapRequest.SecurityOptions.Mode == SecurityMode.SigningAndEncryption)
                {
                    var certificate = soapRequest.SecurityOptions.GetClientCertificate();
                    if (certificate != null)
                    {
                        _logger.LogDebug("Signing SOAP message with X.509 certificate");
                        try
                        {
                            soapEnvelope = XmlSignatureHelper.SignXmlDocument(soapEnvelope, certificate,
                                soapRequest.SecurityOptions.SignatureReferenceUri, $"SIG-{certId}");

                            
                            soapXml = soapEnvelope.OuterXml;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Failed to sign XML document", ex);
                            throw new WsSoapException("Failed to apply XML signature to the SOAP message", ex);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Message signing requested but no certificate provided.");
                    }
                }

                _logger.LogDebug($"Prepared SOAP request for action: {soapRequest.ActionName}");

                using var content = new StringContent(soapXml, Encoding.UTF8, "text/xml");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(soapRequest.Accept));
                _httpClient.DefaultRequestHeaders.Add("SOAPAction", soapRequest.ActionUrl ?? soapRequest.EndpointUrl);

                HttpResponseMessage response = await ExecuteWithRetryAsync(
                    async (token) => await _httpClient.PostAsync(soapRequest.EndpointUrl, content, token),
                    cancellationToken);

                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex)
                {
                    string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError($"HTTP request failed with status code {response.StatusCode}: {errorContent}", ex);

                    throw new WsSoapException(
                        $"HTTP request failed with status code {response.StatusCode}: {ex.Message}", ex);
                }

                string xmlResult = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation($"Successfully received response from {soapRequest.EndpointUrl}");

                return ProcessResponse(soapRequest, xmlResult);
            }
            catch (WsSoapException)
            {
                throw;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning($"SOAP request was canceled: {ex.Message}");

                throw new WsSoapException("The SOAP request was canceled", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing SOAP request: {ex.Message}", ex);

                throw new WsSoapException($"Error executing SOAP request: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Execute an HTTP operation with retry for transient failures
        /// </summary>
        private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
            Func<CancellationToken, Task<HttpResponseMessage>> operation, CancellationToken cancellationToken)
        {
            var attempts = 0;
            var delay = _options.RetryDelayMilliseconds;

            while (true)
            {
                attempts++;

                try
                {
                    return await operation(cancellationToken);
                }
                catch (HttpRequestException ex) when (IsTransientFailure(ex) && attempts <= _options.MaxRetryAttempts)
                {
                    _logger.LogWarning(
                        $"Transient HTTP failure (attempt {attempts}/{_options.MaxRetryAttempts}): {ex.Message}");

                    if (attempts == _options.MaxRetryAttempts)
                    {
                        _logger.LogError($"Maximum retry attempts ({_options.MaxRetryAttempts}) reached", ex);
                        throw;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Request canceled during retry delay");
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Delay(delay, cancellationToken);

                    if (_options.UseExponentialBackoff)
                    {
                        delay = Math.Min(delay * 2, _options.MaxBackoffMilliseconds);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if an exception represents a transient failure that can be retried
        /// </summary>
        private bool IsTransientFailure(HttpRequestException ex)
        {
            if (!ex.StatusCode.HasValue) return ex.InnerException is SocketException or IOException;
            var statusCode = (int)ex.StatusCode.Value;
            return statusCode is >= 500 and < 600;
        }

        private WsSoapResponse ProcessResponse(WsSoapRequest soapRequest, string xmlResult)
        {
            try
            {
                if (soapRequest.GetOnlyBody)
                {
                    _logger.LogDebug("Processing XML response to extract body");
                    string cleanXml = _xmlHelper.RemoveInvalidXmlChars(xmlResult);
                    cleanXml = _xmlHelper.RemoveAllNamespaces(xmlResult);

                    XmlDocument doc = new();
                    doc.LoadXml(cleanXml);

                    var xmlNodeList = doc.SelectNodes("/Envelope/Body");
                    return WsSoapResponse.Success(xmlNodeList, Constants.DataSuccess);
                }
                else
                {
                    return WsSoapResponse.Success(xmlResult, Constants.DataSuccess);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to process XML response", ex);
                throw new WsSoapException("Failed to process XML response", ex);
            }
        }

        private string CreateSoapEnvelope(WsSoapRequest request, string certId, string currentTime, string expiryTime)
        {
            StringBuilder builder = new();

            builder.AppendLine(@$"<s:Envelope
                            xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""
                            xmlns:u=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">");

            builder.AppendLine(@"<s:Header>");

            builder.AppendLine(@"<o:Security s:mustUnderstand=""1""
                                xmlns:o=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"">");

            if (request.SecurityOptions.IncludeTimestamp)
            {
                builder.AppendLine(@$"<u:Timestamp u:Id=""_0"">
                                    <u:Created>{currentTime}</u:Created>
                                    <u:Expires>{expiryTime}</u:Expires>
                                </u:Timestamp>");
            }

            switch (request.SecurityOptions.Mode)
            {
                case SecurityMode.Certificate:
                    AddBinarySecurityToken(builder, request, certId);
                    break;

                case SecurityMode.SamlToken:
                    if (!string.IsNullOrEmpty(request.SecurityOptions.SamlToken))
                    {
                        builder.AppendLine(request.SecurityOptions.SamlToken);
                    }

                    break;

                case SecurityMode.UsernameToken:
                default:
                    AddUsernameToken(builder, request, certId);
                    break;
            }

            builder.AppendLine("</o:Security>");

            builder.AppendLine("</s:Header>");

            builder.AppendLine(@"<s:Body>
                                <{0} xmlns=""http://tempuri.org/"">{1}</{0}>
                            </s:Body>");

            builder.AppendLine("</s:Envelope>");

            string paramsJoin = string.Join(string.Empty,
                request.Parameters.Select(kv => $"<{kv.Key}>{kv.Value}</{kv.Key}>"));

            return string.Format(builder.ToString(), request.ActionName, paramsJoin);
        }

        private void AddUsernameToken(StringBuilder builder, WsSoapRequest request, string certId)
        {
            builder.AppendLine($@"<o:UsernameToken u:Id=""{certId}"">");
            builder.AppendLine($@"<o:Username>{request.Username}</o:Username>");

            if (request.SecurityOptions.IncludeNonce)
            {
                byte[] nonceBytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(nonceBytes);
                }

                string nonce = Convert.ToBase64String(nonceBytes);
                builder.AppendLine($@"<o:Nonce>{nonce}</o:Nonce>");
                builder.AppendLine($@"<o:Created>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</o:Created>");
            }


            builder.AppendLine(
                $@"<o:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">{request.Password}</o:Password>");
            builder.AppendLine("</o:UsernameToken>");
        }

        private void AddBinarySecurityToken(StringBuilder builder, WsSoapRequest request, string certId)
        {
            var certificate = request.SecurityOptions.GetClientCertificate();
            if (certificate == null)
            {
                _logger.LogWarning("Certificate security mode specified but no certificate provided.");
                AddUsernameToken(builder, request, certId);
                return;
            }


            string certBase64 = Convert.ToBase64String(certificate.RawData);


            builder.AppendLine($@"<o:BinarySecurityToken 
                                u:Id=""{certId}"" 
                                ValueType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3"" 
                                EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"">
                                {certBase64}
                                </o:BinarySecurityToken>");
        }

        /// <summary>
        /// Validate all required parameters of the SOAP request
        /// </summary>
        private void ValidateRequestParameters(WsSoapRequest request)
        {
            _logger.LogDebug("Validating SOAP request parameters");

            if (request.Parameters.Count == 0)
                throw new WsSoapValidationException(Constants.ParametersMissing, "Parameters");

            if (string.IsNullOrEmpty(request.ActionName))
                throw new WsSoapValidationException(Constants.ActionNameMissing, "ActionName");

            if (string.IsNullOrEmpty(request.ActionUrl))
                throw new WsSoapValidationException(Constants.SoapActionUrlMissing, "ActionUrl");

            if (string.IsNullOrEmpty(request.EndpointUrl))
                throw new WsSoapValidationException(Constants.SoapUrlMissing, "EndpointUrl");

            if (string.IsNullOrEmpty(request.Username))
                throw new WsSoapValidationException(Constants.UsernameMissing, "Username");

            if (string.IsNullOrEmpty(request.Password))
                throw new WsSoapValidationException(Constants.PasswordMissing, "Password");

            if (!Uri.TryCreate(request.EndpointUrl, UriKind.Absolute, out _))
                throw new WsSoapValidationException(Constants.InvalidSoapUrl, "EndpointUrl");
        }
    }
}