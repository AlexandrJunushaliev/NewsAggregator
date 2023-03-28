using System.Text.Json;
using EKsuNewsScrapperService.Models;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = NLog.ILogger;

namespace EKsuNewsScrapperService.Validators;

public class GetContentValidator : IValidator<GetContentResponse>
{
    private readonly ILogger<GetContentValidator> _logger;
    public GetContentValidator(ILogger<GetContentValidator> logger)
    {
        _logger = logger;
    }

    public bool IsValid(GetContentResponse entity, Uri requestUri)
    {

        if (entity.Id is null)
        {
            _logger.LogCritical($"There is no ID in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        if (entity.Title is null && entity.Header is null)
        {
            _logger.LogCritical($"There is no title or HEADER in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        if (entity.Text is null)
        {
            _logger.LogCritical($"There is no text in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        if (entity.UrlOfDepartment is null)
        {
            _logger.LogCritical($"There is no m_url in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        if (entity.UrlOfPost is null)
        {
            _logger.LogCritical($"There is no c_url in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }
        
        if (entity.RegDate is null)
        {
            _logger.LogCritical($"There is no reg_date in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        return true;
    }
}