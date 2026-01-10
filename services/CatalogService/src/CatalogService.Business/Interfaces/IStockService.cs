using CatalogOrders.Shared.Events;

namespace CatalogService.Business.Interfaces;

public interface IStockService
{
    // Gestisce OrderCreated: riserva stock e pubblica evento risposta
    Task HandleOrderCreatedAsync(OrderCreatedEvent evt);
    
    // Gestisce OrderCancelled: rilascia stock (compensazione)
    Task HandleOrderCancelledAsync(OrderCancelledEvent evt);
}