namespace CatalogService.Business.Dtos;

// Nota come abbiamo "appiattito" la struttura: Quantity è qui, non dentro un oggetto Stock annidato.
public record ProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Quantity // Viene dalla tabella Stock
);