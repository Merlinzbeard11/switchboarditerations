using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EquifaxEnrichmentAPI.Infrastructure.Persistence;

/// <summary>
/// Simple console tool to seed the database with test data.
/// Run this after applying migrations to populate test consumer enrichment records.
///
/// Usage:
///   dotnet run --project tools/SeedDatabase.csproj
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("Equifax Enrichment API - Database Seeder");
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine();

        // Load configuration from API project
        var apiProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "src", "EquifaxEnrichmentAPI.Api"));
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ ERROR: Connection string 'DefaultConnection' not found in appsettings.json");
            Console.ResetColor();
            return;
        }

        Console.WriteLine($"📊 Database: {ExtractDatabaseName(connectionString)}");
        Console.WriteLine($"🔌 Host: {ExtractHost(connectionString)}");
        Console.WriteLine();

        // Create DbContext with connection string
        var optionsBuilder = new DbContextOptionsBuilder<EnrichmentDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        await using var context = new EnrichmentDbContext(optionsBuilder.Options);

        try
        {
            // Verify database connection
            Console.Write("🔍 Checking database connection... ");
            await context.Database.CanConnectAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Connected");
            Console.ResetColor();
            Console.WriteLine();

            // Run seeder
            await DatabaseSeeder.SeedAsync(context);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Database seeding completed successfully!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Stack Trace:");
            Console.WriteLine(ex.StackTrace);

            // Show inner exception details (critical for reflection errors)
            if (ex.InnerException != null)
            {
                Console.WriteLine();
                Console.WriteLine("Inner Exception:");
                Console.WriteLine($"   Type: {ex.InnerException.GetType().Name}");
                Console.WriteLine($"   Message: {ex.InnerException.Message}");
                Console.WriteLine($"   Stack Trace: {ex.InnerException.StackTrace}");
            }

            Console.ResetColor();
            Environment.Exit(1);
        }
    }

    private static string ExtractDatabaseName(string connectionString)
    {
        var match = System.Text.RegularExpressions.Regex.Match(connectionString, @"Database=([^;]+)");
        return match.Success ? match.Groups[1].Value : "unknown";
    }

    private static string ExtractHost(string connectionString)
    {
        var match = System.Text.RegularExpressions.Regex.Match(connectionString, @"Host=([^;]+)");
        return match.Success ? match.Groups[1].Value : "unknown";
    }
}
