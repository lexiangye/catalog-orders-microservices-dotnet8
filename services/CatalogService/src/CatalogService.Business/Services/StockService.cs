using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using CatalogService.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace CatalogService.Business.Services;

public class StockService(
    IStockRepository stockRepository,
    IProcessedEventRepository processedEventRepository,
    IEventPublisher eventPublisher,
    ILogger<StockService> logger) : IStockService
{
    public async Task HandleOrderCreatedAsync(Guid eventId, OrderCreatedEvent evt)
    {
        // 1. Idempotenza: verifica se già processato
        if (await processedEventRepository.IsProcessedAsync(eventId))
        {
            logger.LogWarning("Event {EventId} already processed, skipping", eventId);
            return;
        }

        logger.LogInformation("Processing OrderCreated for Order {OrderId}", evt.OrderId);

        // 2. Converti items
        var itemsToReserve = evt.Items.Select(i => (i.ProductId, i.Quantity)).ToList();

        // 3. Tenta riserva stock
        var failedItems = await stockRepository.TryReserveStockAsync(itemsToReserve);

        // 4. Marca come processato
        await processedEventRepository.MarkAsProcessedAsync(eventId, nameof(OrderCreatedEvent));

        // 5. Pubblica evento risposta
        if (failedItems.Count == 0)
        {
            // Successo
            var reservedEvent = new StockReservedEvent(
                evt.OrderId,
                evt.Items.Select(i => new ReservedItem(i.ProductId, i.Quantity)).ToList(),
                DateTimeOffset.UtcNow
            );
            await eventPublisher.PublishStockReservedAsync(reservedEvent);
            logger.LogInformation("Stock reserved for Order {OrderId}", evt.OrderId);
        }
        else
        {
            // Fallimento
            var failedEvent = new StockReservationFailedEvent(
                evt.OrderId,
                "Insufficient stock",
                failedItems.Select(f => new FailedItem(f.ProductId, f.Requested, f.Available)).ToList(),
                DateTimeOffset.UtcNow
            );
            await eventPublisher.PublishStockReservationFailedAsync(failedEvent);
            logger.LogWarning("Stock reservation failed for Order {OrderId}", evt.OrderId);
        }
    }

    public async Task HandleOrderCancelledAsync(Guid eventId, OrderCancelledEvent evt)
    {
        if (await processedEventRepository.IsProcessedAsync(eventId))
        {
            logger.LogWarning("Event {EventId} already processed, skipping", eventId);
            return;
        }

        logger.LogInformation("Processing OrderCancelled for Order {OrderId}", evt.OrderId);

        // Rilascia stock
        var itemsToRelease = evt.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        await stockRepository.ReleaseStockAsync(itemsToRelease);

        await processedEventRepository.MarkAsProcessedAsync(eventId, nameof(OrderCancelledEvent));

        // Pubblica conferma
        var releasedEvent = new StockReleasedEvent(
            evt.OrderId,
            evt.Items.Select(i => new ReleasedItem(i.ProductId, i.Quantity)).ToList(),
            DateTimeOffset.UtcNow
        );
        await eventPublisher.PublishStockReleasedAsync(releasedEvent);
        
        logger.LogInformation("Stock released for cancelled Order {OrderId}", evt.OrderId);
    }
}