using System.Text.Json;
using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using Confluent.Kafka;

namespace CatalogService.WebApi.Kafka;

/// <summary>
/// HostedService che consuma eventi Kafka (infrastruttura in WebApi)
/// </summary>
public class OrderEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<OrderEventsConsumer> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public OrderEventsConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "catalog-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(new[] { KafkaTopics.OrderCreated, KafkaTopics.OrderCancelled });
        _logger.LogInformation("Subscribed to topics: {Topics}", 
            string.Join(", ", KafkaTopics.OrderCreated, KafkaTopics.OrderCancelled));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null) continue;

                _logger.LogDebug("Received message from {Topic}", result.Topic);

                using var scope = _scopeFactory.CreateScope();
                var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();

                await ProcessMessageAsync(result.Topic, result.Message.Value, stockService);
                
                _consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        }
    }

    private async Task ProcessMessageAsync(string topic, string message, IStockService stockService)
    {
        switch (topic)
        {
            case KafkaTopics.OrderCreated:
                var createdEnvelope = JsonSerializer.Deserialize<EventEnvelope<OrderCreatedEvent>>(message, _jsonOptions);
                if (createdEnvelope is not null)
                    await stockService.HandleOrderCreatedAsync(createdEnvelope.EventId, createdEnvelope.Payload);
                break;

            case KafkaTopics.OrderCancelled:
                var cancelledEnvelope = JsonSerializer.Deserialize<EventEnvelope<OrderCancelledEvent>>(message, _jsonOptions);
                if (cancelledEnvelope is not null)
                    await stockService.HandleOrderCancelledAsync(cancelledEnvelope.EventId, cancelledEnvelope.Payload);
                break;
        }
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}