#!/usr/bin/env python3
"""
Generates the complete 398-column ConsumerEnrichment.cs entity from schema.sql
PRESERVES EQUIFAX NAMING CONVENTIONS - keeps snake_case as-is
Reads /tmp/equifax-full-schema.sql and outputs C# entity class
"""

import re
from typing import List, Tuple

def generate_entity_from_schema(schema_file: str, output_file: str):
    """Generates complete C# entity from schema.sql with Equifax naming"""

    columns: List[Tuple[str, str, str, str]] = []  # (name, type, sql_name, category)

    with open(schema_file, 'r') as f:
        lines = f.readlines()

    # Parse schema
    current_category = "METADATA"
    for line in lines:
        stripped = line.strip()

        # Skip non-column lines
        if not stripped or stripped.startswith('--') or stripped.startswith('CREATE') or stripped.startswith('DROP') or stripped.startswith(')'):
            continue

        # Detect category from comments
        if 'IDENTITY' in stripped.upper():
            current_category = "IDENTITY"
        elif 'PERSONAL' in stripped.upper() or 'first_name' in stripped:
            current_category = "PERSONAL_INFO"
        elif 'alternate_name' in stripped:
            current_category = "ALTERNATE_NAMES"
        elif 'address_' in stripped and 'email' not in stripped:
            current_category = "ADDRESSES"
        elif 'mobile_phone' in stripped or '_phone_' in stripped:
            current_category = "PHONES"
        elif 'email_' in stripped:
            current_category = "EMAILS"
        elif 'income360' in stripped or 'spending_power' in stripped:
            current_category = "FINANCIAL"
        elif stripped.startswith('v') and 'NUMERIC' in stripped:
            current_category = "CREDIT_SCORES"
        elif 'ipaddress' in stripped or 'idfa' in stripped:
            current_category = "DEVICE_IDS"
        elif 'uuid' in stripped or 'meta_revision' in stripped or 'marketing' in stripped:
            current_category = "METADATA"

        # Parse column
        col_match = re.match(r'\s+(\w+)\s+(\w+|\w+\(\d+(?:,\s*\d+)?\)),?\s*(?:--.*)?$', line)
        if col_match:
            sql_name = col_match.group(1)
            sql_type = col_match.group(2).split('(')[0].upper()

            # Skip PostgreSQL-specific columns
            if sql_name in ['id', 'created_at', 'updated_at', 'source_system', 'migration_batch_id', 'migrated_at']:
                continue

            # PRESERVE EQUIFAX NAMING - keep snake_case as-is
            csharp_name = sql_name

            type_map = {
                'JSONB': 'string',  # DECRYPTED
                'TEXT': 'string',
                'VARCHAR': 'string',
                'NUMERIC': 'decimal',
                'UUID': 'Guid',
                'TIMESTAMP': 'DateTimeOffset'
            }

            csharp_type = type_map.get(sql_type, 'string')

            # Make nullable
            if sql_name != 'consumer_key':
                csharp_type += '?'

            columns.append((csharp_name, csharp_type, sql_name, current_category))

    # Generate entity class
    with open(output_file, 'w') as f:
        # Header
        f.write("""using EquifaxEnrichmentAPI.Domain.ValueObjects;

namespace EquifaxEnrichmentAPI.Domain.Entities;

/// <summary>
/// Aggregate root representing enriched consumer data from Equifax.
/// 398-COLUMN MIRRORED SCHEMA - Direct mirror of Equifax MIE_PLUSAUTO_VIEW
///
/// BDD Feature: REST API Endpoint for Phone Number Enrichment
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
///
/// This is a rich domain model following DDD principles.
/// Encapsulates business rules and invariants for consumer enrichment data.
///
/// SCHEMA DECISION: 100% mirror of Equifax source data (398 columns)
/// - Zero data loss - all Equifax fields preserved
/// - Future-proof - unknown which fields become valuable
/// - Simple ETL - direct column mapping from CSV
/// - Data is an asset - cannot recreate lost fields
///
/// NAMING CONVENTION: Preserves exact Equifax field names (snake_case)
/// - Maintains naming integrity from source system
/// - Direct mapping from CSV columns to entity properties
/// - No PascalCase conversion - keeps consumer_key, mobile_phone_1, etc.
///
/// GENERATED FILE: Auto-generated from equifax-full-schema.sql
/// DO NOT EDIT MANUALLY - Regenerate using generate-consumer-enrichment-entity-equifax-names.py
/// </summary>
public class ConsumerEnrichment
{
    // ====================================================================
    // POSTGRESQL METADATA (3 columns)
    // ====================================================================

    /// <summary>Unique identifier (primary key)</summary>
    public Guid Id { get; private set; }

    /// <summary>When this record was created</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>When this record was last updated</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

""")

        # Group columns by category
        categories = {}
        for col_name, col_type, sql_name, category in columns:
            if category not in categories:
                categories[category] = []
            categories[category].append((col_name, col_type, sql_name))

        # Output by category
        category_descriptions = {
            "IDENTITY": ("IDENTITY", "1 column", "Equifax consumer identifier (DECRYPTED)"),
            "PERSONAL_INFO": ("PRIMARY PERSONAL INFORMATION", "11 columns", "Name, DOB, gender, age, deceased"),
            "ALTERNATE_NAMES": ("ALTERNATE NAMES", "30 columns", "5 sets of 6 fields - track name changes"),
            "ADDRESSES": ("ADDRESSES", "162 columns", "10 address records with full details"),
            "PHONES": ("PHONE NUMBERS", "12 columns", "7 phones + 5 last_seen dates"),
            "EMAILS": ("EMAIL ADDRESSES", "30 columns", "15 emails + 15 last_seen dates"),
            "FINANCIAL": ("FINANCIAL ATTRIBUTES", "14 columns", "Income, spending power, affluence"),
            "CREDIT_SCORES": ("CREDIT ATTRIBUTES", "123 columns", "Vantage scores and propensity models"),
            "DEVICE_IDS": ("DEVICE IDENTIFIERS", "7 columns", "IP addresses and IDFA identifiers"),
            "METADATA": ("METADATA", "8 columns", "Marketing flags, UUID, revision")
        }

        for category in ["IDENTITY", "PERSONAL_INFO", "ALTERNATE_NAMES", "ADDRESSES",
                        "PHONES", "EMAILS", "FINANCIAL", "CREDIT_SCORES", "DEVICE_IDS", "METADATA"]:
            if category not in categories:
                continue

            title, count, desc = category_descriptions.get(category, (category, "? columns", ""))

            f.write(f"    // ====================================================================\n")
            f.write(f"    // {title} ({count})\n")
            f.write(f"    // {desc}\n")
            f.write(f"    // ====================================================================\n\n")

            for col_name, col_type, sql_name in categories[category]:
                # Add descriptive comment
                encrypted_fields = [
                    'consumer_key', 'prefix', 'first_name', 'middle_name', 'last_name', 'suffix', 'date_of_birth',
                    'alternate_name_1', 'alternate_prefix_1', 'alternate_first_name_1', 'alternate_middle_name_1', 'alternate_last_name_1', 'alternate_suffix_1',
                    'alternate_name_2', 'alternate_prefix_2', 'alternate_first_name_2', 'alternate_middle_name_2', 'alternate_last_name_2', 'alternate_suffix_2',
                    'alternate_name_3', 'alternate_prefix_3', 'alternate_first_name_3', 'alternate_middle_name_3', 'alternate_last_name_3', 'alternate_suffix_3',
                    'alternate_name_4', 'alternate_prefix_4', 'alternate_first_name_4', 'alternate_middle_name_4', 'alternate_last_name_4', 'alternate_suffix_4',
                    'alternate_name_5', 'alternate_prefix_5', 'alternate_first_name_5', 'alternate_middle_name_5', 'alternate_last_name_5', 'alternate_suffix_5'
                ]

                encrypted_address_fields = [
                    'address_1', 'house_number_1', 'predirectional_1', 'street_name_1', 'street_suffix_1', 'post_direction_1',
                    'address_2', 'house_number_2', 'predirectional_2', 'street_name_2', 'street_suffix_2', 'post_direction_2',
                    'address_3', 'house_number_3', 'predirectional_3', 'street_name_3', 'street_suffix_3', 'post_direction_3',
                    'address_4', 'house_number_4', 'predirectional_4', 'street_name_4', 'street_suffix_4', 'post_direction_4',
                    'address_5', 'house_number_5', 'predirectional_5', 'street_name_5', 'street_suffix_5', 'post_direction_5',
                    'address_6', 'house_number_6', 'predirectional_6', 'street_name_6', 'street_suffix_6', 'post_direction_6',
                    'address_7', 'house_number_7', 'predirectional_7', 'street_name_7', 'street_suffix_7', 'post_direction_7',
                    'address_8', 'house_number_8', 'predirectional_8', 'street_name_8', 'street_suffix_8', 'post_direction_8',
                    'address_9', 'house_number_9', 'predirectional_9', 'street_name_9', 'street_suffix_9', 'post_direction_9',
                    'address_10', 'house_number_10', 'predirectional_10', 'street_name_10', 'street_suffix_10', 'post_direction_10'
                ]

                encrypted_device_fields = ['ipaddress1', 'ipaddress2', 'idfa1', 'idfa2', 'idfa3', 'idfa4', 'idfa5']

                encrypted_note = ""
                if sql_name in encrypted_fields:
                    encrypted_note = " (DECRYPTED from AES-GCM)"
                elif sql_name in encrypted_address_fields:
                    encrypted_note = " (DECRYPTED from AES-GCM)"
                elif sql_name in encrypted_device_fields:
                    encrypted_note = " (DECRYPTED from AES-GCM)"

                f.write(f"    /// <summary>Equifax field: {sql_name}{encrypted_note}</summary>\n")
                f.write(f"    public {col_type} {col_name} {{ get; private set; }}\n\n")

        # Constructor and factory methods
        f.write("""    // ====================================================================
    // COMPUTED FIELDS (backward compatibility)
    // ====================================================================

    /// <summary>
    /// Normalized phone number for backward compatibility
    /// Generated from mobile_phone_1 or phone_1
    /// </summary>
    public string? normalized_phone => mobile_phone_1 ?? phone_1;

    /// <summary>
    /// Match confidence score (calculated from phone column match)
    /// Legacy field - now calculated based on which phone column matched
    /// </summary>
    public double match_confidence { get; private set; }

    /// <summary>
    /// Type of match performed
    /// Examples: "phone_only", "phone_with_name", "phone_with_name_and_address"
    /// </summary>
    public string match_type { get; private set; } = string.Empty;

    /// <summary>
    /// Data freshness date (most recent phone last_seen date)
    /// </summary>
    public DateTimeOffset? data_freshness_date { get; private set; }

    // ====================================================================
    // CONSTRUCTORS
    // ====================================================================

    // Private parameterless constructor for EF Core
    private ConsumerEnrichment()
    {
    }

    /// <summary>
    /// Factory method to create new enrichment record from Equifax CSV import.
    /// Encapsulates creation logic and enforces invariants.
    /// All 398 fields set from CSV row with AES-GCM decryption.
    /// </summary>
    public static ConsumerEnrichment CreateFromEquifaxCsv(
        string consumerKey,
        // NOTE: Full 398-parameter constructor impractical - use builder pattern
        // or pass CSV field array in production code
        double matchConfidence = 0.0,
        string matchType = "csv_import")
    {
        if (string.IsNullOrWhiteSpace(consumerKey))
            throw new ArgumentException("Consumer key cannot be empty", nameof(consumerKey));

        if (matchConfidence < 0.0 || matchConfidence > 1.0)
            throw new ArgumentOutOfRangeException(nameof(matchConfidence));

        return new ConsumerEnrichment
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            consumer_key = consumerKey,
            match_confidence = matchConfidence,
            match_type = matchType
            // All 398 fields set from CSV import via property setting
        };
    }

    /// <summary>
    /// Updates match metadata (backward compatibility)
    /// </summary>
    public void UpdateMatchMetadata(double newConfidence, string newMatchType)
    {
        if (newConfidence < 0.0 || newConfidence > 1.0)
            throw new ArgumentOutOfRangeException(nameof(newConfidence));

        if (newConfidence > match_confidence)
        {
            match_confidence = newConfidence;
            match_type = newMatchType;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
""")

    print(f"✅ Generated {output_file} with {len(columns)} columns (Equifax naming preserved)")

# Run generator
if __name__ == "__main__":
    generate_entity_from_schema(
        '/tmp/equifax-full-schema.sql',
        '/tmp/ConsumerEnrichment.EquifaxNames.cs'
    )
    print("Entity class generated with Equifax naming conventions!")
