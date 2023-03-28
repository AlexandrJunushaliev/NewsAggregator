using EKsuNewsScrapperService.Domain;
using EKsuNewsScrapperService.Models;
using EKsuNewsScrapperService.Validators;
using Microsoft.Extensions.Logging;
using Utils;

namespace EKsuNewsScrapperService.Steps;

public class GetContents
{
    private readonly Limiter _limiter;
    private readonly GetContentValidator _validator;

    public GetContents(ILogger<GetContentValidator> logger, Limiter limiter)
    {
        _limiter = limiter;
        _validator = new GetContentValidator(logger);
    }

    public async Task<GetContentResponse[]> Step(GetContentListResponseEntry[] entries)
    {
        var tasks = entries.Select(x => EksuUri.BuildGetContent(x.Id!))
            .Select<Uri, Func<Task<HttpResponse<GetContentResponse>>>>(x => () => HttpCall.Get<GetContentResponse>(x))
            .ToList();
        var result = await _limiter.AwaitAll(tasks);
        return result
            .Where(x => x.HasResponse && _validator.IsValid(x.Response, x.RequestUri))
            .Select(x => x.Response)
            .ToArray();
    }
}