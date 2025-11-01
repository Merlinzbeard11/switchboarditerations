#!/bin/bash

# ============================================================================
# AWS ECS Fargate Deployment Script - CLI Only
# Deploys Equifax Enrichment API to AWS ECS Fargate with RDS PostgreSQL
#
# Prerequisites:
# - AWS CLI installed and configured (aws configure)
# - Docker installed
# - jq installed (for JSON parsing)
#
# Usage: ./deploy-to-ecs.sh
# ============================================================================

set -e  # Exit on any error

# ============================================================================
# CONFIGURATION VARIABLES
# ============================================================================

PROJECT_NAME="equifax-enrichment-api"
AWS_REGION="us-east-1"
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)

# ECR Configuration (already exists from App Runner deployment)
ECR_REPOSITORY="${PROJECT_NAME}"
IMAGE_TAG="latest"
ECR_URI="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com/${ECR_REPOSITORY}"

# RDS Configuration (already exists from App Runner deployment)
DB_INSTANCE_ID="${PROJECT_NAME}-db"
DB_NAME="equifax_enrichment_api"
DB_USER="dbadmin"
DB_PASSWORD="CHANGE_ME_NPFqE29Y7P4jC4X"  # From previous deployment

# ECS Configuration
CLUSTER_NAME="${PROJECT_NAME}-cluster"
SERVICE_NAME="${PROJECT_NAME}-service"
TASK_FAMILY="${PROJECT_NAME}-task"
CONTAINER_NAME="${PROJECT_NAME}-container"
CPU="512"  # 0.5 vCPU
MEMORY="1024"  # 1 GB
DESIRED_COUNT=2  # Run 2 tasks for availability

# Networking
VPC_CIDR="10.0.0.0/16"
SUBNET_1_CIDR="10.0.1.0/24"
SUBNET_2_CIDR="10.0.2.0/24"

# Load Balancer
LB_NAME="${PROJECT_NAME}-lb"
TARGET_GROUP_NAME="${PROJECT_NAME}-tg"

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
# STEP 1: GET RDS ENDPOINT
# ============================================================================

log "Step 1: Getting RDS database endpoint..."

DB_ENDPOINT=$(aws rds describe-db-instances \
    --db-instance-identifier ${DB_INSTANCE_ID} \
    --region ${AWS_REGION} \
    --query 'DBInstances[0].Endpoint.Address' \
    --output text 2>/dev/null || echo "")

if [ -z "$DB_ENDPOINT" ]; then
    error "RDS instance not found. Please ensure database is created first."
fi

log "✅ Database endpoint: ${DB_ENDPOINT}"

# ============================================================================
# STEP 2: CREATE VPC AND NETWORKING
# ============================================================================

log "Step 2: Setting up VPC and networking..."

# Check if VPC exists
VPC_ID=$(aws ec2 describe-vpcs \
    --filters "Name=tag:Name,Values=${PROJECT_NAME}-vpc" \
    --region ${AWS_REGION} \
    --query 'Vpcs[0].VpcId' \
    --output text 2>/dev/null || echo "None")

if [ "$VPC_ID" = "None" ]; then
    log "Creating VPC..."
    VPC_ID=$(aws ec2 create-vpc \
        --cidr-block ${VPC_CIDR} \
        --region ${AWS_REGION} \
        --tag-specifications "ResourceType=vpc,Tags=[{Key=Name,Value=${PROJECT_NAME}-vpc}]" \
        --query 'Vpc.VpcId' \
        --output text)

    # Enable DNS hostnames
    aws ec2 modify-vpc-attribute \
        --vpc-id ${VPC_ID} \
        --enable-dns-hostnames \
        --region ${AWS_REGION}

    log "✅ VPC created: ${VPC_ID}"
else
    log "✅ VPC already exists: ${VPC_ID}"
fi

# Create Internet Gateway
IGW_ID=$(aws ec2 describe-internet-gateways \
    --filters "Name=tag:Name,Values=${PROJECT_NAME}-igw" \
    --region ${AWS_REGION} \
    --query 'InternetGateways[0].InternetGatewayId' \
    --output text 2>/dev/null || echo "None")

if [ "$IGW_ID" = "None" ]; then
    log "Creating Internet Gateway..."
    IGW_ID=$(aws ec2 create-internet-gateway \
        --region ${AWS_REGION} \
        --tag-specifications "ResourceType=internet-gateway,Tags=[{Key=Name,Value=${PROJECT_NAME}-igw}]" \
        --query 'InternetGateway.InternetGatewayId' \
        --output text)

    aws ec2 attach-internet-gateway \
        --vpc-id ${VPC_ID} \
        --internet-gateway-id ${IGW_ID} \
        --region ${AWS_REGION}

    log "✅ Internet Gateway created: ${IGW_ID}"
