using CatalogOrders.Shared.Events;

namespace CatalogService.Business.Interfaces;

/// <summary>
/// Servizio che gestisce la logica stock per la saga
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Gestisce OrderCreated: riserva stock e pubblica evento risposta
    /// </summary>
    Task HandleOrderCreatedAsync(Guid eventId, OrderCreatedEvent evt);

    /// <summary>
    /// Gestisce OrderCancelled: rilascia stock (compensazione)
    /// </summary>
    Task HandleOrderCancelledAsync(Guid eventId, OrderCancelledEvent evt);
}