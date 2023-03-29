using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;

namespace NewsScrapperBase;

public class NewsScrapperBase
{
    public virtual Type GetBackgroundServiceType() => typeof(BaseBackgroundService);

    public virtual void AppendSettings(HostBuilderContext context, IServiceCollection serviceCollection)
    {
    }

    private static void AppendSettingsInner(HostBuilderContext context, IServiceCollection serviceCollection)
    {
        var type = Assembly.GetEntryAssembly()!.EntryPoint!.DeclaringType!;
        var getServiceMethod = type
            .GetMethods()
            .First(m => m.Name == nameof(AppendSettings));
        getServiceMethod.Invoke(Activator.CreateInstance(type), new[] { (object)context, (object)serviceCollection });
    }

    public static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                AddBackgroundServiceOfExecutedEntity(services);
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(new XmlLoggingConfiguration("NLog.config"));
                });
                AppendSettingsInner(context, services);
            })
            .RunConsoleAsync();
    }

    private static void AddBackgroundServiceOfExecutedEntity(IServiceCollection services)
    {
        var type = Assembly.GetEntryAssembly()!.EntryPoint!.DeclaringType!;
        var getServiceMethod = type
            .GetMethods()
            .First(m => m.Name == nameof(GetBackgroundServiceType));
        var typeOfService = (Type)getServiceMethod.Invoke(Activator.CreateInstance(type), null)!;

        var addMethod = typeof(ServiceCollectionHostedServiceExtensions)
            .GetMethods()
            .Where(m => m.Name == nameof(ServiceCollectionHostedServiceExtensions.AddHostedService))
            .First(m => m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 1);
        addMethod.MakeGenericMethod(typeOfService).Invoke(null, new object?[] { services });
    }
}