using CatalogOrders.Shared.Enums;
using CatalogOrders.Shared.Events;
using CatalogService.ClientHttp;
using Microsoft.Extensions.Logging;
using OrderService.Business.Dtos;
using OrderService.Business.Extensions;
using OrderService.Business.Interfaces;
using OrderService.Repository.Entities;
using OrderService.Repository.Interfaces;

namespace OrderService.Business.Services;

// Servizio applicativo principale degli ordini.
// Qui vive la logica "use case": leggere ordini, creare ordine, cancellare ordine.
// Usa:
// - Repository per DB
// - CatalogService.ClientHttp per validare/recuperare dati prodotto (chiamata sincrona)
// - EventPublisher per pubblicare eventi Kafka (inizio/compensazione saga)
/// <inheritdoc cref="IOrderService" />
public class OrderService(
    IOrderRepository repository,
    ICatalogServiceClient catalogClient,
    IEventPublisher eventPublisher,
    ILogger<OrderService> logger) : IOrderService
{
    /// <inheritdoc />
    public async Task<IEnumerable<OrderDto>> GetOrdersAsync()
    {
        // Recupera dal DB e converte entità -> DTO
        var orders = await repository.GetOrdersAsync();
        return orders.Select(o => o.AsDto());
    }

    /// <inheritdoc />
    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        // Recupera dal DB un ordine singolo e converte entità -> DTO
        var order = await repository.GetOrderByIdAsync(id);
        return order?.AsDto();
    }

    /// <inheritdoc />
    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        // 1) Crea un nuovo ordine (stato iniziale di default: Pending)
        var order = new Order();

        // 2) Per ogni riga, valida/recupera il prodotto dal CatalogService (chiamata HTTP sincrona)
        // Nota: qui è dove il Circuit Breaker del client HTTP fa la differenza se Catalog è giù.
        foreach (var line in dto.Lines)
        {
            var product = await catalogClient.GetProductByIdAsync(line.ProductId);
            if (product is null)
            {
                // Se il prodotto non esiste, interrompiamo la creazione dell'ordine
                throw new InvalidOperationException($"Product {line.ProductId} not found");
            }

            // Salviamo nello OrderService uno "snapshot" del prodotto (nome e prezzo al momento dell'acquisto)
            order.Lines.Add(new OrderLine
            {
                ProductId = line.ProductId,
                ProductName = product.Name,
                Quantity = line.Quantity,
                UnitPrice = product.Price
            });
        }

        // 3) Salva l'ordine nel DB in stato Pending (la saga non ha ancora riservato stock)
        await repository.CreateOrderAsync(order);
        logger.LogInformation("✅ Order {OrderId} created in Pending state", order.Id);

        // 4) Pubblica evento "OrderCreated" su Kafka: qui parte la saga choreography.
        // CatalogService riceverà l'evento e proverà a riservare lo stock.
        var evt = new OrderCreatedEvent(
            order.Id,
            order.Lines.Select(l => new OrderLineItem(l.ProductId, l.Quantity)).ToList(),
            order.CreatedAt
        );
        await eventPublisher.PublishOrderCreatedAsync(evt);

        // 5) Ritorna l'ordine creato (DTO con totali calcolati)
        return order.AsDto();
    }

    /// <inheritdoc />
    public async Task CancelOrderAsync(int id)
    {
        // Recupera l'ordine
        var order = await repository.GetOrderByIdAsync(id);
        if (order is null) return;

        // Regola semplice: cancelliamo solo ordini confermati.
        // (così ha senso pubblicare un evento di compensazione per rilasciare stock)
        if (order.Status != OrderStatus.Confirmed)
        {
            logger.LogWarning("Cannot cancel order {OrderId} with status {Status}", id, order.Status);
            return;
        }

        // 1) Aggiorna lo stato nel DB
        await repository.UpdateOrderStatusAsync(id, OrderStatus.Cancelled);

        // 2) Pubblica evento "OrderCancelled" per compensazione:
        // CatalogService potrà rilasciare lo stock precedentemente riservato.
        var evt = new OrderCancelledEvent(
            order.Id,
            order.Lines.Select(l => new OrderLineItem(l.ProductId, l.Quantity)).ToList(),
            DateTimeOffset.UtcNow,
            "Cancelled by user"
        );
        await eventPublisher.PublishOrderCancelledAsync(evt);

        logger.LogInformation("✅ Order {OrderId} cancelled, compensation event published", id);
    }
}
