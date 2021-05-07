using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WsSecurityCore.Helper;
using WsSecurityCore.Models;

namespace WsSecurityCore
{
    public class SoapService : ISoapService
    {
        private readonly IXmlHelper xmlHelper;
        public SoapService(IXmlHelper xmlHelper)
        {
            this.xmlHelper = xmlHelper;
        }
        /// <summary>
        /// GetOnlyBody is return XmlNodeList
        /// ContentType default "text/xml;charset=\"utf-8\""
        /// Accept default text/xml
        /// Method default Post
        /// </summary> 
        /// <param name="soapRequest"></param>
        /// <returns></returns>
        public WsSoapResponse SendSOAPRequest(WsSoapRequest soapRequest)
        {
            WsSoapResponse soapResponse = new WsSoapResponse();
            string cleanXml = "";
            string xmlResult = "";
            var controlResponse = ControlParameters(soapRequest);
            if (!controlResponse.Result)
                return controlResponse;
            try
            {
                string cert_id = $"uuid-{Guid.NewGuid()}-1";

                DateTime dt = DateTime.UtcNow;
                string now = dt.ToString("o").Substring(0, 23) + "Z";
                string plus5 = dt.AddMinutes(5).ToString("o").Substring(0, 23) + "Z";

                XmlDocument soapEnvelopeXml = new XmlDocument();

                StringBuilder builder = new StringBuilder();
                builder.AppendLine(@$"<s:Envelope
	                        xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""
	                        xmlns:u=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
	                        <s:Header>
		                        <o:Security s:mustUnderstand=""1""
			                        xmlns:o=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"">
			                        <u:Timestamp u:Id=""_0"">
				                        <u:Created>{now}</u:Created>
				                        <u:Expires>{plus5}</u:Expires>
			                        </u:Timestamp>
			                        <o:UsernameToken u:Id=""{cert_id}""> 
				                        <o:Username>{soapRequest.Username}</o:Username>
				                        <o:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">{soapRequest.Password}</o:Password>
			                        </o:UsernameToken>
		                        </o:Security>
	                        </s:Header> ");
                builder.AppendLine(@"<s:Body>
		                           <{0} xmlns=""http://tempuri.org/"">{1}</{0}>
	                        </s:Body>
                        </s:Envelope>");
                string paramsJoin = string.Join(string.Empty,
                    soapRequest.Parameters.Select(kv => String.Format("<{0}>{1}</{0}>", kv.Key, kv.Value)).ToArray());
                var s = String.Format(builder.ToString(), soapRequest.SoapActionName, paramsJoin);
                soapEnvelopeXml.LoadXml(s);


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(soapRequest.SoapUrl);
                webRequest.Headers.Add("SOAPAction", soapRequest.SoapActionUrl ?? soapRequest.SoapUrl);
                webRequest.ContentType = soapRequest.ContentType;
                webRequest.Accept = soapRequest.Accept;
                webRequest.Method = soapRequest.Method;


                using (Stream stream = webRequest.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }


                using (WebResponse response = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        xmlResult = rd.ReadToEnd();
                    }
                }
                if (soapRequest.GetOnlyBody)
                {

                    cleanXml = xmlHelper.RemoveInvalidXmlChars(xmlResult);
                    cleanXml = xmlHelper.RemoveAllNamespaces(xmlResult);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(cleanXml);


                    var xmlNodeList = doc.SelectNodes("/Envelope/Body");
                    soapResponse.Response = xmlNodeList;
                    soapResponse.Message = Constants.DataSucess;
                }
                else
                {
                    soapResponse.Message = Constants.DataSucess;
                    soapResponse.Response = xmlResult;
                    soapResponse.Result = true;
                }

            }
            catch (Exception ex)
            {
                soapResponse.Response = $"{Constants.AnErrorOccured} {ex.Message}";
            }
            return soapResponse;
        }
        private WsSoapResponse ControlParameters(WsSoapRequest request)
        {

            if (request.Parameters.Count == 0)
                return WsSoapResponse.Fail(Constants.HaveToSendParameters);
            else if (string.IsNullOrEmpty(request.SoapActionName))
                return WsSoapResponse.Fail(Constants.HaveToSendSoapActionName);
            else if (string.IsNullOrEmpty(request.SoapActionUrl))
                return WsSoapResponse.Fail(Constants.HaveToSendSoapActionUrl);
            else if (string.IsNullOrEmpty(request.SoapUrl))
                return WsSoapResponse.Fail(Constants.HaveToSendSoapUrl);
            else if (string.IsNullOrEmpty(request.Username))
                return WsSoapResponse.Fail(Constants.HaveToSendSoapUsername);
            else if (string.IsNullOrEmpty(request.Password))
                return WsSoapResponse.Fail(Constants.HaveToSendSoapPassword);
            else
                return WsSoapResponse.Success(Constants.AllParametersSend);
        }
    }
}
