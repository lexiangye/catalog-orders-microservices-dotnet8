using CatalogOrders.Shared.Enums;
using CatalogOrders.Shared.Events;
using Microsoft.Extensions.Logging;
using OrderService.Business.Interfaces;
using OrderService.Repository.Interfaces;

namespace OrderService.Business.Services;

// Handler degli eventi di stock che arrivano dal CatalogService via Kafka.
// In base all'esito della riserva aggiorna lo stato dell'ordine nel DB (parte della saga).
public class StockEventHandler(
    IOrderRepository repository,
    ILogger<StockEventHandler> logger) : IStockEventHandler
{
    // Evento positivo: il Catalog ha riservato lo stock per questo ordine
    public async Task HandleStockReservedAsync(StockReservedEvent evt)
    {
        logger.LogInformation("📦 Stock reserved for Order {OrderId}", evt.OrderId);

        // Aggiorna lo stato dell'ordine -> Confirmed
        await repository.UpdateOrderStatusAsync(evt.OrderId, OrderStatus.Confirmed);

        logger.LogInformation("✅ Order {OrderId} confirmed!", evt.OrderId);
    }

    // Evento negativo: il Catalog non è riuscito a riservare lo stock
    public async Task HandleStockReservationFailedAsync(StockReservationFailedEvent evt)
    {
        logger.LogWarning("❌ Stock reservation failed for Order {OrderId}: {Reason}",
            evt.OrderId, evt.Reason);

        // Aggiorna lo stato dell'ordine -> Rejected (ordine annullato/respinto)
        await repository.UpdateOrderStatusAsync(evt.OrderId, OrderStatus.Rejected);

        logger.LogInformation("🚫 Order {OrderId} rejected", evt.OrderId);
    }
}
