using CatalogService.Business.Dtos;
using CatalogService.Business.Extensions;
using CatalogService.Business.Interfaces;
using CatalogService.Repository.Interfaces;

namespace CatalogService.Business.Services;

public class CatalogService(IProductRepository repository) : ICatalogService
{
    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        var products = await repository.GetProductsAsync();
        // Usiamo LINQ e il nostro mapper AsDto
        return products.Select(p => p.AsDto());
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await repository.GetProductByIdAsync(id);
        return product?.AsDto(); // ?. operatore null-conditional, se product è null, ritorna direttamente null senza
                                 // chiamare AsDto().
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
    {
        // 1. Convertiamo DTO -> Entity
        var product = productDto.ToEntity();

        // 2. Salviamo nel DB tramite Repository
        await repository.CreateProductAsync(product);

        // 3. Restituiamo il DTO creato (con l'ID generato dal DB)
        return product.AsDto();
    }

    public async Task UpdateProductAsync(int id, CreateProductDto productDto)
    {
        var existingProduct = await repository.GetProductByIdAsync(id);
        if (existingProduct == null)
        {
            // In uno scenario reale lanceremmo un'eccezione custom, 
            // per ora lasciamo che il controller gestisca il null o non faccia nulla.
            return;
        }

        // Aggiorniamo i campi
        existingProduct.Name = productDto.Name;
        existingProduct.Description = productDto.Description;
        existingProduct.Price = productDto.Price;

        // Aggiorniamo lo stock (se esiste, altrimenti lo creiamo)
        if (existingProduct.Stock != null)
        {
            existingProduct.Stock.Quantity = productDto.Quantity;
        }

        await repository.UpdateProductAsync(existingProduct);
    }

    public async Task DeleteProductAsync(int id)
    {
        await repository.DeleteProductAsync(id);
    }
}