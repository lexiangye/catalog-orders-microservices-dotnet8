using CatalogOrders.Shared.Enums;
using OrderService.Repository.Entities;

namespace OrderService.Repository.Interfaces;

// Contratto del layer Repository: quali operazioni sul database l'OrderService può fare sugli ordini, senza far sapere
// al resto dell'app come vengono fatte
public interface IOrderRepository
{
    // Recupera la lista di tutti gli ordini
    Task<IEnumerable<Order>> GetOrdersAsync();
    
    // Recupera un ordine specifico
    Task<Order?> GetOrderByIdAsync(int id);
    
    // Crea un nuovo ordine
    Task CreateOrderAsync(Order order);
    
    // Aggiorna lo stato di un ordine
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    
    // Cancella un ordine
    Task DeleteOrderAsync(int id);
}