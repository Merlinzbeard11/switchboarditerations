# AWS App Runner Deployment Guide

Complete CLI-based deployment guide for Equifax Enrichment API to AWS App Runner with RDS PostgreSQL.

## Architecture

- **Compute:** AWS App Runner (Container-based, auto-scaling 2-10 instances)
- **Database:** AWS RDS PostgreSQL 15.14 (db.t3.small, 100GB storage)
- **Registry:** AWS ECR (Elastic Container Registry)
- **Expected Traffic:** 60,000 requests/day (~0.7 req/sec average)
- **Estimated Cost:** $75-90/month

---

## Prerequisites

### 1. Install Required Tools

```bash
# AWS CLI
brew install awscli

# Configure AWS credentials
aws configure
# Enter: AWS Access Key ID, Secret Access Key, Region (us-east-1), Output format (json)

# Docker (if not installed)
brew install --cask docker

# jq (JSON processor)
brew install jq
```

### 2. Verify Prerequisites

```bash
# Check AWS CLI
aws --version

# Check Docker
docker --version

# Check jq
jq --version

# Verify AWS credentials
aws sts get-caller-identity
```

---

## Deployment Steps

### Quick Deploy (Single Command)

```bash
cd "/Users/merlinzbeard/projects/Viable product"
./deploy-to-aws.sh
```

**Total deployment time:** ~15-20 minutes

---

### Manual Step-by-Step Deployment

If you prefer to run steps manually:

#### Step 1: Create ECR Repository

```bash
AWS_REGION="us-east-1"
PROJECT_NAME="equifax-enrichment-api"

aws ecr create-repository \
    --repository-name ${PROJECT_NAME} \
    --region ${AWS_REGION} \
    --image-scanning-configuration scanOnPush=true
```

#### Step 2: Build and Push Docker Image

```bash
# Get AWS account ID
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
ECR_URI="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com/${PROJECT_NAME}"

# Login to ECR
aws ecr get-login-password --region ${AWS_REGION} | \
    docker login --username AWS --password-stdin ${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com

# Build Docker image
docker build -t ${PROJECT_NAME}:latest .

# Tag and push
docker tag ${PROJECT_NAME}:latest ${ECR_URI}:latest
docker push ${ECR_URI}:latest
```

#### Step 3: Create RDS PostgreSQL Database

```bash
DB_INSTANCE_ID="${PROJECT_NAME}-db"
DB_PASSWORD="CHANGE_ME_$(openssl rand -base64 12 | tr -dc 'a-zA-Z0-9')"

aws rds create-db-instance \
    --db-instance-identifier ${DB_INSTANCE_ID} \
    --db-instance-class db.t3.small \
    --engine postgres \
    --engine-version 15.14 \
    --master-username dbadmin \
    --master-user-password ${DB_PASSWORD} \
    --allocated-storage 100 \
    --db-name equifax_enrichment_api \
    --publicly-accessible \
    --region ${AWS_REGION} \
    --backup-retention-period 7

# Wait for database to be available (5-10 minutes)
aws rds wait db-instance-available \
    --db-instance-identifier ${DB_INSTANCE_ID} \
    --region ${AWS_REGION}

# Get database endpoint
DB_ENDPOINT=$(aws rds describe-db-instances \
    --db-instance-identifier ${DB_INSTANCE_ID} \
    --region ${AWS_REGION} \
    --query 'DBInstances[0].Endpoint.Address' \
    --output text)

echo "Database endpoint: ${DB_ENDPOINT}"
echo "Database password: ${DB_PASSWORD}"
```

#### Step 4: Create IAM Role for App Runner

```bash
ROLE_NAME="${PROJECT_NAME}-apprunner-role"

# Create trust policy
cat > /tmp/trust-policy.json <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "build.apprunner.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF

# Create role
aws iam create-role \
    --role-name ${ROLE_NAME} \
    --assume-role-policy-document file:///tmp/trust-policy.json

# Attach ECR access policy
aws iam attach-role-policy \
    --role-name ${ROLE_NAME} \
    --policy-arn arn:aws:iam::aws:policy/service-role/AWSAppRunnerServicePolicyForECRAccess

ROLE_ARN=$(aws iam get-role --role-name ${ROLE_NAME} --query 'Role.Arn' --output text)
```

#### Step 5: Create App Runner Service

