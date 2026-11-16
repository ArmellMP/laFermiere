using PresentationApi.Extensions;
using PresentationApi.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Application.Common.Security;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. CONFIGURATION DES SERVICES
// ============================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    // Erreurs DataAnnotations ([Required], [Range]...) : [ApiController] les détecte AVANT
    // d'appeler le contrôleur et renvoie par défaut un "ProblemDetails" d'ASP.NET.
    // On remplace cette réponse par notre format unique ErrorResponse, identique à celui
    // des erreurs FluentValidation (ExceptionMiddleware) : un client n'a qu'un format à lire.
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // ModelState = dictionnaire "champ -> erreurs" rempli par la validation ASP.NET
            var errors = context.ModelState
                .Where(entry => entry.Value is { Errors.Count: > 0 })
                .GroupBy(entry => PresentationApi.Models.Responses.ErrorResponse.NomDeChamp(entry.Key))
                .ToDictionary(
                    groupe => groupe.Key,
                    groupe => groupe
                        .SelectMany(entry => entry.Value!.Errors)
                        // Message vide = erreur de lecture du JSON (type incorrect...) : on l'explique
                        .Select(error => string.IsNullOrEmpty(error.ErrorMessage)
                            ? "Valeur invalide."
                            : error.ErrorMessage)
                        .Distinct()
                        .ToArray());

            var response = new PresentationApi.Models.Responses.ErrorResponse
            {
                Message = PresentationApi.Models.Responses.ErrorResponse.MessageValidation,
                Errors = errors,
                TraceId = context.HttpContext.TraceIdentifier
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

// Documentation OpenAPI : générateur intégré à .NET 10 (remplace AddSwaggerGen,
// qui n'était plus compatible). Le document est servi sur /openapi/v1.json.
builder.Services.AddOpenApi(options =>
{
    // Déclare l'authentification "Bearer JWT" dans le document OpenAPI :
    // Swagger affiche alors un bouton "Authorize" pour saisir le jeton.
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        document.Components.SecuritySchemes ??=
            new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Colle ici le jeton reçu de /api/v1/Utilisateurs/authenticate"
        };
        document.Security ??= [];
        document.Security.Add(new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
        return Task.CompletedTask;
    });
});

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

// ============================================
// CONFIGURATION JWT
// ============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? "LaFermiereAPI";
var audience = jwtSettings["Audience"] ?? "LaFermiereClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();

// ============================================
// DIAGNOSTIC
// ============================================
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
        var achatRepo = services.GetRequiredService<Application.Features.Achats.Interfaces.IAchatRepository>();
        Console.WriteLine("✅ IAchatRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IAchatRepository: {ex.Message}");
    }
    
    try
    {
        var utilisateurRepo = services.GetRequiredService<Application.Features.Utilisateurs.Interfaces.IUtilisateurRepository>();
        Console.WriteLine("✅ IUtilisateurRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IUtilisateurRepository: {ex.Message}");
    }

    try
    {
        var parametreRepo = services.GetRequiredService<Application.Features.Parametres.Interfaces.IParametreRepository>();
        Console.WriteLine("✅ IParametreRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IParametreRepository: {ex.Message}");
    }

    try
    {
        var alerteRepo = services.GetRequiredService<Application.Features.Alertes.Interfaces.IAlerteRepository>();
        Console.WriteLine("✅ IAlerteRepository est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IAlerteRepository: {ex.Message}");
    }
    
    try
    {
        var jwtService = services.GetRequiredService<IJwtService>();
        Console.WriteLine("✅ IJwtService est bien enregistré !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERREUR IJwtService: {ex.Message}");
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

// Swagger : uniquement en développement (pas exposé en production)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();   // publie /openapi/v1.json
    app.UseSwaggerUI(options =>
    {
        // L'interface /swagger lit le document généré par MapOpenApi
        options.SwaggerEndpoint("/openapi/v1.json", "La Fermière API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Tous les contrôleurs exigent un utilisateur authentifié (jeton JWT valide).
// Exception : les actions marquées [AllowAnonymous], comme /authenticate.
app.MapControllers().RequireAuthorization();
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Json(new
{
    message = "Bienvenue sur l'API La Fermière",
    version = "1.0.0",
    health = "/health"
}));

app.Run();

// Avec les "top-level statements" (code écrit directement dans Program.cs, sans Main),
// le compilateur génère une classe Program INTERNE au projet. Cette déclaration la rend
// publique, pour que les tests d'intégration puissent démarrer l'API en mémoire avec
// WebApplicationFactory<Program> (tests/LaFermiere.IntegrationTests).
public partial class Program { }