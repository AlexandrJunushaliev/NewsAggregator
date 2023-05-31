using System.Diagnostics;
using EKsuNewsScrapperService.Domain;
using EKsuNewsScrapperService.Models;
using EKsuNewsScrapperService.Steps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EKsuNewsScrapperService;

public class EKsuNewsScrapperService : BackgroundService
{
    private async Task Scrap(bool historyMode, int forDaysBefore)
    {
        if (historyMode)
            for (var i = -forDaysBefore; i <= 0; i++)
            {
                try
                {
                    var rt = await _getContentList.Step(DateTime.Now.AddDays(i));
                    var tt = await _getContents.Step(rt.ToArray());
                    _sendToRabbit.Step(tt);
                    await Task.Delay(2000);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception was thrown, but execution completed");
                }
            }
        else
        {
            var rt = await _getContentList.Step(DateTime.Now);
            var tt = await _getContents.Step(rt.ToArray());
            _sendToRabbit.Step(tt);
        }
    }

    public EKsuNewsScrapperService(ILogger<EKsuNewsScrapperService> logger, IConfiguration configuration,
        GetContentList getContentList,
        GetContents getContents, SendToRabbit sendToRabbit)
    {
        _getContentList = getContentList;
        _getContents = getContents;
        _sendToRabbit = sendToRabbit;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sleepTimeMs = _configuration.GetValue<int>("sleepTimeMs");
        var historyMode = _configuration.GetValue<bool>("historyMode");
        var forDaysBefore = _configuration.GetValue<int>("forDaysBefore");
        if (historyMode)
        {
            await BaseRoutine(historyMode, forDaysBefore);
            await StopAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await BaseRoutine(historyMode, forDaysBefore);
            await Task.Delay(sleepTimeMs == default ? TimeSpan.FromMinutes(5).Milliseconds : sleepTimeMs,
                stoppingToken);
        }
    }


    private async Task BaseRoutine(bool historyMode, int forDaysBefore)
    {
        _logger.LogTrace($"Scrapper work started at {DateTime.Now}");
        var sw = new Stopwatch();
        sw.Start();
        try
        {
            await Scrap(historyMode, forDaysBefore);
        }
        finally
        {
            sw.Stop();
            _logger.LogTrace($"Scrapper work finished at {DateTime.Now} in {sw.Elapsed}");
        }
    }

    private readonly GetContentList _getContentList;
    private readonly GetContents _getContents;
    private readonly SendToRabbit _sendToRabbit;
    private readonly ILogger<EKsuNewsScrapperService> _logger;
    private readonly IConfiguration _configuration;
}