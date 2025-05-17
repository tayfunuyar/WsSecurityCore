using System;

namespace WsSecurityCore.Logging;

/// <summary>
/// Default logger implementation that does nothing
/// </summary>
public class DefaultWsLogger : IWsLogger
{
    public void LogDebug(string message) { }
    
    public void LogInformation(string message) { }
    
    public void LogWarning(string message) { }
    
    public void LogError(string message, Exception? exception = null) { }
} 