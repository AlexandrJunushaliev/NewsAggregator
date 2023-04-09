namespace NewsAggregatorApi.Models;

public class Article
{
    public string Id { get; set; } = null!;
    public DateTime RegistrationDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string SourceSite { get; set; } = null!;
    public string SourceName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Text { get; set; } = null!;
    public string ArticleUrl { get; set; } = null!;
}