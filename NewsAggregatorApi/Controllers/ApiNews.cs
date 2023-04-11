using NewsAggregatorApi.Models;

namespace NewsAggregatorApi.Controllers;

public class ApiNews
{
    public DateTime RegistrationDate { get; set; }
    public string SourceSite { get; set; } = null!;
    public string SourceName { get; set; } = null!;
    public string[]? Keywords { get; set; }
    public string? Title { get; set; }
    public string? Header { get; set; }
    public string NewsText { get; set; } = null!;
    public string Id { get; set; }
    public Status Status { get; set; }
    public string NewsUrl { get; set; }
}