else
    log "✅ Internet Gateway already exists: ${IGW_ID}"
fi

# Get availability zones
AZ_1=$(aws ec2 describe-availability-zones --region ${AWS_REGION} --query 'AvailabilityZones[0].ZoneName' --output text)
AZ_2=$(aws ec2 describe-availability-zones --region ${AWS_REGION} --query 'AvailabilityZones[1].ZoneName' --output text)

# Create Subnet 1
SUBNET_1_ID=$(aws ec2 describe-subnets \
    --filters "Name=tag:Name,Values=${PROJECT_NAME}-subnet-1" \
    --region ${AWS_REGION} \
    --query 'Subnets[0].SubnetId' \
    --output text 2>/dev/null || echo "None")

if [ "$SUBNET_1_ID" = "None" ]; then
    log "Creating Subnet 1 in ${AZ_1}..."
    SUBNET_1_ID=$(aws ec2 create-subnet \
        --vpc-id ${VPC_ID} \
        --cidr-block ${SUBNET_1_CIDR} \
        --availability-zone ${AZ_1} \
        --region ${AWS_REGION} \
        --tag-specifications "ResourceType=subnet,Tags=[{Key=Name,Value=${PROJECT_NAME}-subnet-1}]" \
        --query 'Subnet.SubnetId' \
        --output text)

    log "✅ Subnet 1 created: ${SUBNET_1_ID}"
else
    log "✅ Subnet 1 already exists: ${SUBNET_1_ID}"
fi

# Create Subnet 2
SUBNET_2_ID=$(aws ec2 describe-subnets \
    --filters "Name=tag:Name,Values=${PROJECT_NAME}-subnet-2" \
    --region ${AWS_REGION} \
    --query 'Subnets[0].SubnetId' \
    --output text 2>/dev/null || echo "None")

if [ "$SUBNET_2_ID" = "None" ]; then
    log "Creating Subnet 2 in ${AZ_2}..."
    SUBNET_2_ID=$(aws ec2 create-subnet \
        --vpc-id ${VPC_ID} \
        --cidr-block ${SUBNET_2_CIDR} \
        --availability-zone ${AZ_2} \
        --region ${AWS_REGION} \
        --tag-specifications "ResourceType=subnet,Tags=[{Key=Name,Value=${PROJECT_NAME}-subnet-2}]" \
        --query 'Subnet.SubnetId' \
        --output text)

    log "✅ Subnet 2 created: ${SUBNET_2_ID}"
else
    log "✅ Subnet 2 already exists: ${SUBNET_2_ID}"
fi

# Create Route Table
ROUTE_TABLE_ID=$(aws ec2 describe-route-tables \
    --filters "Name=tag:Name,Values=${PROJECT_NAME}-rt" \
    --region ${AWS_REGION} \
    --query 'RouteTables[0].RouteTableId' \
    --output text 2>/dev/null || echo "None")

if [ "$ROUTE_TABLE_ID" = "None" ]; then
    log "Creating Route Table..."
    ROUTE_TABLE_ID=$(aws ec2 create-route-table \
        --vpc-id ${VPC_ID} \
        --region ${AWS_REGION} \
        --tag-specifications "ResourceType=route-table,Tags=[{Key=Name,Value=${PROJECT_NAME}-rt}]" \
        --query 'RouteTable.RouteTableId' \
        --output text)

    # Create route to Internet Gateway
    aws ec2 create-route \
        --route-table-id ${ROUTE_TABLE_ID} \
        --destination-cidr-block 0.0.0.0/0 \
        --gateway-id ${IGW_ID} \
        --region ${AWS_REGION} > /dev/null

    # Associate route table with subnets
    aws ec2 associate-route-table \
        --subnet-id ${SUBNET_1_ID} \
        --route-table-id ${ROUTE_TABLE_ID} \
        --region ${AWS_REGION} > /dev/null

    aws ec2 associate-route-table \
        --subnet-id ${SUBNET_2_ID} \
        --route-table-id ${ROUTE_TABLE_ID} \
        --region ${AWS_REGION} > /dev/null

    log "✅ Route Table created: ${ROUTE_TABLE_ID}"
