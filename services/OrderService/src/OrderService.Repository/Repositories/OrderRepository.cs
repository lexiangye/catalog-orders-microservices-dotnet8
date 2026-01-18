using CatalogOrders.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using OrderService.Repository.Data;
using OrderService.Repository.Entities;
using OrderService.Repository.Interfaces;

namespace OrderService.Repository.Repositories;

// Implementazione concreta di IOrderRepository. In pratica è la classe che verrà utilizzata per interagire con il
// DB MySQL usando EF Core (OrderDbContext) per fare CRUD sugli ordini. 
/// <inheritdoc cref="IOrderRepository"/>
public class OrderRepository(OrderDbContext context) : IOrderRepository
{
    /// <inheritdoc />
    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
        return await context.Orders
            .AsNoTracking() // sola lettura: non tracciamo le entità -> più performance
            .Include(o => o.Lines) // carica anche le righe dell’ordine
            .OrderByDescending(o => o.CreatedAt) // ordini più recenti per primi
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await context.Orders
            .Include(o => o.Lines) // include righe (OrderLines)
            .FirstOrDefaultAsync(o => o.Id == id); // null se non trovato
    }

    /// <inheritdoc />
    public async Task CreateOrderAsync(Order order)
    {
        // Inserisce un nuovo ordine (con relative righe se presenti)
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        // Cerca l'ordine per chiave primaria
        var order = await context.Orders.FindAsync(orderId);
        if (order is not null)
        {
            // Aggiorna solo lo stato (utile per la saga: Pending -> Confirmed/Rejected)
            order.Status = status;
            await context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task DeleteOrderAsync(int id)
    {
        // Rimuove l'ordine (le righe vengono eliminate per cascade delete)
        var order = await context.Orders.FindAsync(id);
        if (order is not null)
        {
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
        }
    }
}
