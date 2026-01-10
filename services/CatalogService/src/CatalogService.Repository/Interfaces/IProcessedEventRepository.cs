namespace CatalogService.Repository.Interfaces;

public interface IProcessedEventRepository
{
    Task<bool> IsProcessedAsync(Guid eventId);
    Task MarkAsProcessedAsync(Guid eventId, string eventType);
}