using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsSecurityCore.Configuration;
using WsSecurityCore.Helper;
using WsSecurityCore.Logging;

namespace WsSecurityCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds WS-Security SOAP services to the service collection with default options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWsSoapServices(this IServiceCollection services)
    {
        return services.AddWsSoapServices(options => { });
    }
    
    /// <summary>
    /// Adds WS-Security SOAP services to the service collection with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">A delegate to configure the WsSoapOptions</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWsSoapServices(
        this IServiceCollection services,
        Action<WsSoapOptions> configureOptions)
    {
        var options = new WsSoapOptions();
        configureOptions(options);
        
        services.AddSingleton(options);
        services.TryAddSingleton<IWsLogger, DefaultWsLogger>();
        services.AddSingleton<IXmlHelper, XmlHelper>();
        services.AddSingleton<ISoapService, SoapService>();
        
        services.AddHttpClient<ISoapService, SoapService>()
            .ConfigureHttpClient(client => 
            {
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });
        
        return services;
    }
    
    /// <summary>
    /// Sets a custom logger for WS-Security SOAP services
    /// </summary>
    /// <typeparam name="TLogger">The logger implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWsSoapLogger<TLogger>(
        this IServiceCollection services)
        where TLogger : class, IWsLogger
    {
        services.AddSingleton<IWsLogger, TLogger>();
        return services;
    }
}
