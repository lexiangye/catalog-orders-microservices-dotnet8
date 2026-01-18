using CatalogOrders.Shared.Events;

namespace OrderService.Business.Interfaces;

/// <summary>
/// Definisce i metodi per gestire le risposte asincrone provenienti dal CatalogService riguardo allo stock.
/// Questo componente è un tassello fondamentale per la coreografia della Saga.
/// </summary>
public interface IStockEventHandler
{
    /// <summary>
    /// Gestisce l'esito positivo della prenotazione stock: conferma l'ordine nel sistema.
    /// </summary>
    /// <param name="evt">L'evento di stock riservato con successo.</param>
    Task HandleStockReservedAsync(StockReservedEvent evt);

    /// <summary>
    /// Gestisce il fallimento della prenotazione stock (es. sottoscorta): rifiuta l'ordine.
    /// </summary>
    /// <param name="evt">L'evento contenente il motivo del fallimento.</param>
    Task HandleStockReservationFailedAsync(StockReservationFailedEvent evt);
}