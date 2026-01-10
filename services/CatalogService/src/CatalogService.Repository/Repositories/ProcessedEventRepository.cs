using CatalogService.Repository.Data;
using CatalogService.Repository.Entities;
using CatalogService.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repository.Repositories;

public class ProcessedEventRepository(CatalogDbContext context) : IProcessedEventRepository
{
    public async Task<bool> IsProcessedAsync(Guid eventId)
    {
        return await context.ProcessedEvents.AnyAsync(e => e.EventId == eventId);
    }

    public async Task MarkAsProcessedAsync(Guid eventId, string eventType)
    {
        context.ProcessedEvents.Add(new ProcessedEvent
        {
            EventId = eventId,
            EventType = eventType,
            ProcessedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();
    }
}