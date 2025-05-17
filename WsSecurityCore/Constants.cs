using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsSecurityCore;

/// <summary>
/// Constants used throughout the application
/// </summary>
public static class Constants
{
    // Success messages
    public const string DataSuccess = "Data retrieved successfully.";
    public const string AllParametersSent = "All parameters sent.";
    
    // Error messages
    public const string ErrorOccurred = "An error occurred. Please check: ";
    public const string ValidationFailed = "Validation failed";
    
    // Validation error messages
    public const string ParametersMissing = "Parameters missing. Please check parameters.";
    public const string SoapUrlMissing = "Endpoint URL missing. Please check endpoint URL.";
    public const string ActionNameMissing = "Action name missing. Please check action name.";
    public const string SoapActionUrlMissing = "Action URL missing. Please check action URL.";
    public const string UsernameMissing = "Username missing. Please check username.";
    public const string PasswordMissing = "Password missing. Please check password.";
    public const string InvalidSoapUrl = "Endpoint URL is not a valid URL.";
    
    // HTTP error messages
    public const string HttpRequestFailed = "HTTP request failed with status code: ";
    public const string ConnectionError = "Connection error occurred: ";
    public const string TimeoutError = "Request timed out after {0} seconds.";
    public const string RetryFailed = "Request failed after {0} retries.";
}
