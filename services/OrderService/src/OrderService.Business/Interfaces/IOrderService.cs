using OrderService.Business.Dtos;

namespace OrderService.Business.Interfaces;

/// <summary>
/// Interfaccia del servizio applicativo per la gestione del ciclo di vita degli ordini.
/// Coordina le operazioni tra database, chiamate esterne (Catalog) e messaggistica (Kafka).
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Recupera tutti gli ordini presenti nel sistema, convertiti in DTO.
    /// </summary>
    /// <returns>Una collezione di <see cref="OrderDto"/>.</returns>
    Task<IEnumerable<OrderDto>> GetOrdersAsync();

    /// <summary>
    /// Recupera un singolo ordine tramite il suo identificatore.
    /// </summary>
    /// <param name="id">L'ID dell'ordine.</param>
    /// <returns>Il DTO dell'ordine se trovato, altrimenti <c>null</c>.</returns>
    Task<OrderDto?> GetOrderByIdAsync(int id);

    /// <summary>
    /// Avvia il processo di creazione di un nuovo ordine.
    /// Valida i prodotti tramite il CatalogService, salva l'ordine in stato 'Pending' 
    /// e pubblica l'evento per l'avvio della Saga.
    /// </summary>
    /// <param name="dto">Dati dell'ordine in creazione.</param>
    /// <returns>Il DTO dell'ordine creato.</returns>
    /// <exception cref="InvalidOperationException">Lanciata se un prodotto non viene trovato nel catalogo.</exception>
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);

    /// <summary>
    /// Annulla un ordine esistente (se in stato Confirmed) e avvia la procedura di compensazione stock.
    /// </summary>
    /// <param name="id">L'ID dell'ordine da annullare.</param>
    Task CancelOrderAsync(int id);
}