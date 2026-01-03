using CatalogService.Business.Dtos;
using CatalogService.Repository.Entities;

namespace CatalogService.Business.Extensions;

public static class DtoExtensions
{
    // Converte da Entity (Database) a DTO (Api)
    public static ProductDto AsDto(this Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock?.Quantity ?? 0 // Gestione null safe
        );
    }

    // Converte da DTO (Api) a Entity (Database)
    // Nota: Riceve un ID opzionale per gli update
    public static Product ToEntity(this CreateProductDto dto)
    {
        return new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            // Creiamo subito anche lo stock associato
            Stock = new Stock
            {
                Quantity = dto.Quantity
            }
        };
    }
}