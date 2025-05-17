using System;

namespace WsSecurityCore.Exceptions;

/// <summary>
/// Exception thrown when SOAP operations fail
/// </summary>
public class WsSoapException : Exception
{
    public WsSoapException() : base() { }
    
    public WsSoapException(string message) : base(message) { }
    
    public WsSoapException(string message, Exception innerException) 
        : base(message, innerException) { }
} 