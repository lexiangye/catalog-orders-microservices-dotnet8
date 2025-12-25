namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: riserva stock fallita (quantità insufficiente)
/// Pubblicato da: CatalogService
/// Ascoltato da: OrderService (per rigettare ordine)
/// </summary>
public class StockReservationFailedEvent
{
    public int OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<FailedItem> FailedItems { get; set; } = new();
    public DateTimeOffset FailedAt { get; set; }
}

public class FailedItem
{
    public int ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}