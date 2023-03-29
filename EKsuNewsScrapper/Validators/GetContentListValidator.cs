using System.Text.Json;
using EKsuNewsScrapper.Models;
using NLog;
using Utils;

namespace EKsuNewsScrapper.Validators;

public class GetContentListValidator : IValidator<GetContentListResponse>
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public bool IsValid(GetContentListResponse entity, Uri requestUri)
    {
        if (entity.GetContentListEntries.IsNullOrEmpty())
        {
            Logger.Warn($"Request for ${requestUri} returned no elements");
            return false;
        }

        if (entity.GetContentListEntries!.All(x => !IsValid(x, requestUri)))
        {
            Logger.Fatal($"Missing id in all retrived news by {requestUri}");
            return false;
        }

        return true;
    }

    private bool IsValid(GetContentListResponseEntry entry, Uri requestUri)
    {
        if (entry.Id is null)
        {
            Logger.Fatal($"$Entry from {requestUri} had no id: {JsonSerializer.Serialize(entry)}");
            return false;
        }

        return true;
    }
}