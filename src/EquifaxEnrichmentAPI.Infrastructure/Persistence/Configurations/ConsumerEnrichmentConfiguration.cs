using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquifaxEnrichmentAPI.Domain.Entities;

namespace EquifaxEnrichmentAPI.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for ConsumerEnrichment entity.
/// 398-COLUMN SCHEMA - Minimal configuration, relies on EF conventions for individual fields.
///
/// CONFIGURATION APPROACH:
/// - EF Core automatically maps 398 individual properties to snake_case columns
/// - Only configure indexes and constraints not inferable by convention
/// - Primary key and table name explicitly configured for clarity
/// </summary>
public class ConsumerEnrichmentConfiguration : IEntityTypeConfiguration<ConsumerEnrichment>
{
    public void Configure(EntityTypeBuilder<ConsumerEnrichment> builder)
    {
        // ====================================================================
        // TABLE AND PRIMARY KEY
        // ====================================================================

        builder.ToTable("consumer_enrichments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Using Guid.NewGuid() in entity

        // ====================================================================
        // CONSUMER KEY (Required, Indexed)
        // ====================================================================

        builder.Property(e => e.consumer_key)
            .HasColumnName("consumer_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(e => e.consumer_key)
            .HasDatabaseName("ix_consumer_enrichments_consumer_key");

        // ====================================================================
        // PHONE INDEXES FOR FAST LOOKUPS
        // Feature 1.3: Multi-column phone search across phone_1-4 and mobile_phone_1-3
        // ====================================================================

        // Index on mobile_phone_1 (highest priority phone)
        builder.HasIndex(e => e.mobile_phone_1)
            .HasDatabaseName("ix_consumer_enrichments_mobile_phone_1");

        // Index on phone_1 (primary landline)
        builder.HasIndex(e => e.phone_1)
            .HasDatabaseName("ix_consumer_enrichments_phone_1");

        // NOTE: normalized_phone is a computed C# property (mobile_phone_1 ?? phone_1)
        // No EF configuration - InMemory database cannot handle computed properties
        // All phone searches use explicit columns (mobile_phone_1-2, phone_1-5)

        // ====================================================================
        // MATCH METADATA (backward compatibility fields)
        // ====================================================================

        builder.Property(e => e.match_confidence)
            .HasColumnName("match_confidence")
            .HasPrecision(5, 4) // e.g., 0.9500
            .IsRequired();

        builder.Property(e => e.match_type)
            .HasColumnName("match_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.data_freshness_date)
            .HasColumnName("data_freshness_date");

        // ====================================================================
        // AUDIT TIMESTAMPS
        // ====================================================================

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_consumer_enrichments_created_at");

        // ====================================================================
        // NOTE: All other 398 fields use EF Core convention-based mapping
        // - Property name → snake_case column name
        // - string? → VARCHAR(nullable)
        // - No explicit configuration needed for data fields
        // ====================================================================
    }
}
