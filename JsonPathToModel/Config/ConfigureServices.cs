using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JsonPathToModel;

public static class ConfigureServices
{
    public static IServiceCollection AddJsonPathToModel(this IServiceCollection services)
    {
        services.TryAddSingleton<IJsonPathModelNavigator>(new JsonPathModelNavigator());
        return services;
    }
    public static IServiceCollection AddJsonPathToModel(this IServiceCollection services, Action<NavigatorConfigOptions> configure)
    {
        var options = new NavigatorConfigOptions();
        configure(options);
        services.TryAddSingleton<IJsonPathModelNavigator>(new JsonPathModelNavigator(options));
        return services;
    }
}
