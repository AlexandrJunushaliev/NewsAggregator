using EKsuNewsScrapperService.Domain;
using EKsuNewsScrapperService.Models;
using Infrastructure;
using NewsProcessor.Domain;

namespace EKsuNewsScrapperService.Steps;

public class SendToRabbit
{
    private readonly RabbitProducer _processorProducer;
    private readonly RabbitProducer _apiProducer;

    public SendToRabbit(NewsRabbitMqProducer processorProducer,
        NewsApiRabbitMqProducer apiProducer)
    {
        _processorProducer = processorProducer;
        _apiProducer = apiProducer;
    }

    public void Step(GetContentResponse[] news)
    {
        var message = news.Select(x => new NewsMessageEntry
        {
            Id = x.Id!,
            Text = x.Text!,
            Images = x.Images?.ImagesUrls,
            SourceName = x.UrlOfDepartment!,
            SourceUrl = $"https://kpfu.ru/{x.UrlOfDepartment!}",
            NewsUrl = $"https://kpfu.ru/{x.UrlOfPost!}.html",
            Title = x.Title,
            MainImage = x.Picture,
            RegDate = x.RegDate!,
            UpdDate = x.UpdDate,
            VideoUrls = x.VideoUrl,
            Header = x.Header
        }).ToArray();
        _processorProducer.Publish(message);
        _apiProducer.Publish(message);
    }
}