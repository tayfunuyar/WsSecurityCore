using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsSecurityCore.Models
{
    public class WsSoapRequest
    {
        public WsSoapRequest()
        {
            Method = "POST";
            GetOnlyBody = false;
            ContentType = "text/xml;charset=\"utf-8\"";
            Accept = "text/xml";
        }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SoapActionName { get; set; }
        public string SoapActionUrl { get; set; }
        public string SoapUrl { get; set; }
        public Dictionary<string, string> Parameters;
        public string Method { get; set; }
        public bool GetOnlyBody { get; set; }
        public string Accept { get; set; }
        public string ContentType { get; set; }


    }
}
