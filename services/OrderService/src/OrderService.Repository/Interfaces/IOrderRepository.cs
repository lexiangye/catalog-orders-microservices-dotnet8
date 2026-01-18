using CatalogOrders.Shared.Enums;
using OrderService.Repository.Entities;

namespace OrderService.Repository.Interfaces;

/// <summary>
/// Definisce il contratto per le operazioni di persistenza degli ordini nel database.
/// Astrae la logica di accesso ai dati (EF Core) dal resto dell'applicazione.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Recupera l'elenco completo di tutti gli ordini, includendo le relative righe.
    /// </summary>
    /// <returns>Una collezione di entità <see cref="Order"/>.</returns>
    Task<IEnumerable<Order>> GetOrdersAsync();
    
    /// <summary>
    /// Recupera un ordine specifico tramite il suo identificatore univoco.
    /// </summary>
    /// <param name="id">L'ID dell'ordine da cercare.</param>
    /// <returns>L'entità <see cref="Order"/> se trovata, altrimenti <c>null</c>.</returns>
    Task<Order?> GetOrderByIdAsync(int id);
    
    /// <summary>
    /// Persiste un nuovo ordine nel database.
    /// </summary>
    /// <param name="order">L'entità ordine da creare.</param>
    Task CreateOrderAsync(Order order);
    
    /// <summary>
    /// Aggiorna esclusivamente lo stato di un ordine esistente.
    /// Tipicamente usato per far avanzare lo stato durante la saga (es. da Pending a Confirmed).
    /// </summary>
    /// <param name="orderId">L'ID dell'ordine da aggiornare.</param>
    /// <param name="status">Il nuovo stato <see cref="OrderStatus"/> da impostare.</param>
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    
    /// <summary>
    /// Rimuove un ordine dal database.
    /// </summary>
    /// <param name="id">L'ID dell'ordine da eliminare.</param>
    Task DeleteOrderAsync(int id);
}