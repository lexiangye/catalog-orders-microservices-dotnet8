using OrderService.Business.Dtos;
using OrderService.Repository.Entities;

namespace OrderService.Business.Extensions;

/// <summary>
/// Fornisce metodi di estensione per mappare le entità di database in oggetti di trasporto (DTO).
/// </summary>
public static class DtoExtensions
{
    /// <summary>
    /// Converte un'entità <see cref="Order"/> nel suo corrispondente <see cref="OrderDto"/>,
    /// calcolando i totali per riga e il totale complessivo dell'ordine.
    /// </summary>
    /// <param name="order">L'entità sorgente.</param>
    /// <returns>Un DTO popolato con i dati dell'ordine.</returns>
    public static OrderDto AsDto(this Order order)
    {
        // Mappa ogni OrderLine (entità) in OrderLineDto (output)
        // Calcola anche TotalPrice per riga (Quantity * UnitPrice).
        var lines = order.Lines.Select(l => new OrderLineDto(
            l.ProductId,
            l.ProductName,
            l.Quantity,
            l.UnitPrice,
            l.Quantity * l.UnitPrice
        )).ToList();

        // Costruisce l'OrderDto completo, includendo TotalAmount come somma delle righe.
        return new OrderDto(
            order.Id,
            order.CreatedAt,
            order.Status,
            lines,
            lines.Sum(l => l.TotalPrice)
        );
    }
}
