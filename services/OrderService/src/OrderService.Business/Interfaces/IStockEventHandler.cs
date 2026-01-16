using CatalogOrders.Shared.Events;

namespace OrderService.Business.Interfaces;

// Gestisce gli eventi di stock che arrivano dal CatalogService tramite Kafka.
// Serve per aggiornare lo stato dell’ordine in base all’esito della riserva stock (saga).
public interface IStockEventHandler
{
    // Evento positivo: lo stock è stato riservato -> ordine può passare a Confirmed
    Task HandleStockReservedAsync(StockReservedEvent evt);

    // Evento negativo: riserva stock fallita -> ordine passa a Rejected/Cancelled
    Task HandleStockReservationFailedAsync(StockReservationFailedEvent evt);
}