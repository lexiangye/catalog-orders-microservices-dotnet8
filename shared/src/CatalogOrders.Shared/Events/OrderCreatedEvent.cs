namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: nuovo ordine creato (stato PENDING)
/// Pubblicato da: OrderService
/// Ascoltato da: CatalogService (per riservare stock)
/// </summary>
public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public List<OrderLineItem> Items { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Riga dell'ordine nell'evento (versione semplificata)
/// </summary>
public class OrderLineItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}