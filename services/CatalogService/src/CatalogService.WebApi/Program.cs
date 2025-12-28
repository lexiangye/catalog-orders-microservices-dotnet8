using CatalogService.Repository.Data;
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

// Configurazione Database MySQL
var connectionString = builder.Configuration.GetConnectionString("CatalogConnection");
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ==========================================
// 2. COSTRUZIONE DELL'APP
// ==========================================
var app = builder.Build();

// ==========================================
// 3. PIPELINE HTTP (Middleware)
// ==========================================

// Abilita Swagger sia in dev che in prod (utile per l'esame)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// Mappa le rotte dei Controller
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
