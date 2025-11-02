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
// Production: Build connection string from environment variables
// Development: Use appsettings.json
// ====================================================================
string connectionString;

if (builder.Environment.IsProduction())
{
    // Build connection string from environment variables in production
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new InvalidOperationException("DB_HOST not set");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
    var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new InvalidOperationException("DB_NAME not set");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? throw new InvalidOperationException("DB_USER not set");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new InvalidOperationException("DB_PASSWORD not set");

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};Include Error Detail=true";
}
else
{
    // Use appsettings.json in development
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection not found in appsettings.json");
}

builder.Services.AddDbContext<EnrichmentDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("EquifaxEnrichmentAPI.Infrastructure")
    ));

// ====================================================================
// REDIS CONFIGURATION (Rate Limiting & Caching)
// CRITICAL GOTCHA: ConnectionMultiplexer MUST be singleton (13x faster)
// Production: Uses AWS ElastiCache Redis endpoint
// Development: Uses local Redis instance
// ====================================================================
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var redisConnectionString = builder.Environment.IsProduction()
        ? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "localhost:6379"
        : "localhost:6379";

    var configuration = StackExchange.Redis.ConfigurationOptions.Parse(redisConnectionString);
    configuration.AbortOnConnectFail = false; // Graceful degradation if Redis unavailable
    configuration.ConnectRetry = 3;
    configuration.ConnectTimeout = 5000;

    return StackExchange.Redis.ConnectionMultiplexer.Connect(configuration);
});

// ====================================================================
// FCRA AUDIT LOGGING SERVICE
// Background service using Channel<T> for fire-and-forget async logging
// Registered as both IAuditLoggingService (for DI) and IHostedService (for background execution)
// ====================================================================
builder.Services.AddSingleton<EquifaxEnrichmentAPI.Api.Services.AuditLoggingService>();
builder.Services.AddSingleton<EquifaxEnrichmentAPI.Api.Services.IAuditLoggingService>(sp =>
    sp.GetRequiredService<EquifaxEnrichmentAPI.Api.Services.AuditLoggingService>());
builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<EquifaxEnrichmentAPI.Api.Services.AuditLoggingService>());

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

// Load full enrichment example JSON for Swagger documentation
var fullExamplePath = Path.Combine(AppContext.BaseDirectory, "docs", "full-enrichment-example.json");
if (!File.Exists(fullExamplePath))
{
    // Try relative path for local development
    fullExamplePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../docs/full-enrichment-example.json");
}
var fullExampleJson = File.Exists(fullExamplePath)
    ? File.ReadAllText(fullExamplePath)
    : "Full enrichment example not available in this build.";

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Equifax Enrichment API",
        Version = "v1",
        Description = @"
# Equifax Phone Number Enrichment API

Enrich phone numbers with comprehensive consumer data for marketing, fraud prevention, and identity verification.

## Quick Start Guide

### 1. Authentication
All API requests require an API key in the `X-API-Key` header.

Click the **Authorize** button above and enter your API key to test endpoints in this documentation.

### 2. Make Your First Request

**Endpoint:** `POST /api/data_enhancement/lookup`

**Minimal Request:**
```json
{
  ""phone"": ""555-123-4567""
}
```

**Enhanced Request (Better Match Confidence):**
```json
{
  ""phone"": ""555-123-4567"",
  ""first_name"": ""John"",
  ""last_name"": ""Doe"",
  ""postal_code"": ""12345"",
  ""state"": ""CA""
}
```

### 3. Response Structure

**Success Response:**
```json
{
  ""response"": ""success"",
  ""message"": ""Record found with high confidence"",
  ""data"": {
    ""consumer_key"": ""ABC123XYZ789"",
    ""personal_info"": { ... },
    ""addresses"": [ ... ],
    ""phones"": [ ... ],
    ""financial"": { ... }
  },
  ""metadata"": {
    ""match_confidence"": 0.95,
    ""match_type"": ""phone_with_name"",
    ""response_time_ms"": 145,
    ""request_id"": ""uuid-here""
  }
}
```

**No Match Response:**
```json
{
  ""response"": ""error"",
  ""message"": ""No consumer record found"",
  ""data"": {
    ""phone"": ""555-123-4567"",
    ""match_attempted"": true,
    ""match_confidence"": 0.0
  }
}
```

## Key Features

