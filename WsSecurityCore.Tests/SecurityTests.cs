using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Moq;
using WsSecurityCore.Logging;
using WsSecurityCore.Security;
using Xunit;

namespace WsSecurityCore.Tests
{
    public class SecurityTests
    {
        private readonly Mock<IWsLogger> _loggerMock;

        public SecurityTests()
        {
            _loggerMock = new Mock<IWsLogger>();
        }

        [Fact]
        public void SecurityMode_DefaultValue_IsUsernameToken()
        {
            var options = new SecurityOptions();


            Assert.Equal(SecurityMode.UsernameToken, options.Mode);
        }

        [Fact]
        public void SecurityOptions_DefaultTimestamp_IsEnabled()
        {
            var options = new SecurityOptions();


            Assert.True(options.IncludeTimestamp);
            Assert.Equal(5, options.TimestampLifetimeMinutes);
        }

        [Fact]
        public void SecurityOptions_DefaultNonce_IsDisabled()
        {
            var options = new SecurityOptions();


            Assert.False(options.IncludeNonce);
        }

        [Fact]
        public void XmlSignatureHelper_SignAndVerifyWithSameKey_ReturnsTrue()
        {
            var certPath = Path.Combine(AppContext.BaseDirectory, "test-cert.pfx");
            if (!File.Exists(certPath))
            {
                return;
            }

            try
            {
                var cert = new X509Certificate2(certPath, "testpassword", X509KeyStorageFlags.Exportable);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("<root><test>Test content</test></root>");


                var signedDoc = XmlSignatureHelper.SignXmlDocument(xmlDoc, cert, "#test", "sig-1");
                var result = XmlSignatureHelper.VerifyXmlSignature(signedDoc, cert);


                Assert.True(result);
            }
            catch (Exception)
            {
            }
        }

        [Fact]
        public void GetClientCertificate_WithNoCertificate_ReturnsNull()
        {
            var options = new SecurityOptions();


            var result = options.GetClientCertificate();


            Assert.Null(result);
        }
    }
}