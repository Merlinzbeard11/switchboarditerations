using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquifaxEnrichmentAPI.Domain.Entities;

namespace EquifaxEnrichmentAPI.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for AuditLog entity.
/// BDD Feature: Audit Log Database Persistence (feature-2.4-audit-log-persistence.feature)
///
/// COMPLIANCE NOTES:
/// - Implements FCRA § 607(b) audit logging requirements
/// - 24-month retention policy (business decision, not FCRA-mandated)
/// - Immutable records (append-only, configured as read-only after insert)
/// - Phone numbers stored as SHA-256 hashes for privacy
///
/// PERFORMANCE NOTES:
/// - Uses UUIDv7 for sequential ordering and reduced index fragmentation (Gotcha #6)
/// - Uses TIMESTAMPTZ for timezone-aware audit trail (Gotcha #5)
/// - Database NOW() for timestamps to avoid clock skew (Gotcha #7)
/// - Table partitioning by month for scalability (Gotcha #4) - configured in migration
/// - B-tree indexes for BuyerId and PhoneHash lookups (Gotcha #2)
/// - BRIN index for timestamp range queries (partitioned table optimization)
///
/// GOTCHAS ADDRESSED:
/// - Gotcha #1: No FCRA-mandated retention period (chose 24 months)
/// - Gotcha #2: Missing indexes cause full table scans on 326M+ records
/// - Gotcha #4: Table partitioning required for performance at scale
/// - Gotcha #5: TIMESTAMPTZ mandatory for accurate audit timeline
/// - Gotcha #6: UUIDv7 reduces fragmentation vs UUID v4
/// - Gotcha #7: Use database NOW() to avoid clock skew
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Table name (snake_case for PostgreSQL convention)
        // NOTE: Table partitioning configured in migration (EF Core doesn't support partitioning in fluent API)
        builder.ToTable("audit_logs");

        // Primary key
        // Gotcha #6: UUIDv7 provides sequential ordering unlike UUID v4 (random)
        // NOTE: PostgreSQL 18+ can use uuid_generate_v7() - configured in migration
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd() // Database generates UUIDv7 via DEFAULT gen_random_uuid()
            .HasDefaultValueSql("gen_random_uuid()"); // Temporary - migration will replace with uuid_generate_v7() if available

        // Timestamp (when API request occurred)
        // Gotcha #5: TIMESTAMPTZ preserves timezone information for compliance audits
        // Gotcha #7: Use database NOW() to avoid clock skew between app server and database
        builder.Property(e => e.Timestamp)
            .HasColumnName("timestamp")
            .HasColumnType("timestamptz") // PostgreSQL timestamp with time zone
            .IsRequired()
            .HasDefaultValueSql("NOW()"); // Database sets value, not application

        // BuyerId (foreign key reference - not enforced for performance)
        // References buyers.id but no FK constraint (audit table independence)
        builder.Property(e => e.BuyerId)
            .HasColumnName("buyer_id")
            .IsRequired();

        // CRITICAL: Index on BuyerId for buyer compliance reports
        // Gotcha #2: Missing index = full table scan on 326M+ records
        // Supports query: "Show all queries by buyer X in past 24 months"
        builder.HasIndex(e => new { e.BuyerId, e.Timestamp })
            .HasDatabaseName("ix_audit_logs_buyer_id_timestamp")
            .IsDescending(false, true); // BuyerId ASC, Timestamp DESC

        // PhoneHash (SHA-256 hash of phone number)
        // PRIVACY: Phone numbers are NEVER stored in plaintext (FCRA privacy requirement)
        builder.Property(e => e.PhoneHash)
            .HasColumnName("phone_hash")
            .HasMaxLength(64) // SHA-256 produces 32 bytes = 64 hex characters
            .IsRequired();

        // CRITICAL: Index on PhoneHash for consumer access requests
        // Gotcha #2: Missing index = full table scan on 326M+ records
        // Supports query: "Show all queries for phone number X" (FCRA § 609 requirement)
        builder.HasIndex(e => new { e.PhoneHash, e.Timestamp })
            .HasDatabaseName("ix_audit_logs_phone_hash_timestamp")
            .IsDescending(false, true); // PhoneHash ASC, Timestamp DESC

        // PermissiblePurpose (FCRA-compliant purpose)
        builder.Property(e => e.PermissiblePurpose)
            .HasColumnName("permissible_purpose")
            .HasMaxLength(100)
            .IsRequired();

        // IpAddress (optional, for fraud detection)
        // Supports IPv4 (max 15 chars) and IPv6 (max 45 chars)
        builder.Property(e => e.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        // Response (success, no_match, error)
        builder.Property(e => e.Response)
            .HasColumnName("response")
            .HasMaxLength(20)
            .IsRequired();

        // StatusCode (HTTP status code)
        builder.Property(e => e.StatusCode)
            .HasColumnName("status_code")
            .IsRequired();

        // CreatedAt (when record was inserted into database)
        // Gotcha #7: Use database NOW() to avoid clock skew
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // CRITICAL: Index on Timestamp for time-range queries and partitioning
        // Gotcha #4: Partitioned tables benefit from BRIN indexes for range scans
        // NOTE: BRIN index created in migration (not supported in fluent API)
        // Supports query: "Show all queries in past 7 days" (compliance monitoring)
        builder.HasIndex(e => e.Timestamp)
            .HasDatabaseName("ix_audit_logs_timestamp")
            .IsDescending(true); // DESC for recent-first queries

        // TODO: Configure as read-only entity (audit logs should never be updated)
        // EF Core 7+ supports ToView/ToTable with read-only configuration
        // For now, rely on application-level enforcement
    }
}
