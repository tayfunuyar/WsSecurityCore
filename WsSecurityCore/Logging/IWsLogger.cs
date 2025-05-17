using System;

namespace WsSecurityCore.Logging;

/// <summary>
/// Logger interface for WS Security operations
/// </summary>
public interface IWsLogger
{
    void LogDebug(string message);
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
} 