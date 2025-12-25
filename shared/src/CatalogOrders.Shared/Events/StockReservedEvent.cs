using System.Text.Json.Serialization;

namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: stock riservato con successo
/// Pubblicato da: CatalogService
/// Ascoltato da: OrderService (per confermare ordine)
/// </summary>
public record StockReservedEvent(
    [property: JsonPropertyName("orderId")] int OrderId,
    [property: JsonPropertyName("reservedItems")] List<ReservedItem> ReservedItems,
    [property: JsonPropertyName("reservedAt")] DateTimeOffset ReservedAt
);

public record ReservedItem(
    [property: JsonPropertyName("productId")] int ProductId,
    [property: JsonPropertyName("quantityReserved")] int QuantityReserved
);