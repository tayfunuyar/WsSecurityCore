namespace WsSecurityCore.Security;

/// <summary>
/// Defines the security mode for WS-Security operations
/// </summary>
public enum SecurityMode
{
    /// <summary>
    /// Basic username token authentication (default)
    /// </summary>
    UsernameToken = 0,
    
    /// <summary>
    /// X.509 certificate-based authentication
    /// </summary>
    Certificate = 1,
    
    /// <summary>
    /// XML digital signature for message signing
    /// </summary>
    MessageSigning = 2,
    
    /// <summary>
    /// XML encryption for message encryption
    /// </summary>
    MessageEncryption = 3,
    
    /// <summary>
    /// Both signing and encryption
    /// </summary>
    SigningAndEncryption = 4,
    
    /// <summary>
    /// SAML token-based authentication
    /// </summary>
    SamlToken = 5
} 