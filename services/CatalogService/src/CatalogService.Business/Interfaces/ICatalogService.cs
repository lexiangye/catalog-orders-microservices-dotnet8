using CatalogService.Business.Dtos;

namespace CatalogService.Business.Interfaces;

public interface ICatalogService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
    Task UpdateProductAsync(int id, CreateProductDto productDto);
    Task DeleteProductAsync(int id);
}