```bash
SERVICE_NAME="${PROJECT_NAME}"

aws apprunner create-service \
    --service-name ${SERVICE_NAME} \
    --source-configuration '{
        "ImageRepository": {
            "ImageIdentifier": "'${ECR_URI}':latest",
            "ImageRepositoryType": "ECR",
            "ImageConfiguration": {
                "Port": "8080",
                "RuntimeEnvironmentVariables": {
                    "ASPNETCORE_ENVIRONMENT": "Production",
                    "DB_HOST": "'${DB_ENDPOINT}'",
                    "DB_PORT": "5432",
                    "DB_NAME": "equifax_enrichment_api",
                    "DB_USER": "dbadmin",
                    "DB_PASSWORD": "'${DB_PASSWORD}'"
                }
            }
        },
        "AuthenticationConfiguration": {
            "AccessRoleArn": "'${ROLE_ARN}'"
        },
        "AutoDeploymentsEnabled": false
    }' \
    --instance-configuration '{
        "Cpu": "1024",
        "Memory": "2048"
    }' \
    --health-check-configuration '{
        "Protocol": "HTTP",
        "Path": "/health",
        "Interval": 10,
        "Timeout": 5,
        "HealthyThreshold": 1,
        "UnhealthyThreshold": 5
    }' \
    --region ${AWS_REGION}

# Wait for service to be running (3-5 minutes)
SERVICE_ARN=$(aws apprunner list-services --region ${AWS_REGION} --query "ServiceSummaryList[?ServiceName=='${SERVICE_NAME}'].ServiceArn" --output text)

aws apprunner wait service-running \
    --service-arn ${SERVICE_ARN} \
    --region ${AWS_REGION}

# Get service URL
SERVICE_URL=$(aws apprunner describe-service \
    --service-arn ${SERVICE_ARN} \
    --region ${AWS_REGION} \
    --query 'Service.ServiceUrl' \
    --output text)

echo "✅ Service URL: https://${SERVICE_URL}"
```

---

## Post-Deployment

### 1. Run Database Migrations

```bash
# Update connection string with production database
export ConnectionStrings__DefaultConnection="Host=${DB_ENDPOINT};Port=5432;Database=equifax_enrichment_api;Username=dbadmin;Password=${DB_PASSWORD};Pooling=true;Minimum Pool Size=5;Maximum Pool Size=50"

# Run migrations
cd src/EquifaxEnrichmentAPI.Infrastructure
dotnet ef database update --startup-project ../EquifaxEnrichmentAPI.Api
```

### 2. Seed Production Data

If you need to seed initial production data:

```bash
# Connect to database and run seeder
# (Modify DatabaseSeeder.cs to use production-safe data first)
```

### 3. Test the Deployment

```bash
# Test health endpoint
curl https://${SERVICE_URL}/health

# Should return:
# {"status":"healthy","timestamp":"2025-11-01T...","service":"Equifax Enrichment API"}

# Test API endpoint (requires API key)
curl -X POST "https://${SERVICE_URL}/api/data_enhancement/lookup" \
    -H "Content-Type: application/json" \
    -H "X-API-Key: YOUR_API_KEY" \
    -d '{
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "8015551234",
        "permissible_purpose": "insurance_underwriting",
        "fields": "basic"
    }'
```

### 4. Configure Auto-Scaling (Optional)

```bash
# Create auto-scaling configuration
aws apprunner create-auto-scaling-configuration \
    --auto-scaling-configuration-name ${SERVICE_NAME}-autoscaling \
    --min-size 2 \
    --max-size 10 \
    --max-concurrency 100 \
    --region ${AWS_REGION}

# Get configuration ARN
AUTOSCALING_ARN=$(aws apprunner list-auto-scaling-configurations \
    --region ${AWS_REGION} \
    --query "AutoScalingConfigurationSummaryList[?AutoScalingConfigurationName=='${SERVICE_NAME}-autoscaling'].AutoScalingConfigurationArn" \
    --output text)

# Update service with auto-scaling
aws apprunner update-service \
    --service-arn ${SERVICE_ARN} \
    --auto-scaling-configuration-arn ${AUTOSCALING_ARN} \
    --region ${AWS_REGION}
```

---

## Monitoring

### View App Runner Logs

