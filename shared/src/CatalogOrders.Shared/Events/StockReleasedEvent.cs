using System.Text.Json.Serialization;

namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: stock rilasciato dopo cancellazione
/// Pubblicato da: CatalogService
/// Ascoltato da: (opzionale, può servire per logging/audit)
/// </summary>
public record StockReleasedEvent(
    [property: JsonPropertyName("orderId")] int OrderId,
    [property: JsonPropertyName("releasedItems")] List<ReleasedItem> ReleasedItems,
    [property: JsonPropertyName("releasedAt")] DateTimeOffset ReleasedAt
);

public record ReleasedItem(
    [property: JsonPropertyName("productId")] int ProductId,
    [property: JsonPropertyName("quantityReleased")] int QuantityReleased
);