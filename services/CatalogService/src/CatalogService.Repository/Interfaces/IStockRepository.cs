namespace CatalogService.Repository.Interfaces;

public interface IStockRepository
{
    /// <summary>
    /// Tenta di riservare stock per più prodotti atomicamente
    /// </summary>
    /// <returns>Lista di prodotti con stock insufficiente (vuota se tutto OK)</returns>
    Task<List<(int ProductId, int Requested, int Available)>> TryReserveStockAsync(
        IEnumerable<(int ProductId, int Quantity)> items);

    /// <summary>
    /// Rilascia stock (compensazione per ordine cancellato)
    /// </summary>
    Task ReleaseStockAsync(IEnumerable<(int ProductId, int Quantity)> items);
}