using CatalogOrders.Shared.Enums;

namespace CatalogOrders.Shared.Events;

/// <summary>
/// Wrapper generico per tutti gli eventi Kafka.
/// Include metadata per idempotenza e tracking.
/// </summary>
public class EventEnvelope<T> where T : class
{
    /// <summary>
    /// ID univoco per gestire idempotenza (evita duplicati)
    /// </summary>
    public Guid EventId { get; set; }
    
    /// <summary>
    /// Tipo di evento (per logging e routing)
    /// </summary>
    public EventType EventType { get; set; }
    
    /// <summary>
    /// Timestamp UTC di quando è avvenuto l'evento
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }
    
    /// <summary>
    /// Payload dell'evento (il contenuto vero e proprio)
    /// </summary>
    public T Payload { get; set; } = null!;
    
    /// <summary>
    /// Factory method per creare un envelope in modo comodo
    /// </summary>
    public static EventEnvelope<T> Create(T payload, EventType eventType)
    {
        return new EventEnvelope<T>
        {
            EventId = Guid.NewGuid(),
            EventType = eventType,
            OccurredAt = DateTimeOffset.UtcNow,
            Payload = payload
        };
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