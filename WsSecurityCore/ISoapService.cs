using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsSecurityCore.Models;

namespace WsSecurityCore
{
    public interface ISoapService
    {
       /// <summary>
       /// Soap Request
       /// </summary>
       /// <param name="soapRequest"></param>
       /// <returns></returns>
        public WsSoapResponse SendSOAPRequest(WsSoapRequest soapRequest);
    }
}
