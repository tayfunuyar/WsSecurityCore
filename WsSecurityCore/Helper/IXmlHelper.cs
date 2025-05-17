using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WsSecurityCore.Helper;

public interface IXmlHelper
{
    XElement RemoveAllNamespaces(XElement e);
    string RemoveInvalidXmlChars(string text);
    string RemoveAllNamespaces(string xmlDocument);
}
