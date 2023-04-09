﻿using Infrastructure;
using Microsoft.EntityFrameworkCore;
using NewsAggregatorApi.Infrastructure.EntityFramework;
using NewsAggregatorApi.Models;
using NewsProcessor.Domain;
using NewsProcessor.Index;
using RabbitMQ.Client;

namespace NewsAggregatorApi;

class NewsProcessorService : RabbitConsumer, IHostedService
{
    public NewsProcessorService(ILogger<RabbitConsumer> baseLogger,
        ILogger<RabbitMqClientBase> clientLogger, ConnectionFactory factory, IServiceProvider sp) : base(factory,
        baseLogger,
        clientLogger)
    {
        SetConsume<NewsMessageEntry[]>((news) =>
        {
            using (var scope = sp.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<NewsAggregatorContext>();
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                var distinct = news.DistinctBy(x => x.Id).ToArray();
                var distinctIds = distinct.Select(x => x.Id).ToArray();
                var toUpdateIds = dbContext.Articles.Select(x => x.Id).Where(x => distinctIds.Contains(x)).ToHashSet();

                foreach (var entry in distinct)
                {
                    var article = new Article
                    {
                        Id = entry.Id,
                        RegistrationDate = DateTime.Parse(entry.RegDate).ToUniversalTime(),
                        UpdateDate = entry.UpdDate is null ? default :
                            DateTime.TryParse(entry.UpdDate, out var upd) ? upd.ToUniversalTime() : default,
                        Text = entry.Text,
                        SourceName = entry.SourceName,
                        Title = entry.Title, SourceSite = entry.SourceUrl, ArticleUrl = entry.NewsUrl
                    };
                    if (toUpdateIds.Contains(entry.Id))
                        dbContext.Update(article);
                    else
                        dbContext.Add(article);
                }

                dbContext.SaveChanges();
            }
        });
    }


    protected override string GetExchange() => "newsForApi";
    protected override string GetQueueAndExchangeRoutingKey() => "newsForApi";

    protected override string QueueName => "localhost.newsForApi";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }
}