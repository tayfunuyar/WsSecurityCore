using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsSecurityCore.Helper;

namespace WsSecurityCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWsSoapServices(this IServiceCollection services)
        {
            services.AddSingleton<IXmlHelper, XmlHelper>();
            services.AddSingleton<ISoapService, SoapService>();
            return services;
        }
    }
}
