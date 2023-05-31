// See https://aka.ms/new-console-template for more information

using System.Text;
using EKsuNewsScrapperService.Domain;
using EKsuNewsScrapperService.Steps;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using RabbitMQ.Client;
using Utils;

namespace EKsuNewsScrapperService;

public class EKsuNewsScrapperBase
{
    public static void AppendSettings(HostBuilderContext _, IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ConnectionFactory>(_ => new ConnectionFactory()
        {
            HostName = "localhost"
        });
        serviceCollection.AddSingleton<NewsRabbitMqProducer>();
        serviceCollection.AddSingleton<NewsApiRabbitMqProducer>();
        serviceCollection.AddSingleton<GetContents>();
        serviceCollection.AddSingleton<GetContentList>();
        serviceCollection.AddSingleton<SendToRabbit>();
        serviceCollection.AddSingleton<Limiter>(_ => new Limiter(5));
    }

    public static async Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<EKsuNewsScrapperService>();
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(new XmlLoggingConfiguration("NLog.config"));
                });
                AppendSettings(context, services);
            })
            .RunConsoleAsync();
    }
}