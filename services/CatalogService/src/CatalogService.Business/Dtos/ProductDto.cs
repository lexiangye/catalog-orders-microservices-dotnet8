namespace CatalogService.Business.Dtos;

/// <summary>
/// Rappresenta i dati di un prodotto pronti per essere consumati dall'interfaccia API.
/// </summary>
/// <param name="Id">Identificativo univoco del prodotto.</param>
/// <param name="Name">Nome commerciale.</param>
/// <param name="Description">Descrizione testuale.</param>
/// <param name="Price">Prezzo di vendita.</param>
/// <param name="Quantity">Disponibilità attuale a magazzino (aggregata dall'entità Stock).</param>
public record ProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Quantity // Viene dalla tabella Stock
);