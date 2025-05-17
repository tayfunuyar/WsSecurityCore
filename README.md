# WsSecurityCore
WS-Security SOAP Request library for .NET 8

## Installation

```bash
dotnet add package WsSecurityCore
```

## Quick Start

1. Register the services in your dependency injection container:

```csharp
// In Program.cs or Startup.cs
builder.Services.AddWsSoapServices(options => 
{
    // Optional: Configure retry policy
    options.MaxRetryAttempts = 3;
    options.RetryDelayMilliseconds = 500;
    options.UseExponentialBackoff = true;
    
    // Optional: Configure timeouts
    options.TimeoutSeconds = 30;
    
    // Optional: Throw exceptions instead of returning error responses
    options.ThrowExceptions = false;
});

// Optional: Add custom logger implementation
builder.Services.AddWsSoapLogger<YourCustomLogger>();
```

2. Inject and use the service in your classes:

```csharp
// Inject ISoapService into your class
public class YourClass
{
    private readonly ISoapService _soapService;

    public YourClass(ISoapService soapService)
    {
        _soapService = soapService;
    }
    
    public async Task<YourResponseType> CallSoapServiceAsync()
    {
        var request = new WsSoapRequest
        {
            EndpointUrl = "https://your-soap-service-url",
            ActionName = "ActionName", // Example: OrderListAsync
            ActionUrl = "ActionUrl", // Example: "http://tempuri.org/IService/OrderList"
            Parameters = new Dictionary<string, object>
            {
                {"YourParameter1", "YourParameterValue1"},
                {"YourParameter2", "YourParameterValue2"},
                // Add more parameters as needed
            },
            Username = "ServiceUsername",
            Password = "ServicePassword",
            GetOnlyBody = false, // If true, returns only the body as XmlNodeList
            ContentType = "text/xml; charset=utf-8" // Optional: override default content type
        };

        // Using the async method (recommended)
        var response = await _soapService.SendSoapRequestAsync(request);
        
        // Or using the synchronous method
        // var response = _soapService.SendSoapRequest(request);
        
        if (response.Result)
        {
            // Process successful response
            // Convert XML to your model using XmlSerializer or other methods
            return ProcessResponse(response.Response);
        }
        else
        {
            // Handle error
            throw new Exception($"SOAP request failed: {response.Message}");
        }
    }
    
    private YourResponseType ProcessResponse(object? responseData)
    {
        // Process the response data
        // Example implementation depends on your specific needs
        // ...
    }
}
```

## Features

- WS-Security SOAP request handling for .NET 8
- Supports both synchronous and asynchronous operations
- Uses modern HttpClient for better performance
- Robust exception handling with custom exception types
- Automatic retry for transient failures with exponential backoff
- Built-in logging capabilities with customizable loggers
- Built with .NET 8 best practices and modern C# features
- Easily integrates with dependency injection
- Configurable timeout and retry policies

## Custom Logging

To implement a custom logger:

```csharp
// Implement the IWsLogger interface
public class YourCustomLogger : IWsLogger
{
    private readonly ILogger<YourCustomLogger> _logger;
    
    public YourCustomLogger(ILogger<YourCustomLogger> logger)
    {
        _logger = logger;
    }
    
    public void LogDebug(string message)
    {
        _logger.LogDebug(message);
    }
    
    public void LogInformation(string message)
    {
        _logger.LogInformation(message);
    }
    
    public void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }
    
    public void LogError(string message, Exception? exception = null)
    {
        _logger.LogError(exception, message);
    }
}

// Register your custom logger
services.AddWsSoapLogger<YourCustomLogger>();
```

## Nuget Package
https://www.nuget.org/packages/WsSecurityCore/

## Issues and Questions
If you have any questions or issues, please open an issue on GitHub.

## Advanced WS-Security Features

The library supports advanced WS-Security features:

### X.509 Certificate Authentication

```csharp
// Using a certificate from the store
var certificate = new X509Certificate2("client.pfx", "password");

var request = new WsSoapRequest
{
    // Basic fields...
    EndpointUrl = "https://secure-service-url",
    ActionName = "SecureAction",
    ActionUrl = "http://example.org/SecureAction",
    Username = "user",
    Password = "pass",
    SecurityOptions = new SecurityOptions
    {
        Mode = SecurityMode.Certificate,
        ClientCertificate = certificate
    }
};
```

### XML Digital Signatures

```csharp
var request = new WsSoapRequest
{
    // Basic fields...
    EndpointUrl = "https://secure-service-url",
    ActionName = "SignedAction",
    ActionUrl = "http://example.org/SignedAction",
    SecurityOptions = new SecurityOptions
    {
        Mode = SecurityMode.MessageSigning,
        ClientCertificatePath = "path/to/signing-cert.pfx",
        ClientCertificatePassword = "password",
        SignatureReferenceUri = "#_body" // Default value
    }
};
```

### Using Nonce for Additional Security

```csharp
var request = new WsSoapRequest
{
    // Basic fields...
    EndpointUrl = "https://secure-service-url",
    ActionName = "SecureAction",
    ActionUrl = "http://example.org/SecureAction",
    SecurityOptions = new SecurityOptions
    {
        IncludeNonce = true // Adds a nonce for replay protection
    }
};
```

### SAML Token Support

```csharp
var request = new WsSoapRequest
{
    // Basic fields...
    EndpointUrl = "https://saml-service-url",
    ActionName = "SamlAction",
    ActionUrl = "http://example.org/SamlAction",
    SecurityOptions = new SecurityOptions
    {
        Mode = SecurityMode.SamlToken,
        SamlToken = "<your SAML token XML>"
    }
};
```

### Customizing Timestamp

```csharp
var request = new WsSoapRequest
{
    // Basic fields...
    EndpointUrl = "https://service-url",
    ActionName = "TimestampAction",
    ActionUrl = "http://example.org/TimestampAction",
    SecurityOptions = new SecurityOptions
    {
        IncludeTimestamp = true, // Default is true
        TimestampLifetimeMinutes = 10 // Default is 5
    }
};
```
