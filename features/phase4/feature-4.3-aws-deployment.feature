Feature: AWS Deployment (Production-Grade Elastic Beanstalk + ElastiCache + RDS)
  As a DevOps team deploying to AWS production
  I want robust deployment with monitoring and error prevention
  So that the API runs reliably with high availability and performance

  Background:
    Given AWS Elastic Beanstalk is configured for .NET Core
    And AWS ElastiCache Redis cluster-mode is enabled
    And AWS RDS PostgreSQL 18 with Multi-AZ is configured
    And AWS RDS Proxy is configured for connection multiplexing
    And AWS Secrets Manager stores database and Redis credentials
    And Application Load Balancer distributes traffic across availability zones
    And Auto Scaling Group maintains 2-10 EC2 instances based on load
    And IMDSv2 is enforced on all EC2 instances
    And CloudWatch alarms monitor system health

  # ============================================================================
  # SCENARIO 1: CRITICAL - Zip File Structure MUST Be Root-Level (90% of Failures)
  # ============================================================================
  Scenario: Nested folder structure causes deployment failure
    Given I create deployment zip with nested structure:
      """
      enrichment-api.zip
      └─ EnrichmentAPI/
         ├─ EnrichmentAPI.dll
         └─ EnrichmentAPI.runtimeconfig.json
      """
    When I deploy to Elastic Beanstalk
    Then the deployment should show SUCCESS status
    But the application should fail to start
    And the error should be "There is no .runtimeconfig.json file"
    And this is THE #1 CAUSE of .NET Elastic Beanstalk deployment failures (90%)

  Scenario: Root-level file structure enables successful deployment
    Given I create deployment zip with root-level structure:
      """
      enrichment-api.zip
      ├─ EnrichmentAPI.dll
      ├─ EnrichmentAPI.runtimeconfig.json
      ├─ appsettings.json
      └─ web.config
      """
    When I deploy to Elastic Beanstalk
    Then the deployment should succeed
    And the application should start successfully
    And /health endpoint should return 200 OK

  Scenario: Verify zip structure before deployment
    Given I have created deployment package enrichment-api.zip
    When I run: unzip -l enrichment-api.zip | head -20
    Then the output should show files at root level (no nested folders)
    And EnrichmentAPI.dll should be at zip root
    And EnrichmentAPI.runtimeconfig.json should be at zip root
    And this prevents 90% of deployment failures

  # ============================================================================
  # SCENARIO 2: ElastiCache EngineCPUUtilization MUST Stay Below 90%
  # ============================================================================
  Scenario: Redis CPU above 90% causes performance cliff and cascading failures
    Given ElastiCache Redis is handling 100,000 requests/second
    When EngineCPUUtilization reaches 90%
    Then request timeouts should occur
    And Redis throughput should degrade
    And cascading failures should begin (no burst capacity)
    And this is a CRITICAL performance cliff

  Scenario: Multi-level CloudWatch alarms for Redis CPU
    Given ElastiCache Redis is deployed
    When I configure CloudWatch alarms
    Then I should create:
      | alarm_name         | threshold | severity | action                    |
      | Redis-CPU-Warning  | 65%       | WARNING  | Notify team               |
      | Redis-CPU-Critical | 90%       | CRITICAL | Auto-scale / page on-call |
    And alarms should use Average statistic over 60-second period
    And 65% threshold provides early warning
    And 90% threshold prevents performance cliff

  Scenario: Use cluster-mode ENABLED for horizontal scaling beyond 100K RPS
    Given Redis single shard is limited to ~100,000 RPS
    When traffic exceeds 100,000 RPS
    Then I should use cluster-mode ENABLED
    And I should configure multiple shards (3 recommended)
    And total throughput should be: 100,000 RPS × shard count
    And this enables horizontal scaling

  # ============================================================================
  # SCENARIO 3: Secrets Only Fetched at Instance Bootstrap (No Auto-Rotation)
  # ============================================================================
  Scenario: Secret rotation does NOT propagate to running instances automatically
    Given Elastic Beanstalk instance fetched database password at startup
    And the password is stored in environment variable
    When I rotate the secret in Secrets Manager
    Then running instances still have OLD password in environment variables
    And database authentication fails after rotation
    And this is a CRITICAL limitation of Elastic Beanstalk secret handling

  Scenario: Force environment to refetch secrets after rotation
    Given I rotate database password in Secrets Manager
    When I run: aws elasticbeanstalk restart-app-server --environment-name enrichment-api-prod
    Then instances should refetch secrets from Secrets Manager
    And environment variables should be updated
    And database authentication should succeed with new password

  Scenario: Use direct SDK retrieval instead of environment variables (BEST practice)
    Given I need automatic secret rotation support
    When I configure Startup.cs to fetch secrets directly:
      """
      var secretsClient = new AmazonSecretsManagerClient(RegionEndpoint.USEast1);
      var secretResponse = await secretsClient.GetSecretValueAsync(new GetSecretValueRequest
      {
          SecretId = "enrichment-api-prod/database",
          VersionStage = "AWSCURRENT"
      });
      var secrets = JsonSerializer.Deserialize<DatabaseSecrets>(secretResponse.SecretString);
      """
    Then the application should always fetch LATEST secret version
    And secret rotation should propagate automatically
    And no environment restart is required

  # ============================================================================
  # SCENARIO 4: ElastiCache Cluster-Mode ENABLED Required for Horizontal Scaling
  # ============================================================================
  Scenario: Cluster-mode DISABLED limits throughput to single shard (~100K RPS)
    Given I use ElastiCache with cluster-mode DISABLED
    And single shard handles ~100,000 RPS
    When traffic grows to 300,000 RPS
    Then I cannot scale horizontally (single shard limitation)
    And I can only scale vertically (bigger instance)
    And this limits maximum throughput

  Scenario: Cluster-mode ENABLED enables horizontal scaling with multiple shards
    Given I configure ElastiCache with cluster-mode ENABLED
    And I configure 3 shards with 2 replicas each
    When traffic grows to 300,000 RPS
    Then the load is distributed across 3 shards
    And total throughput is: 100,000 RPS × 3 = 300,000 RPS
    And I can add more shards to scale horizontally
    And this is the BEST practice for production workloads

  Scenario: Configure cluster-mode connection string correctly
    Given ElastiCache cluster-mode is ENABLED
    When I configure Redis connection string
    Then I should use configuration endpoint (NOT individual node endpoints):
      """
      EndPoints = { "enrichment-api-redis.abc123.clustercfg.use1.cache.amazonaws.com:6379" }
      """
    And I should set AbortOnConnectFail = false
    And I should set AllowAdmin = false (NEVER use KEYS/FLUSHDB in cluster-mode)
    And this ensures proper cluster-mode connectivity

  # ============================================================================
  # SCENARIO 5: RDS Proxy REQUIRED for High Connection Churn
  # ============================================================================
  Scenario: Without RDS Proxy connection pool exhausts under high load
    Given 1000 API instances × 200 connections each = 200,000 connections
    And RDS max_connections = 5000
    When all API instances connect directly to RDS
    Then RDS connection pool exhausts
    And error occurs: "remaining connection slots are reserved for superuser"
    And API returns 500 errors during traffic spikes

  Scenario: RDS Proxy multiplexes connections to prevent exhaustion
    Given 1000 API instances × 200 connections = 200,000 to RDS Proxy
    And RDS Proxy multiplexes to 500 physical connections to RDS
    And RDS max_connections = 5000
    When all API instances connect to RDS
    Then RDS connection utilization is 10% (500/5000)
    And no connection exhaustion occurs
    And RDS Proxy provides connection multiplexing
    And this is REQUIRED for high connection churn workloads

  Scenario: Configure connection string to use RDS Proxy endpoint
    Given RDS Proxy is deployed
    When I configure connection string
    Then Host should be RDS Proxy endpoint (NOT direct RDS endpoint):
      """
      Host=enrichment-api-rds-proxy.proxy-abc123.us-east-1.rds.amazonaws.com
      """
    And connection pooling settings remain the same
    And RDS Proxy handles connection multiplexing transparently

  # ============================================================================
  # SCENARIO 6: Slow Internet = Incomplete Zip Uploads (Silent Failures)
  # ============================================================================
  Scenario: Incomplete upload causes silent deployment failures
    Given deployment package is 50MB
    And network connection is slow/unstable
    When I upload deployment package to S3
    Then the upload may complete with SUCCESS status
    But only partial file is uploaded (e.g., 45MB)
    And deployment shows SUCCESS but files are missing at runtime
    And this causes intermittent "File not found" errors

  Scenario: Verify S3 object size matches local zip before deployment
    Given I upload deployment package enrichment-api.zip (50MB)
    When I verify S3 object size:
      """
      aws s3 ls s3://elasticbeanstalk-us-east-1-123456789012/enrichment-api/enrichment-api.zip
      """
    Then S3 object size should match local file size exactly (50MB)
    And if sizes don't match, re-upload package
    And this prevents incomplete upload failures

  Scenario: Use AWS CLI multipart upload for large files
    Given deployment package is large (> 10MB)
    When I upload using AWS CLI:
      """
      aws s3 cp enrichment-api.zip s3://bucket/path/enrichment-api.zip \
        --storage-class STANDARD \
        --metadata "md5=$(md5sum enrichment-api.zip | awk '{print $1}')"
      """
    Then multipart upload should handle network interruptions
    And upload integrity should be verified with MD5 checksum
    And this prevents incomplete uploads

  # ============================================================================
  # SCENARIO 7: IMDSv1 Security Risk (SSRF Vulnerability)
  # ============================================================================
  Scenario: IMDSv1 allows SSRF attacks to steal IAM credentials
    Given EC2 instance uses IMDSv1 (default)
    And application has SSRF vulnerability
    When attacker exploits SSRF to access: http://169.254.169.254/latest/meta-data/iam/security-credentials/
    Then attacker receives temporary IAM credentials
    And attacker can use credentials to access AWS resources
    And this is a CRITICAL security vulnerability

  Scenario: IMDSv2 requires session token (blocks SSRF attacks)
    Given EC2 instance is configured with IMDSv2 (http_tokens = required)
    And application has SSRF vulnerability
    When attacker tries to access metadata service
    Then the request is blocked (no session token)
    And attacker cannot retrieve IAM credentials
    And this prevents SSRF credential theft

  Scenario: Enforce IMDSv2 on all Elastic Beanstalk instances
    Given I deploy to Elastic Beanstalk
    When I configure launch template
    Then I should set metadata_options:
      """
      metadata_options {
        http_tokens                 = "required"  # Enforce IMDSv2
        http_put_response_hop_limit = 1
        http_endpoint               = "enabled"
      }
      """
    And all new instances should use IMDSv2
    And this is AWS security best practice (2025)

  # ============================================================================
  # SCENARIO 8: Missing .NET Runtime Version = Deployment Fails
  # ============================================================================
  Scenario: Elastic Beanstalk platform lags behind .NET releases (3-6 months)
    Given application targets .NET 9.0
    And Elastic Beanstalk latest platform supports .NET 8.0
    When I deploy application
    Then deployment fails with error:
      """
      The specified framework 'Microsoft.NETCore.App', version '9.0.0' was not found
      """
    And this is a COMMON issue with latest .NET versions

  Scenario: Check available .NET versions before deployment
    When I run: aws elasticbeanstalk list-available-solution-stacks --query 'SolutionStacks[?contains(@, `.NET`)]'
    Then I should see list of supported .NET versions:
      """
      64bit Amazon Linux 2023 v3.1.1 running .NET 8
      64bit Amazon Linux 2 v2.7.4 running .NET Core 6
      """
    And I should verify my application version is supported
    And if not supported, use Docker or self-contained deployment

  Scenario: Use Docker for latest .NET versions (BEST practice)
    Given Elastic Beanstalk doesn't support .NET 9 yet
    When I create Dockerfile with .NET 9 base image:
      """
      FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
      WORKDIR /app
      COPY publish/ .
      ENTRYPOINT ["dotnet", "EnrichmentAPI.dll"]
      """
    Then I can deploy to Elastic Beanstalk Docker platform
    And application runs with .NET 9 runtime
    And this works with ANY .NET version

  Scenario: Use self-contained deployment (ALTERNATIVE approach)
    Given Elastic Beanstalk doesn't support .NET 9 yet
    When I publish with self-contained=true:
      """
      dotnet publish -c Release -r linux-x64 --self-contained
      """
    Then .NET runtime is included in deployment package
    And application runs without platform .NET runtime
    And this increases deployment package size (~70MB)

  # ============================================================================
  # SCENARIO 9: 4096 Byte Environment Variable Limit (Silent Truncation)
  # ============================================================================
  Scenario: Environment variables exceeding 4096 bytes are silently truncated
    Given I configure 50+ environment variables in Elastic Beanstalk
    And total size exceeds 4096 bytes
    When I deploy application
    Then environment variables are silently truncated
    And variables at the end of the list are missing
    And application fails with "configuration not found" errors
    And this is a SILENT failure (no error during deployment)

  Scenario: Calculate total environment variable size before deployment
    Given I have configured environment variables
    When I run: eb printenv enrichment-api-prod | awk '{print length($0)}' | awk '{sum += $1} END {print sum}'
    Then I should verify total is less than 4096 bytes
    And if exceeding limit, use AWS Systems Manager Parameter Store instead
    And this prevents silent truncation

  Scenario: Use AWS Systems Manager Parameter Store for large configurations (BEST practice)
    Given I need to store large connection strings and configurations
    When I store parameters in Systems Manager Parameter Store:
      """
      aws ssm put-parameter \
        --name "/enrichment-api/prod/ConnectionStrings__EquifaxDb" \
        --value "Host=...;Port=5432;..." \
        --type "SecureString" \
        --tier "Advanced"
      """
    Then I load configuration in Startup.cs:
      """
      config.AddSystemsManager("/enrichment-api/prod", new AWSOptions { Region = RegionEndpoint.USEast1 });
      """
    And environment variable size remains under 4096 bytes
    And this supports 8KB parameters (Advanced tier)

  Scenario: Use Secrets Manager for sensitive configurations (BEST practice)
    Given I need to store database credentials and API keys
    When I store secrets in Secrets Manager
    Then environment variables only contain ARNs (small):
      """
      DATABASE_SECRET_ARN=arn:aws:secretsmanager:us-east-1:123456789012:secret:db-ABC123
      REDIS_SECRET_ARN=arn:aws:secretsmanager:us-east-1:123456789012:secret:redis-XYZ789
      """
    And total environment variable size is ~180 bytes (well under limit)
    And application fetches secrets at runtime

  # ============================================================================
  # SCENARIO 10: No Reset On Close for PgBouncer Compatibility
  # ============================================================================
  Scenario: Npgsql default "Reset On Close" breaks PgBouncer transaction pooling
    Given application uses PgBouncer in TRANSACTION pooling mode
    And Npgsql default sends DISCARD ALL when closing connection
    When connection is closed
    Then Npgsql sends DISCARD ALL to reset session state
    And DISCARD ALL fails in PgBouncer transaction mode
    And error occurs: "DISCARD ALL not allowed in transaction mode"
    And connection pooling breaks

  Scenario: Set "No Reset On Close=true" for PgBouncer compatibility
    Given application uses PgBouncer in TRANSACTION pooling mode
    When I configure connection string:
      """
      Host=pgbouncer.internal;Port=6432;Database=postgres;Pooling=true;No Reset On Close=true;Max Auto Prepare=0
      """
    Then Npgsql skips DISCARD ALL on connection close
    And PgBouncer transaction pooling works correctly
    And this is REQUIRED for PgBouncer compatibility

  Scenario: When to use "No Reset On Close"
    Given I am configuring connection pooling
    Then I should use "No Reset On Close=true" when:
      | scenario                              | use_no_reset_on_close |
      | PgBouncer in TRANSACTION pooling mode | true                  |
      | AWS RDS Proxy                         | true (recommended)    |
      | High connection churn workloads       | true                  |
      | Direct RDS connection (no pooler)     | false                 |
      | PgBouncer in SESSION pooling mode     | false                 |
    And this depends on architecture

  # ============================================================================
  # SCENARIO 11: Multi-AZ Deployment for High Availability
  # ============================================================================
  Scenario: Deploy across multiple availability zones for fault tolerance
    Given I configure Elastic Beanstalk environment
    Then I should enable multi-AZ deployment
    And Auto Scaling Group should span 2+ availability zones
    And RDS should use Multi-AZ with automatic failover
    And ElastiCache should use Multi-AZ with replicas
    And this provides high availability (99.99% SLA)

  Scenario: Application Load Balancer distributes traffic across AZs
    Given ALB is configured for multi-AZ
    When traffic arrives at ALB
    Then requests are distributed across availability zones
    And target groups exist in each AZ
    And health checks verify instance availability
    And unhealthy instances are removed from rotation

  # ============================================================================
  # SCENARIO 12: Auto Scaling Configuration
  # ============================================================================
  Scenario: Configure Auto Scaling based on CPU and request count
    Given I configure Auto Scaling Group
    Then I should set scaling policies:
      | metric                      | target | action             |
      | Average CPU > 70%           | 70%    | Scale up           |
      | Average CPU < 30%           | 30%    | Scale down         |
      | Request count > 1000/minute | 1000   | Scale up           |
    And minimum instances should be 2 (high availability)
    And maximum instances should be 10 (cost control)
    And cooldown period should be 300 seconds

  # ============================================================================
  # SCENARIO 13: Health Check Configuration
  # ============================================================================
  Scenario: Configure health check endpoint for monitoring
    Given I implement /health endpoint
    When ALB performs health check
    Then /health should return 200 OK if system is healthy
    And response should include:
      | field       | example        |
      | status      | healthy        |
      | version     | 20251031.1200  |
      | database    | connected      |
      | redis_cache | connected      |
    And unhealthy instances should be replaced automatically

  # ============================================================================
  # SCENARIO 14: CloudWatch Logging and Monitoring
  # ============================================================================
  Scenario: Stream application logs to CloudWatch Logs
    Given application uses structured logging (Serilog)
    When application logs events
    Then logs should be streamed to CloudWatch Logs
    And log groups should be organized by environment
    And retention should be 30 days for production logs
    And this enables centralized log aggregation

  Scenario: Configure CloudWatch alarms for critical metrics
    Given CloudWatch metrics are collected
    Then I should create alarms for:
      | metric                        | threshold | action              |
      | API 5xx error rate > 5%       | 5%        | Page on-call        |
      | API p95 latency > 300ms       | 300ms     | Notify team         |
      | RDS CPU > 80%                 | 80%       | Auto-scale / alert  |
      | Redis CPU > 90%               | 90%       | Critical alert      |
      | Connection pool > 80% full    | 80%       | Notify team         |
    And alarms should integrate with PagerDuty/SNS

  # ============================================================================
  # SCENARIO 15: Deployment Strategy (Rolling Update)
  # ============================================================================
  Scenario: Use rolling deployment with health checks
    Given I deploy new application version
    When Elastic Beanstalk performs rolling update
    Then new version is deployed to 25% of instances at a time
    And health checks verify new instances are healthy
    And if health checks fail, deployment is rolled back
    And this ensures zero-downtime deployment

  # ============================================================================
  # SCENARIO 16: Environment Variables vs Configuration Files
  # ============================================================================
  Scenario: Use environment-specific configuration files
    Given I have appsettings.Production.json
    When I deploy to production
    Then ASPNETCORE_ENVIRONMENT should be set to "Production"
    And appsettings.Production.json should override appsettings.json
    And environment-specific settings should be applied
    And this follows ASP.NET Core configuration hierarchy

  # ============================================================================
  # SCENARIO 17: Security Groups and Network Configuration
  # ============================================================================
  Scenario: Configure security groups for defense in depth
    Given I deploy to VPC with private subnets
    Then security groups should enforce:
      | component        | inbound_allowed                    | outbound_allowed |
      | ALB              | Internet (80, 443)                 | EC2 instances    |
      | EC2 Instances    | ALB only                           | RDS, Redis, Internet |
      | RDS              | EC2 instances only                 | None             |
      | ElastiCache      | EC2 instances only                 | None             |
    And this prevents unauthorized access

  # ============================================================================
  # SCENARIO 18: Backup and Disaster Recovery
  # ============================================================================
  Scenario: Configure automated RDS backups
    Given RDS is deployed
    Then I should enable automated backups
    And backup retention period should be 7 days
    And backup window should be during low-traffic hours (03:00-04:00 UTC)
    And Multi-AZ provides automatic failover

  Scenario: Configure ElastiCache snapshots
    Given ElastiCache is deployed
    Then I should enable automatic snapshots
    And snapshot retention should be 7 days
    And snapshot window should be during low-traffic hours (03:00-04:00 UTC)
    And this enables point-in-time recovery

  # ============================================================================
  # SCENARIO 19: Cost Optimization
  # ============================================================================
  Scenario: Use Reserved Instances for baseline capacity
    Given baseline capacity is 2 EC2 instances
    When I purchase 1-year Reserved Instances
    Then cost should be reduced by 40% for baseline capacity
    And Auto Scaling uses On-Demand instances for spikes
    And this optimizes cost while maintaining flexibility

  Scenario: Use cache.r7g instances for ElastiCache (Graviton3)
    Given I configure ElastiCache
    Then I should use cache.r7g.large (Graviton3)
    And Graviton3 provides 40% better price/performance than Intel
    And this reduces ElastiCache costs

  # ============================================================================
  # SCENARIO 20: Deployment Verification
  # ============================================================================
  Scenario: Verify deployment success with health checks and smoke tests
    Given deployment is complete
    When I verify deployment
    Then I should check:
      | verification                     | expected_result |
      | /health endpoint returns 200     | healthy         |
      | Version matches deployed version | 20251031.1200   |
      | Database connection successful   | connected       |
      | Redis cache connection successful| connected       |
      | Sample API request succeeds      | 200 OK          |
    And if any check fails, rollback deployment
    And this ensures deployment correctness
