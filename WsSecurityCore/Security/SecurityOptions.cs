using System.Security.Cryptography.X509Certificates;

namespace WsSecurityCore.Security;

/// <summary>
/// Options for configuring WS-Security features
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// The security mode to use (defaults to UsernameToken)
    /// </summary>
    public SecurityMode Mode { get; set; } = SecurityMode.UsernameToken;
    
    /// <summary>
    /// Client certificate for authentication or signing
    /// </summary>
    public X509Certificate2? ClientCertificate { get; set; }
    
    /// <summary>
    /// Path to client certificate file (optional alternative to ClientCertificate)
    /// </summary>
    public string? ClientCertificatePath { get; set; }
    
    /// <summary>
    /// Password for the client certificate file
    /// </summary>
    public string? ClientCertificatePassword { get; set; }
    
    /// <summary>
    /// Whether to include a timestamp in the security header (default true)
    /// </summary>
    public bool IncludeTimestamp { get; set; } = true;
    
    /// <summary>
    /// Lifetime of the timestamp in minutes (default 5)
    /// </summary>
    public int TimestampLifetimeMinutes { get; set; } = 5;
    
    /// <summary>
    /// Whether to include a nonce in the username token (default false)
    /// </summary>
    public bool IncludeNonce { get; set; } = false;
    
    /// <summary>
    /// Whether to use message addressing (WS-Addressing) (default false)
    /// </summary>
    public bool UseAddressing { get; set; } = false;
    
    /// <summary>
    /// Reference URI for signature (default "#_body")
    /// </summary>
    public string SignatureReferenceUri { get; set; } = "#_body";
    
    /// <summary>
    /// SAML token if using SAML authentication
    /// </summary>
    public string? SamlToken { get; set; }
    
    /// <summary>
    /// SAML assertion ID if using SAML authentication
    /// </summary>
    public string? SamlAssertionId { get; set; }
    
    /// <summary>
    /// Custom namespace prefixes for XML processing
    /// </summary>
    public Dictionary<string, string>? NamespacePrefixes { get; set; }
    
    /// <summary>
    /// Load client certificate from file if specified
    /// </summary>
    public X509Certificate2? GetClientCertificate()
    {
        if (ClientCertificate != null)
            return ClientCertificate;
            
        if (!string.IsNullOrEmpty(ClientCertificatePath))
        {
            return string.IsNullOrEmpty(ClientCertificatePassword)
                ? new X509Certificate2(ClientCertificatePath)
                : new X509Certificate2(ClientCertificatePath, ClientCertificatePassword);
        }
        
        return null;
    }
} 