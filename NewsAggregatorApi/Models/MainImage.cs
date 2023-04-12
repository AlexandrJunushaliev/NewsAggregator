namespace NewsAggregatorApi.Models;

public class MainImage
{
    public int Id { get; set; }
    public string ArticleId { get; set; } = null!;
    public string Url { get; set; } = null!;
}