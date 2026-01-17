using CatalogOrders.Shared.Events;

namespace OrderService.Business.Interfaces;

// Astrarre la pubblicazione degli eventi (Kafka) dietro un'interfaccia
// permette al Business di non dipendere direttamente dal broker/libreria Kafka.
// Così puoi testare il Business mockando questo publisher.
public interface IEventPublisher
{
    // Pubblica l’evento “OrderCreated” per far partire la saga (Catalog scalerà lo stock)
    Task PublishOrderCreatedAsync(OrderCreatedEvent evt);

    // Pubblica l’evento “OrderCancelled” per la compensazione/rollback (se usato nel tuo flusso)
    Task PublishOrderCancelledAsync(OrderCancelledEvent evt);
}