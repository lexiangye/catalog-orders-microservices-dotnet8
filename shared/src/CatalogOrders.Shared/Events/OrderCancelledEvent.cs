using System.Text.Json.Serialization;

namespace CatalogOrders.Shared.Events;

/// <summary>
/// Evento: ordine cancellato dall'utente
/// Pubblicato da: OrderService
/// Ascoltato da: CatalogService (per rilasciare stock - compensazione)
/// </summary>
public record OrderCancelledEvent(
    [property: JsonPropertyName("orderId")] int OrderId,
    [property: JsonPropertyName("items")] List<OrderLineItem> Items, // Fondamentale per il rilascio!
    [property: JsonPropertyName("cancelledAt")] DateTimeOffset CancelledAt,
    [property: JsonPropertyName("reason")] string? Reason = null
);