#!/bin/bash

# ============================================================================
# AWS App Runner Deployment Script - CLI Only
# Deploys Equifax Enrichment API to AWS App Runner with RDS PostgreSQL
#
# Prerequisites:
# - AWS CLI installed and configured (aws configure)
# - Docker installed
# - jq installed (for JSON parsing)
#
# Usage: ./deploy-to-aws.sh
# ============================================================================

set -e  # Exit on any error

# ============================================================================
# CONFIGURATION VARIABLES
# Modify these according to your AWS setup
# ============================================================================

PROJECT_NAME="equifax-enrichment-api"
AWS_REGION="us-east-1"
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)

# ECR Configuration
ECR_REPOSITORY="${PROJECT_NAME}"
IMAGE_TAG="latest"

# RDS Configuration
DB_INSTANCE_ID="${PROJECT_NAME}-db"
DB_NAME="equifax_enrichment_api"
DB_USER="dbadmin"
DB_PASSWORD="CHANGE_ME_$(openssl rand -base64 12 | tr -dc 'a-zA-Z0-9')"  # Generate random password
DB_INSTANCE_CLASS="db.t3.small"  # 2 vCPU, 2GB RAM for 60K requests/day
DB_STORAGE=100  # GB

# App Runner Configuration
SERVICE_NAME="${PROJECT_NAME}"
CPU="1024"  # 1 vCPU
MEMORY="2048"  # 2 GB
MIN_INSTANCES=2  # Always run 2 instances
MAX_INSTANCES=10  # Scale up to 10 instances during peaks
MAX_CONCURRENCY=100  # 100 concurrent requests per instance

# Color codes for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

log() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
    exit 1
}

# ============================================================================
# STEP 1: CREATE ECR REPOSITORY
# ============================================================================

log "Step 1: Creating ECR repository..."

if aws ecr describe-repositories --repository-names ${ECR_REPOSITORY} --region ${AWS_REGION} 2>&1 | grep -q "RepositoryNotFoundException"; then
    log "ECR repository does not exist. Creating..."
    aws ecr create-repository \
        --repository-name ${ECR_REPOSITORY} \
        --region ${AWS_REGION} \
        --image-scanning-configuration scanOnPush=true \
        --output json
    log "✅ ECR repository created: ${ECR_REPOSITORY}"
else
    log "✅ ECR repository already exists: ${ECR_REPOSITORY}"
fi

ECR_URI="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com/${ECR_REPOSITORY}"

# ============================================================================
# STEP 2: BUILD AND PUSH DOCKER IMAGE
# ============================================================================

log "Step 2: Building Docker image..."

