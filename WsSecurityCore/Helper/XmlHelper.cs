using System.Xml;
using System.Xml.Linq;
using WsSecurityCore.Logging;

namespace WsSecurityCore.Helper
{
    public class XmlHelper : IXmlHelper
    {
        private readonly IWsLogger _logger;

        public XmlHelper(IWsLogger logger)
        {
            _logger = logger;
        }

        public string RemoveAllNamespaces(string xmlDocument)
        {
            try
            {
                _logger.LogDebug($"Removing namespaces from XML document");
                XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));
                return xmlDocumentWithoutNs.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to remove namespaces from XML document", ex);
                return xmlDocument;
            }
        }

        public XElement RemoveAllNamespaces(XElement e)
        {
            try
            {
                return new XElement(e.Name.LocalName,
                    e.Nodes().Select(n => n is XElement element ? RemoveAllNamespaces(element) : n),
                    e.HasAttributes
                        ? e.Attributes().Where(a => !a.IsNamespaceDeclaration)
                            .Select(a => new XAttribute(a.Name.LocalName, a.Value))
                        : null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to remove namespaces from XElement: {e.Name}", ex);
                return e;
            }
        }

        public string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger.LogDebug("Empty text provided to RemoveInvalidXmlChars");
                return string.Empty;
            }

            try
            {
                _logger.LogDebug("Removing invalid XML characters from text");
                return new string(text.Where(XmlConvert.IsXmlChar).ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to remove invalid XML characters", ex);
                return string.Empty;
            }
        }
    }
}