using System;

namespace WsSecurityCore.Exceptions;

/// <summary>
/// Exception thrown when SOAP request validation fails
/// </summary>
public class WsSoapValidationException : WsSoapException
{
    public string? ParameterName { get; }
    
    public WsSoapValidationException() : base() { }
    
    public WsSoapValidationException(string message) : base(message) { }
    
    public WsSoapValidationException(string message, string parameterName) 
        : base(message) 
    { 
        ParameterName = parameterName;
    }
    
    public WsSoapValidationException(string message, Exception innerException) 
        : base(message, innerException) { }
} 