# Login to ECR
aws ecr get-login-password --region ${AWS_REGION} | \
    docker login --username AWS --password-stdin ${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com

# Build Docker image
docker build -t ${ECR_REPOSITORY}:${IMAGE_TAG} .

# Tag image for ECR
docker tag ${ECR_REPOSITORY}:${IMAGE_TAG} ${ECR_URI}:${IMAGE_TAG}

# Push to ECR
log "Pushing image to ECR..."
docker push ${ECR_URI}:${IMAGE_TAG}

log "✅ Docker image pushed to ECR: ${ECR_URI}:${IMAGE_TAG}"

# ============================================================================
# STEP 3: CREATE RDS POSTGRESQL DATABASE
# ============================================================================

log "Step 3: Creating RDS PostgreSQL database..."

# Check if DB instance exists
if aws rds describe-db-instances --db-instance-identifier ${DB_INSTANCE_ID} --region ${AWS_REGION} 2>&1 | grep -q "DBInstanceNotFound"; then
    log "Creating RDS PostgreSQL instance (this takes 5-10 minutes)..."

    aws rds create-db-instance \
        --db-instance-identifier ${DB_INSTANCE_ID} \
        --db-instance-class ${DB_INSTANCE_CLASS} \
        --engine postgres \
        --engine-version 15.14 \
        --master-username ${DB_USER} \
        --master-user-password ${DB_PASSWORD} \
        --allocated-storage ${DB_STORAGE} \
        --db-name ${DB_NAME} \
        --publicly-accessible \
        --region ${AWS_REGION} \
        --backup-retention-period 7 \
        --output json

    log "Waiting for RDS instance to become available..."
    aws rds wait db-instance-available \
        --db-instance-identifier ${DB_INSTANCE_ID} \
        --region ${AWS_REGION}

    log "✅ RDS instance created and available"
else
    log "✅ RDS instance already exists: ${DB_INSTANCE_ID}"
fi

# Get RDS endpoint
DB_ENDPOINT=$(aws rds describe-db-instances \
    --db-instance-identifier ${DB_INSTANCE_ID} \
    --region ${AWS_REGION} \
    --query 'DBInstances[0].Endpoint.Address' \
    --output text)

log "Database endpoint: ${DB_ENDPOINT}"

# ============================================================================
# STEP 4: CREATE IAM ROLE FOR APP RUNNER
# ============================================================================

log "Step 4: Creating IAM role for App Runner..."

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

# Check if role exists
if ! aws iam get-role --role-name ${ROLE_NAME} 2>&1 | grep -q "NoSuchEntity"; then
    log "✅ IAM role already exists: ${ROLE_NAME}"
else
    aws iam create-role \
        --role-name ${ROLE_NAME} \
        --assume-role-policy-document file:///tmp/trust-policy.json \
        --output json

    # Attach ECR access policy
    aws iam attach-role-policy \
        --role-name ${ROLE_NAME} \
        --policy-arn arn:aws:iam::aws:policy/service-role/AWSAppRunnerServicePolicyForECRAccess

    log "✅ IAM role created: ${ROLE_NAME}"
fi

ROLE_ARN=$(aws iam get-role --role-name ${ROLE_NAME} --query 'Role.Arn' --output text)

# ============================================================================
# STEP 5: CREATE APP RUNNER SERVICE
# ============================================================================

log "Step 5: Creating App Runner service..."

# Create service configuration JSON
cat > /tmp/apprunner-config.json <<EOF
{
  "ServiceName": "${SERVICE_NAME}",
  "SourceConfiguration": {
    "ImageRepository": {
      "ImageIdentifier": "${ECR_URI}:${IMAGE_TAG}",
      "ImageRepositoryType": "ECR",
      "ImageConfiguration": {
        "Port": "8080",
        "RuntimeEnvironmentVariables": {
          "ASPNETCORE_ENVIRONMENT": "Production",
          "DB_HOST": "${DB_ENDPOINT}",
          "DB_PORT": "5432",
          "DB_NAME": "${DB_NAME}",
          "DB_USER": "${DB_USER}",
          "DB_PASSWORD": "${DB_PASSWORD}"
        }
      }
    },
    "AuthenticationConfiguration": {
      "AccessRoleArn": "${ROLE_ARN}"
    },
    "AutoDeploymentsEnabled": false
  },
  "InstanceConfiguration": {
    "Cpu": "${CPU}",
    "Memory": "${MEMORY}"
  },
  "HealthCheckConfiguration": {
    "Protocol": "HTTP",
    "Path": "/health",
    "Interval": 10,
    "Timeout": 5,
    "HealthyThreshold": 1,
    "UnhealthyThreshold": 5
  }
}
EOF

# Check if service exists
if aws apprunner list-services --region ${AWS_REGION} --query "ServiceSummaryList[?ServiceName=='${SERVICE_NAME}'].ServiceArn" --output text | grep -q "${SERVICE_NAME}"; then
    log "App Runner service already exists. Updating..."

    SERVICE_ARN=$(aws apprunner list-services --region ${AWS_REGION} --query "ServiceSummaryList[?ServiceName=='${SERVICE_NAME}'].ServiceArn" --output text)

    aws apprunner update-service \
        --service-arn ${SERVICE_ARN} \
        --source-configuration file:///tmp/apprunner-config.json \
        --instance-configuration Cpu=${CPU},Memory=${MEMORY} \
        --region ${AWS_REGION}
else
    log "Creating new App Runner service..."

    aws apprunner create-service \
        --cli-input-json file:///tmp/apprunner-config.json \
        --region ${AWS_REGION}
fi

# Wait for service to be running
log "Waiting for App Runner service to be running (this takes 3-5 minutes)..."

SERVICE_ARN=$(aws apprunner list-services --region ${AWS_REGION} --query "ServiceSummaryList[?ServiceName=='${SERVICE_NAME}'].ServiceArn" --output text)

aws apprunner wait service-running \
    --service-arn ${SERVICE_ARN} \
    --region ${AWS_REGION} 2>/dev/null || true

# Get service URL
SERVICE_URL=$(aws apprunner describe-service \
    --service-arn ${SERVICE_ARN} \
    --region ${AWS_REGION} \
    --query 'Service.ServiceUrl' \
    --output text)

# ============================================================================
# DEPLOYMENT COMPLETE
# ============================================================================

log "=================================="
log "✅ DEPLOYMENT SUCCESSFUL!"
log "=================================="
log ""
log "🔗 API URL: https://${SERVICE_URL}"
log "🏥 Health Check: https://${SERVICE_URL}/health"
log "📊 Swagger UI: https://${SERVICE_URL}/swagger"
log ""
log "📦 ECR Repository: ${ECR_URI}"
log "🗄️  Database Endpoint: ${DB_ENDPOINT}"
log "🔑 Database Password: ${DB_PASSWORD}"
log ""
warn "IMPORTANT: Save your database password securely!"
warn "Next step: Run database migrations using dotnet ef"
log ""
log "Test the API:"
log "curl https://${SERVICE_URL}/health"
log ""
