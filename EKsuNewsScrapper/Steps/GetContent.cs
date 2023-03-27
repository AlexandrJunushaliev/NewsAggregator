using EKsuNewsScrapper.Domain;
using EKsuNewsScrapper.Models;
using EKsuNewsScrapper.Validators;
using NewsScrapper.Models;
using Utils;

namespace EKsuNewsScrapper.Steps;

public class GetContents : IScrapperIntermediateWalkStep<GetContentListResponseEntry[], Task<GetContentResponse[]>>
{
    private Limiter _limiter;
    private GetContentValidator _validator;

    public GetContents()
    {
        _validator = new GetContentValidator();
        _limiter = new Limiter(5);
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