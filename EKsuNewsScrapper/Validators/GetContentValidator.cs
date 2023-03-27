using System.Text.Json;
using EKsuNewsScrapper.Models;
using NLog;

namespace EKsuNewsScrapper.Validators;

public class GetContentValidator : IValidator<GetContentResponse>
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public bool IsValid(GetContentResponse entity, Uri requestUri)
    {

        if (entity.Id is null)
        {
            Logger.Fatal($"There is no ID in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        if (entity.Title is null && entity.Header is null)
        {
            Logger.Fatal($"There is no title or HEADER in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        if (entity.Text is null)
        {
            Logger.Fatal($"There is no text in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        if (entity.UrlOfDepartment is null)
        {
            Logger.Fatal($"There is no m_url in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        if (entity.UrlOfPost is null)
        {
            Logger.Fatal($"There is no c_url in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }
        
        if (entity.RegDate is null)
        {
            Logger.Fatal($"There is no reg_date in entity:{JsonSerializer.Serialize(entity)}");
            return false;
        }

        return true;
    }
}