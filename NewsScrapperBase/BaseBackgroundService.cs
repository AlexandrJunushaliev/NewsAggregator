using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = NLog.ILogger;

namespace NewsScrapperBase;

public class BaseBackgroundService : BackgroundService
{
    private readonly ILogger<BaseBackgroundService> _logger;

    public BaseBackgroundService(ILogger<BaseBackgroundService> logger)
    {
        _logger = logger;
    }

    public async virtual Task Scrap() => _logger.LogTrace("I am teapot");

    private async Task BaseRoutine()
    {
        _logger.LogTrace($"Scrapper work started at {DateTime.Now}");
        var sw = new Stopwatch();
        try
        {
            await Scrap();
        }
        finally
        {
            sw.Stop();
            _logger.LogTrace($"Scrapper work finished at {DateTime.Now} in {sw.Elapsed}");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await BaseRoutine();
            await Task.Delay(10000000, stoppingToken);
        }
    }
}