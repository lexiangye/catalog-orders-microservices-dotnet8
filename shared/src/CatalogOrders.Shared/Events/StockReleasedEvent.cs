namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: stock rilasciato dopo cancellazione
/// Pubblicato da: CatalogService
/// Ascoltato da: (opzionale, può servire per logging/audit)
/// </summary>
public class StockReleasedEvent
{
    public int OrderId { get; set; }
    public List<ReleasedItem> ReleasedItems { get; set; } = new();
    public DateTimeOffset ReleasedAt { get; set; }
}

public class ReleasedItem
{
    public int ProductId { get; set; }
    public int QuantityReleased { get; set; }
}