namespace NewsAggregatorApi.Models;

public class Video
{
    public int Id { get; set; }
    public string ArticleId { get; set; } = null!;
    public string Url { get; set; } = null!;
}