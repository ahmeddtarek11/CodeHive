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

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor background service started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxProcessor encountered an Error");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        _logger.LogInformation("OutboxProcessor background service stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CodeHiveDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await db.Set<OutboxMessage>().Where(m => m.ProcessedAt == null && m.Error == null)
                                                    .OrderBy(m => m.CreatedAt)
                                                    .Take(20)
                                                    .ToListAsync(ct);




        foreach (var message in messages)
        {
            try
            {
                _logger.LogInformation("OutboxProcessor processing message {Id} of type {Type}", message.Id, message.EventType);
                var concreteType = Type.GetType(message.EventType);
                if (concreteType == null)
                {
                    _logger.LogWarning("OutboxProcessor could not resolve type {EventType}", message.EventType);
                    message.Error = $"Could not resolve type: {message.EventType}";
                    await db.SaveChangesAsync(ct);
                    continue;
                }

                var payload = JsonSerializer.Deserialize(message.Payload, concreteType)!;

                // MassTransit consumers listen for the INTERFACE type (e.g. IPostLikedEvent),
                // not the concrete type (e.g. PostLikedEvent).
                // We must publish using the interface type so MassTransit routes
                // the message to the correct exchange/queue that the consumer is bound to.
                // Filter by interfaces defined in CodeHive.Shared (already a dependency).
                var sharedAssembly = typeof(CodeHive.Shared.BaseEntity).Assembly;
                var messageType = concreteType.GetInterfaces()
                    .FirstOrDefault(i => i.Assembly == sharedAssembly) ?? concreteType;

                _logger.LogInformation("OutboxProcessor publishing message {Id} as {MessageType}", message.Id, messageType.FullName);

                await publisher.Publish(payload, messageType, ct);
                message.ProcessedAt = DateTime.UtcNow;
                _logger.LogInformation("OutboxProcessor successfully published message {Id}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {Id}", message.Id);
                message.Error = ex.Message;
            }

            await db.SaveChangesAsync(ct);
        }
    }







}
