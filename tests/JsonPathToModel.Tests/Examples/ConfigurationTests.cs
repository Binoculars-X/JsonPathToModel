using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace JsonPathToModel.Tests.Examples;

public class ConfigurationTests
{
    protected IServiceProvider _services;

    public ConfigurationTests()
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
                // default options
                //services.AddJsonPathToModel();

                // specific options
                services.AddJsonPathToModel(options => 
                { 
                    options.OptimizeWithCodeEmitter = true;
                });
            })
            .Build();

        _services = host.Services;
    }

    [Fact]
    public void GetJsonPathModelNavigatorFromServicesTest()
    {
        var navi = _services.GetService<IJsonPathModelNavigator>() as JsonPathModelNavigator;
        Assert.NotNull(navi);
        Assert.True(navi._options.OptimizeWithCodeEmitter);
    }
}
