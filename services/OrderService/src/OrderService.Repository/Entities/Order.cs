using CatalogOrders.Shared.Enums;

namespace OrderService.Repository.Entities;

public class Order
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    // Navigation property
    public virtual List<OrderLine> Lines { get; set; } = new();
}