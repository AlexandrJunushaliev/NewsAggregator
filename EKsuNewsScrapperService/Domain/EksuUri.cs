namespace EKsuNewsScrapperService.Domain;

public static class EksuUri
{
    private const string EKsuBaseUri = "https://shelly.kpfu.ru/e-ksu/";
    private static readonly Uri GetContentList = new(EKsuBaseUri + "ss_journal.get_content_list");
    private static readonly Uri GetContent = new(EKsuBaseUri + "ss_journal.get_content");

    public static Uri BuildGetContentListRequest(DateTime forDate, PortalContentType portalContentType,
        bool isChangedRequest)
    {
        var uriBuilder = new UriBuilder(GetContentList)
        {
            Query = $"p_day={forDate:dd.MM.yyyy}&p_portal_content_type={(int)portalContentType}{(isChangedRequest ? "&p_type=1" : string.Empty)}"
        };
        return uriBuilder.Uri;
    }

    public static Uri BuildGetContent(string id)
    {
        var uriBuilder = new UriBuilder(GetContent)
        {
            Query = $"p_id={id}"
        };
        return uriBuilder.Uri;
    }
}