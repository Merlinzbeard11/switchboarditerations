-- ====================================================================
-- Database Seeder for Buyer Authentication
-- Populates test buyer data for BDD scenarios
-- BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
-- ====================================================================

-- Connect to database
\c equifax_enrichment_api_dev;

-- Check if data already exists
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM buyers LIMIT 1) THEN
        RAISE NOTICE 'Buyers already seeded. Skipping seed operation.';
    ELSE
        RAISE NOTICE 'Seeding buyers with test data...';

        -- ================================================================
        -- TEST BUYER 1: Active buyer with valid API key
        -- API Key (plaintext - for testing ONLY): test-api-key-123
        -- SHA-256 Hash: ouSrBHLICKH/LOFHrk9s2ezYvMiknEg1D5fmgRrOdGQ=
        -- BDD Scenario 1: Successfully authenticate request with valid API key
        -- ================================================================
        INSERT INTO buyers (
            id, api_key_hash, name, is_active, created_at, updated_at
        ) VALUES (
            'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'::uuid,
            'ouSrBHLICKH/LOFHrk9s2ezYvMiknEg1D5fmgRrOdGQ=', -- SHA-256("test-api-key-123")
            'Test Buyer - Active',
            true,
            NOW(),
            NOW()
        );

        -- ================================================================
        -- TEST BUYER 2: Inactive buyer (for testing account deactivation)
        -- API Key (plaintext - for testing ONLY): inactive-api-key-456
        -- SHA-256 Hash: 4C/JzuxByOId9HxmmBeyyyBCD8yWNI5VlGABhxiSC7Y=
        -- BDD Scenario 5: Reject request for inactive buyer account
        -- ================================================================
        INSERT INTO buyers (
            id, api_key_hash, name, is_active, created_at, updated_at
        ) VALUES (
            'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'::uuid,
            '4C/JzuxByOId9HxmmBeyyyBCD8yWNI5VlGABhxiSC7Y=', -- SHA-256("inactive-api-key-456")
            'Test Buyer - Inactive',
            false,
            NOW(),
            NOW()
        );

        RAISE NOTICE 'Seeded 2 test buyer records';
        RAISE NOTICE '  - Test Buyer Active: API key = test-api-key-123';
        RAISE NOTICE '  - Test Buyer Inactive: API key = inactive-api-key-456';
        RAISE NOTICE '';
        RAISE NOTICE 'SECURITY NOTE: These are TEST keys only for development.';
        RAISE NOTICE 'Production keys MUST be generated using cryptographically secure methods.';
    END IF;
END $$;

-- Verify data was inserted
SELECT
    id,
    name,
    is_active,
    LEFT(api_key_hash, 8) as api_key_hash_prefix,
    created_at
FROM buyers
ORDER BY name;
