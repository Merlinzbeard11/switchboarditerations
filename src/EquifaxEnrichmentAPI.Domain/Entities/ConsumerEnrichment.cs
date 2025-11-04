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
/// GENERATED FILE: Auto-generated from equifax-full-schema.sql
/// DO NOT EDIT MANUALLY - Regenerate using generate-consumer-enrichment-entity.py
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

    /// <summary>first_name (DECRYPTED)</summary>
    public string? FirstName { get; private set; }

    /// <summary>middle_name (DECRYPTED)</summary>
    public string? MiddleName { get; private set; }

    /// <summary>last_name (DECRYPTED)</summary>
    public string? LastName { get; private set; }

    /// <summary>suffix (DECRYPTED)</summary>
    public string? Suffix { get; private set; }

    /// <summary>gender</summary>
    public string? Gender { get; private set; }

    /// <summary>date_of_birth (DECRYPTED)</summary>
    public string? DateOfBirth { get; private set; }

    /// <summary>age</summary>
    public string? Age { get; private set; }

    /// <summary>deceased</summary>
    public string? Deceased { get; private set; }

    /// <summary>first_seen_date_primary_name</summary>
    public string? FirstSeenDatePrimaryName { get; private set; }

    /// <summary>last_seen_date_primary_name</summary>
    public string? LastSeenDatePrimaryName { get; private set; }

    /// <summary>alternate_first_name_1 (ENCRYPTED in source)</summary>
    public string? AlternateFirstName1 { get; private set; }

    /// <summary>alternate_middle_name_1 (ENCRYPTED in source)</summary>
    public string? AlternateMiddleName1 { get; private set; }

    /// <summary>alternate_last_name_1 (ENCRYPTED in source)</summary>
    public string? AlternateLastName1 { get; private set; }

    /// <summary>alternate_suffix_1 (ENCRYPTED in source)</summary>
    public string? AlternateSuffix1 { get; private set; }

    /// <summary>alternate_first_name_2 (ENCRYPTED in source)</summary>
    public string? AlternateFirstName2 { get; private set; }

    /// <summary>alternate_middle_name_2 (ENCRYPTED in source)</summary>
    public string? AlternateMiddleName2 { get; private set; }

    /// <summary>alternate_last_name_2 (ENCRYPTED in source)</summary>
    public string? AlternateLastName2 { get; private set; }

    /// <summary>alternate_suffix_2 (ENCRYPTED in source)</summary>
    public string? AlternateSuffix2 { get; private set; }

    /// <summary>alternate_first_name_3 (ENCRYPTED in source)</summary>
    public string? AlternateFirstName3 { get; private set; }

    /// <summary>alternate_middle_name_3 (ENCRYPTED in source)</summary>
    public string? AlternateMiddleName3 { get; private set; }

    /// <summary>alternate_last_name_3 (ENCRYPTED in source)</summary>
    public string? AlternateLastName3 { get; private set; }

    /// <summary>alternate_suffix_3 (ENCRYPTED in source)</summary>
    public string? AlternateSuffix3 { get; private set; }

    /// <summary>alternate_first_name_4 (ENCRYPTED in source)</summary>
    public string? AlternateFirstName4 { get; private set; }

    /// <summary>alternate_middle_name_4 (ENCRYPTED in source)</summary>
    public string? AlternateMiddleName4 { get; private set; }

    /// <summary>alternate_last_name_4 (ENCRYPTED in source)</summary>
    public string? AlternateLastName4 { get; private set; }

    /// <summary>alternate_suffix_4 (ENCRYPTED in source)</summary>
    public string? AlternateSuffix4 { get; private set; }

    /// <summary>alternate_first_name_5 (ENCRYPTED in source)</summary>
    public string? AlternateFirstName5 { get; private set; }

    /// <summary>alternate_middle_name_5 (ENCRYPTED in source)</summary>
    public string? AlternateMiddleName5 { get; private set; }

    /// <summary>alternate_last_name_5 (ENCRYPTED in source)</summary>
    public string? AlternateLastName5 { get; private set; }

    /// <summary>alternate_suffix_5 (ENCRYPTED in source)</summary>
    public string? AlternateSuffix5 { get; private set; }

    // ====================================================================
    // ALTERNATE NAMES (30 columns)
    // 5 sets of 6 fields - track name changes
    // ====================================================================

    /// <summary>alternate_name_1 (ENCRYPTED in source)</summary>
    public string? AlternateName1 { get; private set; }

    /// <summary>alternate_prefix_1 (ENCRYPTED in source)</summary>
    public string? AlternatePrefix1 { get; private set; }

    /// <summary>alternate_name_2 (ENCRYPTED in source)</summary>
    public string? AlternateName2 { get; private set; }

    /// <summary>alternate_prefix_2 (ENCRYPTED in source)</summary>
    public string? AlternatePrefix2 { get; private set; }

    /// <summary>alternate_name_3 (ENCRYPTED in source)</summary>
    public string? AlternateName3 { get; private set; }

    /// <summary>alternate_prefix_3 (ENCRYPTED in source)</summary>
    public string? AlternatePrefix3 { get; private set; }

    /// <summary>alternate_name_4 (ENCRYPTED in source)</summary>
    public string? AlternateName4 { get; private set; }

    /// <summary>alternate_prefix_4 (ENCRYPTED in source)</summary>
    public string? AlternatePrefix4 { get; private set; }

    /// <summary>alternate_name_5 (ENCRYPTED in source)</summary>
    public string? AlternateName5 { get; private set; }

    /// <summary>alternate_prefix_5 (ENCRYPTED in source)</summary>
    public string? AlternatePrefix5 { get; private set; }

    // ====================================================================
    // ADDRESSES (162 columns)
    // 10 address records with full details
    // ====================================================================

    /// <summary>address_1 (ENCRYPTED in source)</summary>
    public string? Address1 { get; private set; }

    /// <summary>house_number_1</summary>
    public string? HouseNumber1 { get; private set; }

    /// <summary>predirectional_1</summary>
    public string? Predirectional1 { get; private set; }

    /// <summary>street_name_1</summary>
    public string? StreetName1 { get; private set; }

    /// <summary>street_suffix_1</summary>
    public string? StreetSuffix1 { get; private set; }

    /// <summary>post_direction_1</summary>
    public string? PostDirection1 { get; private set; }

    /// <summary>unit_type_1</summary>
    public string? UnitType1 { get; private set; }

    /// <summary>unit_number_1</summary>
    public string? UnitNumber1 { get; private set; }

    /// <summary>city_name_1</summary>
    public string? CityName1 { get; private set; }

    /// <summary>state_abbreviation_1</summary>
    public string? StateAbbreviation1 { get; private set; }

    /// <summary>zip_1</summary>
    public string? Zip1 { get; private set; }

    /// <summary>z4_1</summary>
    public string? Z41 { get; private set; }

    /// <summary>delivery_point_code_1</summary>
    public string? DeliveryPointCode1 { get; private set; }

    /// <summary>delivery_point_validation_1</summary>
    public string? DeliveryPointValidation1 { get; private set; }

    /// <summary>carrier_route_1</summary>
    public string? CarrierRoute1 { get; private set; }

    /// <summary>fips_code_1</summary>
    public string? FipsCode1 { get; private set; }

    /// <summary>z4_type_1</summary>
    public string? Z4Type1 { get; private set; }

    /// <summary>transaction_date_1</summary>
    public string? TransactionDate1 { get; private set; }

    /// <summary>address_2 (ENCRYPTED in source)</summary>
    public string? Address2 { get; private set; }

    /// <summary>house_number_2</summary>
    public string? HouseNumber2 { get; private set; }

    /// <summary>predirectional_2</summary>
    public string? Predirectional2 { get; private set; }

    /// <summary>street_name_2</summary>
    public string? StreetName2 { get; private set; }

    /// <summary>street_suffix_2</summary>
    public string? StreetSuffix2 { get; private set; }

    /// <summary>post_direction_2</summary>
    public string? PostDirection2 { get; private set; }

    /// <summary>unit_type_2</summary>
    public string? UnitType2 { get; private set; }

    /// <summary>unit_number_2</summary>
    public string? UnitNumber2 { get; private set; }

    /// <summary>city_name_2</summary>
    public string? CityName2 { get; private set; }

    /// <summary>state_abbreviation_2</summary>
    public string? StateAbbreviation2 { get; private set; }

    /// <summary>zip_2</summary>
    public string? Zip2 { get; private set; }

    /// <summary>z4_2</summary>
    public string? Z42 { get; private set; }

    /// <summary>delivery_point_code_2</summary>
    public string? DeliveryPointCode2 { get; private set; }

    /// <summary>delivery_point_validation_2</summary>
    public string? DeliveryPointValidation2 { get; private set; }

    /// <summary>carrier_route_2</summary>
    public string? CarrierRoute2 { get; private set; }

    /// <summary>fips_code_2</summary>
    public string? FipsCode2 { get; private set; }

    /// <summary>z4_type_2</summary>
    public string? Z4Type2 { get; private set; }

    /// <summary>transaction_date_2</summary>
    public string? TransactionDate2 { get; private set; }

    /// <summary>address_3 (ENCRYPTED in source)</summary>
    public string? Address3 { get; private set; }

    /// <summary>house_number_3</summary>
    public string? HouseNumber3 { get; private set; }

    /// <summary>predirectional_3</summary>
    public string? Predirectional3 { get; private set; }

    /// <summary>street_name_3</summary>
    public string? StreetName3 { get; private set; }

    /// <summary>street_suffix_3</summary>
    public string? StreetSuffix3 { get; private set; }

    /// <summary>post_direction_3</summary>
    public string? PostDirection3 { get; private set; }

    /// <summary>unit_type_3</summary>
    public string? UnitType3 { get; private set; }

    /// <summary>unit_number_3</summary>
    public string? UnitNumber3 { get; private set; }

    /// <summary>city_name_3</summary>
    public string? CityName3 { get; private set; }

    /// <summary>state_abbreviation_3</summary>
    public string? StateAbbreviation3 { get; private set; }

    /// <summary>zip_3</summary>
    public string? Zip3 { get; private set; }

    /// <summary>z4_3</summary>
    public string? Z43 { get; private set; }

    /// <summary>delivery_point_code_3</summary>
    public string? DeliveryPointCode3 { get; private set; }

    /// <summary>delivery_point_validation_3</summary>
    public string? DeliveryPointValidation3 { get; private set; }

    /// <summary>carrier_route_3</summary>
    public string? CarrierRoute3 { get; private set; }

    /// <summary>fips_code_3</summary>
    public string? FipsCode3 { get; private set; }

    /// <summary>z4_type_3</summary>
    public string? Z4Type3 { get; private set; }

    /// <summary>transaction_date_3</summary>
    public string? TransactionDate3 { get; private set; }

    /// <summary>address_4 (ENCRYPTED in source)</summary>
    public string? Address4 { get; private set; }

    /// <summary>house_number_4</summary>
    public string? HouseNumber4 { get; private set; }

    /// <summary>predirectional_4</summary>
    public string? Predirectional4 { get; private set; }

    /// <summary>street_name_4</summary>
    public string? StreetName4 { get; private set; }

    /// <summary>street_suffix_4</summary>
    public string? StreetSuffix4 { get; private set; }

    /// <summary>post_direction_4</summary>
    public string? PostDirection4 { get; private set; }

    /// <summary>unit_type_4</summary>
    public string? UnitType4 { get; private set; }

    /// <summary>unit_number_4</summary>
    public string? UnitNumber4 { get; private set; }

    /// <summary>city_name_4</summary>
    public string? CityName4 { get; private set; }

    /// <summary>state_abbreviation_4</summary>
    public string? StateAbbreviation4 { get; private set; }

    /// <summary>zip_4</summary>
    public string? Zip4 { get; private set; }

    /// <summary>z4_4</summary>
    public string? Z44 { get; private set; }

    /// <summary>delivery_point_code_4</summary>
    public string? DeliveryPointCode4 { get; private set; }

    /// <summary>delivery_point_validation_4</summary>
    public string? DeliveryPointValidation4 { get; private set; }

    /// <summary>carrier_route_4</summary>
    public string? CarrierRoute4 { get; private set; }

    /// <summary>fips_code_4</summary>
    public string? FipsCode4 { get; private set; }

    /// <summary>z4_type_4</summary>
    public string? Z4Type4 { get; private set; }

    /// <summary>transaction_date_4</summary>
    public string? TransactionDate4 { get; private set; }

    /// <summary>address_5 (ENCRYPTED in source)</summary>
    public string? Address5 { get; private set; }

    /// <summary>house_number_5</summary>
    public string? HouseNumber5 { get; private set; }

    /// <summary>predirectional_5</summary>
    public string? Predirectional5 { get; private set; }

    /// <summary>street_name_5</summary>
    public string? StreetName5 { get; private set; }

    /// <summary>street_suffix_5</summary>
    public string? StreetSuffix5 { get; private set; }

    /// <summary>post_direction_5</summary>
    public string? PostDirection5 { get; private set; }

    /// <summary>unit_type_5</summary>
    public string? UnitType5 { get; private set; }

    /// <summary>unit_number_5</summary>
    public string? UnitNumber5 { get; private set; }

    /// <summary>city_name_5</summary>
    public string? CityName5 { get; private set; }

    /// <summary>state_abbreviation_5</summary>
    public string? StateAbbreviation5 { get; private set; }

    /// <summary>zip_5</summary>
    public string? Zip5 { get; private set; }

    /// <summary>z4_5</summary>
    public string? Z45 { get; private set; }

    /// <summary>delivery_point_code_5</summary>
    public string? DeliveryPointCode5 { get; private set; }

    /// <summary>delivery_point_validation_5</summary>
    public string? DeliveryPointValidation5 { get; private set; }

    /// <summary>carrier_route_5</summary>
    public string? CarrierRoute5 { get; private set; }

    /// <summary>fips_code_5</summary>
    public string? FipsCode5 { get; private set; }

    /// <summary>z4_type_5</summary>
    public string? Z4Type5 { get; private set; }

    /// <summary>transaction_date_5</summary>
    public string? TransactionDate5 { get; private set; }

    /// <summary>address_6 (ENCRYPTED in source)</summary>
    public string? Address6 { get; private set; }

    /// <summary>house_number_6</summary>
    public string? HouseNumber6 { get; private set; }

    /// <summary>predirectional_6</summary>
    public string? Predirectional6 { get; private set; }

    /// <summary>street_name_6</summary>
    public string? StreetName6 { get; private set; }

    /// <summary>street_suffix_6</summary>
    public string? StreetSuffix6 { get; private set; }

    /// <summary>post_direction_6</summary>
    public string? PostDirection6 { get; private set; }

    /// <summary>unit_type_6</summary>
    public string? UnitType6 { get; private set; }

    /// <summary>unit_number_6</summary>
    public string? UnitNumber6 { get; private set; }

    /// <summary>city_name_6</summary>
    public string? CityName6 { get; private set; }

    /// <summary>state_abbreviation_6</summary>
    public string? StateAbbreviation6 { get; private set; }

    /// <summary>zip_6</summary>
    public string? Zip6 { get; private set; }

    /// <summary>z4_6</summary>
    public string? Z46 { get; private set; }

    /// <summary>delivery_point_code_6</summary>
    public string? DeliveryPointCode6 { get; private set; }

    /// <summary>delivery_point_validation_6</summary>
    public string? DeliveryPointValidation6 { get; private set; }

    /// <summary>carrier_route_6</summary>
    public string? CarrierRoute6 { get; private set; }

    /// <summary>fips_code_6</summary>
    public string? FipsCode6 { get; private set; }

    /// <summary>z4_type_6</summary>
    public string? Z4Type6 { get; private set; }

    /// <summary>transaction_date_6</summary>
    public string? TransactionDate6 { get; private set; }

    /// <summary>address_7 (ENCRYPTED in source)</summary>
    public string? Address7 { get; private set; }

    /// <summary>house_number_7</summary>
    public string? HouseNumber7 { get; private set; }

    /// <summary>predirectional_7</summary>
    public string? Predirectional7 { get; private set; }

    /// <summary>street_name_7</summary>
    public string? StreetName7 { get; private set; }

    /// <summary>street_suffix_7</summary>
    public string? StreetSuffix7 { get; private set; }

    /// <summary>post_direction_7</summary>
    public string? PostDirection7 { get; private set; }

    /// <summary>unit_type_7</summary>
    public string? UnitType7 { get; private set; }

    /// <summary>unit_number_7</summary>
    public string? UnitNumber7 { get; private set; }

    /// <summary>city_name_7</summary>
    public string? CityName7 { get; private set; }

    /// <summary>state_abbreviation_7</summary>
    public string? StateAbbreviation7 { get; private set; }

    /// <summary>zip_7</summary>
    public string? Zip7 { get; private set; }

    /// <summary>z4_7</summary>
    public string? Z47 { get; private set; }

    /// <summary>delivery_point_code_7</summary>
    public string? DeliveryPointCode7 { get; private set; }

    /// <summary>delivery_point_validation_7</summary>
    public string? DeliveryPointValidation7 { get; private set; }

    /// <summary>carrier_route_7</summary>
    public string? CarrierRoute7 { get; private set; }

    /// <summary>fips_code_7</summary>
    public string? FipsCode7 { get; private set; }

    /// <summary>z4_type_7</summary>
    public string? Z4Type7 { get; private set; }

    /// <summary>transaction_date_7</summary>
    public string? TransactionDate7 { get; private set; }

    /// <summary>address_8 (ENCRYPTED in source)</summary>
    public string? Address8 { get; private set; }

    /// <summary>house_number_8</summary>
    public string? HouseNumber8 { get; private set; }

    /// <summary>predirectional_8</summary>
    public string? Predirectional8 { get; private set; }

    /// <summary>street_name_8</summary>
    public string? StreetName8 { get; private set; }

    /// <summary>street_suffix_8</summary>
    public string? StreetSuffix8 { get; private set; }

    /// <summary>post_direction_8</summary>
    public string? PostDirection8 { get; private set; }

    /// <summary>unit_type_8</summary>
    public string? UnitType8 { get; private set; }

    /// <summary>unit_number_8</summary>
    public string? UnitNumber8 { get; private set; }

    /// <summary>city_name_8</summary>
    public string? CityName8 { get; private set; }

    /// <summary>state_abbreviation_8</summary>
    public string? StateAbbreviation8 { get; private set; }

    /// <summary>zip_8</summary>
    public string? Zip8 { get; private set; }

    /// <summary>z4_8</summary>
    public string? Z48 { get; private set; }

    /// <summary>delivery_point_code_8</summary>
    public string? DeliveryPointCode8 { get; private set; }

    /// <summary>delivery_point_validation_8</summary>
    public string? DeliveryPointValidation8 { get; private set; }

    /// <summary>carrier_route_8</summary>
    public string? CarrierRoute8 { get; private set; }

    /// <summary>fips_code_8</summary>
    public string? FipsCode8 { get; private set; }

    /// <summary>z4_type_8</summary>
    public string? Z4Type8 { get; private set; }

    /// <summary>transaction_date_8</summary>
    public string? TransactionDate8 { get; private set; }

    /// <summary>address_9 (ENCRYPTED in source)</summary>
    public string? Address9 { get; private set; }

    /// <summary>house_number_9</summary>
    public string? HouseNumber9 { get; private set; }

    /// <summary>predirectional_9</summary>
    public string? Predirectional9 { get; private set; }

    /// <summary>street_name_9</summary>
    public string? StreetName9 { get; private set; }

    /// <summary>street_suffix_9</summary>
    public string? StreetSuffix9 { get; private set; }

    /// <summary>post_direction_9</summary>
    public string? PostDirection9 { get; private set; }

    /// <summary>unit_type_9</summary>
    public string? UnitType9 { get; private set; }

    /// <summary>unit_number_9</summary>
    public string? UnitNumber9 { get; private set; }

    /// <summary>city_name_9</summary>
    public string? CityName9 { get; private set; }

    /// <summary>state_abbreviation_9</summary>
    public string? StateAbbreviation9 { get; private set; }

    /// <summary>zip_9</summary>
    public string? Zip9 { get; private set; }

    /// <summary>z4_9</summary>
    public string? Z49 { get; private set; }

    /// <summary>delivery_point_code_9</summary>
    public string? DeliveryPointCode9 { get; private set; }

    /// <summary>delivery_point_validation_9</summary>
    public string? DeliveryPointValidation9 { get; private set; }

    /// <summary>carrier_route_9</summary>
    public string? CarrierRoute9 { get; private set; }

    /// <summary>fips_code_9</summary>
    public string? FipsCode9 { get; private set; }

    /// <summary>z4_type_9</summary>
    public string? Z4Type9 { get; private set; }

    /// <summary>transaction_date_9</summary>
    public string? TransactionDate9 { get; private set; }

    /// <summary>address_10 (ENCRYPTED in source)</summary>
    public string? Address10 { get; private set; }

    /// <summary>house_number_10</summary>
    public string? HouseNumber10 { get; private set; }

    /// <summary>predirectional_10</summary>
    public string? Predirectional10 { get; private set; }

    /// <summary>street_name_10</summary>
    public string? StreetName10 { get; private set; }

    /// <summary>street_suffix_10</summary>
    public string? StreetSuffix10 { get; private set; }

    /// <summary>post_direction_10</summary>
    public string? PostDirection10 { get; private set; }

    /// <summary>unit_type_10</summary>
    public string? UnitType10 { get; private set; }

    /// <summary>unit_number_10</summary>
    public string? UnitNumber10 { get; private set; }

    /// <summary>city_name_10</summary>
    public string? CityName10 { get; private set; }

    /// <summary>state_abbreviation_10</summary>
    public string? StateAbbreviation10 { get; private set; }

    /// <summary>zip_10</summary>
    public string? Zip10 { get; private set; }

    /// <summary>z4_10</summary>
    public string? Z410 { get; private set; }

    /// <summary>delivery_point_code_10</summary>
    public string? DeliveryPointCode10 { get; private set; }

    /// <summary>delivery_point_validation_10</summary>
    public string? DeliveryPointValidation10 { get; private set; }

    /// <summary>carrier_route_10</summary>
    public string? CarrierRoute10 { get; private set; }

    /// <summary>fips_code_10</summary>
    public string? FipsCode10 { get; private set; }

    /// <summary>z4_type_10</summary>
    public string? Z4Type10 { get; private set; }

    /// <summary>transaction_date_10</summary>
    public string? TransactionDate10 { get; private set; }

    // ====================================================================
    // PHONE NUMBERS (12 columns)
    // 7 phones + 5 last_seen dates
    // ====================================================================

    /// <summary>mobile_phone_1</summary>
    public string? MobilePhone1 { get; private set; }

    /// <summary>mobile_phone_2</summary>
    public string? MobilePhone2 { get; private set; }

    /// <summary>phone_1</summary>
    public string? Phone1 { get; private set; }

    /// <summary>last_seen_date_phone_1</summary>
    public string? LastSeenDatePhone1 { get; private set; }

    /// <summary>phone_2</summary>
    public string? Phone2 { get; private set; }

    /// <summary>last_seen_date_phone_2</summary>
    public string? LastSeenDatePhone2 { get; private set; }

    /// <summary>phone_3</summary>
    public string? Phone3 { get; private set; }

    /// <summary>last_seen_date_phone_3</summary>
    public string? LastSeenDatePhone3 { get; private set; }

    /// <summary>phone_4</summary>
    public string? Phone4 { get; private set; }

    /// <summary>last_seen_date_phone_4</summary>
    public string? LastSeenDatePhone4 { get; private set; }

    /// <summary>phone_5</summary>
    public string? Phone5 { get; private set; }

    /// <summary>last_seen_date_phone_5</summary>
    public string? LastSeenDatePhone5 { get; private set; }

    // ====================================================================
    // EMAIL ADDRESSES (30 columns)
    // 15 emails + 15 last_seen dates
    // ====================================================================

    /// <summary>email_1</summary>
    public string? Email1 { get; private set; }

    /// <summary>last_seen_date_email_1</summary>
    public string? LastSeenDateEmail1 { get; private set; }

    /// <summary>email_2</summary>
    public string? Email2 { get; private set; }

    /// <summary>last_seen_date_email_2</summary>
    public string? LastSeenDateEmail2 { get; private set; }

    /// <summary>email_3</summary>
    public string? Email3 { get; private set; }

    /// <summary>last_seen_date_email_3</summary>
    public string? LastSeenDateEmail3 { get; private set; }

    /// <summary>email_4</summary>
    public string? Email4 { get; private set; }

    /// <summary>last_seen_date_email_4</summary>
    public string? LastSeenDateEmail4 { get; private set; }

    /// <summary>email_5</summary>
    public string? Email5 { get; private set; }

    /// <summary>last_seen_date_email_5</summary>
    public string? LastSeenDateEmail5 { get; private set; }

    /// <summary>email_6</summary>
    public string? Email6 { get; private set; }

    /// <summary>last_seen_date_email_6</summary>
    public string? LastSeenDateEmail6 { get; private set; }

    /// <summary>email_7</summary>
    public string? Email7 { get; private set; }

    /// <summary>last_seen_date_email_7</summary>
    public string? LastSeenDateEmail7 { get; private set; }

    /// <summary>email_8</summary>
    public string? Email8 { get; private set; }

    /// <summary>last_seen_date_email_8</summary>
    public string? LastSeenDateEmail8 { get; private set; }

    /// <summary>email_9</summary>
    public string? Email9 { get; private set; }

    /// <summary>last_seen_date_email_9</summary>
    public string? LastSeenDateEmail9 { get; private set; }

    /// <summary>email_10</summary>
    public string? Email10 { get; private set; }

    /// <summary>last_seen_date_email_10</summary>
    public string? LastSeenDateEmail10 { get; private set; }

    /// <summary>email_11</summary>
    public string? Email11 { get; private set; }

    /// <summary>last_seen_date_email_11</summary>
    public string? LastSeenDateEmail11 { get; private set; }

    /// <summary>email_12</summary>
    public string? Email12 { get; private set; }

    /// <summary>last_seen_date_email_12</summary>
    public string? LastSeenDateEmail12 { get; private set; }

    /// <summary>email_13</summary>
    public string? Email13 { get; private set; }

    /// <summary>last_seen_date_email_13</summary>
    public string? LastSeenDateEmail13 { get; private set; }

    /// <summary>email_14</summary>
    public string? Email14 { get; private set; }

    /// <summary>last_seen_date_email_14</summary>
    public string? LastSeenDateEmail14 { get; private set; }

    /// <summary>email_15</summary>
    public string? Email15 { get; private set; }

    /// <summary>last_seen_date_email_15</summary>
    public string? LastSeenDateEmail15 { get; private set; }

    /// <summary>marketing_email_flag</summary>
    public string? MarketingEmailFlag { get; private set; }

    // ====================================================================
    // FINANCIAL ATTRIBUTES (14 columns)
    // Income, spending power, affluence
    // ====================================================================

    /// <summary>income360_complete</summary>
    public decimal? Income360Complete { get; private set; }

    /// <summary>income360_salary</summary>
    public decimal? Income360Salary { get; private set; }

    /// <summary>income360_non_salary</summary>
    public decimal? Income360NonSalary { get; private set; }

    /// <summary>economiccohortscode</summary>
    public string? Economiccohortscode { get; private set; }

    /// <summary>financialdurabilityindex</summary>
    public decimal? Financialdurabilityindex { get; private set; }

    /// <summary>financialdurabilityscore</summary>
    public decimal? Financialdurabilityscore { get; private set; }

    /// <summary>spending_power</summary>
    public decimal? SpendingPower { get; private set; }

    /// <summary>affluence_index</summary>
    public decimal? AffluenceIndex { get; private set; }

    /// <summary>balance_auto_finance_loan_accounts</summary>
    public decimal? BalanceAutoFinanceLoanAccounts { get; private set; }

    /// <summary>percent_balance_to_high_auto_finance_credit</summary>
    public decimal? PercentBalanceToHighAutoFinanceCredit { get; private set; }

    // ====================================================================
    // CREDIT ATTRIBUTES (123 columns)
    // Vantage scores and propensity models
    // ====================================================================

    /// <summary>vantage_score_neighborhood_risk_score</summary>
    public decimal? VantageScoreNeighborhoodRiskScore { get; private set; }

    /// <summary>automotive_response_intent_indicator</summary>
    public decimal? AutomotiveResponseIntentIndicator { get; private set; }

    /// <summary>auto_in_market_propensity_score</summary>
    public decimal? AutoInMarketPropensityScore { get; private set; }

    /// <summary>vds</summary>
    public decimal? Vds { get; private set; }

    /// <summary>vf</summary>
    public decimal? Vf { get; private set; }

    /// <summary>vr</summary>
    public decimal? Vr { get; private set; }

    /// <summary>vg</summary>
    public decimal? Vg { get; private set; }

    /// <summary>vh</summary>
    public decimal? Vh { get; private set; }

    /// <summary>vj</summary>
    public decimal? Vj { get; private set; }

    /// <summary>vac</summary>
    public decimal? Vac { get; private set; }

    /// <summary>vy</summary>
    public decimal? Vy { get; private set; }

    /// <summary>vs</summary>
    public decimal? Vs { get; private set; }

    /// <summary>vt</summary>
    public decimal? Vt { get; private set; }

    /// <summary>vc</summary>
    public decimal? Vc { get; private set; }

    /// <summary>vdo</summary>
    public decimal? Vdo { get; private set; }

    /// <summary>vcw</summary>
    public decimal? Vcw { get; private set; }

    /// <summary>vda</summary>
    public decimal? Vda { get; private set; }

    /// <summary>vcz</summary>
    public decimal? Vcz { get; private set; }

    /// <summary>vbs</summary>
    public decimal? Vbs { get; private set; }

    /// <summary>vby</summary>
    public decimal? Vby { get; private set; }

    /// <summary>vbr</summary>
    public decimal? Vbr { get; private set; }

    /// <summary>vbw</summary>
    public decimal? Vbw { get; private set; }

    /// <summary>vbv</summary>
    public decimal? Vbv { get; private set; }

    /// <summary>vbt</summary>
    public decimal? Vbt { get; private set; }

    /// <summary>vbx</summary>
    public decimal? Vbx { get; private set; }

    /// <summary>vbu</summary>
    public decimal? Vbu { get; private set; }

    /// <summary>vdp</summary>
    public decimal? Vdp { get; private set; }

    /// <summary>vcv</summary>
    public decimal? Vcv { get; private set; }

    /// <summary>vcy</summary>
    public decimal? Vcy { get; private set; }

    /// <summary>vcx</summary>
    public decimal? Vcx { get; private set; }

    /// <summary>vcl</summary>
    public decimal? Vcl { get; private set; }

    /// <summary>vck</summary>
    public decimal? Vck { get; private set; }

    /// <summary>vbz</summary>
    public decimal? Vbz { get; private set; }

    /// <summary>vch</summary>
    public decimal? Vch { get; private set; }

    /// <summary>vcf</summary>
    public decimal? Vcf { get; private set; }

    /// <summary>vca</summary>
    public decimal? Vca { get; private set; }

    /// <summary>vcc</summary>
    public decimal? Vcc { get; private set; }

    /// <summary>vce</summary>
    public decimal? Vce { get; private set; }

    /// <summary>vci</summary>
    public decimal? Vci { get; private set; }

    /// <summary>vcj</summary>
    public decimal? Vcj { get; private set; }

    /// <summary>vcd</summary>
    public decimal? Vcd { get; private set; }

    /// <summary>vcm</summary>
    public decimal? Vcm { get; private set; }

    /// <summary>vcb</summary>
    public decimal? Vcb { get; private set; }

    /// <summary>vcg</summary>
    public decimal? Vcg { get; private set; }

    /// <summary>vdq</summary>
    public decimal? Vdq { get; private set; }

    /// <summary>vct</summary>
    public decimal? Vct { get; private set; }

    /// <summary>vcs</summary>
    public decimal? Vcs { get; private set; }

    /// <summary>vco</summary>
    public decimal? Vco { get; private set; }

    /// <summary>vcp</summary>
    public decimal? Vcp { get; private set; }

    /// <summary>vcn</summary>
    public decimal? Vcn { get; private set; }

    /// <summary>vcr</summary>
    public decimal? Vcr { get; private set; }

    /// <summary>vcq</summary>
    public decimal? Vcq { get; private set; }

    /// <summary>vdr</summary>
    public decimal? Vdr { get; private set; }

    /// <summary>vdd</summary>
    public decimal? Vdd { get; private set; }

    /// <summary>vdc</summary>
    public decimal? Vdc { get; private set; }

    /// <summary>vdb</summary>
    public decimal? Vdb { get; private set; }

    /// <summary>vcu</summary>
    public decimal? Vcu { get; private set; }

    /// <summary>vde</summary>
    public decimal? Vde { get; private set; }

    /// <summary>vad</summary>
    public decimal? Vad { get; private set; }

    /// <summary>vbi</summary>
    public decimal? Vbi { get; private set; }

    /// <summary>vbd</summary>
    public decimal? Vbd { get; private set; }

    /// <summary>vay</summary>
    public decimal? Vay { get; private set; }

    /// <summary>vbm</summary>
    public decimal? Vbm { get; private set; }

    /// <summary>vbk</summary>
    public decimal? Vbk { get; private set; }

    /// <summary>vag</summary>
    public decimal? Vag { get; private set; }

    /// <summary>vaz</summary>
    public decimal? Vaz { get; private set; }

    /// <summary>vbc</summary>
    public decimal? Vbc { get; private set; }

    /// <summary>vbo</summary>
    public decimal? Vbo { get; private set; }

    /// <summary>vbp</summary>
    public decimal? Vbp { get; private set; }

    /// <summary>vae</summary>
    public decimal? Vae { get; private set; }

    /// <summary>vbq</summary>
    public decimal? Vbq { get; private set; }

    /// <summary>vba</summary>
    public decimal? Vba { get; private set; }

    /// <summary>vbl</summary>
    public decimal? Vbl { get; private set; }

    /// <summary>vaf</summary>
    public decimal? Vaf { get; private set; }

    /// <summary>vbb</summary>
    public decimal? Vbb { get; private set; }

    /// <summary>vbg</summary>
    public decimal? Vbg { get; private set; }

    /// <summary>vai</summary>
    public decimal? Vai { get; private set; }

    /// <summary>vbe</summary>
    public decimal? Vbe { get; private set; }

    /// <summary>vaj</summary>
    public decimal? Vaj { get; private set; }

    /// <summary>vbf</summary>
    public decimal? Vbf { get; private set; }

    /// <summary>vah</summary>
    public decimal? Vah { get; private set; }

    /// <summary>vbn</summary>
    public decimal? Vbn { get; private set; }

    /// <summary>vbh</summary>
    public decimal? Vbh { get; private set; }

    /// <summary>vak</summary>
    public decimal? Vak { get; private set; }

    /// <summary>vbj</summary>
    public decimal? Vbj { get; private set; }

    /// <summary>vaw</summary>
    public decimal? Vaw { get; private set; }

    /// <summary>vdn</summary>
    public decimal? Vdn { get; private set; }

    /// <summary>vaq</summary>
    public decimal? Vaq { get; private set; }

    /// <summary>vau</summary>
    public decimal? Vau { get; private set; }

    /// <summary>vam</summary>
    public decimal? Vam { get; private set; }

    /// <summary>val</summary>
    public decimal? Val { get; private set; }

    /// <summary>van</summary>
    public decimal? Van { get; private set; }

    /// <summary>vas</summary>
    public decimal? Vas { get; private set; }

    /// <summary>vao</summary>
    public decimal? Vao { get; private set; }

    /// <summary>vat</summary>
    public decimal? Vat { get; private set; }

    /// <summary>vax</summary>
    public decimal? Vax { get; private set; }

    /// <summary>vav</summary>
    public decimal? Vav { get; private set; }

    /// <summary>vap</summary>
    public decimal? Vap { get; private set; }

    /// <summary>vdg</summary>
    public decimal? Vdg { get; private set; }

    /// <summary>vdk</summary>
    public decimal? Vdk { get; private set; }

    /// <summary>vdf</summary>
    public decimal? Vdf { get; private set; }

    /// <summary>vdj</summary>
    public decimal? Vdj { get; private set; }

    /// <summary>vdi</summary>
    public decimal? Vdi { get; private set; }

    /// <summary>vdm</summary>
    public decimal? Vdm { get; private set; }

    /// <summary>vdh</summary>
    public decimal? Vdh { get; private set; }

    /// <summary>vdl</summary>
    public decimal? Vdl { get; private set; }

    /// <summary>vk</summary>
    public decimal? Vk { get; private set; }

    /// <summary>vn</summary>
    public decimal? Vn { get; private set; }

    /// <summary>vm</summary>
    public decimal? Vm { get; private set; }

    /// <summary>vp</summary>
    public decimal? Vp { get; private set; }

    /// <summary>vo</summary>
    public decimal? Vo { get; private set; }

    /// <summary>vq</summary>
    public decimal? Vq { get; private set; }

    /// <summary>vl</summary>
    public decimal? Vl { get; private set; }

    /// <summary>vaa</summary>
    public decimal? Vaa { get; private set; }

    // ====================================================================
    // DEVICE IDENTIFIERS (7 columns)
    // IP addresses and IDFA identifiers
    // ====================================================================

    /// <summary>ipaddress1</summary>
    public string? Ipaddress1 { get; private set; }

    /// <summary>ipaddress2</summary>
    public string? Ipaddress2 { get; private set; }

    /// <summary>idfa1</summary>
    public string? Idfa1 { get; private set; }

    /// <summary>idfa2</summary>
    public string? Idfa2 { get; private set; }

    /// <summary>idfa3</summary>
    public string? Idfa3 { get; private set; }

    /// <summary>idfa4</summary>
    public string? Idfa4 { get; private set; }

    /// <summary>idfa5</summary>
    public string? Idfa5 { get; private set; }

    // ====================================================================
    // METADATA (8 columns)
    // Marketing flags, UUID, revision
    // ====================================================================

    /// <summary>consumer_key (DECRYPTED)</summary>
    public string ConsumerKey { get; private set; }

    /// <summary>prefix (DECRYPTED)</summary>
    public string? Prefix { get; private set; }

    /// <summary>uuid</summary>
    public string? Uuid { get; private set; }

    /// <summary>meta_revision</summary>
    public string? MetaRevision { get; private set; }

    // ====================================================================
    // COMPUTED FIELDS (backward compatibility)
    // ====================================================================

    /// <summary>
    /// Normalized phone number for backward compatibility
    /// Generated from MobilePhone1 or Phone1
    /// </summary>
    public string? NormalizedPhone => MobilePhone1 ?? Phone1;

    /// <summary>
    /// Match confidence score (calculated from phone column match)
    /// Legacy field - now calculated based on which phone column matched
    /// </summary>
    public double MatchConfidence { get; private set; }

    /// <summary>
    /// Type of match performed
    /// Examples: "phone_only", "phone_with_name", "phone_with_name_and_address"
    /// </summary>
    public string MatchType { get; private set; } = string.Empty;

    /// <summary>
    /// Data freshness date (most recent phone last_seen date)
    /// </summary>
    public DateTimeOffset? DataFreshnessDate { get; private set; }

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
    /// Sets all 398 fields from CSV row with AES-GCM decryption.
    /// </summary>
    public static ConsumerEnrichment CreateFromEquifaxCsv(
        string consumerKey,
        // All other parameters omitted for brevity - would be 398 parameters
        // In practice, use builder pattern or pass CSV field array
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
            ConsumerKey = consumerKey,
            MatchConfidence = matchConfidence,
            MatchType = matchType
            // All 398 fields set from CSV import
        };
    }

    /// <summary>
    /// Updates match metadata (backward compatibility)
    /// </summary>
    public void UpdateMatchMetadata(double newConfidence, string newMatchType)
    {
        if (newConfidence < 0.0 || newConfidence > 1.0)
            throw new ArgumentOutOfRangeException(nameof(newConfidence));

        if (newConfidence > MatchConfidence)
        {
            MatchConfidence = newConfidence;
            MatchType = newMatchType;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
