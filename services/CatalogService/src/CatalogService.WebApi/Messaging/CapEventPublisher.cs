using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Enums;
using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using CatalogService.Repository.Data;
using DotNetCore.CAP;

namespace CatalogService.WebApi.Messaging;

/// <summary>
/// Publisher che utilizza CAP per garantire l'invio transazionale dei messaggi.
/// I messaggi vengono salvati nella tabella Outbox insieme alle modifiche al DB.
/// </summary>
public class CapEventPublisher(
    ICapPublisher capPublisher, 
    CatalogDbContext dbContext,
    ILogger<CapEventPublisher> logger) : IEventPublisher
{
    /// <inheritdoc />
    public async Task PublishStockReservedAsync(StockReservedEvent evt)
    {
        var envelope = EventEnvelope<StockReservedEvent>.Create(evt, EventType.StockReserved);
        
        // Usa la transazione del DbContext per garantire atomicità
        await using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);
        try
        {
            // Pubblica il messaggio (verrà salvato nella tabella Outbox)
            await capPublisher.PublishAsync(KafkaTopics.StockReserved, envelope);
            
            // Commit della transazione (DB + Outbox insieme)
            await transaction.CommitAsync();
            
            logger.LogInformation(" Published {EventType} for Order {OrderId} via CAP Outbox",
                EventType.StockReserved, evt.OrderId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task PublishStockReservationFailedAsync(StockReservationFailedEvent evt)
    {
        var envelope = EventEnvelope<StockReservationFailedEvent>.Create(evt, EventType.StockReservationFailed);

        await using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);
        try
        {
            await capPublisher.PublishAsync(KafkaTopics.StockReservationFailed, envelope);
            await transaction.CommitAsync();
            
            logger.LogInformation(" Published {EventType} for Order {OrderId} via CAP Outbox",
                EventType.StockReservationFailed, evt.OrderId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task PublishStockReleasedAsync(StockReleasedEvent evt)
    {
        var envelope = EventEnvelope<StockReleasedEvent>.Create(evt, EventType.StockReleased);

        await using var transaction = dbContext.Database.BeginTransaction(capPublisher, autoCommit: false);
        try
        {
            await capPublisher.PublishAsync(KafkaTopics.StockReleased, envelope);
            await transaction.CommitAsync();
            
            logger.LogInformation(" Published {EventType} for Order {OrderId} via CAP Outbox",
                EventType.StockReleased, evt.OrderId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}