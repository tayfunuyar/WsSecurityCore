using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace WsSecurityCore.Helper
{
    public class XmlHelper : IXmlHelper
    {
        public  string RemoveAllNamespaces(string xmlDocument)
        {
            try
            {
                XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

                return xmlDocumentWithoutNs.ToString();
            }
            catch (Exception ex)
            {
                return xmlDocument;
            }
        }
        public XElement RemoveAllNamespaces(XElement e)
        {
            try
            {
                return new XElement(e.Name.LocalName,
                    (from n in e.Nodes()
                     select ((n is XElement) ? RemoveAllNamespaces(n as XElement) : n)),
                    (e.HasAttributes) ?
                        (from a in e.Attributes()
                         where (!a.IsNamespaceDeclaration)
                         select new XAttribute(a.Name.LocalName, a.Value)) : null);
            }
            catch (Exception)
            {
                return e;
            }
        }
        public  string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(XmlConvert.IsXmlChar).ToArray();

            text = String.Empty;


            return new string(validXmlChars);
        }

     
    }
}
