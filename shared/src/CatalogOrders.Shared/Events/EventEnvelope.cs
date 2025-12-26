using CatalogOrders.Shared.Enums;
using System.Text.Json.Serialization;

namespace CatalogOrders.Shared.Events;

/// <summary>
/// Wrapper generico per tutti gli eventi Kafka.
/// Include metadata per idempotenza e tracking.
/// </summary>
public record EventEnvelope<T>(
    [property: JsonPropertyName("eventId")] Guid EventId,
    [property: JsonPropertyName("eventType")] EventType EventType,
    [property: JsonPropertyName("occurredAt")] DateTimeOffset OccurredAt,
    [property: JsonPropertyName("payload")] T Payload
) where T : class
{
    /// <summary>
    /// Factory method statico per creare un envelope in modo comodo
    /// </summary>
    public static EventEnvelope<T> Create(T payload, EventType eventType)
    {
        return new EventEnvelope<T>(
            Guid.NewGuid(),
            eventType,
            DateTimeOffset.UtcNow,
            payload
        );
    }
}

/*
A cosa serve: È un wrapper generico che avvolge tutti gli eventi Kafka.

Perché serve:
  - EventId: per l'idempotenza (evitare di processare lo stesso messaggio due volte)
  - EventType: per sapere che tipo di evento è (utile per logging/debugging)
  - OccurredAt: timestamp dell'evento
  - Payload: il contenuto vero e proprio (OrderCreatedEvent, StockReservedEvent, etc.)
*/