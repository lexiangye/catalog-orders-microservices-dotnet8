using CatalogOrders.Shared.Enums;

namespace OrderService.Repository.Entities;

// Rappresenta un ordine nel database
public class Order
{
    // Identificatore univoco
    public int Id { get; set; }
    
    // Quando è stato creato
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Stato ordine
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    // Righe ordine
    public virtual List<OrderLine> Lines { get; set; } = new(); // navigation property
}