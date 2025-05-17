using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsSecurityCore.Security;

namespace WsSecurityCore.Models;

public class WsSoapRequest
{
    public WsSoapRequest()
    {
        Method = "POST";
        GetOnlyBody = false;
        ContentType = "text/xml; charset=utf-8";
        Accept = "text/xml";
        Parameters = new Dictionary<string, object>();
        SecurityOptions = new SecurityOptions();
    }
    
    /// <summary>
    /// Username for authentication
    /// </summary>
    public required string Username { get; set; }
    
    /// <summary>
    /// Password for authentication
    /// </summary>
    public required string Password { get; set; }
    
    /// <summary>
    /// The name of the SOAP action/method to invoke (e.g. "GetOrder")
    /// </summary>
    public required string ActionName { get; set; }
    
    /// <summary>
    /// The URL for the SOAP action (e.g. "http://example.org/GetOrder")
    /// </summary>
    public required string ActionUrl { get; set; }
    
    /// <summary>
    /// The endpoint URL of the SOAP service
    /// </summary>
    public required string EndpointUrl { get; set; }
    
    /// <summary>
    /// Parameters to include in the SOAP request
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; }
    
    /// <summary>
    /// HTTP method (default: POST)
    /// </summary>
    public string Method { get; set; }
    
    /// <summary>
    /// If true, only returns the body content of the SOAP response
    /// </summary>
    public bool GetOnlyBody { get; set; }
    
    /// <summary>
    /// HTTP Accept header (default: text/xml)
    /// </summary>
    public string Accept { get; set; }
    
    /// <summary>
    /// HTTP Content-Type header (default: text/xml; charset=utf-8)
    /// </summary>
    public string ContentType { get; set; }
    
    /// <summary>
    /// Advanced security options for WS-Security
    /// </summary>
    public SecurityOptions SecurityOptions { get; set; }
}
