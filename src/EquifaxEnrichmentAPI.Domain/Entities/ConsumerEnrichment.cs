using EquifaxEnrichmentAPI.Domain.ValueObjects;

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

    // ====================================================================
    // PRIMARY PERSONAL INFORMATION (11 columns)
    // Name, DOB, gender, age, deceased
    // ====================================================================

    /// <summary>Equifax field: first_name (DECRYPTED from AES-GCM)</summary>
    public string? first_name { get; private set; }

    /// <summary>Equifax field: middle_name (DECRYPTED from AES-GCM)</summary>
    public string? middle_name { get; private set; }

    /// <summary>Equifax field: last_name (DECRYPTED from AES-GCM)</summary>
    public string? last_name { get; private set; }

    /// <summary>Equifax field: suffix (DECRYPTED from AES-GCM)</summary>
    public string? suffix { get; private set; }

    /// <summary>Equifax field: gender</summary>
    public string? gender { get; private set; }

    /// <summary>Equifax field: date_of_birth (DECRYPTED from AES-GCM)</summary>
    public string? date_of_birth { get; private set; }

    /// <summary>Equifax field: age</summary>
    public string? age { get; private set; }

    /// <summary>Equifax field: deceased</summary>
    public string? deceased { get; private set; }

    /// <summary>Equifax field: first_seen_date_primary_name</summary>
    public string? first_seen_date_primary_name { get; private set; }

    /// <summary>Equifax field: last_seen_date_primary_name</summary>
    public string? last_seen_date_primary_name { get; private set; }

    /// <summary>Equifax field: alternate_first_name_1 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_first_name_1 { get; private set; }

    /// <summary>Equifax field: alternate_middle_name_1 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_middle_name_1 { get; private set; }

    /// <summary>Equifax field: alternate_last_name_1 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_last_name_1 { get; private set; }

    /// <summary>Equifax field: alternate_suffix_1 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_suffix_1 { get; private set; }

    /// <summary>Equifax field: alternate_first_name_2 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_first_name_2 { get; private set; }

    /// <summary>Equifax field: alternate_middle_name_2 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_middle_name_2 { get; private set; }

    /// <summary>Equifax field: alternate_last_name_2 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_last_name_2 { get; private set; }

    /// <summary>Equifax field: alternate_suffix_2 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_suffix_2 { get; private set; }

    /// <summary>Equifax field: alternate_first_name_3 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_first_name_3 { get; private set; }

    /// <summary>Equifax field: alternate_middle_name_3 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_middle_name_3 { get; private set; }

    /// <summary>Equifax field: alternate_last_name_3 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_last_name_3 { get; private set; }

    /// <summary>Equifax field: alternate_suffix_3 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_suffix_3 { get; private set; }

    /// <summary>Equifax field: alternate_first_name_4 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_first_name_4 { get; private set; }

    /// <summary>Equifax field: alternate_middle_name_4 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_middle_name_4 { get; private set; }

    /// <summary>Equifax field: alternate_last_name_4 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_last_name_4 { get; private set; }

    /// <summary>Equifax field: alternate_suffix_4 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_suffix_4 { get; private set; }

    /// <summary>Equifax field: alternate_first_name_5 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_first_name_5 { get; private set; }

    /// <summary>Equifax field: alternate_middle_name_5 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_middle_name_5 { get; private set; }

    /// <summary>Equifax field: alternate_last_name_5 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_last_name_5 { get; private set; }

    /// <summary>Equifax field: alternate_suffix_5 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_suffix_5 { get; private set; }

    // ====================================================================
    // ALTERNATE NAMES (30 columns)
    // 5 sets of 6 fields - track name changes
    // ====================================================================

    /// <summary>Equifax field: alternate_name_1 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_name_1 { get; private set; }

    /// <summary>Equifax field: alternate_prefix_1 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_prefix_1 { get; private set; }

    /// <summary>Equifax field: alternate_name_2 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_name_2 { get; private set; }

    /// <summary>Equifax field: alternate_prefix_2 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_prefix_2 { get; private set; }

    /// <summary>Equifax field: alternate_name_3 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_name_3 { get; private set; }

    /// <summary>Equifax field: alternate_prefix_3 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_prefix_3 { get; private set; }

    /// <summary>Equifax field: alternate_name_4 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_name_4 { get; private set; }

    /// <summary>Equifax field: alternate_prefix_4 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_prefix_4 { get; private set; }

    /// <summary>Equifax field: alternate_name_5 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_name_5 { get; private set; }

    /// <summary>Equifax field: alternate_prefix_5 (DECRYPTED from AES-GCM)</summary>
    public string? alternate_prefix_5 { get; private set; }

    // ====================================================================
    // ADDRESSES (162 columns)
    // 10 address records with full details
    // ====================================================================

    /// <summary>Equifax field: address_1 (DECRYPTED from AES-GCM)</summary>
    public string? address_1 { get; private set; }

    /// <summary>Equifax field: house_number_1 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_1 { get; private set; }

    /// <summary>Equifax field: predirectional_1 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_1 { get; private set; }

    /// <summary>Equifax field: street_name_1 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_1 { get; private set; }

    /// <summary>Equifax field: street_suffix_1 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_1 { get; private set; }

    /// <summary>Equifax field: post_direction_1 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_1 { get; private set; }

    /// <summary>Equifax field: unit_type_1</summary>
    public string? unit_type_1 { get; private set; }

    /// <summary>Equifax field: unit_number_1</summary>
    public string? unit_number_1 { get; private set; }

    /// <summary>Equifax field: city_name_1</summary>
    public string? city_name_1 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_1</summary>
    public string? state_abbreviation_1 { get; private set; }

    /// <summary>Equifax field: zip_1</summary>
    public string? zip_1 { get; private set; }

    /// <summary>Equifax field: z4_1</summary>
    public string? z4_1 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_1</summary>
    public string? delivery_point_code_1 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_1</summary>
    public string? delivery_point_validation_1 { get; private set; }

    /// <summary>Equifax field: carrier_route_1</summary>
    public string? carrier_route_1 { get; private set; }

    /// <summary>Equifax field: fips_code_1</summary>
    public string? fips_code_1 { get; private set; }

    /// <summary>Equifax field: z4_type_1</summary>
    public string? z4_type_1 { get; private set; }

    /// <summary>Equifax field: transaction_date_1</summary>
    public string? transaction_date_1 { get; private set; }

    /// <summary>Equifax field: address_2 (DECRYPTED from AES-GCM)</summary>
    public string? address_2 { get; private set; }

    /// <summary>Equifax field: house_number_2 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_2 { get; private set; }

    /// <summary>Equifax field: predirectional_2 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_2 { get; private set; }

    /// <summary>Equifax field: street_name_2 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_2 { get; private set; }

    /// <summary>Equifax field: street_suffix_2 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_2 { get; private set; }

    /// <summary>Equifax field: post_direction_2 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_2 { get; private set; }

    /// <summary>Equifax field: unit_type_2</summary>
    public string? unit_type_2 { get; private set; }

    /// <summary>Equifax field: unit_number_2</summary>
    public string? unit_number_2 { get; private set; }

    /// <summary>Equifax field: city_name_2</summary>
    public string? city_name_2 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_2</summary>
    public string? state_abbreviation_2 { get; private set; }

    /// <summary>Equifax field: zip_2</summary>
    public string? zip_2 { get; private set; }

    /// <summary>Equifax field: z4_2</summary>
    public string? z4_2 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_2</summary>
    public string? delivery_point_code_2 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_2</summary>
    public string? delivery_point_validation_2 { get; private set; }

    /// <summary>Equifax field: carrier_route_2</summary>
    public string? carrier_route_2 { get; private set; }

    /// <summary>Equifax field: fips_code_2</summary>
    public string? fips_code_2 { get; private set; }

    /// <summary>Equifax field: z4_type_2</summary>
    public string? z4_type_2 { get; private set; }

    /// <summary>Equifax field: transaction_date_2</summary>
    public string? transaction_date_2 { get; private set; }

    /// <summary>Equifax field: address_3 (DECRYPTED from AES-GCM)</summary>
    public string? address_3 { get; private set; }

    /// <summary>Equifax field: house_number_3 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_3 { get; private set; }

    /// <summary>Equifax field: predirectional_3 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_3 { get; private set; }

    /// <summary>Equifax field: street_name_3 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_3 { get; private set; }

    /// <summary>Equifax field: street_suffix_3 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_3 { get; private set; }

    /// <summary>Equifax field: post_direction_3 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_3 { get; private set; }

    /// <summary>Equifax field: unit_type_3</summary>
    public string? unit_type_3 { get; private set; }

    /// <summary>Equifax field: unit_number_3</summary>
    public string? unit_number_3 { get; private set; }

    /// <summary>Equifax field: city_name_3</summary>
    public string? city_name_3 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_3</summary>
    public string? state_abbreviation_3 { get; private set; }

    /// <summary>Equifax field: zip_3</summary>
    public string? zip_3 { get; private set; }

    /// <summary>Equifax field: z4_3</summary>
    public string? z4_3 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_3</summary>
    public string? delivery_point_code_3 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_3</summary>
    public string? delivery_point_validation_3 { get; private set; }

    /// <summary>Equifax field: carrier_route_3</summary>
    public string? carrier_route_3 { get; private set; }

    /// <summary>Equifax field: fips_code_3</summary>
    public string? fips_code_3 { get; private set; }

    /// <summary>Equifax field: z4_type_3</summary>
    public string? z4_type_3 { get; private set; }

    /// <summary>Equifax field: transaction_date_3</summary>
    public string? transaction_date_3 { get; private set; }

    /// <summary>Equifax field: address_4 (DECRYPTED from AES-GCM)</summary>
    public string? address_4 { get; private set; }

    /// <summary>Equifax field: house_number_4 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_4 { get; private set; }

    /// <summary>Equifax field: predirectional_4 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_4 { get; private set; }

    /// <summary>Equifax field: street_name_4 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_4 { get; private set; }

    /// <summary>Equifax field: street_suffix_4 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_4 { get; private set; }

    /// <summary>Equifax field: post_direction_4 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_4 { get; private set; }

    /// <summary>Equifax field: unit_type_4</summary>
    public string? unit_type_4 { get; private set; }

    /// <summary>Equifax field: unit_number_4</summary>
    public string? unit_number_4 { get; private set; }

    /// <summary>Equifax field: city_name_4</summary>
    public string? city_name_4 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_4</summary>
    public string? state_abbreviation_4 { get; private set; }

    /// <summary>Equifax field: zip_4</summary>
    public string? zip_4 { get; private set; }

    /// <summary>Equifax field: z4_4</summary>
    public string? z4_4 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_4</summary>
    public string? delivery_point_code_4 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_4</summary>
    public string? delivery_point_validation_4 { get; private set; }

    /// <summary>Equifax field: carrier_route_4</summary>
    public string? carrier_route_4 { get; private set; }

    /// <summary>Equifax field: fips_code_4</summary>
    public string? fips_code_4 { get; private set; }

    /// <summary>Equifax field: z4_type_4</summary>
    public string? z4_type_4 { get; private set; }

    /// <summary>Equifax field: transaction_date_4</summary>
    public string? transaction_date_4 { get; private set; }

    /// <summary>Equifax field: address_5 (DECRYPTED from AES-GCM)</summary>
    public string? address_5 { get; private set; }

    /// <summary>Equifax field: house_number_5 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_5 { get; private set; }

    /// <summary>Equifax field: predirectional_5 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_5 { get; private set; }

    /// <summary>Equifax field: street_name_5 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_5 { get; private set; }

    /// <summary>Equifax field: street_suffix_5 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_5 { get; private set; }

    /// <summary>Equifax field: post_direction_5 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_5 { get; private set; }

    /// <summary>Equifax field: unit_type_5</summary>
    public string? unit_type_5 { get; private set; }

    /// <summary>Equifax field: unit_number_5</summary>
    public string? unit_number_5 { get; private set; }

    /// <summary>Equifax field: city_name_5</summary>
    public string? city_name_5 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_5</summary>
    public string? state_abbreviation_5 { get; private set; }

    /// <summary>Equifax field: zip_5</summary>
    public string? zip_5 { get; private set; }

    /// <summary>Equifax field: z4_5</summary>
    public string? z4_5 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_5</summary>
    public string? delivery_point_code_5 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_5</summary>
    public string? delivery_point_validation_5 { get; private set; }

    /// <summary>Equifax field: carrier_route_5</summary>
    public string? carrier_route_5 { get; private set; }

    /// <summary>Equifax field: fips_code_5</summary>
    public string? fips_code_5 { get; private set; }

    /// <summary>Equifax field: z4_type_5</summary>
    public string? z4_type_5 { get; private set; }

    /// <summary>Equifax field: transaction_date_5</summary>
    public string? transaction_date_5 { get; private set; }

    /// <summary>Equifax field: address_6 (DECRYPTED from AES-GCM)</summary>
    public string? address_6 { get; private set; }

    /// <summary>Equifax field: house_number_6 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_6 { get; private set; }

    /// <summary>Equifax field: predirectional_6 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_6 { get; private set; }

    /// <summary>Equifax field: street_name_6 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_6 { get; private set; }

    /// <summary>Equifax field: street_suffix_6 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_6 { get; private set; }

    /// <summary>Equifax field: post_direction_6 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_6 { get; private set; }

    /// <summary>Equifax field: unit_type_6</summary>
    public string? unit_type_6 { get; private set; }

    /// <summary>Equifax field: unit_number_6</summary>
    public string? unit_number_6 { get; private set; }

    /// <summary>Equifax field: city_name_6</summary>
    public string? city_name_6 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_6</summary>
    public string? state_abbreviation_6 { get; private set; }

    /// <summary>Equifax field: zip_6</summary>
    public string? zip_6 { get; private set; }

    /// <summary>Equifax field: z4_6</summary>
    public string? z4_6 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_6</summary>
    public string? delivery_point_code_6 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_6</summary>
    public string? delivery_point_validation_6 { get; private set; }

    /// <summary>Equifax field: carrier_route_6</summary>
    public string? carrier_route_6 { get; private set; }

    /// <summary>Equifax field: fips_code_6</summary>
    public string? fips_code_6 { get; private set; }

    /// <summary>Equifax field: z4_type_6</summary>
    public string? z4_type_6 { get; private set; }

    /// <summary>Equifax field: transaction_date_6</summary>
    public string? transaction_date_6 { get; private set; }

    /// <summary>Equifax field: address_7 (DECRYPTED from AES-GCM)</summary>
    public string? address_7 { get; private set; }

    /// <summary>Equifax field: house_number_7 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_7 { get; private set; }

    /// <summary>Equifax field: predirectional_7 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_7 { get; private set; }

    /// <summary>Equifax field: street_name_7 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_7 { get; private set; }

    /// <summary>Equifax field: street_suffix_7 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_7 { get; private set; }

    /// <summary>Equifax field: post_direction_7 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_7 { get; private set; }

    /// <summary>Equifax field: unit_type_7</summary>
    public string? unit_type_7 { get; private set; }

    /// <summary>Equifax field: unit_number_7</summary>
    public string? unit_number_7 { get; private set; }

    /// <summary>Equifax field: city_name_7</summary>
    public string? city_name_7 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_7</summary>
    public string? state_abbreviation_7 { get; private set; }

    /// <summary>Equifax field: zip_7</summary>
    public string? zip_7 { get; private set; }

    /// <summary>Equifax field: z4_7</summary>
    public string? z4_7 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_7</summary>
    public string? delivery_point_code_7 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_7</summary>
    public string? delivery_point_validation_7 { get; private set; }

    /// <summary>Equifax field: carrier_route_7</summary>
    public string? carrier_route_7 { get; private set; }

    /// <summary>Equifax field: fips_code_7</summary>
    public string? fips_code_7 { get; private set; }

    /// <summary>Equifax field: z4_type_7</summary>
    public string? z4_type_7 { get; private set; }

    /// <summary>Equifax field: transaction_date_7</summary>
    public string? transaction_date_7 { get; private set; }

    /// <summary>Equifax field: address_8 (DECRYPTED from AES-GCM)</summary>
    public string? address_8 { get; private set; }

    /// <summary>Equifax field: house_number_8 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_8 { get; private set; }

    /// <summary>Equifax field: predirectional_8 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_8 { get; private set; }

    /// <summary>Equifax field: street_name_8 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_8 { get; private set; }

    /// <summary>Equifax field: street_suffix_8 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_8 { get; private set; }

    /// <summary>Equifax field: post_direction_8 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_8 { get; private set; }

    /// <summary>Equifax field: unit_type_8</summary>
    public string? unit_type_8 { get; private set; }

    /// <summary>Equifax field: unit_number_8</summary>
    public string? unit_number_8 { get; private set; }

    /// <summary>Equifax field: city_name_8</summary>
    public string? city_name_8 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_8</summary>
    public string? state_abbreviation_8 { get; private set; }

    /// <summary>Equifax field: zip_8</summary>
    public string? zip_8 { get; private set; }

    /// <summary>Equifax field: z4_8</summary>
    public string? z4_8 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_8</summary>
    public string? delivery_point_code_8 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_8</summary>
    public string? delivery_point_validation_8 { get; private set; }

    /// <summary>Equifax field: carrier_route_8</summary>
    public string? carrier_route_8 { get; private set; }

    /// <summary>Equifax field: fips_code_8</summary>
    public string? fips_code_8 { get; private set; }

    /// <summary>Equifax field: z4_type_8</summary>
    public string? z4_type_8 { get; private set; }

    /// <summary>Equifax field: transaction_date_8</summary>
    public string? transaction_date_8 { get; private set; }

    /// <summary>Equifax field: address_9 (DECRYPTED from AES-GCM)</summary>
    public string? address_9 { get; private set; }

    /// <summary>Equifax field: house_number_9 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_9 { get; private set; }

    /// <summary>Equifax field: predirectional_9 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_9 { get; private set; }

    /// <summary>Equifax field: street_name_9 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_9 { get; private set; }

    /// <summary>Equifax field: street_suffix_9 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_9 { get; private set; }

    /// <summary>Equifax field: post_direction_9 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_9 { get; private set; }

    /// <summary>Equifax field: unit_type_9</summary>
    public string? unit_type_9 { get; private set; }

    /// <summary>Equifax field: unit_number_9</summary>
    public string? unit_number_9 { get; private set; }

    /// <summary>Equifax field: city_name_9</summary>
    public string? city_name_9 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_9</summary>
    public string? state_abbreviation_9 { get; private set; }

    /// <summary>Equifax field: zip_9</summary>
    public string? zip_9 { get; private set; }

    /// <summary>Equifax field: z4_9</summary>
    public string? z4_9 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_9</summary>
    public string? delivery_point_code_9 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_9</summary>
    public string? delivery_point_validation_9 { get; private set; }

    /// <summary>Equifax field: carrier_route_9</summary>
    public string? carrier_route_9 { get; private set; }

    /// <summary>Equifax field: fips_code_9</summary>
    public string? fips_code_9 { get; private set; }

    /// <summary>Equifax field: z4_type_9</summary>
    public string? z4_type_9 { get; private set; }

    /// <summary>Equifax field: transaction_date_9</summary>
    public string? transaction_date_9 { get; private set; }

    /// <summary>Equifax field: address_10 (DECRYPTED from AES-GCM)</summary>
    public string? address_10 { get; private set; }

    /// <summary>Equifax field: house_number_10 (DECRYPTED from AES-GCM)</summary>
    public string? house_number_10 { get; private set; }

    /// <summary>Equifax field: predirectional_10 (DECRYPTED from AES-GCM)</summary>
    public string? predirectional_10 { get; private set; }

    /// <summary>Equifax field: street_name_10 (DECRYPTED from AES-GCM)</summary>
    public string? street_name_10 { get; private set; }

    /// <summary>Equifax field: street_suffix_10 (DECRYPTED from AES-GCM)</summary>
    public string? street_suffix_10 { get; private set; }

    /// <summary>Equifax field: post_direction_10 (DECRYPTED from AES-GCM)</summary>
    public string? post_direction_10 { get; private set; }

    /// <summary>Equifax field: unit_type_10</summary>
    public string? unit_type_10 { get; private set; }

    /// <summary>Equifax field: unit_number_10</summary>
    public string? unit_number_10 { get; private set; }

    /// <summary>Equifax field: city_name_10</summary>
    public string? city_name_10 { get; private set; }

    /// <summary>Equifax field: state_abbreviation_10</summary>
    public string? state_abbreviation_10 { get; private set; }

    /// <summary>Equifax field: zip_10</summary>
    public string? zip_10 { get; private set; }

    /// <summary>Equifax field: z4_10</summary>
    public string? z4_10 { get; private set; }

    /// <summary>Equifax field: delivery_point_code_10</summary>
    public string? delivery_point_code_10 { get; private set; }

    /// <summary>Equifax field: delivery_point_validation_10</summary>
    public string? delivery_point_validation_10 { get; private set; }

    /// <summary>Equifax field: carrier_route_10</summary>
    public string? carrier_route_10 { get; private set; }

    /// <summary>Equifax field: fips_code_10</summary>
    public string? fips_code_10 { get; private set; }

    /// <summary>Equifax field: z4_type_10</summary>
    public string? z4_type_10 { get; private set; }

    /// <summary>Equifax field: transaction_date_10</summary>
    public string? transaction_date_10 { get; private set; }

    // ====================================================================
    // PHONE NUMBERS (12 columns)
    // 7 phones + 5 last_seen dates
    // ====================================================================

    /// <summary>Equifax field: mobile_phone_1</summary>
    public string? mobile_phone_1 { get; private set; }

    /// <summary>Equifax field: mobile_phone_2</summary>
    public string? mobile_phone_2 { get; private set; }

    /// <summary>Equifax field: phone_1</summary>
    public string? phone_1 { get; private set; }

    /// <summary>Equifax field: last_seen_date_phone_1</summary>
    public string? last_seen_date_phone_1 { get; private set; }

    /// <summary>Equifax field: phone_2</summary>
    public string? phone_2 { get; private set; }

    /// <summary>Equifax field: last_seen_date_phone_2</summary>
    public string? last_seen_date_phone_2 { get; private set; }

    /// <summary>Equifax field: phone_3</summary>
    public string? phone_3 { get; private set; }

    /// <summary>Equifax field: last_seen_date_phone_3</summary>
    public string? last_seen_date_phone_3 { get; private set; }

    /// <summary>Equifax field: phone_4</summary>
    public string? phone_4 { get; private set; }

    /// <summary>Equifax field: last_seen_date_phone_4</summary>
    public string? last_seen_date_phone_4 { get; private set; }

    /// <summary>Equifax field: phone_5</summary>
    public string? phone_5 { get; private set; }

    /// <summary>Equifax field: last_seen_date_phone_5</summary>
    public string? last_seen_date_phone_5 { get; private set; }

    // ====================================================================
    // EMAIL ADDRESSES (30 columns)
    // 15 emails + 15 last_seen dates
    // ====================================================================

    /// <summary>Equifax field: email_1</summary>
    public string? email_1 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_1</summary>
    public string? last_seen_date_email_1 { get; private set; }

    /// <summary>Equifax field: email_2</summary>
    public string? email_2 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_2</summary>
    public string? last_seen_date_email_2 { get; private set; }

    /// <summary>Equifax field: email_3</summary>
    public string? email_3 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_3</summary>
    public string? last_seen_date_email_3 { get; private set; }

    /// <summary>Equifax field: email_4</summary>
    public string? email_4 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_4</summary>
    public string? last_seen_date_email_4 { get; private set; }

    /// <summary>Equifax field: email_5</summary>
    public string? email_5 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_5</summary>
    public string? last_seen_date_email_5 { get; private set; }

    /// <summary>Equifax field: email_6</summary>
    public string? email_6 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_6</summary>
    public string? last_seen_date_email_6 { get; private set; }

    /// <summary>Equifax field: email_7</summary>
    public string? email_7 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_7</summary>
    public string? last_seen_date_email_7 { get; private set; }

    /// <summary>Equifax field: email_8</summary>
    public string? email_8 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_8</summary>
    public string? last_seen_date_email_8 { get; private set; }

    /// <summary>Equifax field: email_9</summary>
    public string? email_9 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_9</summary>
    public string? last_seen_date_email_9 { get; private set; }

    /// <summary>Equifax field: email_10</summary>
    public string? email_10 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_10</summary>
    public string? last_seen_date_email_10 { get; private set; }

    /// <summary>Equifax field: email_11</summary>
    public string? email_11 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_11</summary>
    public string? last_seen_date_email_11 { get; private set; }

    /// <summary>Equifax field: email_12</summary>
    public string? email_12 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_12</summary>
    public string? last_seen_date_email_12 { get; private set; }

    /// <summary>Equifax field: email_13</summary>
    public string? email_13 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_13</summary>
    public string? last_seen_date_email_13 { get; private set; }

    /// <summary>Equifax field: email_14</summary>
    public string? email_14 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_14</summary>
    public string? last_seen_date_email_14 { get; private set; }

    /// <summary>Equifax field: email_15</summary>
    public string? email_15 { get; private set; }

    /// <summary>Equifax field: last_seen_date_email_15</summary>
    public string? last_seen_date_email_15 { get; private set; }

    /// <summary>Equifax field: marketing_email_flag</summary>
    public string? marketing_email_flag { get; private set; }

    // ====================================================================
    // FINANCIAL ATTRIBUTES (14 columns)
    // Income, spending power, affluence
    // ====================================================================

    /// <summary>Equifax field: income360_complete</summary>
    public decimal? income360_complete { get; private set; }

    /// <summary>Equifax field: income360_salary</summary>
    public decimal? income360_salary { get; private set; }

    /// <summary>Equifax field: income360_non_salary</summary>
    public decimal? income360_non_salary { get; private set; }

    /// <summary>Equifax field: economiccohortscode</summary>
    public string? economiccohortscode { get; private set; }

    /// <summary>Equifax field: financialdurabilityindex</summary>
    public decimal? financialdurabilityindex { get; private set; }

    /// <summary>Equifax field: financialdurabilityscore</summary>
    public decimal? financialdurabilityscore { get; private set; }

    /// <summary>Equifax field: spending_power</summary>
    public decimal? spending_power { get; private set; }

    /// <summary>Equifax field: affluence_index</summary>
    public decimal? affluence_index { get; private set; }

    /// <summary>Equifax field: balance_auto_finance_loan_accounts</summary>
    public decimal? balance_auto_finance_loan_accounts { get; private set; }

    /// <summary>Equifax field: percent_balance_to_high_auto_finance_credit</summary>
    public decimal? percent_balance_to_high_auto_finance_credit { get; private set; }

    // ====================================================================
    // CREDIT ATTRIBUTES (123 columns)
    // Vantage scores and propensity models
    // ====================================================================

    /// <summary>Equifax field: vantage_score_neighborhood_risk_score</summary>
    public decimal? vantage_score_neighborhood_risk_score { get; private set; }

    /// <summary>Equifax field: automotive_response_intent_indicator</summary>
    public decimal? automotive_response_intent_indicator { get; private set; }

    /// <summary>Equifax field: auto_in_market_propensity_score</summary>
    public decimal? auto_in_market_propensity_score { get; private set; }

    /// <summary>Equifax field: vds</summary>
    public decimal? vds { get; private set; }

    /// <summary>Equifax field: vf</summary>
    public decimal? vf { get; private set; }

    /// <summary>Equifax field: vr</summary>
    public decimal? vr { get; private set; }

    /// <summary>Equifax field: vg</summary>
    public decimal? vg { get; private set; }

    /// <summary>Equifax field: vh</summary>
    public decimal? vh { get; private set; }

    /// <summary>Equifax field: vj</summary>
    public decimal? vj { get; private set; }

    /// <summary>Equifax field: vac</summary>
    public decimal? vac { get; private set; }

    /// <summary>Equifax field: vy</summary>
    public decimal? vy { get; private set; }

    /// <summary>Equifax field: vs</summary>
    public decimal? vs { get; private set; }

    /// <summary>Equifax field: vt</summary>
    public decimal? vt { get; private set; }

    /// <summary>Equifax field: vc</summary>
    public decimal? vc { get; private set; }

    /// <summary>Equifax field: vdo</summary>
    public decimal? vdo { get; private set; }

    /// <summary>Equifax field: vcw</summary>
    public decimal? vcw { get; private set; }

    /// <summary>Equifax field: vda</summary>
    public decimal? vda { get; private set; }

    /// <summary>Equifax field: vcz</summary>
    public decimal? vcz { get; private set; }

    /// <summary>Equifax field: vbs</summary>
    public decimal? vbs { get; private set; }

    /// <summary>Equifax field: vby</summary>
    public decimal? vby { get; private set; }

    /// <summary>Equifax field: vbr</summary>
    public decimal? vbr { get; private set; }

    /// <summary>Equifax field: vbw</summary>
    public decimal? vbw { get; private set; }

    /// <summary>Equifax field: vbv</summary>
    public decimal? vbv { get; private set; }

    /// <summary>Equifax field: vbt</summary>
    public decimal? vbt { get; private set; }

    /// <summary>Equifax field: vbx</summary>
    public decimal? vbx { get; private set; }

    /// <summary>Equifax field: vbu</summary>
    public decimal? vbu { get; private set; }

    /// <summary>Equifax field: vdp</summary>
    public decimal? vdp { get; private set; }

    /// <summary>Equifax field: vcv</summary>
    public decimal? vcv { get; private set; }

    /// <summary>Equifax field: vcy</summary>
    public decimal? vcy { get; private set; }

    /// <summary>Equifax field: vcx</summary>
    public decimal? vcx { get; private set; }

    /// <summary>Equifax field: vcl</summary>
    public decimal? vcl { get; private set; }

    /// <summary>Equifax field: vck</summary>
    public decimal? vck { get; private set; }

    /// <summary>Equifax field: vbz</summary>
    public decimal? vbz { get; private set; }

    /// <summary>Equifax field: vch</summary>
    public decimal? vch { get; private set; }

    /// <summary>Equifax field: vcf</summary>
    public decimal? vcf { get; private set; }

    /// <summary>Equifax field: vca</summary>
    public decimal? vca { get; private set; }

    /// <summary>Equifax field: vcc</summary>
    public decimal? vcc { get; private set; }

    /// <summary>Equifax field: vce</summary>
    public decimal? vce { get; private set; }

    /// <summary>Equifax field: vci</summary>
    public decimal? vci { get; private set; }

    /// <summary>Equifax field: vcj</summary>
    public decimal? vcj { get; private set; }

    /// <summary>Equifax field: vcd</summary>
    public decimal? vcd { get; private set; }

    /// <summary>Equifax field: vcm</summary>
    public decimal? vcm { get; private set; }

    /// <summary>Equifax field: vcb</summary>
    public decimal? vcb { get; private set; }

    /// <summary>Equifax field: vcg</summary>
    public decimal? vcg { get; private set; }

    /// <summary>Equifax field: vdq</summary>
    public decimal? vdq { get; private set; }

    /// <summary>Equifax field: vct</summary>
    public decimal? vct { get; private set; }

    /// <summary>Equifax field: vcs</summary>
    public decimal? vcs { get; private set; }

    /// <summary>Equifax field: vco</summary>
    public decimal? vco { get; private set; }

    /// <summary>Equifax field: vcp</summary>
    public decimal? vcp { get; private set; }

    /// <summary>Equifax field: vcn</summary>
    public decimal? vcn { get; private set; }

    /// <summary>Equifax field: vcr</summary>
    public decimal? vcr { get; private set; }

    /// <summary>Equifax field: vcq</summary>
    public decimal? vcq { get; private set; }

    /// <summary>Equifax field: vdr</summary>
    public decimal? vdr { get; private set; }

    /// <summary>Equifax field: vdd</summary>
    public decimal? vdd { get; private set; }

    /// <summary>Equifax field: vdc</summary>
    public decimal? vdc { get; private set; }

    /// <summary>Equifax field: vdb</summary>
    public decimal? vdb { get; private set; }

    /// <summary>Equifax field: vcu</summary>
    public decimal? vcu { get; private set; }

    /// <summary>Equifax field: vde</summary>
    public decimal? vde { get; private set; }

    /// <summary>Equifax field: vad</summary>
    public decimal? vad { get; private set; }

    /// <summary>Equifax field: vbi</summary>
    public decimal? vbi { get; private set; }

    /// <summary>Equifax field: vbd</summary>
    public decimal? vbd { get; private set; }

    /// <summary>Equifax field: vay</summary>
    public decimal? vay { get; private set; }

    /// <summary>Equifax field: vbm</summary>
    public decimal? vbm { get; private set; }

    /// <summary>Equifax field: vbk</summary>
    public decimal? vbk { get; private set; }

    /// <summary>Equifax field: vag</summary>
    public decimal? vag { get; private set; }

    /// <summary>Equifax field: vaz</summary>
    public decimal? vaz { get; private set; }

    /// <summary>Equifax field: vbc</summary>
    public decimal? vbc { get; private set; }

    /// <summary>Equifax field: vbo</summary>
    public decimal? vbo { get; private set; }

    /// <summary>Equifax field: vbp</summary>
    public decimal? vbp { get; private set; }

    /// <summary>Equifax field: vae</summary>
    public decimal? vae { get; private set; }

    /// <summary>Equifax field: vbq</summary>
    public decimal? vbq { get; private set; }

    /// <summary>Equifax field: vba</summary>
    public decimal? vba { get; private set; }

    /// <summary>Equifax field: vbl</summary>
    public decimal? vbl { get; private set; }

    /// <summary>Equifax field: vaf</summary>
    public decimal? vaf { get; private set; }

    /// <summary>Equifax field: vbb</summary>
    public decimal? vbb { get; private set; }

    /// <summary>Equifax field: vbg</summary>
    public decimal? vbg { get; private set; }

    /// <summary>Equifax field: vai</summary>
    public decimal? vai { get; private set; }

    /// <summary>Equifax field: vbe</summary>
    public decimal? vbe { get; private set; }

    /// <summary>Equifax field: vaj</summary>
    public decimal? vaj { get; private set; }

    /// <summary>Equifax field: vbf</summary>
    public decimal? vbf { get; private set; }

    /// <summary>Equifax field: vah</summary>
    public decimal? vah { get; private set; }

    /// <summary>Equifax field: vbn</summary>
    public decimal? vbn { get; private set; }

    /// <summary>Equifax field: vbh</summary>
    public decimal? vbh { get; private set; }

    /// <summary>Equifax field: vak</summary>
    public decimal? vak { get; private set; }

    /// <summary>Equifax field: vbj</summary>
    public decimal? vbj { get; private set; }

    /// <summary>Equifax field: vaw</summary>
    public decimal? vaw { get; private set; }

    /// <summary>Equifax field: vdn</summary>
    public decimal? vdn { get; private set; }

    /// <summary>Equifax field: vaq</summary>
    public decimal? vaq { get; private set; }

    /// <summary>Equifax field: vau</summary>
    public decimal? vau { get; private set; }

    /// <summary>Equifax field: vam</summary>
    public decimal? vam { get; private set; }

    /// <summary>Equifax field: val</summary>
    public decimal? val { get; private set; }

    /// <summary>Equifax field: van</summary>
    public decimal? van { get; private set; }

    /// <summary>Equifax field: vas</summary>
    public decimal? vas { get; private set; }

    /// <summary>Equifax field: vao</summary>
    public decimal? vao { get; private set; }

    /// <summary>Equifax field: vat</summary>
    public decimal? vat { get; private set; }

    /// <summary>Equifax field: vax</summary>
    public decimal? vax { get; private set; }

    /// <summary>Equifax field: vav</summary>
    public decimal? vav { get; private set; }

    /// <summary>Equifax field: vap</summary>
    public decimal? vap { get; private set; }

    /// <summary>Equifax field: vdg</summary>
    public decimal? vdg { get; private set; }

    /// <summary>Equifax field: vdk</summary>
    public decimal? vdk { get; private set; }

    /// <summary>Equifax field: vdf</summary>
    public decimal? vdf { get; private set; }

    /// <summary>Equifax field: vdj</summary>
    public decimal? vdj { get; private set; }

    /// <summary>Equifax field: vdi</summary>
    public decimal? vdi { get; private set; }

    /// <summary>Equifax field: vdm</summary>
    public decimal? vdm { get; private set; }

    /// <summary>Equifax field: vdh</summary>
    public decimal? vdh { get; private set; }

    /// <summary>Equifax field: vdl</summary>
    public decimal? vdl { get; private set; }

    /// <summary>Equifax field: vk</summary>
    public decimal? vk { get; private set; }

    /// <summary>Equifax field: vn</summary>
    public decimal? vn { get; private set; }

    /// <summary>Equifax field: vm</summary>
    public decimal? vm { get; private set; }

    /// <summary>Equifax field: vp</summary>
    public decimal? vp { get; private set; }

    /// <summary>Equifax field: vo</summary>
    public decimal? vo { get; private set; }

    /// <summary>Equifax field: vq</summary>
    public decimal? vq { get; private set; }

    /// <summary>Equifax field: vl</summary>
    public decimal? vl { get; private set; }

    /// <summary>Equifax field: vaa</summary>
    public decimal? vaa { get; private set; }

    // ====================================================================
    // DEVICE IDENTIFIERS (7 columns)
    // IP addresses and IDFA identifiers
    // ====================================================================

    /// <summary>Equifax field: ipaddress1 (DECRYPTED from AES-GCM)</summary>
    public string? ipaddress1 { get; private set; }

    /// <summary>Equifax field: ipaddress2 (DECRYPTED from AES-GCM)</summary>
    public string? ipaddress2 { get; private set; }

    /// <summary>Equifax field: idfa1 (DECRYPTED from AES-GCM)</summary>
    public string? idfa1 { get; private set; }

    /// <summary>Equifax field: idfa2 (DECRYPTED from AES-GCM)</summary>
    public string? idfa2 { get; private set; }

    /// <summary>Equifax field: idfa3 (DECRYPTED from AES-GCM)</summary>
    public string? idfa3 { get; private set; }

    /// <summary>Equifax field: idfa4 (DECRYPTED from AES-GCM)</summary>
    public string? idfa4 { get; private set; }

    /// <summary>Equifax field: idfa5 (DECRYPTED from AES-GCM)</summary>
    public string? idfa5 { get; private set; }

    // ====================================================================
    // METADATA (8 columns)
    // Marketing flags, UUID, revision
    // ====================================================================

    /// <summary>Equifax field: consumer_key (DECRYPTED from AES-GCM)</summary>
    public string consumer_key { get; private set; }

    /// <summary>Equifax field: prefix (DECRYPTED from AES-GCM)</summary>
    public string? prefix { get; private set; }

    /// <summary>Equifax field: uuid</summary>
    public string? uuid { get; private set; }

    /// <summary>Equifax field: meta_revision</summary>
    public string? meta_revision { get; private set; }

    // ====================================================================
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
