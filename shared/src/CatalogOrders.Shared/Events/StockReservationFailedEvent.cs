using System.Text.Json.Serialization;

namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: riserva stock fallita (quantità insufficiente)
/// Pubblicato da: CatalogService
/// Ascoltato da: OrderService (per rigettare ordine)
/// </summary>
public record StockReservationFailedEvent(
    [property: JsonPropertyName("orderId")] int OrderId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("failedItems")] List<FailedItem> FailedItems,
    [property: JsonPropertyName("failedAt")] DateTimeOffset FailedAt
);

public record FailedItem(
    [property: JsonPropertyName("productId")] int ProductId,
    [property: JsonPropertyName("requestedQuantity")] int RequestedQuantity,
    [property: JsonPropertyName("availableQuantity")] int AvailableQuantity
);