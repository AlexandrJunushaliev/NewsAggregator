using System.Text.Json;
using EKsuNewsScrapperService.Models;
using Microsoft.Extensions.Logging;
using NLog;
using Utils;

namespace EKsuNewsScrapperService.Validators;

public class GetContentListValidator : IValidator<GetContentListResponse>
{
    private readonly ILogger<GetContentListValidator> _logger;

    public GetContentListValidator(ILogger<GetContentListValidator> logger)
    {
        _logger = logger;
    }

    public bool IsValid(GetContentListResponse entity, Uri requestUri)
    {
        if (entity.GetContentListEntries.IsNullOrEmpty())
        {
            _logger.LogWarning($"Request for ${requestUri} returned no elements");
            return false;
        }

        if (entity.GetContentListEntries!.All(x => !IsValid(x, requestUri)))
        {
            _logger.LogCritical($"Missing id in all retrived news by {requestUri}");
            return false;
        }

        return true;
    }

    private bool IsValid(GetContentListResponseEntry entry, Uri requestUri)
    {
        if (entry.Id is null)
        {
            _logger.LogCritical($"$Entry from {requestUri} had no id: {JsonSerializer.Serialize(entry)}");
            return false;
        }

        return true;
    }
}