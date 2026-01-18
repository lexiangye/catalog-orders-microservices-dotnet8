using CatalogOrders.Shared.Events;

namespace OrderService.Business.Interfaces;

/// <summary>
/// Astrae il meccanismo di pubblicazione degli eventi verso l'esterno (es. Kafka).
/// Permette al dominio Order di notificare cambiamenti di stato senza conoscere l'infrastruttura sottostante.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Invia una notifica di ordine creato per richiedere la prenotazione dello stock al CatalogService.
    /// </summary>
    /// <param name="evt">Dati dell'evento di creazione.</param>
    Task PublishOrderCreatedAsync(OrderCreatedEvent evt);

    /// <summary>
    /// Invia una notifica di ordine cancellato per richiedere il rilascio dello stock precedentemente impegnato.
    /// </summary>
    /// <param name="evt">Dati dell'evento di cancellazione.</param>
    Task PublishOrderCancelledAsync(OrderCancelledEvent evt);
}