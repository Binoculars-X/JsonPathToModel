using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Helpers;
public static class ConfigHelper
{
    public static IServiceProvider GetConfigurationServices()
    {
        return GetConfigurationServices(false);
    }

    public static IServiceProvider GetConfigurationServices(bool optimizeWithCodeEmitter)
    {
        IConfiguration? configuration = null;

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Sources.Clear();
                configuration = config.Build();
            })
            .ConfigureServices(services =>
            {
                // specific options
                services.AddJsonPathToModel(options =>
                {
                    options.OptimizeWithCodeEmitter = optimizeWithCodeEmitter;
                });
            })
            .Build();

        return host.Services;
    }
}
