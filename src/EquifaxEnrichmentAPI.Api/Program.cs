using Microsoft.EntityFrameworkCore;
using EquifaxEnrichmentAPI.Infrastructure.Persistence;
using EquifaxEnrichmentAPI.Domain.Repositories;
using EquifaxEnrichmentAPI.Infrastructure.Repositories;
using EquifaxEnrichmentAPI.Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// DATABASE CONFIGURATION
// PostgreSQL 18 with Npgsql provider
// ====================================================================
builder.Services.AddDbContext<EnrichmentDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("EquifaxEnrichmentAPI.Infrastructure")
    ));

// ====================================================================
// DEPENDENCY INJECTION
// Repository Pattern following Clean Architecture
// ====================================================================
builder.Services.AddScoped<IEnrichmentRepository, EnrichmentRepository>();

// ====================================================================
// MEDIATR (CQRS)
// Registers all handlers from Application assembly
// ====================================================================
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EquifaxEnrichmentAPI.Application.Queries.Lookup.LookupQuery).Assembly));

// ====================================================================
// FLUENT VALIDATION
// Automatic validation on controller actions
// Registers validators from API assembly (DTOs)
// ====================================================================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// ====================================================================
// API CONTROLLERS
// ====================================================================
builder.Services.AddControllers();

// ====================================================================
// SWAGGER/OPENAPI DOCUMENTATION
// Configured for development and production
// ====================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Equifax Enrichment API",
        Version = "v1",
        Description = "Phone number enrichment API for consumer data lookup",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });
});

var app = builder.Build();

// ====================================================================
// HTTP REQUEST PIPELINE
// ====================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Equifax Enrichment API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root URL
    });
}

app.UseHttpsRedirection();

// ====================================================================
// API KEY AUTHENTICATION MIDDLEWARE
// BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
// CRITICAL: Must be registered BEFORE MapControllers() to authenticate all requests
// ====================================================================
app.UseApiKeyAuthentication();

app.MapControllers();

app.Run();

// ✅ REQUIRED for integration tests (.NET 6+)
// Without this, WebApplicationFactory<Program> fails with:
// "Program is inaccessible due to its protection level"
public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
