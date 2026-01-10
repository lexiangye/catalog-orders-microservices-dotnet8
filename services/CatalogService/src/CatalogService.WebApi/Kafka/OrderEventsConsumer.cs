using System.Text.Json;
using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using Confluent.Kafka;

namespace CatalogService.WebApi.Kafka;

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
            EnableAutoCommit = true // Semplificato: auto-commit
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe([KafkaTopics.OrderCreated, KafkaTopics.OrderCancelled]);
        _logger.LogInformation("📡 Subscribed to Kafka topics");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null) continue;

                using var scope = _scopeFactory.CreateScope();
                var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();

                switch (result.Topic)
                {
                    case KafkaTopics.OrderCreated:
                        var created = JsonSerializer.Deserialize<EventEnvelope<OrderCreatedEvent>>(result.Message.Value, _jsonOptions);
                        if (created is not null)
                            await stockService.HandleOrderCreatedAsync(created.Payload);
                        break;

                    case KafkaTopics.OrderCancelled:
                        var cancelled = JsonSerializer.Deserialize<EventEnvelope<OrderCancelledEvent>>(result.Message.Value, _jsonOptions);
                        if (cancelled is not null)
                            await stockService.HandleOrderCancelledAsync(cancelled.Payload);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message");
            }
        }
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}