# Guia de Implantação em Produção - NetRedisASide3

## 🚀 Checklist Pré-Deploy

### Segurança

- [ ] Alterar **todas** as senhas padrão no `.env`
- [ ] Gerar novo `KEYCLOAK_CLIENT_SECRET` forte
- [ ] Configurar certificados SSL/TLS válidos
- [ ] Habilitar `RequireHttpsMetadata: true` no Keycloak
- [ ] Configurar CORS apenas para domínios autorizados
- [ ] Ativar rate limiting por IP
- [ ] Remover usuários de teste (admin/user)
- [ ] Configurar firewall e security groups
- [ ] Implementar secrets manager (AWS Secrets Manager, Azure Key Vault)
- [ ] Ativar logs de auditoria

### Performance

- [ ] Configurar connection pooling adequado
- [ ] Ajustar TTL do cache Redis conforme necessidade
- [ ] Configurar índices adicionais no PostgreSQL
- [ ] Ativar compression (Brotli/Gzip)
- [ ] Configurar CDN para assets estáticos
- [ ] Implementar health checks robustos
- [ ] Configurar auto-scaling

### Monitoramento

- [ ] Configurar Application Insights / New Relic
- [ ] Implementar alertas de erro
- [ ] Configurar dashboards de métricas
- [ ] Ativar distributed tracing
- [ ] Configurar backup automático dos bancos
- [ ] Implementar disaster recovery plan

### Infraestrutura

- [ ] Configurar alta disponibilidade (HA)
- [ ] Implementar load balancer
- [ ] Configurar persistent volumes
- [ ] Planejar estratégia de backup
- [ ] Documentar runbooks de incidentes

---

## 🔧 Configuração para Produção

### 1. Atualizar appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  "AllowedHosts": "api.seudominio.com",
  "Redis": {
    "Connection": "${REDIS_CONNECTION_STRING}"
  },
  "Keycloak": {
    "Authority": "https://auth.seudominio.com/realms/netredisaside3",
    "Audience": "netredisaside3-api",
    "MetadataAddress": "https://auth.seudominio.com/realms/netredisaside3/.well-known/openid-configuration",
    "RequireHttpsMetadata": true,
    "ValidateAudience": true,
    "ValidateIssuer": true,
    "ValidateLifetime": true
  },
  "ConnectionStrings": {
    "AssuntoDb": "${ASSUNTO_DB_CONNECTION}",
    "MovimentacaoDb": "${MOVIMENTACAO_DB_CONNECTION}",
    "TipoDocumentoDb": "${TIPODOCUMENTO_DB_CONNECTION}"
  }
}
```

### 2. Dockerfile Otimizado para Produção

```dockerfile
# Multi-stage build otimizado
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copiar apenas arquivos de projeto primeiro (cache de layers)
COPY ["NetRedisASide3.csproj", "./"]
RUN dotnet restore "NetRedisASide3.csproj" \
    --runtime linux-musl-x64

# Copiar código fonte
COPY . .
RUN dotnet build "NetRedisASide3.csproj" \
    -c Release \
    -o /app/build \
    --no-restore

# Publish
FROM build AS publish
RUN dotnet publish "NetRedisASide3.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false

