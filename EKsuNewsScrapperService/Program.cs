// See https://aka.ms/new-console-template for more information

using System.Text;
using EKsuNewsScrapperService.Domain;
using EKsuNewsScrapperService.Steps;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Utils;

namespace EKsuNewsScrapperService;

public class EKsuNewsScrapperBase : NewsScrapperBase.NewsScrapperBase
{
    public override Type GetBackgroundServiceType() => typeof(EKsuNewsScrapperService);

    public override void AppendSettings(HostBuilderContext context, IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ConnectionFactory>(sp => new ConnectionFactory()
        {
            HostName = "localhost"
        });
        serviceCollection.AddSingleton<RabbitProducer,NewsRabbitMqProducer>();
        serviceCollection.AddSingleton<GetContents>();
        serviceCollection.AddSingleton<GetContentList>();
        serviceCollection.AddSingleton<SendToRabbit>();
        serviceCollection.AddSingleton<Limiter>(sp=>new Limiter(5));
    }

    public new static async Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        await NewsScrapperBase.NewsScrapperBase.Main(args);
    }
}