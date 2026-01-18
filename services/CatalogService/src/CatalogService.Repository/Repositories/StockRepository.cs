using CatalogService.Repository.Data;
using CatalogService.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repository.Repositories;

/// <summary>
/// Repository dedicato alla gestione delle quantità a magazzino e delle transazioni di stock.
/// </summary>
/// <param name="context">Il contesto del database.</param>
public class StockRepository(CatalogDbContext context) : IStockRepository
{
    /// <summary>
    /// Tenta di riservare lo stock per una serie di prodotti all'interno di una transazione.
    /// Se uno dei prodotti non ha disponibilità sufficiente, l'intera operazione fallisce (rollback).
    /// </summary>
    /// <param name="items">Lista di coppie (IdProdotto, QuantitàRichiesta).</param>
    /// <returns>Una lista di eventuali fallimenti. Se vuota, l'operazione è riuscita.</returns>
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

    /// <summary>
    /// Rilascia le quantità di stock precedentemente impegnate. 
    /// Tipicamente utilizzato per operazioni di compensazione a seguito di annullamento ordini.
    /// </summary>
    /// <param name="items">Collezione di coppie (IdProdotto, QuantitàDaRilasciare).</param>
    public async Task ReleaseStockAsync(IEnumerable<(int ProductId, int Quantity)> items)
    {
        foreach (var (productId, quantity) in items)
        {
            var stock = await context.Stocks.FirstOrDefaultAsync(s => s.ProductId == productId);
            if (stock is not null)
            {
                // Incrementa la disponibilità a magazzino
                stock.Quantity += quantity;
            }
        }
        await context.SaveChangesAsync();
    }
}