CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031232551_InitialCreate') THEN
    CREATE TABLE consumer_enrichments (
        id uuid NOT NULL,
        consumer_key character varying(100) NOT NULL,
        normalized_phone character varying(10) NOT NULL,
        match_confidence double precision NOT NULL,
        match_type character varying(50) NOT NULL,
        data_freshness_date timestamp with time zone NOT NULL,
        personal_info_json text NOT NULL,
        addresses_json text NOT NULL,
        phones_json text NOT NULL,
        financial_json text NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_consumer_enrichments" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031232551_InitialCreate') THEN
    CREATE INDEX ix_consumer_enrichments_consumer_key ON consumer_enrichments (consumer_key);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031232551_InitialCreate') THEN
    CREATE INDEX ix_consumer_enrichments_created_at ON consumer_enrichments (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031232551_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_consumer_enrichments_normalized_phone_unique ON consumer_enrichments (normalized_phone);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031232551_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251031232551_InitialCreate', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101001222_AddBuyerTable') THEN
    CREATE TABLE buyers (
        id uuid NOT NULL,
        api_key_hash character varying(64) NOT NULL,
        is_active boolean NOT NULL,
        name character varying(200) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_buyers" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101001222_AddBuyerTable') THEN
    CREATE UNIQUE INDEX ix_buyers_api_key_hash_unique ON buyers (api_key_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101001222_AddBuyerTable') THEN
    CREATE INDEX ix_buyers_created_at ON buyers (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101001222_AddBuyerTable') THEN
    CREATE INDEX ix_buyers_is_active ON buyers (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101001222_AddBuyerTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251101001222_AddBuyerTable', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone1" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone10" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone2" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone3" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone4" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone5" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone6" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone7" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone8" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    ALTER TABLE consumer_enrichments ADD "Phone9" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone1" ON consumer_enrichments ("Phone1");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone2" ON consumer_enrichments ("Phone2");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone3" ON consumer_enrichments ("Phone3");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone4" ON consumer_enrichments ("Phone4");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone5" ON consumer_enrichments ("Phone5");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone6" ON consumer_enrichments ("Phone6");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone7" ON consumer_enrichments ("Phone7");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone8" ON consumer_enrichments ("Phone8");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone9" ON consumer_enrichments ("Phone9");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    CREATE INDEX "IX_consumer_enrichments_Phone10" ON consumer_enrichments ("Phone10");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101005756_AddPhone1ThroughPhone10Columns') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251101005756_AddPhone1ThroughPhone10Columns', '9.0.10');
    END IF;
END $EF$;
COMMIT;

