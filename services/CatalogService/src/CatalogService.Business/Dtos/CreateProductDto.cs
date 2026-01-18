using System.ComponentModel.DataAnnotations;

namespace CatalogService.Business.Dtos;

/// <summary>
/// Oggetto utilizzato per la creazione o l'aggiornamento di un prodotto.
/// </summary>
/// <param name="Name">Nome del prodotto (Obbligatorio).</param>
/// <param name="Description">Descrizione opzionale.</param>
/// <param name="Price">Prezzo (min 0.01).</param>
/// <param name="Quantity">Quantità iniziale da caricare a magazzino.</param>
public record CreateProductDto(
    [Required] string Name,
    string? Description,
    [Range(0.01, 10000)] decimal Price,
    [Range(0, 10000)] int Quantity
);