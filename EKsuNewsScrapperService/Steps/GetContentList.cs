using EKsuNewsScrapperService.Domain;
using EKsuNewsScrapperService.Models;
using EKsuNewsScrapperService.Validators;
using Microsoft.Extensions.Logging;
using Utils;

namespace EKsuNewsScrapperService.Steps;

public class GetContentList
{
    private readonly GetContentListValidator _contentListValidator;

    public GetContentList(ILogger<GetContentListValidator> logger)
    {
        _contentListValidator = new GetContentListValidator(logger);
    }

    public async Task<GetContentListResponseEntry[]> Step()
    {
        var now = DateTime.Now;
        var tasks = TypesToCheck
            .Select(x => GetUris(x, now))
            .Flatten()
            .Select(HttpCall.Get<GetContentListResponse>)
            .ToList();

        return (await Task.WhenAll(tasks))
            .Where(x => x.HasResponse && _contentListValidator.IsValid(x.Response, x.RequestUri))
            .SelectMany(x => x.Response.GetContentListEntries!)
            .ToArray();
    }

    private static readonly PortalContentType[] TypesToCheck =
        { PortalContentType.News, PortalContentType.Announcement, PortalContentType.Article };

    private static IEnumerable<Uri> GetUris(PortalContentType type, DateTime date)
    {
        yield return EksuUri.BuildGetContentListRequest(date, type, false);
        yield return EksuUri.BuildGetContentListRequest(date, type, true);
    }
}