else
    log "✅ Route Table already exists: ${ROUTE_TABLE_ID}"
fi

# ============================================================================
# STEP 3: CREATE SECURITY GROUPS
# ============================================================================

log "Step 3: Creating security groups..."

# ALB Security Group
ALB_SG_ID=$(aws ec2 describe-security-groups \
    --filters "Name=tag:Name,Values=${PROJECT_NAME}-alb-sg" \
    --region ${AWS_REGION} \
    --query 'SecurityGroups[0].GroupId' \
    --output text 2>/dev/null || echo "None")

if [ "$ALB_SG_ID" = "None" ]; then
    log "Creating ALB security group..."
    ALB_SG_ID=$(aws ec2 create-security-group \
        --group-name "${PROJECT_NAME}-alb-sg" \
        --description "Security group for ${PROJECT_NAME} ALB" \
        --vpc-id ${VPC_ID} \
        --region ${AWS_REGION} \
        --tag-specifications "ResourceType=security-group,Tags=[{Key=Name,Value=${PROJECT_NAME}-alb-sg}]" \
        --query 'GroupId' \
        --output text)

    # Allow HTTP and HTTPS from anywhere
    aws ec2 authorize-security-group-ingress \
        --group-id ${ALB_SG_ID} \
        --protocol tcp \
        --port 80 \
        --cidr 0.0.0.0/0 \
        --region ${AWS_REGION}

    aws ec2 authorize-security-group-ingress \
        --group-id ${ALB_SG_ID} \
        --protocol tcp \
        --port 443 \
        --cidr 0.0.0.0/0 \
        --region ${AWS_REGION}

    log "✅ ALB Security Group created: ${ALB_SG_ID}"
else
    log "✅ ALB Security Group already exists: ${ALB_SG_ID}"
fi

# ECS Tasks Security Group
ECS_SG_ID=$(aws ec2 describe-security-groups \
    --filters "Name=tag:Name,Values=${PROJECT_NAME}-ecs-sg" \
    --region ${AWS_REGION} \
    --query 'SecurityGroups[0].GroupId' \
    --output text 2>/dev/null || echo "None")

if [ "$ECS_SG_ID" = "None" ]; then
    log "Creating ECS security group..."
    ECS_SG_ID=$(aws ec2 create-security-group \
        --group-name "${PROJECT_NAME}-ecs-sg" \
        --description "Security group for ${PROJECT_NAME} ECS tasks" \
        --vpc-id ${VPC_ID} \
        --region ${AWS_REGION} \
        --tag-specifications "ResourceType=security-group,Tags=[{Key=Name,Value=${PROJECT_NAME}-ecs-sg}]" \
        --query 'GroupId' \
        --output text)

    # Allow traffic from ALB on port 8080
    aws ec2 authorize-security-group-ingress \
        --group-id ${ECS_SG_ID} \
        --protocol tcp \
        --port 8080 \
        --source-group ${ALB_SG_ID} \
        --region ${AWS_REGION}

    log "✅ ECS Security Group created: ${ECS_SG_ID}"
else
    log "✅ ECS Security Group already exists: ${ECS_SG_ID}"
fi

# ============================================================================
# STEP 4: CREATE APPLICATION LOAD BALANCER
# ============================================================================

log "Step 4: Creating Application Load Balancer..."

LB_ARN=$(aws elbv2 describe-load-balancers \
    --names ${LB_NAME} \
    --region ${AWS_REGION} \
    --query 'LoadBalancers[0].LoadBalancerArn' \
    --output text 2>/dev/null || echo "None")

if [ "$LB_ARN" = "None" ]; then
    log "Creating Application Load Balancer..."
    LB_ARN=$(aws elbv2 create-load-balancer \
        --name ${LB_NAME} \
        --subnets ${SUBNET_1_ID} ${SUBNET_2_ID} \
        --security-groups ${ALB_SG_ID} \
        --region ${AWS_REGION} \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text)

    log "✅ Load Balancer created: ${LB_ARN}"
else
    log "✅ Load Balancer already exists: ${LB_ARN}"
fi

# Get LB DNS name
LB_DNS=$(aws elbv2 describe-load-balancers \
    --load-balancer-arns ${LB_ARN} \
    --region ${AWS_REGION} \
    --query 'LoadBalancers[0].DNSName' \
    --output text)

# Create Target Group
TG_ARN=$(aws elbv2 describe-target-groups \
    --names ${TARGET_GROUP_NAME} \
    --region ${AWS_REGION} \
    --query 'TargetGroups[0].TargetGroupArn' \
    --output text 2>/dev/null || echo "None")

