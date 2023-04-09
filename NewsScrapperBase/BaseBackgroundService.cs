using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NewsScrapperBase;

public class BaseBackgroundService : BackgroundService
{
    private readonly ILogger<BaseBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public BaseBackgroundService(ILogger<BaseBackgroundService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected virtual Task Scrap()
    {
        _logger.LogTrace("I am teapot");
        return Task.CompletedTask;
    }

    private async Task BaseRoutine()
    {
        _logger.LogTrace($"Scrapper work started at {DateTime.Now}");
        var sw = new Stopwatch();
        sw.Start();
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
        var sleepTimeMs = _configuration.GetValue<int>("sleepTimeMs");
        while (!stoppingToken.IsCancellationRequested)
        {
            await BaseRoutine();
            await Task.Delay(sleepTimeMs == default ? TimeSpan.FromMinutes(5).Milliseconds : sleepTimeMs, stoppingToken);
        }
    }
}