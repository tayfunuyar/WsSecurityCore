using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsSecurityCore.Models;
using System.Threading;

namespace WsSecurityCore;

/// <summary>
/// Interface for SOAP service with WS-Security
/// </summary>
public interface ISoapService
{
   /// <summary>
   /// Sends a SOAP request with WS-Security headers
   /// </summary>
   /// <param name="soapRequest">The SOAP request parameters</param>
   /// <returns>Response from the SOAP service</returns>
   WsSoapResponse SendSoapRequest(WsSoapRequest soapRequest);
   
   /// <summary>
   /// Sends a SOAP request with WS-Security headers asynchronously
   /// </summary>
   /// <param name="soapRequest">The SOAP request parameters</param>
   /// <param name="cancellationToken">Optional cancellation token</param>
   /// <returns>Response from the SOAP service</returns>
   Task<WsSoapResponse> SendSoapRequestAsync(WsSoapRequest soapRequest, CancellationToken cancellationToken = default);
}
