namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: stock riservato con successo
/// Pubblicato da: CatalogService
/// Ascoltato da: OrderService (per confermare ordine)
/// </summary>
public class StockReservedEvent
{
    public int OrderId { get; set; }
    public List<ReservedItem> ReservedItems { get; set; } = new();
    public DateTimeOffset ReservedAt { get; set; }
}

public class ReservedItem
{
    public int ProductId { get; set; }
    public int QuantityReserved { get; set; }
}