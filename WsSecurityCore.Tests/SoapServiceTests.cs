using System.Net;
using Moq;
using Moq.Protected;
using WsSecurityCore.Configuration;
using WsSecurityCore.Helper;
using WsSecurityCore.Logging;
using WsSecurityCore.Models;
using WsSecurityCore.Security;
using Xunit;

namespace WsSecurityCore.Tests;

public class SoapServiceTests
{
    private readonly Mock<IXmlHelper> _mockXmlHelper;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<IWsLogger> _mockLogger;
    private readonly WsSoapOptions _options;
    private readonly HttpClient _httpClient;
    private readonly SoapService _soapService;

    public SoapServiceTests()
    {
        _mockXmlHelper = new Mock<IXmlHelper>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<IWsLogger>();
        _options = new WsSoapOptions { ThrowExceptions = false };

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _soapService = new SoapService(_mockXmlHelper.Object, _httpClient, _mockLogger.Object, _options);
    }

    [Fact]
    public async Task SendSoapRequestAsync_ValidRequest_ReturnsSuccessResponse()
    {
        var request = new WsSoapRequest
        {
            EndpointUrl = "https://example.com/soap",
            ActionName = "TestAction",
            ActionUrl = "http://example.com/TestAction",
            Username = "testuser",
            Password = "testpass",
            Parameters = new Dictionary<string, object> { { "param1", "value1" }, { "param2", "value2" } }
        };

        var responseContent = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
                                <s:Body>
                                    <TestActionResponse xmlns=""http://example.com/"">
                                        <TestActionResult>Success</TestActionResult>
                                    </TestActionResponse>
                                </s:Body>
                            </s:Envelope>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK, Content = new StringContent(responseContent)
            });


        var result = await _soapService.SendSoapRequestAsync(request);


        Assert.True(result.Result);
        Assert.Equal(Constants.DataSuccess, result.Message);
        Assert.Equal(responseContent, result.Response);
    }

    [Fact]
    public async Task SendSoapRequestAsync_WithSecurityOptions_UsesCorrectSecurityMode()
    {
        var request = new WsSoapRequest
        {
            EndpointUrl = "https://example.com/soap",
            ActionName = "TestAction",
            ActionUrl = "http://example.com/TestAction",
            Username = "testuser",
            Password = "testpass",
            Parameters = new Dictionary<string, object> { { "param1", "value1" } },
            SecurityOptions = new SecurityOptions { Mode = SecurityMode.UsernameToken, IncludeNonce = true }
        };

        var responseContent =
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body><TestActionResponse /></s:Body></s:Envelope>";


        string? capturedContent = null;

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).Callback<HttpRequestMessage, CancellationToken>(async (req, _) =>
            {
                if (req.Content != null)
                {
                    capturedContent = await req.Content.ReadAsStringAsync();
                }
            }).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK, Content = new StringContent(responseContent)
            });


        var result = await _soapService.SendSoapRequestAsync(request);


        Assert.True(result.Result);
        Assert.NotNull(capturedContent);
        Assert.Contains("<o:Nonce>", capturedContent!);
        Assert.Contains("<o:Created>", capturedContent);
    }

    [Fact]
    public void SendSoapRequest_MissingParameters_ReturnsFailResponse()
    {
        var request = new WsSoapRequest
        {
            EndpointUrl = "https://example.com/soap",
            ActionName = "TestAction",
            ActionUrl = "http://example.com/TestAction",
            Username = "testuser",
            Password = "testpass",
        };


        var result = _soapService.SendSoapRequest(request);


        Assert.False(result.Result);
        Assert.Contains(Constants.ParametersMissing, result.Message);
    }
}