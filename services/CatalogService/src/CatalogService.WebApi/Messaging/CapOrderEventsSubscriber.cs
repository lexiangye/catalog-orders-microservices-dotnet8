using System.Text.Json;
using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using DotNetCore.CAP;

namespace CatalogService.WebApi.Messaging;

/// <summary>
/// Subscriber CAP che ascolta gli eventi degli ordini da Kafka.
/// I messaggi ricevuti vengono salvati nella tabella "cap.received" per garantire idempotenza.
/// </summary>
public class CapOrderEventsSubscriber(
    IStockService stockService,
    ILogger<CapOrderEventsSubscriber> logger) : ICapSubscribe
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Gestisce l'evento di creazione ordine.
    /// CAP garantisce che questo metodo venga chiamato esattamente una volta per ogni messaggio.
    /// </summary>
    [CapSubscribe(KafkaTopics.OrderCreated)]
    public async Task HandleOrderCreatedAsync(EventEnvelope<OrderCreatedEvent> envelope)
    {
        logger.LogInformation(
            "📥 Received OrderCreated event [EventId: {EventId}] for Order {OrderId}",
            envelope.EventId, envelope.Payload.OrderId);

        try
        {
            await stockService.HandleOrderCreatedAsync(envelope.Payload);
            
            logger.LogInformation(
                "✅ Successfully processed OrderCreated for Order {OrderId}",
                envelope.Payload.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "❌ Failed to process OrderCreated for Order {OrderId}",
                envelope.Payload.OrderId);
            throw; // CAP gestirà il retry automaticamente
        }
    }

    /// <summary>
    /// Gestisce l'evento di cancellazione ordine.
    /// Rilascia lo stock precedentemente riservato (compensazione).
    /// </summary>
    [CapSubscribe(KafkaTopics.OrderCancelled)]
    public async Task HandleOrderCancelledAsync(EventEnvelope<OrderCancelledEvent> envelope)
    {
        logger.LogInformation(
            "📥 Received OrderCancelled event [EventId: {EventId}] for Order {OrderId}",
            envelope.EventId, envelope.Payload.OrderId);

        try
        {
            await stockService.HandleOrderCancelledAsync(envelope.Payload);
            
            logger.LogInformation(
                "✅ Successfully processed OrderCancelled for Order {OrderId} - Stock released",
                envelope.Payload.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "❌ Failed to process OrderCancelled for Order {OrderId}",
                envelope.Payload.OrderId);
            throw;
        }
    }
}