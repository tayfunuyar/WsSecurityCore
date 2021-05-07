using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsSecurityCore.Models
{
    public class WsSoapResponse
    {
        public string Message { get; set; }
        public bool Result { get; set; }
        public object Response { get; set; }

        public static WsSoapResponse Success(object response, string message = "")
        {
            return new WsSoapResponse
            {
                Result = true,
                Response = response,
                Message = message,
            };
        }
        public static WsSoapResponse Fail(string message)
        {
            return new WsSoapResponse
            {
                Result = false,
                Message = message,
            };
        }
    }
}
