using CatalogService.Repository.Entities;

namespace CatalogService.Repository.Interfaces;

public interface IProductRepository
{
    // Lettura (Queries)
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    
    // Scrittura (Commands)
    Task CreateProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
}