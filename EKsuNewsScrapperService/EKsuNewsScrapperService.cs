using EKsuNewsScrapperService.Domain;
using EKsuNewsScrapperService.Steps;
using Infrastructure;
using Microsoft.Extensions.Logging;
using NewsScrapperBase;

namespace EKsuNewsScrapperService;

public class EKsuNewsScrapperService : BaseBackgroundService
{
    private readonly ILogger<EKsuNewsScrapperService> _logger;
    private readonly RabbitProducer _producer;
    private readonly GetContentList _getContentList;
    private readonly GetContents _getContents;
    private readonly SendToRabbit _sendToRabbit;

    public override async Task Scrap()
    {
        var rt = await _getContentList.Step();
        var tt = await _getContents.Step(rt.ToArray());
        _sendToRabbit.Step(tt);
    }

    public EKsuNewsScrapperService(ILogger<EKsuNewsScrapperService> logger, ILogger<BaseBackgroundService> baseLogger,
        RabbitProducer producer, GetContentList getContentList, GetContents getContents, SendToRabbit sendToRabbit) : base(baseLogger)
    {
        _logger = logger;
        _producer = producer;
        _getContentList = getContentList;
        _getContents = getContents;
        _sendToRabbit = sendToRabbit;
    }
}