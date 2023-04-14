using Microsoft.EntityFrameworkCore;
using NewsAggregatorApi.Domain;
using NewsAggregatorApi.Infrastructure.EntityFramework;
using NewsProcessor.Domain;
using NewsProcessor.Index;
using NLog.Config;
using NLog.Extensions.Logging;
using RabbitMQ.Client;

namespace NewsAggregatorApi;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(o => o.AddDefaultPolicy(x =>
            x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed((host) => true)));
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new SearchIndexEntryIdJsonConverter());
        });
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddNLog(new XmlLoggingConfiguration("NLog.config"));
        });
        services.AddDbContext<DbContext, NewsAggregatorContext>(options =>
        {
            options.UseNpgsql(_configuration.GetConnectionString("NpgSqlConnection"));
        });
        services.AddHostedService<NewsAggreagatorApiService>();
        services.AddSingleton<ConnectionFactory>(_ => new ConnectionFactory()
        {
            HostName = "localhost"
        });
        services.AddSingleton<SearchIndexClient>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors();
        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}