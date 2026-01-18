using CatalogService.Business.Dtos;
using CatalogService.Repository.Entities;

namespace CatalogService.Business.Extensions;

/// <summary>
/// Metodi di estensione per la mappatura tra entità di database e oggetti di trasferimento dati (DTO).
/// </summary>
public static class DtoExtensions
{
    /// <summary>
    /// Mappa un'entità <see cref="Product"/> in un <see cref="ProductDto"/>.
    /// </summary>
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

    /// <summary>
    /// Mappa un'entità <see cref="ProductDto"/> in un <see cref="Product"/>.
    /// </summary>
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