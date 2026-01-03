using System.ComponentModel.DataAnnotations;

namespace CatalogService.Business.Dtos;

public record CreateProductDto(
    [Required] string Name,
    string? Description,
    [Range(0.01, 10000)] decimal Price,
    [Range(0, 10000)] int Quantity
);