# Runtime final (Alpine Linux)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Instalar dependências mínimas
RUN apk add --no-cache \
    icu-libs \
    icu-data-full \
    curl \
    && rm -rf /var/cache/apk/*

# Criar usuário não-root
RUN addgroup -g 1000 appuser \
    && adduser -u 1000 -G appuser -s /bin/sh -D appuser

# Copiar aplicação
COPY --from=publish --chown=appuser:appuser /app/publish .

# Configurar permissões
USER appuser

# Expor porta
EXPOSE 8080

# Variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "NetRedisASide3.dll"]
```

### 3. Docker Compose para Produção

```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  app:
    image: netredisaside3:${VERSION:-latest}
    container_name: netredisaside3-app
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
    env_file:
      - .env.production
    ports:
      - "8080:8080"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      keycloak:
        condition: service_healthy
    networks:
      - app-network
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
        order: start-first
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3

  postgres:
    image: postgres:16-alpine
    container_name: postgres-prod
    restart: unless-stopped
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD_FILE: /run/secrets/postgres_password
      POSTGRES_DB: postgres
    secrets:
      - postgres_password
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./backups:/backups
    networks:
      - app-network
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 4G
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: redis-prod
    restart: unless-stopped
    command: >
      redis-server
      --requirepass ${REDIS_PASSWORD}
      --maxmemory 2gb
      --maxmemory-policy allkeys-lru
      --save 60 1000
      --appendonly yes
    volumes:
      - redis_data:/data
    networks:
      - app-network
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 2G
    healthcheck:
      test: ["CMD", "redis-cli", "--raw", "incr", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  nginx:
    image: nginx:alpine
    container_name: nginx-prod
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
      - nginx_logs:/var/log/nginx
    depends_on:
      - app
    networks:
      - app-network
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M

volumes:
  postgres_data:
    driver: local
  redis_data:
    driver: local
  nginx_logs:
    driver: local

secrets:
  postgres_password:
    file: ./secrets/postgres_password.txt
  redis_password:
    file: ./secrets/redis_password.txt

networks:
  app-network:
    driver: bridge
    ipam:
      config:
        - subnet: 172.20.0.0/16
```

### 4. Configuração do Nginx

```nginx
# nginx/nginx.conf
user nginx;
worker_processes auto;
error_log /var/log/nginx/error.log warn;
pid /var/run/nginx.pid;

events {
    worker_connections 4096;
    use epoll;
    multi_accept on;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                    '$status $body_bytes_sent "$http_referer" '
                    '"$http_user_agent" "$http_x_forwarded_for"';

    access_log /var/log/nginx/access.log main;

    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;
    client_max_body_size 50M;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types text/plain text/css text/xml text/javascript 
               application/json application/javascript application/xml+rss 
               application/rss+xml font/truetype font/opentype 
               application/vnd.ms-fontobject image/svg+xml;

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/s;
    limit_req_status 429;

    # Upstream para load balancing
    upstream api_backend {
        least_conn;
        server app:8080 max_fails=3 fail_timeout=30s;
        keepalive 32;
    }

    # Redirect HTTP to HTTPS
    server {
        listen 80;
        server_name api.seudominio.com;
        return 301 https://$server_name$request_uri;
    }

    # HTTPS Server
    server {
        listen 443 ssl http2;
        server_name api.seudominio.com;

        # SSL Configuration
        ssl_certificate /etc/nginx/ssl/fullchain.pem;
        ssl_certificate_key /etc/nginx/ssl/privkey.pem;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;
        ssl_prefer_server_ciphers on;
        ssl_session_cache shared:SSL:10m;
        ssl_session_timeout 10m;

        # Security Headers
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-XSS-Protection "1; mode=block" always;
        add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
        add_header Content-Security-Policy "default-src 'self'" always;

        # API Endpoints
        location /api/ {
            limit_req zone=api_limit burst=20 nodelay;
            
            proxy_pass http://api_backend;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            
            proxy_connect_timeout 60s;
            proxy_send_timeout 60s;
            proxy_read_timeout 60s;
            
            proxy_buffering off;
            proxy_cache_bypass $http_upgrade;
        }

        # Health Checks (sem rate limit)
        location /health {
            proxy_pass http://api_backend;
            access_log off;
        }

        # Swagger (apenas em desenvolvimento - comentar em produção)
        # location /swagger {
        #     proxy_pass http://api_backend;
        # }

        # Status do Nginx
        location /nginx-status {
            stub_status on;
            access_log off;
            allow 127.0.0.1;
            deny all;
        }
    }
}
```

---

## 🌐 Deploy em Cloud Providers

### AWS (ECS + RDS + ElastiCache)

#### 1. Criar infraestrutura com Terraform

```hcl
# main.tf
provider "aws" {
  region = "us-east-1"
}

# VPC
resource "aws_vpc" "main" {
  cidr_block = "10.0.0.0/16"
  enable_dns_hostnames = true
  enable_dns_support = true

  tags = {
    Name = "netredisaside3-vpc"
  }
}

# RDS PostgreSQL
resource "aws_db_instance" "postgres" {
  identifier = "netredisaside3-db"
  engine = "postgres"
  engine_version = "16.1"
  instance_class = "db.t3.medium"
  allocated_storage = 100
  storage_type = "gp3"
  
  db_name = "netredisaside3"
  username = var.db_username
  password = var.db_password
  
  multi_az = true
  backup_retention_period = 7
  backup_window = "03:00-04:00"
  
  vpc_security_group_ids = [aws_security_group.rds.id]
  db_subnet_group_name = aws_db_subnet_group.main.name
  
  skip_final_snapshot = false
  final_snapshot_identifier = "netredisaside3-final-snapshot"
  
  tags = {
    Name = "netredisaside3-postgres"
  }
}

# ElastiCache Redis
resource "aws_elasticache_cluster" "redis" {
  cluster_id = "netredisaside3-cache"
  engine = "redis"
  node_type = "cache.t3.medium"
  num_cache_nodes = 1
  parameter_group_name = "default.redis7"
  port = 6379
  
  subnet_group_name = aws_elasticache_subnet_group.main.name
  security_group_ids = [aws_security_group.redis.id]
  
  tags = {
    Name = "netredisaside3-redis"
  }
}

# ECS Cluster
resource "aws_ecs_cluster" "main" {
  name = "netredisaside3-cluster"
  
  setting {
    name = "containerInsights"
    value = "enabled"
  }
}

# ECS Task Definition
resource "aws_ecs_task_definition" "app" {
  family = "netredisaside3-app"
  network_mode = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu = "1024"
  memory = "2048"
  
  execution_role_arn = aws_iam_role.ecs_execution.arn
  task_role_arn = aws_iam_role.ecs_task.arn
  
  container_definitions = jsonencode([{
    name = "netredisaside3"
    image = "${var.ecr_repository}:${var.app_version}"
    portMappings = [{
      containerPort = 8080
      protocol = "tcp"
    }]
    environment = [
      {
        name = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
    ]
    secrets = [
      {
        name = "ConnectionStrings__AssuntoDb"
        valueFrom = aws_secretsmanager_secret.db_connection.arn
      }
    ]
    logConfiguration = {
      logDriver = "awslogs"
      options = {
        "awslogs-group" = aws_cloudwatch_log_group.app.name
        "awslogs-region" = "us-east-1"
        "awslogs-stream-prefix" = "ecs"
      }
    }
  }])
}

# Application Load Balancer
resource "aws_lb" "main" {
  name = "netredisaside3-alb"
  internal = false
  load_balancer_type = "application"
  security_groups = [aws_security_group.alb.id]
  subnets = aws_subnet.public[*].id
  
  enable_deletion_protection = true
  
  tags = {
    Name = "netredisaside3-alb"
  }
}

# ECS Service
resource "aws_ecs_service" "app" {
  name = "netredisaside3-service"
  cluster = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.app.arn
  desired_count = 3
  launch_type = "FARGATE"
  
  network_configuration {
    subnets = aws_subnet.private[*].id
    security_groups = [aws_security_group.app.id]
    assign_public_ip = false
  }
  
  load_balancer {
    target_group_arn = aws_lb_target_group.app.arn
    container_name = "netredisaside3"
    container_port = 8080
  }
  
  depends_on = [aws_lb_listener.https]
}
```

#### 2. Deploy com GitHub Actions

```yaml
# .github/workflows/deploy.yml
name: Deploy to AWS ECS

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v3
      
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1
      
      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v1
      
      - name: Build and push Docker image
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: netredisaside3
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG .
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
          docker tag $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG $ECR_REGISTRY/$ECR_REPOSITORY:latest
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:latest
      
      - name: Update ECS service
        run: |
          aws ecs update-service \
            --cluster netredisaside3-cluster \
            --service netredisaside3-service \
            --force-new-deployment
```

---

### Azure (AKS + Azure SQL + Azure Cache)

#### 1. Kubernetes Manifests

```yaml
# k8s/deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: netredisaside3
  namespace: production
spec:
  replicas: 3
  selector:
    matchLabels:
      app: netredisaside3
  template:
    metadata:
      labels:
        app: netredisaside3
    spec:
      containers:
      - name: netredisaside3
        image: yourregistry.azurecr.io/netredisaside3:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__AssuntoDb
          valueFrom:
            secretKeyRef:
              name: db-secrets
              key: connection-string
        resources:
          requests:
            memory: "1Gi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: netredisaside3-service
  namespace: production
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 8080
    protocol: TCP
  selector:
    app: netredisaside3
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: netredisaside3-hpa
  namespace: production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: netredisaside3
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

#### 2. Deploy Script

```bash
#!/bin/bash
# deploy-azure.sh

set -e

echo "🚀 Deploy NetRedisASide3 para Azure..."

# Build e push da imagem
az acr build \
  --registry yourregistry \
  --image netredisaside3:latest \
  --image netredisaside3:$(git rev-parse --short HEAD) \
  --file Dockerfile .

# Aplicar secrets
kubectl apply -f k8s/secrets.yml

# Deploy da aplicação
kubectl apply -f k8s/deployment.yml

# Aguardar rollout
kubectl rollout status deployment/netredisaside3 -n production

echo "✅ Deploy concluído com sucesso!"
```

---

## 📊 Monitoramento em Produção

### 1. Application Insights (Azure)

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Adicionar pacote
// dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### 2. Prometheus + Grafana

```yaml
# docker-compose.monitoring.yml
version: '3.8'

services:
  prometheus:
    image: prom/prometheus:latest
    container_name: prometheus
    volumes:
      - ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    ports:
      - "9090:9090"
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
    networks:
      - monitoring

  grafana:
    image: grafana/grafana:latest
    container_name: grafana
    volumes:
      - grafana_data:/var/lib/grafana
      - ./grafana/dashboards:/etc/grafana/provisioning/dashboards
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
      - GF_USERS_ALLOW_SIGN_UP=false
    ports:
      - "3000:3000"
    depends_on:
      - prometheus
    networks:
      - monitoring

volumes:
  prometheus_data:
  grafana_data:

networks:
  monitoring:
    driver: bridge
```

```yaml
# prometheus/prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'netredisaside3'
    static_configs:
      - targets: ['app:8080']
    metrics_path: '/metrics'

  - job_name: 'postgres'
    static_configs:
      - targets: ['postgres-exporter:9187']

  - job_name: 'redis'
    static_configs:
      - targets: ['redis-exporter:9121']
```

---

## 🔄 Estratégias de Deploy

### Blue-Green Deployment

```bash
#!/bin/bash
# blue-green-deploy.sh

BLUE_IMAGE="netredisaside3:blue"
GREEN_IMAGE="netredisaside3:green"
CURRENT_ENV=$(kubectl get service netredisaside3 -o jsonpath='{.spec.selector.version}')

if [ "$CURRENT_ENV" == "blue" ]; then
  NEW_ENV="green"
  NEW_IMAGE=$GREEN_IMAGE
else
  NEW_ENV="blue"
  NEW_IMAGE=$BLUE_IMAGE
fi

echo "Deploy para ambiente $NEW_ENV..."

# Deploy nova versão
kubectl set image deployment/netredisaside3-$NEW_ENV \
  netredisaside3=$NEW_IMAGE

# Aguardar rollout
kubectl rollout status deployment/netredisaside3-$NEW_ENV

# Executar smoke tests
./scripts/smoke-tests.sh $NEW_ENV

# Switch de tráfego
kubectl patch service netredisaside3 \
  -p "{\"spec\":{\"selector\":{\"version\":\"$NEW_ENV\"}}}"

echo "✅ Deploy concluído! Tráfego redirecionado para $NEW_ENV"
```

### Canary Deployment

```yaml
# k8s/canary.yml
apiVersion: v1
kind: Service
metadata:
  name: netredisaside3-canary
spec:
  selector:
    app: netredisaside3
    version: canary
  ports:
  - port: 80
    targetPort: 8080
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: netredisaside3-canary
spec:
  replicas: 1  # 10% do tráfego
  selector:
    matchLabels:
      app: netredisaside3
      version: canary
  template:
    metadata:
      labels:
        app: netredisaside3
        version: canary
    spec:
      containers:
      - name: netredisaside3
        image: yourregistry/netredisaside3:canary
```

---

## 📋 Backup e Restore

### Backup Automatizado

```bash
#!/bin/bash
# backup.sh

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backups/$DATE"

mkdir -p $BACKUP_DIR

echo "🔄 Iniciando backup..."

# Backup PostgreSQL
docker exec postgres pg_dumpall -U postgres | gzip > $BACKUP_DIR/postgres_$DATE.sql.gz

# Backup Redis
docker exec redis redis-cli -a $REDIS_PASSWORD --rdb /data/dump.rdb
docker cp redis:/data/dump.rdb $BACKUP_DIR/redis_$DATE.rdb

# Upload para S3
aws s3 cp $BACKUP_DIR s3://netredisaside3-backups/$DATE/ --recursive

# Limpar backups locais antigos (manter últimos 7 dias)
find /backups -type d -mtime +7 -exec rm -rf {} \;

echo "✅ Backup concluído: $BACKUP_DIR"
```

### Cron para Backup Diário

```cron
# Adicionar ao crontab
0 2 * * * /usr/local/bin/backup.sh >> /var/log/backup.log 2>&1
```

---

## 🚨 Plano de Disaster Recovery

### 1. Identificação de Incidente

```bash
# Verificar health de todos os serviços
curl -f https://api.seudominio.com/health || echo "❌ API DOWN"
curl -f http://keycloak:8080/health || echo "❌ Keycloak DOWN"

# Verificar logs
kubectl logs -f deployment/netredisaside3 --tail=100
```

### 2. Rollback Rápido

```bash
#!/bin/bash
# rollback.sh

echo "⚠️  Iniciando rollback..."

# Kubernetes
kubectl rollout undo deployment/netredisaside3

# Docker Compose
docker-compose pull
docker-compose up -d --force-recreate

echo "✅ Rollback concluído"
```

### 3. Restore de Backup

```bash
#!/bin/bash
# restore.sh

BACKUP_DATE=$1

if [ -z "$BACKUP_DATE" ]; then
  echo "Uso: ./restore.sh YYYYMMDD_HHMMSS"
  exit 1
fi

echo "🔄 Restaurando backup de $BACKUP_DATE..."

# Download do S3
aws s3 cp s3://netredisaside3-backups/$BACKUP_DATE/ ./restore/$BACKUP_DATE/ --recursive

# Restore PostgreSQL
gunzip < ./restore/$BACKUP_DATE/postgres_$BACKUP_DATE.sql.gz | \
  docker exec -i postgres psql -U postgres

# Restore Redis
docker cp ./restore/$BACKUP_DATE/redis_$BACKUP_DATE.rdb redis:/data/dump.rdb
docker-compose restart redis

echo "✅ Restore concluído"
```

---

## 📈 Métricas Importantes

### KPIs para Monitorar

| Métrica | Objetivo | Alerta |
|---------|----------|--------|
| Response Time (p95) | < 200ms | > 500ms |
| Error Rate | < 0.1% | > 1% |
| CPU Usage | < 70% | > 85% |
| Memory Usage | < 80% | > 90% |
| Database Connections | < 80% pool | > 90% pool |
| Cache Hit Rate | > 80% | < 60% |
| Uptime | 99.9% | < 99% |

---

## ✅ Checklist Final

- [ ] Todos os testes passando (unitários, integração, E2E)
- [ ] Documentação atualizada
- [ ] Secrets configurados
- [ ] SSL/TLS ativo
- [ ] Monitoramento configurado
- [ ] Alertas configurados
- [ ] Backup automatizado
- [ ] Disaster recovery testado
- [ ] Load testing executado
- [ ] Security scan aprovado
- [ ] Performance baseline estabelecido
- [ ] Runbooks documentados
- [ ] Equipe treinada

---

**🎯 Pronto para produção!**