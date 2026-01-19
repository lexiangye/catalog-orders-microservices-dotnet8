using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Events;
using DotNetCore.CAP;
using OrderService.Business.Interfaces;

namespace OrderService.WebApi.Messaging;

/// <summary>
/// Consumer CAP per gli eventi di stock.
/// CAP gestisce automaticamente l'Inbox Pattern: 
/// - I messaggi vengono salvati su DB prima di essere processati
/// - Deduplicazione automatica (idempotenza)
/// - Retry automatico in caso di fallimento
/// </summary>
public class CapStockEventsConsumer : ICapSubscribe
{
    private readonly IStockEventHandler _handler;
    private readonly ILogger<CapStockEventsConsumer> _logger;

    public CapStockEventsConsumer(
        IStockEventHandler handler,
        ILogger<CapStockEventsConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    /// <summary>
    /// Gestisce l'evento di stock riservato con successo.
    /// L'attributo CapSubscribe indica a CAP di sottoscriversi al topic specificato.
    /// </summary>
    [CapSubscribe(KafkaTopics.StockReserved)]
    public async Task HandleStockReservedAsync(EventEnvelope<StockReservedEvent> envelope)
    {
        _logger.LogInformation("📥 [CAP Inbox] Received StockReserved for Order {OrderId}", 
            envelope.Payload.OrderId);
        
        await _handler.HandleStockReservedAsync(envelope.Payload);
    }

    /// <summary>
    /// Gestisce l'evento di fallimento prenotazione stock.
    /// </summary>
    [CapSubscribe(KafkaTopics.StockReservationFailed)]
    public async Task HandleStockReservationFailedAsync(EventEnvelope<StockReservationFailedEvent> envelope)
    {
        _logger.LogInformation("📥 [CAP Inbox] Received StockReservationFailed for Order {OrderId}", 
            envelope.Payload.OrderId);
        
        await _handler.HandleStockReservationFailedAsync(envelope.Payload);
    }
}