namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: ordine cancellato dall'utente
/// Pubblicato da: OrderService
/// Ascoltato da: CatalogService (per rilasciare stock - compensazione)
/// </summary>
public class OrderCancelledEvent
{
    public int OrderId { get; set; }
    public List<OrderLineItem> Items { get; set; } = new();
    public DateTimeOffset CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}