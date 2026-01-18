using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using CatalogService.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace CatalogService.Business.Services;

/// <inheritdoc cref="IStockService"/>
public class StockService(
    IStockRepository stockRepository,
    IEventPublisher eventPublisher,
    ILogger<StockService> logger) : IStockService
{
    /// <inheritdoc />
    public async Task HandleOrderCreatedAsync(OrderCreatedEvent evt)
    {
        logger.LogInformation("Processing OrderCreated for Order {OrderId}", evt.OrderId);

        // Estrae solo le informazioni necessarie (ID e quantità) dall'evento di creazione ordine
        var itemsToReserve = evt.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        
        // Esegue la logica transazionale sul DB: o riserva tutto o niente
        var failedItems = await stockRepository.TryReserveStockAsync(itemsToReserve);

        // Pubblica evento risposta
        if (failedItems.Count == 0)
        {
            // Se lo stock è disponibile, conferma la prenotazione al sistema ordini
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
            // Se mancano pezzi, notifica il fallimento specificando quali prodotti sono insufficienti
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

    /// <inheritdoc />
    public async Task HandleOrderCancelledAsync(OrderCancelledEvent evt)
    {
        logger.LogInformation("Processing OrderCancelled for Order {OrderId}", evt.OrderId);

        // Rilascia stock (compensazione)
        var itemsToRelease = evt.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        await stockRepository.ReleaseStockAsync(itemsToRelease);

        // Conferma l'avvenuto rilascio tramite evento asincrono
        var releasedEvent = new StockReleasedEvent(
            evt.OrderId,
            evt.Items.Select(i => new ReleasedItem(i.ProductId, i.Quantity)).ToList(),
            DateTimeOffset.UtcNow
        );
        await eventPublisher.PublishStockReleasedAsync(releasedEvent);
        
        logger.LogInformation("✅ Stock released for cancelled Order {OrderId}", evt.OrderId);
    }
}