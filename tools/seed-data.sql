-- ====================================================================
-- Database Seeder for Equifax Enrichment API
-- Populates test data for BDD scenarios
-- ====================================================================

-- Connect to database
\c equifax_enrichment_api_dev;

-- Check if data already exists
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM consumer_enrichments LIMIT 1) THEN
        RAISE NOTICE 'Database already seeded. Skipping seed operation.';
    ELSE
        RAISE NOTICE 'Seeding database with test data...';

        -- BDD SCENARIO 1-3, 10: Successful lookup for phone 8015551234
        INSERT INTO consumer_enrichments (
            id, consumer_key, normalized_phone, match_confidence, match_type,
            data_freshness_date, personal_info_json, addresses_json, phones_json,
            financial_json, created_at, updated_at
        ) VALUES (
            '11111111-1111-1111-1111-111111111111'::uuid,
            'EQF_A1B2C3D4E5F6G7H8I9J0K1L2',
            '8015551234',
            0.75,
            'phone_only',
            NOW() - INTERVAL '7 days',
            '{"first_name": "Bob", "last_name": "Barker", "middle_initial": "W", "date_of_birth": "1950-12-25", "ssn_last_4": "1234", "gender": "M", "age": 73}',
            '[{"address_type": "current", "street": "123 Main St", "city": "Bountiful", "state": "UT", "postal_code": "84010", "county": "Davis", "country": "USA", "residence_type": "single_family", "ownership": "owner", "move_in_date": "2015-03-15", "confidence": 0.95}]',
            '[{"phone": "8015551234", "phone_type": "mobile", "carrier": "Verizon Wireless", "is_active": true, "first_seen": "2015-03-15", "last_verified": "2025-10-24", "confidence": 0.98}]',
            '{"credit_score": 720, "credit_score_date": "2025-10-01", "estimated_income": 85000, "homeowner": true, "estimated_home_value": 450000}',
            NOW(),
            NOW()
        );

        -- Additional test data: High confidence record
        INSERT INTO consumer_enrichments (
            id, consumer_key, normalized_phone, match_confidence, match_type,
            data_freshness_date, personal_info_json, addresses_json, phones_json,
            financial_json, created_at, updated_at
        ) VALUES (
            '22222222-2222-2222-2222-222222222222'::uuid,
            'EQF_X9Y8Z7W6V5U4T3S2R1Q0P9O8',
            '3105552000',
            0.95,
            'phone_with_name_and_address',
            NOW() - INTERVAL '1 day',
            '{"first_name": "Alice", "last_name": "Smith", "age": 42}',
            '[{"street": "789 Pine Rd", "city": "Los Angeles", "state": "CA", "postal_code": "90001"}]',
            '[{"phone": "3105552000", "phone_type": "mobile", "is_active": true}]',
            '{"credit_score": 800, "estimated_income": 120000}',
            NOW(),
            NOW()
        );

        -- Additional test data: Lower confidence record
        INSERT INTO consumer_enrichments (
            id, consumer_key, normalized_phone, match_confidence, match_type,
            data_freshness_date, personal_info_json, addresses_json, phones_json,
            financial_json, created_at, updated_at
        ) VALUES (
            '33333333-3333-3333-3333-333333333333'::uuid,
            'EQF_M1N2O3P4Q5R6S7T8U9V0W1X2',
            '2125553000',
            0.60,
            'phone_only',
            NOW() - INTERVAL '90 days',
            '{"first_name": "John", "last_name": "Doe"}',
            '[{"city": "New York", "state": "NY", "postal_code": "10001"}]',
            '[{"phone": "2125553000", "phone_type": "landline"}]',
            '{"credit_score": 650}',
            NOW(),
            NOW()
        );

        RAISE NOTICE 'Seeded 3 test consumer enrichment records';
        RAISE NOTICE '  - 8015551234: Bob Barker (base confidence 0.75)';
        RAISE NOTICE '  - 3105552000: Alice Smith (high confidence 0.95)';
        RAISE NOTICE '  - 2125553000: John Doe (low confidence 0.60)';
        RAISE NOTICE '  - 5559999999: NOT SEEDED (for no-match scenario testing)';
    END IF;
END $$;

-- Verify data was inserted
SELECT
    normalized_phone,
    match_confidence,
    personal_info_json->>'first_name' as first_name,
    personal_info_json->>'last_name' as last_name
FROM consumer_enrichments
ORDER BY normalized_phone;
