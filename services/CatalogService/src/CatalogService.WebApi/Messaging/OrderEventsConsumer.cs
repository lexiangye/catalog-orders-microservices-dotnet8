using System.Text.Json;
using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using Confluent.Kafka;

namespace CatalogService.WebApi.Messaging;

/// <summary>
/// Worker in background che consuma eventi riguardanti gli ordini (creazione/cancellazione).
/// Reagisce aggiornando lo stato dello stock nel database locale del catalogo.
/// </summary>
public class OrderEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<OrderEventsConsumer> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public OrderEventsConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        // Configurazione del Consumer
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            // Identifica il gruppo di consumer. Più istanze con lo stesso GroupId si dividono il lavoro.
            GroupId = "catalog-service-group",
            // Se non c'è un offset salvato, ricomincia dall'inizio del topic
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // Conferma automaticamente la lettura del messaggio (at-most-once/at-least-once semplificato)
            EnableAutoCommit = true 
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    /// <summary>
    /// Ciclo principale di esecuzione del worker.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Sottoscrizione ai topic di interesse definiti nelle costanti condivise
        _consumer.Subscribe([KafkaTopics.OrderCreated, KafkaTopics.OrderCancelled]);
        _logger.LogInformation("📡 Subscribed to Kafka topics: {Created}, {Cancelled}", 
            KafkaTopics.OrderCreated, KafkaTopics.OrderCancelled);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Operazione bloccante che attende nuovi messaggi
                var result = _consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null) continue;

                // Creiamo uno scope per risolvere i servizi Scoped (come il DB context tramite IStockService)
                using var scope = _scopeFactory.CreateScope();
                var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();

                // Routing dei messaggi in base al topic di provenienza
                switch (result.Topic)
                {
                    case KafkaTopics.OrderCreated:
                        var created = JsonSerializer.Deserialize<EventEnvelope<OrderCreatedEvent>>(result.Message.Value, _jsonOptions);
                        if (created is not null)
                            await stockService.HandleOrderCreatedAsync(created.Payload);
                        break;

                    case KafkaTopics.OrderCancelled:
                        var cancelled = JsonSerializer.Deserialize<EventEnvelope<OrderCancelledEvent>>(result.Message.Value, _jsonOptions);
                        if (cancelled is not null)
                            await stockService.HandleOrderCancelledAsync(cancelled.Payload);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Chiusura pulita richiesta dal sistema
                break;
            }
            catch (Exception ex)
            {
                // Logghiamo l'errore e aspettiamo prima di riprovare per evitare loop infiniti su errori fatali
                _logger.LogError(ex, "Error processing Kafka message from topic {Topic}", _consumer.Subscription);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Chiude correttamente il consumer e rilascia le risorse.
    /// </summary>
    public override void Dispose()
    {
        _consumer?.Close(); // Notifica al broker che il consumer sta lasciando il gruppo
        _consumer?.Dispose();
        base.Dispose();
    }
}