- **High Match Rates:** 95%+ match confidence with enhanced matching fields
- **Fast Response Times:** < 200ms average response time
- **Comprehensive Data:** Personal info, address history, phone history, financial indicators
- **FCRA Compliant:** Full audit logging for permissible purpose tracking
- **Real-time:** Live data lookups with millisecond response times

## Field Optimization

**Request fewer fields for faster responses:**
- Default (basic): ~50 core fields, < 200ms response time
- Full dataset: 398 fields, < 300ms response time

**Specify fields parameter:**
```json
{
  ""phone"": ""555-123-4567"",
  ""fields"": ""basic""
}
```

## Best Practices

1. **Include matching fields** (first_name, last_name, postal_code) for higher confidence
2. **Use unique_id** to track requests in your system
3. **Monitor match_confidence** - scores < 0.7 may indicate poor matches
4. **Handle 'no match' gracefully** - not all phone numbers will have data
5. **Respect rate limits** - Default: 100 requests/minute per API key

## Support

For API key requests, technical support, or integration assistance:
- Email: support@example.com
- Documentation: See endpoint details below

## Full 398-Field Enrichment Example

Below is a complete example showing all 398 fields available in the enrichment response:

```json
" + fullExampleJson + @"
```
",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });

    // Include XML comments in Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Add API Key authentication to Swagger
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key authentication. Include 'X-API-Key' header with your API key.",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = Microsoft.OpenApi.Models.ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// ====================================================================
// DATABASE INITIALIZATION & SEEDING
// PRODUCTION: Run migrations and seed buyers
// DEVELOPMENT: Run migrations and seed test data
// ====================================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EnrichmentDbContext>();

    if (app.Environment.IsDevelopment())
    {
        // Development: Apply migrations and seed test data
        await context.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(context);
        Console.WriteLine("✅ Database initialized and seeded with test data");
    }
    else
    {
        // Production: Migrations handled by init container, just seed buyers
        await BuyerSeeder.SeedAsync(context);
        Console.WriteLine("✅ Buyers seeded in production");
    }
}

// ====================================================================
// HTTP REQUEST PIPELINE
// ====================================================================
// Enable Swagger in all environments (public documentation)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Equifax Enrichment API v1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger (bypasses auth middleware)
    c.DocumentTitle = "Equifax Enrichment API Documentation";

    // UX Enhancement: Expand DTO fields by default for better documentation discoverability
    c.DefaultModelExpandDepth(2);        // Expand model fields 2 levels deep (shows field descriptions)
    c.DefaultModelsExpandDepth(1);       // Keep Models section visible
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Keep operations collapsed
});

// HTTPS Redirection: Only in development
// AWS App Runner handles HTTPS termination at load balancer
// Container receives HTTP on port 8080, so don't redirect
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ====================================================================
// HEALTH CHECK ENDPOINT (BEFORE AUTHENTICATION)
// AWS App Runner requirement: /health endpoint for container health monitoring
// CRITICAL: Mapped BEFORE authentication middleware to allow unauthenticated health checks
// Returns 200 OK if application is running
// ====================================================================
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "Equifax Enrichment API"
}));

// ====================================================================
// STATIC FILE: Full Enrichment Example (BEFORE AUTHENTICATION)
// Publicly accessible JSON example showing all 398 enrichment fields
// Buyers can reference this to understand complete data structure
// ====================================================================
app.MapGet("/docs/full-enrichment-example.json", () =>
{
    var filePath = Path.Combine(AppContext.BaseDirectory, "docs", "full-enrichment-example.json");
    if (File.Exists(filePath))
    {
        var jsonContent = File.ReadAllText(filePath);
        return Results.Content(jsonContent, "application/json");
    }
    return Results.NotFound();
});

// ====================================================================
// RATE LIMITING MIDDLEWARE
// BDD Feature: Rate Limiting (feature-2.2-rate-limiting.feature)
// CRITICAL: Must be registered BEFORE authentication to protect against abuse
// Uses Redis with Lua scripts for atomic operations (prevents race conditions)
// Implements sliding window algorithm (prevents 2x burst at boundaries)
// ====================================================================
app.UseMiddleware<RateLimitingMiddleware>();

// ====================================================================
// API KEY AUTHENTICATION MIDDLEWARE
// BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
// CRITICAL: Must be registered BEFORE MapControllers() to authenticate all requests
// NOTE: /health endpoint mapped above bypasses this middleware
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
