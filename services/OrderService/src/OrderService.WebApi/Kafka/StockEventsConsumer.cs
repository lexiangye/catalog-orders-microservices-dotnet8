using System.Text.Json;
using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Events;
using Confluent.Kafka;
using OrderService.Business.Interfaces;

namespace OrderService.WebApi.Kafka;

// BackgroundService = servizio in background che gira insieme alla WebApi.
// Qui ascoltiamo Kafka per eventi di stock provenienti da CatalogService
// e li inoltriamo al Business (IStockEventHandler).
public class StockEventsConsumer : BackgroundService
{
    // Serve per creare uno scope DI per ogni messaggio (così possiamo risolvere servizi scoped)
    private readonly IServiceScopeFactory _scopeFactory;

    // Consumer Kafka che riceve messaggi (key/value)
    private readonly IConsumer<string, string> _consumer;

    // Logger per tracciare subscribe/errori
    private readonly ILogger<StockEventsConsumer> _logger;

    // Opzioni JSON: camelCase per allinearsi ai messaggi prodotti
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public StockEventsConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<StockEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        // Config del consumer Kafka
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "order-service-group",        // consumer group: determina come vengono distribuiti i messaggi
            AutoOffsetReset = AutoOffsetReset.Earliest, // se non ci sono offset salvati, parte dall’inizio
            EnableAutoCommit = true                // commit automatico degli offset (semplice, ok per esame)
        };

        // Costruisce il consumer
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    // Metodo principale del BackgroundService: gira finché l’app è viva
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Iscrizione ai topic di interesse (eventi di stock)
        _consumer.Subscribe([KafkaTopics.StockReserved, KafkaTopics.StockReservationFailed]);
        _logger.LogInformation("📡 OrderService subscribed to stock events");

        // Loop di consumo: resta in ascolto finché l’app non si ferma
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Consume è bloccante: aspetta un messaggio o la cancellazione
                var result = _consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null) continue;

                // Crea uno scope per risolvere handler scoped (e.g. dipendenze su repository/dbcontext)
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IStockEventHandler>();

                // In base al topic, deserializza l’envelope e invoca il metodo giusto
                switch (result.Topic)
                {
                    case KafkaTopics.StockReserved:
                        var reserved = JsonSerializer.Deserialize<EventEnvelope<StockReservedEvent>>(
                            result.Message.Value, _jsonOptions);

                        if (reserved is not null)
                            await handler.HandleStockReservedAsync(reserved.Payload);
                        break;

                    case KafkaTopics.StockReservationFailed:
                        var failed = JsonSerializer.Deserialize<EventEnvelope<StockReservationFailedEvent>>(
                            result.Message.Value, _jsonOptions);

                        if (failed is not null)
                            await handler.HandleStockReservationFailedAsync(failed.Payload);
                        break;
                }
            }
            catch (Exception ex)
            {
                // Logga errori ma continua il loop (così il consumer non muore)
                _logger.LogError(ex, "Error processing Kafka message");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    // Chiusura pulita del consumer
    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}
