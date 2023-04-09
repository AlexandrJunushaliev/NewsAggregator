namespace NewsAggregatorApi.Controllers;

public class ApiNews
{
    public DateTime RegistrationDate { get; set; }
    public string SourceSite { get; set; } = null!;
    public string SourceName { get; set; } = null!;
    public string? Keywords { get; set; }
    public string Title { get; set; } = null!;
    public string NewsText { get; set; } = null!;
}