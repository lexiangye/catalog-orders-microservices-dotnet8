using CatalogService.Repository.Data;
using CatalogService.Repository.Interfaces;
using CatalogService.Repository.Repositories;
using CatalogService.Business.Interfaces;
using CatalogService.Business.Services;
using CatalogService.WebApi.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURAZIONE DEI SERVIZI (DI)
// ==========================================

// Abilita i Controller (fondamentale per la nostra architettura)
builder.Services.AddControllers();

// Configurazione Swagger/OpenAPI (per testare le API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === DATABASE ===
var connectionString = builder.Configuration.GetConnectionString("CatalogConnection");
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// === REPOSITORY (Scoped) ===
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();

// === BUSINESS SERVICES (Scoped) ===
builder.Services.AddScoped<ICatalogService, CatalogService.Business.Services.CatalogService>();
builder.Services.AddScoped<IStockService, StockService>();

// === EVENT PUBLISHER (ora usa CAP invece di Kafka diretto) ===
builder.Services.AddScoped<IEventPublisher, CapEventPublisher>();

// === CAP SUBSCRIBER (riceve messaggi da Kafka) ===
builder.Services.AddTransient<OrderEventsSubscriber>();

// === CAP (Transactional Outbox) ===
builder.Services.AddCap(options =>
{
    // 1. Configura MySQL come storage per l'Outbox
    //    CAP creerà automaticamente le tabelle `cap.published` e `cap.received`
    options.UseMySql(connectionString!);

    // 2. Configura Kafka come message broker
    options.UseKafka(kafkaOptions =>
    {
        kafkaOptions.Servers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
    });

    // 3. Abilita la Dashboard (accessibile a /cap)
    options.UseDashboard(dashboardOptions =>
    {
        dashboardOptions.PathMatch = "/cap"; // URL: http://localhost:5052/cap
    });

    // 4. Opzioni avanzate (opzionali ma utili)
    options.FailedRetryCount = 5;           // Riprova 5 volte in caso di fallimento
    options.FailedRetryInterval = 60;       // Secondi tra un retry e l'altro
    options.DefaultGroupName = "catalog-service-group"; // Consumer group per Kafka
});

// ==========================================
// 2. COSTRUZIONE DELL'APP
// ==========================================
var app = builder.Build();

// ==========================================
// 3. PIPELINE HTTP (Middleware)
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();
app.MapControllers();

// ==========================================
// 4. MIGRATION AUTOMATICA (Avvio)
// ==========================================
// Applica le modifiche al DB appena il container parte
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CatalogDbContext>();
        // Crea il DB e le tabelle se non esistono
        context.Database.Migrate();
        Console.WriteLine("✅ Database migration applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while migrating the database: {ex.Message}");
    }
}

app.Run();
