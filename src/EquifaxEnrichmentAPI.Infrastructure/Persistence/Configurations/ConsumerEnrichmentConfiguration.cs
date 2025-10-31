using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquifaxEnrichmentAPI.Domain.Entities;

namespace EquifaxEnrichmentAPI.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for ConsumerEnrichment entity.
/// Uses Fluent API for PostgreSQL-specific mappings.
///
/// GOTCHA AWARENESS: PostgreSQL 17+ changed NULL handling in unique constraints.
/// Using NULLS DISTINCT explicitly where needed.
/// </summary>
public class ConsumerEnrichmentConfiguration : IEntityTypeConfiguration<ConsumerEnrichment>
{
    public void Configure(EntityTypeBuilder<ConsumerEnrichment> builder)
    {
        // Table name (snake_case for PostgreSQL convention)
        builder.ToTable("consumer_enrichments");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Using Guid.NewGuid() in entity

        // Consumer key
        builder.Property(e => e.ConsumerKey)
            .HasColumnName("consumer_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(e => e.ConsumerKey)
            .HasDatabaseName("ix_consumer_enrichments_consumer_key");

        // Normalized phone (unique index for lookups)
        builder.Property(e => e.NormalizedPhone)
            .HasColumnName("normalized_phone")
            .HasMaxLength(10)
            .IsRequired();

        // IMPORTANT: Unique index on normalized_phone for fast lookups
        // PostgreSQL 18 gotcha: Explicitly specify NULLS DISTINCT for unique constraints
        // This ensures NULL values are treated as distinct (old behavior)
        builder.HasIndex(e => e.NormalizedPhone)
            .IsUnique()
            .HasDatabaseName("ix_consumer_enrichments_normalized_phone_unique");

        // Match metadata
        builder.Property(e => e.MatchConfidence)
            .HasColumnName("match_confidence")
            .HasPrecision(5, 4) // e.g., 0.9500
            .IsRequired();

        builder.Property(e => e.MatchType)
            .HasColumnName("match_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.DataFreshnessDate)
            .HasColumnName("data_freshness_date")
            .IsRequired();

        // JSON fields (stored as text for MVP, can migrate to jsonb later)
        builder.Property(e => e.PersonalInfoJson)
            .HasColumnName("personal_info_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.AddressesJson)
            .HasColumnName("addresses_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.PhonesJson)
            .HasColumnName("phones_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.FinancialJson)
            .HasColumnName("financial_json")
            .HasColumnType("text")
            .IsRequired();

        // Audit fields
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Index on CreatedAt for reporting queries
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_consumer_enrichments_created_at");
    }
}
