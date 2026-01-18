using CatalogOrders.Shared.Events;

namespace CatalogService.Business.Interfaces;

/// <summary>
/// Servizio per il coordinamento della logica di magazzino in risposta ad eventi esterni.
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Gestisce l'evento di creazione ordine, tentando di riservare la merce.
    /// Pubblica <c>StockReservedEvent</c> in caso di successo o <c>StockReservationFailedEvent</c> in caso di errore.
    /// </summary>
    Task HandleOrderCreatedAsync(OrderCreatedEvent evt);
    
    /// <summary>
    /// Gestisce l'evento di cancellazione ordine, rilasciando la merce precedentemente riservata.
    /// Pubblica <c>StockReleasedEvent</c> per confermare il rilascio.
    /// </summary>
    Task HandleOrderCancelledAsync(OrderCancelledEvent evt);
}