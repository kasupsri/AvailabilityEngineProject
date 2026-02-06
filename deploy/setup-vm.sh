#!/bin/bash
set -e

echo "=========================================="
echo "Availability Engine - Proxmox VM Setup Script"
echo "=========================================="

if [ "$EUID" -ne 0 ]; then
    echo "Please run as root (use sudo)"
    exit 1
fi

echo "Updating system packages..."
apt-get update
apt-get upgrade -y

echo "Installing required packages..."
apt-get install -y \
    ca-certificates \
    curl \
    gnupg \
    lsb-release \
    wget \
    unzip \
    git

if ! command -v docker &> /dev/null; then
    echo "Installing Docker..."

    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
    chmod a+r /etc/apt/keyrings/docker.gpg

    echo \
      "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
      $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
      tee /etc/apt/sources.list.d/docker.list > /dev/null

    apt-get update
    apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

    systemctl start docker
    systemctl enable docker

    echo "Docker installed successfully"
else
    echo "Docker is already installed"
fi

if ! docker compose version &> /dev/null; then
    echo "Docker Compose plugin not found, installing standalone..."
    curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
    echo "Docker Compose installed successfully"
else
    echo "Docker Compose is already installed"
fi

echo "Skipping Cloudflared installation - using existing Cloudflare Tunnel instance"
echo "Please configure your existing tunnel to route your domain to this VM's IP:80"

echo "Creating application directory structure..."
mkdir -p /opt/availabilityengine/{data,config}
chmod 755 /opt/availabilityengine
chmod 755 /opt/availabilityengine/data
chmod 755 /opt/availabilityengine/config

echo ""
echo "=========================================="
echo "VM Setup Complete!"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. Copy your application files to /opt/availabilityengine (docker-compose.yml, deploy/, nginx/)"
echo "2. Create .env.production from deploy/.env.production.example and set VITE_API_URL to your public URL"
echo "3. Load Docker images (docker load < images.tar.gz)"
echo "4. Run deploy/deploy.sh to deploy the application"
echo "5. Point your Cloudflare Tunnel at this VM's port 80"
echo ""
