using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquifaxEnrichmentAPI.Domain.Entities;

namespace EquifaxEnrichmentAPI.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Buyer entity.
/// BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
///
/// SECURITY NOTES:
/// - api_key_hash column stores SHA-256 hashes (NEVER plaintext keys)
/// - Unique index on api_key_hash for O(log n) lookups (BDD Scenario 17)
/// - B-tree index ensures < 5ms authentication for 10K buyers
/// </summary>
public class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        // Table name (snake_case for PostgreSQL convention)
        builder.ToTable("buyers");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Using Guid.NewGuid() in entity

        // API key hash (SHA-256, Base64-encoded)
        // BDD Scenario 8: Store API key as SHA-256 hash in database
        builder.Property(e => e.ApiKeyHash)
            .HasColumnName("api_key_hash")
            .HasMaxLength(64) // SHA-256 Base64 is 44 chars, allow buffer
            .IsRequired();

        // CRITICAL: Unique index on API key hash for fast authentication lookups
        // BDD Scenario 17: Use B-tree index for fast API key hash lookups
        // O(log n) complexity - < 5ms for 10K records
        builder.HasIndex(e => e.ApiKeyHash)
            .IsUnique()
            .HasDatabaseName("ix_buyers_api_key_hash_unique");

        // Buyer name
        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        // IsActive flag (soft delete)
        // BDD Scenario 5: Inactive buyers cannot authenticate (returns 401)
        // BDD Scenario 14: Zero Trust - verify on EVERY request
        // BDD Scenario 21: Key revocation (soft delete with audit trail)
        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Index on IsActive for filtering active buyers
        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_buyers_is_active");

        // Audit fields
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Index on CreatedAt for reporting and key age monitoring
        // BDD Scenario 6: API key rotation warning (90-day policy)
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_buyers_created_at");
    }
}
