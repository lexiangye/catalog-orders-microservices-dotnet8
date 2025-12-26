using System.Text.Json.Serialization;

namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: nuovo ordine creato (stato PENDING)
/// Pubblicato da: OrderService
/// Ascoltato da: CatalogService (per riservare stock)
/// </summary>
public record OrderCreatedEvent(
    [property: JsonPropertyName("orderId")] int OrderId,
    [property: JsonPropertyName("items")] List<OrderLineItem> Items,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt
);

/// <summary>
/// Riga dell'ordine nell'evento (versione semplificata)
/// </summary>
public record OrderLineItem(
    [property: JsonPropertyName("productId")] int ProductId,
    [property: JsonPropertyName("quantity")] int Quantity
);