```bash
# Get service ARN
SERVICE_ARN=$(aws apprunner list-services --region ${AWS_REGION} --query "ServiceSummaryList[?ServiceName=='${SERVICE_NAME}'].ServiceArn" --output text)

# View logs in CloudWatch (open in browser)
aws logs tail "/aws/apprunner/${SERVICE_NAME}/service" --follow
```

### Check Service Status

```bash
aws apprunner describe-service \
    --service-arn ${SERVICE_ARN} \
    --region ${AWS_REGION} \
    --query 'Service.{Status:Status,URL:ServiceUrl,CPU:InstanceConfiguration.Cpu,Memory:InstanceConfiguration.Memory}'
```

### Database Performance

```bash
# Check RDS metrics
aws cloudwatch get-metric-statistics \
    --namespace AWS/RDS \
    --metric-name DatabaseConnections \
    --dimensions Name=DBInstanceIdentifier,Value=${DB_INSTANCE_ID} \
    --start-time $(date -u -d '1 hour ago' +%Y-%m-%dT%H:%M:%S) \
    --end-time $(date -u +%Y-%m-%dT%H:%M:%S) \
    --period 300 \
    --statistics Average \
    --region ${AWS_REGION}
```

---

## Updating the Application

### Deploy New Version

```bash
# Build new Docker image
docker build -t ${PROJECT_NAME}:latest .

# Tag and push
docker tag ${PROJECT_NAME}:latest ${ECR_URI}:latest
docker push ${ECR_URI}:latest

# Trigger App Runner deployment
aws apprunner start-deployment \
    --service-arn ${SERVICE_ARN} \
    --region ${AWS_REGION}

# Wait for deployment
aws apprunner wait service-running \
    --service-arn ${SERVICE_ARN} \
    --region ${AWS_REGION}
```

---

## Cost Breakdown

| Service | Configuration | Monthly Cost |
|---------|--------------|--------------|
| **App Runner** | 1 vCPU, 2GB RAM, 2-10 instances | ~$30-40 |
| **RDS PostgreSQL** | db.t3.small (2 vCPU, 2GB RAM, 100GB) | ~$35-40 |
| **Data Transfer** | Outbound to internet | ~$5-10 |
| **ECR Storage** | Container images | ~$1-2 |
| **TOTAL** | | **~$75-90/month** |

---

## Troubleshooting

### Health Check Failures

```bash
# Check health check configuration
aws apprunner describe-service \
    --service-arn ${SERVICE_ARN} \
    --region ${AWS_REGION} \
    --query 'Service.HealthCheckConfiguration'

# Common fixes:
# 1. Ensure /health endpoint returns 200 OK
# 2. Verify container listens on port 8080
# 3. Check container logs for startup errors
```

### Database Connection Failures

```bash
# Test database connectivity from local machine
psql -h ${DB_ENDPOINT} -U dbadmin -d equifax_enrichment_api

# Check RDS security group allows App Runner connections
# (App Runner uses AWS-managed IPs - ensure RDS is publicly accessible OR configure VPC)
```

### View Container Logs

```bash
# Stream logs in real-time
aws logs tail "/aws/apprunner/${SERVICE_NAME}/service" --follow --format short
```

---

## Cleanup (Delete Everything)

```bash
# Delete App Runner service
aws apprunner delete-service \
    --service-arn ${SERVICE_ARN} \
    --region ${AWS_REGION}

# Delete RDS instance
aws rds delete-db-instance \
    --db-instance-identifier ${DB_INSTANCE_ID} \
    --skip-final-snapshot \
    --region ${AWS_REGION}

# Delete ECR repository
aws ecr delete-repository \
    --repository-name ${PROJECT_NAME} \
    --force \
    --region ${AWS_REGION}

# Delete IAM role
aws iam detach-role-policy \
    --role-name ${ROLE_NAME} \
    --policy-arn arn:aws:iam::aws:policy/service-role/AWSAppRunnerServicePolicyForECRAccess

aws iam delete-role --role-name ${ROLE_NAME}
```

---

## Next Steps

1. ✅ Configure custom domain (optional)
2. ✅ Set up CloudWatch alarms for monitoring
3. ✅ Configure backup retention policy
4. ✅ Implement CI/CD pipeline with GitHub Actions
5. ✅ Set up multi-region failover (if needed)

---

## Support

For issues or questions:
- AWS App Runner Docs: https://docs.aws.amazon.com/apprunner/
- RDS PostgreSQL Docs: https://docs.aws.amazon.com/rds/