if [ "$TG_ARN" = "None" ]; then
    log "Creating Target Group..."
    TG_ARN=$(aws elbv2 create-target-group \
        --name ${TARGET_GROUP_NAME} \
        --protocol HTTP \
        --port 8080 \
        --vpc-id ${VPC_ID} \
        --target-type ip \
        --health-check-enabled \
        --health-check-protocol HTTP \
        --health-check-path /health \
        --health-check-interval-seconds 30 \
        --health-check-timeout-seconds 5 \
        --healthy-threshold-count 2 \
        --unhealthy-threshold-count 3 \
        --region ${AWS_REGION} \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text)

    log "✅ Target Group created: ${TG_ARN}"
else
    log "✅ Target Group already exists: ${TG_ARN}"
fi

# Create Listener
LISTENER_ARN=$(aws elbv2 describe-listeners \
    --load-balancer-arn ${LB_ARN} \
    --region ${AWS_REGION} \
    --query 'Listeners[0].ListenerArn' \
    --output text 2>/dev/null || echo "None")

if [ "$LISTENER_ARN" = "None" ]; then
    log "Creating Listener..."
    LISTENER_ARN=$(aws elbv2 create-listener \
        --load-balancer-arn ${LB_ARN} \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn=${TG_ARN} \
        --region ${AWS_REGION} \
        --query 'Listeners[0].ListenerArn' \
        --output text)

    log "✅ Listener created: ${LISTENER_ARN}"
else
    log "✅ Listener already exists: ${LISTENER_ARN}"
fi

# ============================================================================
# STEP 5: CREATE ECS CLUSTER
# ============================================================================

log "Step 5: Creating ECS cluster..."

CLUSTER_ARN=$(aws ecs describe-clusters \
    --clusters ${CLUSTER_NAME} \
    --region ${AWS_REGION} \
    --query 'clusters[0].clusterArn' \
    --output text 2>/dev/null || echo "None")

if [ "$CLUSTER_ARN" = "None" ] || [ "$CLUSTER_ARN" = "" ]; then
    log "Creating ECS cluster..."
    CLUSTER_ARN=$(aws ecs create-cluster \
        --cluster-name ${CLUSTER_NAME} \
        --region ${AWS_REGION} \
        --query 'cluster.clusterArn' \
        --output text)

    log "✅ ECS Cluster created: ${CLUSTER_ARN}"
else
    log "✅ ECS Cluster already exists: ${CLUSTER_ARN}"
fi

# ============================================================================
# STEP 6: CREATE IAM ROLE FOR ECS TASKS
# ============================================================================

log "Step 6: Creating IAM roles..."

EXECUTION_ROLE_NAME="${PROJECT_NAME}-execution-role"
TASK_ROLE_NAME="${PROJECT_NAME}-task-role"

# Create execution role trust policy
cat > /tmp/ecs-execution-trust-policy.json <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "ecs-tasks.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF

# Check if execution role exists
EXECUTION_ROLE_ARN=$(aws iam get-role --role-name ${EXECUTION_ROLE_NAME} --query 'Role.Arn' --output text 2>/dev/null || echo "")

if [ -z "$EXECUTION_ROLE_ARN" ]; then
    log "Creating ECS execution role..."
    EXECUTION_ROLE_ARN=$(aws iam create-role \
        --role-name ${EXECUTION_ROLE_NAME} \
        --assume-role-policy-document file:///tmp/ecs-execution-trust-policy.json \
        --query 'Role.Arn' \
        --output text)

    # Attach AWS managed policy
    aws iam attach-role-policy \
        --role-name ${EXECUTION_ROLE_NAME} \
        --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy

    log "✅ Execution Role created: ${EXECUTION_ROLE_ARN}"
else
    log "✅ Execution Role already exists: ${EXECUTION_ROLE_ARN}"
fi

# Check if task role exists
TASK_ROLE_ARN=$(aws iam get-role --role-name ${TASK_ROLE_NAME} --query 'Role.Arn' --output text 2>/dev/null || echo "")

if [ -z "$TASK_ROLE_ARN" ]; then
    log "Creating ECS task role..."
    TASK_ROLE_ARN=$(aws iam create-role \
        --role-name ${TASK_ROLE_NAME} \
        --assume-role-policy-document file:///tmp/ecs-execution-trust-policy.json \
        --query 'Role.Arn' \
        --output text)

    log "✅ Task Role created: ${TASK_ROLE_ARN}"
