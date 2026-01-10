using CatalogOrders.Shared.Events;

namespace CatalogService.Business.Interfaces;

/// <summary>
/// Astrazione per pubblicare eventi (implementazione in WebApi)
/// </summary>
public interface IEventPublisher
{
    Task PublishStockReservedAsync(StockReservedEvent evt);
    Task PublishStockReservationFailedAsync(StockReservationFailedEvent evt);
    Task PublishStockReleasedAsync(StockReleasedEvent evt);
}