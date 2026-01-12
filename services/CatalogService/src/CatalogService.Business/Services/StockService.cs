using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using CatalogService.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace CatalogService.Business.Services;

public class StockService(
    IStockRepository stockRepository,
    IEventPublisher eventPublisher,
    ILogger<StockService> logger) : IStockService
{
    public async Task HandleOrderCreatedAsync(OrderCreatedEvent evt)
    {
        logger.LogInformation("Processing OrderCreated for Order {OrderId}", evt.OrderId);

        // Converti items e tenta riserva stock
        var itemsToReserve = evt.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        var failedItems = await stockRepository.TryReserveStockAsync(itemsToReserve);

        // Pubblica evento risposta
        if (failedItems.Count == 0)
        {
            var reservedEvent = new StockReservedEvent(
                evt.OrderId,
                evt.Items.Select(i => new ReservedItem(i.ProductId, i.Quantity)).ToList(),
                DateTimeOffset.UtcNow
            );
            await eventPublisher.PublishStockReservedAsync(reservedEvent);
            logger.LogInformation("✅ Stock reserved for Order {OrderId}", evt.OrderId);
        }
        else
        {
            var failedEvent = new StockReservationFailedEvent(
                evt.OrderId,
                "Insufficient stock",
                failedItems.Select(f => new FailedItem(f.ProductId, f.Requested, f.Available)).ToList(),
                DateTimeOffset.UtcNow
            );
            await eventPublisher.PublishStockReservationFailedAsync(failedEvent);
            logger.LogWarning("❌ Stock reservation failed for Order {OrderId}", evt.OrderId);
        }
    }

    public async Task HandleOrderCancelledAsync(OrderCancelledEvent evt)
    {
        logger.LogInformation("Processing OrderCancelled for Order {OrderId}", evt.OrderId);

        // Rilascia stock (compensazione)
        var itemsToRelease = evt.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        await stockRepository.ReleaseStockAsync(itemsToRelease);

        var releasedEvent = new StockReleasedEvent(
            evt.OrderId,
            evt.Items.Select(i => new ReleasedItem(i.ProductId, i.Quantity)).ToList(),
            DateTimeOffset.UtcNow
        );
        await eventPublisher.PublishStockReleasedAsync(releasedEvent);
        
        logger.LogInformation("✅ Stock released for cancelled Order {OrderId}", evt.OrderId);
    }
}