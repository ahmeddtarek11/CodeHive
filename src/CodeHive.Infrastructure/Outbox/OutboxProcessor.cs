using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodeHive.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;
    //IserviceScopeFactory instead of DbContext because dbContext is scoped while a background task is a singleton
    // injecting a scoped service into a singleton one creates "Captive Dependency"
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory , ILogger<OutboxProcessor> logger )
    {
        _scopeFactory =scopeFactory;
        _logger =logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Outboxproccessor encountred an Error");

            }

            await Task.Delay(TimeSpan.FromSeconds(5) , stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CodeHiveDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await db.Set<OutboxMessage>().Where(m=>m.ProcessedAt == null && m.Error == null )
                                                    .OrderBy(m=>m.CreatedAt)
                                                    .Take(20)
                                                    .ToListAsync(ct);




        foreach(var message in messages)
        {
            try
            {
                var type = Type.GetType(message.EventType)!;
                var payload = JsonSerializer.Deserialize(message.Payload , type)!;

                await publisher.Publish(payload , type , ct );
                message.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to publish outbox message {Id}" , message.Id);
                message.Error =  ex.Message;
            }

            await db.SaveChangesAsync(ct);
        }
    }







}
