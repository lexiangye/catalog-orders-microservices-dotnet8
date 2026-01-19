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
    /// <inheritdoc />
    public async Task PublishOrderCreatedAsync(OrderCreatedEvent evt)
    {
        var envelope = EventEnvelope<OrderCreatedEvent>.Create(evt, EventType.OrderCreated);
        
        await capPublisher.PublishAsync(KafkaTopics.OrderCreated, envelope); // 👈 Passa l'oggetto, non la stringa
        logger.LogInformation("📤 Published {EventType} to {Topic} via CAP Outbox", 
            envelope.EventType, KafkaTopics.OrderCreated);
    }

    /// <inheritdoc />
    public async Task PublishOrderCancelledAsync(OrderCancelledEvent evt)
    {
        var envelope = EventEnvelope<OrderCancelledEvent>.Create(evt, EventType.OrderCancelled);
        
        await capPublisher.PublishAsync(KafkaTopics.OrderCancelled, envelope); // 👈 Passa l'oggetto, non la stringa
        logger.LogInformation("📤 Published {EventType} to {Topic} via CAP Outbox", 
            envelope.EventType, KafkaTopics.OrderCancelled);
    }
}