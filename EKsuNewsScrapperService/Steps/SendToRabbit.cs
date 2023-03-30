using EKsuNewsScrapperService.Models;
using Infrastructure;
using NewsProcessor.Domain;

namespace EKsuNewsScrapperService.Steps;

public class SendToRabbit
{
    private readonly RabbitProducer _rabbitClient;

    public SendToRabbit(RabbitProducer rabbitClient)
    {
        _rabbitClient = rabbitClient;
    }

    public void Step(GetContentResponse[] news)
    {
        var message = news.Select(x => new NewsMessageEntry
        {
            Id = x.Id!,
            Text = x.Text!,
            Images = x.Images?.ImagesUrls,
            UrlOfDepartment = x.UrlOfDepartment!,
            UrlOfPost = x.UrlOfPost!,
            Title = x.Title,
            Header = x.Header,
            MainImage = x.Picture,
            Status = x.Status,
            RegDate = x.RegDate!,
            UpdDate = x.UpdDate!,
            VideoUrl = x.VideoUrl
        }).ToArray();
        _rabbitClient.Publish(message);
    }
}