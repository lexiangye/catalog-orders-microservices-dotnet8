using CatalogService.Repository.Data;
using CatalogService.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repository.Repositories;

public class StockRepository(CatalogDbContext context) : IStockRepository
{
    public async Task<List<(int ProductId, int Requested, int Available)>> TryReserveStockAsync(
        IEnumerable<(int ProductId, int Quantity)> items)
    {
        var failedItems = new List<(int ProductId, int Requested, int Available)>();
        var itemsList = items.ToList();

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Prima verifica disponibilità
            foreach (var (productId, quantity) in itemsList)
            {
                var stock = await context.Stocks.FirstOrDefaultAsync(s => s.ProductId == productId);
                if (stock is null || stock.Quantity < quantity)
                {
                    failedItems.Add((productId, quantity, stock?.Quantity ?? 0));
                }
            }

            if (failedItems.Count > 0)
            {
                await transaction.RollbackAsync();
                return failedItems;
            }

            // Scala lo stock
            foreach (var (productId, quantity) in itemsList)
            {
                var stock = await context.Stocks.FirstAsync(s => s.ProductId == productId);
                stock.Quantity -= quantity;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return failedItems; // Lista vuota = successo
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ReleaseStockAsync(IEnumerable<(int ProductId, int Quantity)> items)
    {
        foreach (var (productId, quantity) in items)
        {
            var stock = await context.Stocks.FirstOrDefaultAsync(s => s.ProductId == productId);
            if (stock is not null)
            {
                stock.Quantity += quantity;
            }
        }
        await context.SaveChangesAsync();
    }
}