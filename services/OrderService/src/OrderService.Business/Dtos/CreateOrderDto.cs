using System.ComponentModel.DataAnnotations;

namespace OrderService.Business.Dtos;

// DTO usato per creare un ordine: contiene l’elenco delle righe (prodotti + quantità).
// Le DataAnnotations servono per validare l'input (es. in WebApi con [ApiController]).
public record CreateOrderDto(
    [Required] List<CreateOrderLineDto> Lines
);

// DTO di una singola riga dell’ordine in creazione.
// Contiene l’identificatore del prodotto e la quantità richiesta.
public record CreateOrderLineDto(
    [Required] int ProductId,
    [Range(1, 100)] int Quantity // quantità valida: almeno 1, massimo 100
);