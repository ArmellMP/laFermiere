using PresentationAPI.Extensions;
using PresentationAPI.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. CONFIGURATION DES SERVICES
// ============================================

// 1.1 Contrôleurs avec configuration JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 1.2 Documentation API (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1.3 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 1.4 Health Checks
builder.Services.AddHealthChecks();

// 1.5 NOS SERVICES MÉTIER
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// ✅ DIAGNOSTIC : Vérifier les services enregistrés
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    // Vérification IBandeRepository (existant)
    try
    {
        var repo = services.GetRequiredService<Application.Features.Bandes.Interfaces.IBandeRepository>();
        Console.WriteLine("✅ IBandeRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IBandeRepository: {ex.Message}");
    }
    
    // ✅ NOUVEAU : Vérification IStockRepository
    try
    {
        var stockRepo = services.GetRequiredService<Application.Features.Stocks.Interfaces.IStockRepository>();
        Console.WriteLine("✅ IStockRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IStockRepository: {ex.Message}");
    }
    
    // Vérification DapperContext
    try
    {
        var context = services.GetRequiredService<Infrastructure.DbContext.DapperContext.DapperContext>();
        Console.WriteLine("✅ DapperContext est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR DapperContext: {ex.Message}");
    }
}

// ============================================
// 2. CONFIGURATION DU PIPELINE HTTP
// ============================================

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Json(new
{
    message = "Bienvenue sur l'API La Fermière",
    version = "1.0.0",
    documentation = "/swagger",
    health = "/health"
}));

app.Run();