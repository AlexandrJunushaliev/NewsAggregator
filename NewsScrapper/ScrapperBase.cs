// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Reflection;
using Infrastructure;
using NewsScrapper.Models;
using NLog;
using NLog.Config;

namespace NewsScrapper;

public abstract class ScrapperBase
{
    public abstract Task Walk();

    public static void BaseRoutine()
    {
        LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");
        var logger = LogManager.GetCurrentClassLogger();
        logger.Trace($"Scrapper work started at {DateTime.Now}");
        var type = Assembly.GetEntryAssembly()!.GetTypes().FirstOrDefault(x => x.BaseType == typeof(ScrapperBase));
        if (type is null)
        {
            logger.Fatal(
                $"There is no {nameof(Walk)} method defined. Perhaps launched default abstract {nameof(ScrapperBase)}?");
            throw new Exception();
        }

        var walkMethodInfo = type.GetMethod(nameof(Walk));
        var sw = new Stopwatch();
        sw.Start();
        try
        {
            ((Task)walkMethodInfo!.Invoke(Activator.CreateInstance(type), null)!).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            logger.Fatal(e.InnerException);
            throw;
        }
        finally
        {
            sw.Stop();
            logger.Trace($"Scrapper work finished at {DateTime.Now} in {sw.Elapsed}");
        }
    }
}