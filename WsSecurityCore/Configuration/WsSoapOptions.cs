using System;

namespace WsSecurityCore.Configuration;

/// <summary>
/// Configuration options for the WS SOAP service
/// </summary>
public class WsSoapOptions
{
    /// <summary>
    /// Default timeout for HTTP requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Number of retry attempts for failed requests
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Delay between retry attempts in milliseconds
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 500;
    
    /// <summary>
    /// Whether to use exponential backoff for retries
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
    
    /// <summary>
    /// Maximum backoff time in milliseconds
    /// </summary>
    public int MaxBackoffMilliseconds { get; set; } = 30000; // 30 seconds
    
    /// <summary>
    /// Whether to throw exceptions on errors instead of returning error responses
    /// </summary>
    public bool ThrowExceptions { get; set; } = false;
} 