using CatalogService.Repository.Data;
using CatalogService.Repository.Entities;
using CatalogService.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repository.Repositories;

public class ProductRepository(CatalogDbContext context) : IProductRepository
{
    // Lettura di tutti i Products
    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        // AsNoTracking: Ottimizzazione per sola lettura (non traccia modifiche)
        // Include: Carica anche i dati della tabella Stocks collegata
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Stock) 
            .ToListAsync();
    }

    // Lettura di un Product tramite il suo Id
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await context.Products
            .Include(p => p.Stock)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Creazione di un Product
    public async Task CreateProductAsync(Product product)
    {
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
    }

    // Aggiornamento di un Product
    public async Task UpdateProductAsync(Product product)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();
    }

    // Cancellazione di un Product
    public async Task DeleteProductAsync(int id)
    {
        // Prima dobbiamo trovarlo
        var product = await context.Products.FindAsync(id);
        if (product != null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
    }
}