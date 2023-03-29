using EKsuNewsScrapperService.Steps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsScrapperBase;

namespace EKsuNewsScrapperService;

public class EKsuNewsScrapperService : BaseBackgroundService
{
    private readonly GetContentList _getContentList;
    private readonly GetContents _getContents;
    private readonly SendToRabbit _sendToRabbit;

    protected override async Task Scrap()
    {
        var rt = await _getContentList.Step();
        var tt = await _getContents.Step(rt.ToArray());
        _sendToRabbit.Step(tt);
    }

    public EKsuNewsScrapperService(ILogger<BaseBackgroundService> baseLogger, IConfiguration configuration, GetContentList getContentList,
        GetContents getContents, SendToRabbit sendToRabbit) : base(baseLogger, configuration)
    {
        _getContentList = getContentList;
        _getContents = getContents;
        _sendToRabbit = sendToRabbit;
    }
}