namespace NewsAggregatorApi.Models;

public class PagedResponse<T>
{
    public ICollection<T> Items { get; set; } = null!;
    public int Count { get; set; }
    public int Total { get; set; }
}