using System.Text.Json;
using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Enums;
using CatalogOrders.Shared.Events;
using DotNetCore.CAP;
using OrderService.Business.Interfaces;

namespace OrderService.WebApi.Messaging;

/// <summary>
/// Implementazione del publisher di eventi tramite DotNetCore.CAP (Transactional Outbox).
/// Gli eventi vengono salvati nella stessa transazione del DB e poi inoltrati a Kafka.
/// </summary>
public class CapEventPublisher(ICapPublisher capPublisher, ILogger<CapEventPublisher> logger) : IEventPublisher
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc />
    public async Task PublishOrderCreatedAsync(OrderCreatedEvent evt)
    {
        var envelope = EventEnvelope<OrderCreatedEvent>.Create(evt, EventType.OrderCreated);
        var json = JsonSerializer.Serialize(envelope, _jsonOptions);
        
        await capPublisher.PublishAsync(KafkaTopics.OrderCreated, json);
        logger.LogInformation("📤 Published {EventType} to {Topic} via CAP Outbox", 
            envelope.EventType, KafkaTopics.OrderCreated);
    }

    /// <inheritdoc />
    public async Task PublishOrderCancelledAsync(OrderCancelledEvent evt)
    {
        var envelope = EventEnvelope<OrderCancelledEvent>.Create(evt, EventType.OrderCancelled);
        var json = JsonSerializer.Serialize(envelope, _jsonOptions);
        
        await capPublisher.PublishAsync(KafkaTopics.OrderCancelled, json);
        logger.LogInformation("📤 Published {EventType} to {Topic} via CAP Outbox", 
            envelope.EventType, KafkaTopics.OrderCancelled);
    }
}