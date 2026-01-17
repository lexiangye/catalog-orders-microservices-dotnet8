using OrderService.Business.Dtos;

namespace OrderService.Business.Interfaces;

// Interfaccia del servizio applicativo degli ordini.
// Definisce i casi d’uso principali che la WebApi può chiamare (senza conoscere EF/Core o Kafka).
public interface IOrderService
{
    // Restituisce l’elenco ordini (tipico endpoint GET /orders)
    Task<IEnumerable<OrderDto>> GetOrdersAsync();

    // Restituisce un ordine singolo (tipico endpoint GET /orders/{id})
    Task<OrderDto?> GetOrderByIdAsync(int id);

    // Crea un ordine a partire dai dati ricevuti (tipico endpoint POST /orders)
    // Di solito salva sul DB e pubblica l’evento OrderCreated per avviare la saga.
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);

    // Annulla un ordine (tipico endpoint POST/DELETE /orders/{id}/cancel)
    Task CancelOrderAsync(int id);
}