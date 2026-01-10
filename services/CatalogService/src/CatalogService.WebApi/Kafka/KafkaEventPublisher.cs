using System.Text.Json;
using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Enums;
using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using Confluent.Kafka;

namespace CatalogService.WebApi.Kafka;

/// <summary>
/// Implementazione concreta del publisher Kafka (infrastruttura in WebApi)
/// </summary>
public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.All,
            EnableIdempotence = true
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishStockReservedAsync(StockReservedEvent evt)
    {
        var envelope = EventEnvelope<StockReservedEvent>.Create(evt, EventType.StockReserved);
        await PublishAsync(KafkaTopics.StockReserved, envelope);
    }

    public async Task PublishStockReservationFailedAsync(StockReservationFailedEvent evt)
    {
        var envelope = EventEnvelope<StockReservationFailedEvent>.Create(evt, EventType.StockReservationFailed);
        await PublishAsync(KafkaTopics.StockReservationFailed, envelope);
    }

    public async Task PublishStockReleasedAsync(StockReleasedEvent evt)
    {
        var envelope = EventEnvelope<StockReleasedEvent>.Create(evt, EventType.StockReleased);
        await PublishAsync(KafkaTopics.StockReleased, envelope);
    }

    private async Task PublishAsync<T>(string topic, EventEnvelope<T> envelope) where T : class
    {
        var json = JsonSerializer.Serialize(envelope, _jsonOptions);
        var message = new Message<string, string>
        {
            Key = envelope.EventId.ToString(),
            Value = json
        };

        var result = await _producer.ProduceAsync(topic, message);
        _logger.LogInformation("Published {EventType} to {Topic} [partition {Partition}]",
            envelope.EventType, topic, result.Partition.Value);
    }

    public void Dispose() => _producer?.Dispose();
}