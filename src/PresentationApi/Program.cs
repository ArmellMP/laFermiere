using PresentationAPI.Extensions;
using PresentationAPI.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. CONFIGURATION DES SERVICES
// ============================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// ✅ DIAGNOSTIC
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        var repo = services.GetRequiredService<Application.Features.Bandes.Interfaces.IBandeRepository>();
        Console.WriteLine("✅ IBandeRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IBandeRepository: {ex.Message}");
    }
    
    try
    {
        var stockRepo = services.GetRequiredService<Application.Features.Stocks.Interfaces.IStockRepository>();
        Console.WriteLine("✅ IStockRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IStockRepository: {ex.Message}");
    }
    
    try
    {
        var suiviRepo = services.GetRequiredService<Application.Features.SuivisJournaliers.Interfaces.ISuiviJournalierRepository>();
        Console.WriteLine("✅ ISuiviJournalierRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR ISuiviJournalierRepository: {ex.Message}");
    }
    
    try
    {
        var clientRepo = services.GetRequiredService<Application.Features.Clients.Interfaces.IClientRepository>();
        Console.WriteLine("✅ IClientRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IClientRepository: {ex.Message}");
    }
    
    try
    {
        var fournisseurRepo = services.GetRequiredService<Application.Features.Fournisseurs.Interfaces.IFournisseurRepository>();
        Console.WriteLine("✅ IFournisseurRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IFournisseurRepository: {ex.Message}");
    }
    
    // ✅ NOUVEAU : Vérification Vente
    try
    {
        var venteRepo = services.GetRequiredService<Application.Features.Ventes.Interfaces.IVenteRepository>();
        Console.WriteLine("✅ IVenteRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IVenteRepository: {ex.Message}");
    }
    
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
// 2. PIPELINE HTTP
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