else
    log "✅ Task Role already exists: ${TASK_ROLE_ARN}"
fi

# ============================================================================
# STEP 7: CREATE TASK DEFINITION
# ============================================================================

log "Step 7: Creating ECS task definition..."

cat > /tmp/task-definition.json <<EOF
{
  "family": "${TASK_FAMILY}",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "${CPU}",
  "memory": "${MEMORY}",
  "executionRoleArn": "${EXECUTION_ROLE_ARN}",
  "taskRoleArn": "${TASK_ROLE_ARN}",
  "containerDefinitions": [
    {
      "name": "${CONTAINER_NAME}",
      "image": "${ECR_URI}:${IMAGE_TAG}",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "essential": true,
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "DB_HOST",
          "value": "${DB_ENDPOINT}"
        },
        {
          "name": "DB_PORT",
          "value": "5432"
        },
        {
          "name": "DB_NAME",
          "value": "${DB_NAME}"
        },
        {
          "name": "DB_USER",
          "value": "${DB_USER}"
        },
        {
          "name": "DB_PASSWORD",
          "value": "${DB_PASSWORD}"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/${PROJECT_NAME}",
          "awslogs-region": "${AWS_REGION}",
          "awslogs-stream-prefix": "ecs",
          "awslogs-create-group": "true"
        }
      }
    }
  ]
}
EOF

aws ecs register-task-definition \
    --cli-input-json file:///tmp/task-definition.json \
    --region ${AWS_REGION} > /dev/null

log "✅ Task definition registered: ${TASK_FAMILY}"

# ============================================================================
# STEP 8: CREATE ECS SERVICE
# ============================================================================

log "Step 8: Creating ECS service..."

# Check if service exists
SERVICE_COUNT=$(aws ecs describe-services \
    --cluster ${CLUSTER_NAME} \
    --services ${SERVICE_NAME} \
    --region ${AWS_REGION} \
    --query 'length(services[?status==`ACTIVE`])' \
    --output text 2>/dev/null || echo "0")

if [ "$SERVICE_COUNT" = "0" ]; then
    log "Creating ECS service..."
    aws ecs create-service \
        --cluster ${CLUSTER_NAME} \
        --service-name ${SERVICE_NAME} \
        --task-definition ${TASK_FAMILY} \
        --desired-count ${DESIRED_COUNT} \
        --launch-type FARGATE \
        --network-configuration "awsvpcConfiguration={subnets=[${SUBNET_1_ID},${SUBNET_2_ID}],securityGroups=[${ECS_SG_ID}],assignPublicIp=ENABLED}" \
        --load-balancers "targetGroupArn=${TG_ARN},containerName=${CONTAINER_NAME},containerPort=8080" \
        --region ${AWS_REGION} > /dev/null

    log "✅ ECS Service created: ${SERVICE_NAME}"
else
    log "Updating ECS service..."
    aws ecs update-service \
        --cluster ${CLUSTER_NAME} \
        --service ${SERVICE_NAME} \
        --task-definition ${TASK_FAMILY} \
        --desired-count ${DESIRED_COUNT} \
        --region ${AWS_REGION} > /dev/null

    log "✅ ECS Service updated: ${SERVICE_NAME}"
fi

# ============================================================================
# WAIT FOR SERVICE TO STABILIZE
# ============================================================================

log "Waiting for service to become stable (this may take 2-3 minutes)..."

aws ecs wait services-stable \
    --cluster ${CLUSTER_NAME} \
    --services ${SERVICE_NAME} \
    --region ${AWS_REGION}

# ============================================================================
# DEPLOYMENT COMPLETE
# ============================================================================

log "=================================="
log "✅ DEPLOYMENT SUCCESSFUL!"
log "=================================="
log ""
log "🔗 Load Balancer URL: http://${LB_DNS}"
log "🏥 Health Check: http://${LB_DNS}/health"
log "📊 Swagger UI: http://${LB_DNS}/swagger"
log ""
log "📦 ECR Repository: ${ECR_URI}"
log "🗄️  Database Endpoint: ${DB_ENDPOINT}"
log "🔑 Database Password: ${DB_PASSWORD}"
log ""
log "📋 ECS Cluster: ${CLUSTER_NAME}"
log "📋 ECS Service: ${SERVICE_NAME}"
log "📋 Task Definition: ${TASK_FAMILY}"
log ""
log "Test the deployment:"
log "curl http://${LB_DNS}/health"
log ""
