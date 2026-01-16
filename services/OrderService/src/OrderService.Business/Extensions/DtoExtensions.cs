using OrderService.Business.Dtos;
using OrderService.Repository.Entities;

namespace OrderService.Business.Extensions;

// Metodi di estensione per convertire entità (EF) in DTO (modelli di output).
// Così la logica di mapping resta in un punto solo e non sparsa nei services/controllers.
public static class DtoExtensions
{
    // Converte un'entità Order (con le sue Lines) in un OrderDto pronto per essere restituito dall'API.
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
