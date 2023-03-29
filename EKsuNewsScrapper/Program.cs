using System.Text;
using EKsuNewsScrapper.Models;
using EKsuNewsScrapper.Steps;
using Infrastructure;
using NewsScrapper;
using NewsScrapper.Models;

namespace EKsuNewsScrapper;

class EKsuScrapper : ScrapperBase
{
    private static RabbitClient _sendNewsRabbitClient;
    private static RabbitClient _getTaskRabbitClient;
    public override async Task Walk()
    {
        var rt = await new GetContentList().Step();
        var tt = await new GetContents().Step(rt.ToArray());
        new SendToRabbit(_sendNewsRabbitClient).Step(tt);
    }

    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _sendNewsRabbitClient = new RabbitClient("localhost", "news");
        if (args[0] == "--rabbit")
        {
            _getTaskRabbitClient = new RabbitClient("localhost", "tasks");
            _getTaskRabbitClient.SubscribeToQueue<NewsRequest>(x=>BaseRoutine());
            Console.WriteLine("Press any key to stop app");
            Console.ReadKey();
        }
        
        
        BaseRoutine();
    }
}