#!/bin/bash
set -e

echo "=========================================="
echo "Availability Engine - Deployment Script"
echo "=========================================="

if [ "$EUID" -ne 0 ]; then
    echo "Please run as root (use sudo)"
    exit 1
fi

APP_DIR="/opt/availabilityengine"
ENV_FILE="$APP_DIR/.env.production"
COMPOSE_FILES="-f docker-compose.yml -f deploy/docker-compose.production.yml"

if [ ! -f "$ENV_FILE" ]; then
    echo "ERROR: .env.production file not found at $ENV_FILE"
    echo "Please create it from deploy/.env.production.example and configure your settings"
    exit 1
fi

set -a
source "$ENV_FILE"
set +a

if [ ! -d "$APP_DIR" ]; then
    echo "ERROR: Application directory not found at $APP_DIR"
    exit 1
fi

cd "$APP_DIR"

echo "Stopping existing services..."
docker compose $COMPOSE_FILES down 2>/dev/null || true
systemctl stop availabilityengine.service 2>/dev/null || true

IMAGE_VERSION="${IMAGE_VERSION:-latest}"
export IMAGE_VERSION

echo "Checking for required Docker images..."
API_IMAGE="availabilityengine-api:${IMAGE_VERSION}"
WEB_IMAGE="availabilityengine-web:${IMAGE_VERSION}"

if ! docker image inspect "$API_IMAGE" >/dev/null 2>&1; then
    echo "WARNING: $API_IMAGE not found, trying :latest"
    if docker image inspect "availabilityengine-api:latest" >/dev/null 2>&1; then
        docker tag "availabilityengine-api:latest" "$API_IMAGE"
        echo "  ✓ Tagged availabilityengine-api:latest as $API_IMAGE"
    else
        echo "ERROR: availabilityengine-api image not found"
        echo "Please load images first using: docker load < image.tar.gz"
        exit 1
    fi
fi

if ! docker image inspect "$WEB_IMAGE" >/dev/null 2>&1; then
    echo "WARNING: $WEB_IMAGE not found, trying :latest"
    if docker image inspect "availabilityengine-web:latest" >/dev/null 2>&1; then
        docker tag "availabilityengine-web:latest" "$WEB_IMAGE"
        echo "  ✓ Tagged availabilityengine-web:latest as $WEB_IMAGE"
    else
        echo "ERROR: availabilityengine-web image not found"
        echo "Please load images first using: docker load < image.tar.gz"
        exit 1
    fi
fi

echo "  ✓ API image: $API_IMAGE"
echo "  ✓ Web image: $WEB_IMAGE"

echo "Pulling nginx image..."
docker compose $COMPOSE_FILES pull nginx || true

echo "Starting services with pre-loaded images..."
docker compose $COMPOSE_FILES up -d

echo "Waiting for services to be healthy..."
sleep 10

echo ""
echo "Service Status:"
docker compose $COMPOSE_FILES ps

echo ""
echo "Initializing database..."
docker compose $COMPOSE_FILES --profile init run --rm dbprimer || echo "Database may already be initialized"

echo ""
echo "Installing systemd service if needed..."
cp -f "$APP_DIR/deploy/availabilityengine.service" /etc/systemd/system/ 2>/dev/null || true
echo "Enabling systemd service..."
systemctl daemon-reload
systemctl enable availabilityengine.service
systemctl start availabilityengine.service

echo ""
echo "Systemd Service Status:"
systemctl status availabilityengine.service --no-pager -l || true

echo ""
echo "=========================================="
echo "Deployment Complete!"
echo "=========================================="
echo ""
echo "Application should be running at: http://localhost:80"
echo "Health check: http://localhost:80/health"
echo ""
echo "To view logs:"
echo "  docker compose $COMPOSE_FILES logs -f"
echo ""
echo "To check service status:"
echo "  systemctl status availabilityengine.service"
echo ""
