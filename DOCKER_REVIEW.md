# Docker Configuration Review

## Issues Fixed

### 1. Certificate Error (CRITICAL - FIXED)

**Problem:**
- API was configured to use HTTPS in Docker with certificate at `/https/aspnetapp.pfx`
- Certificate didn't exist, causing startup failure
- Volume mount `~/.aspnet/https:/https:ro` was trying to mount non-existent certificates

**Solution:**
- Changed API to use HTTP only (`http://+:8080`) for Docker deployment
- Removed HTTPS certificate configuration
- Removed certificate volume mount
- Updated port exposure from 5071/7109 to 8080

**Rationale:**
- HTTPS is unnecessary for containerized services communicating internally
- Simplifies development environment
- Production HTTPS should be handled by reverse proxy (nginx, Traefik, etc.)
- Accessing API locally: `http://localhost:5071` (maps to container's 8080)

### 2. Port Configuration

**Changes:**
- API Dockerfile: Changed from `EXPOSE 5071 7109` to `EXPOSE 8080`
- docker-compose.yml: Changed from `5071:5071` and `7109:7109` to `5071:8080`
- Health check: Now uses `http://localhost:8080/health` instead of `5071`

**Result:**
- Consistent port usage across all services (API: 8080, Worker: 8080)
- External access via `http://localhost:5071` for API
- External access via `http://localhost:8080` for Worker

## Remaining Issues to Address

### 1. Security - Hardcoded Secrets (IMPORTANT)

**Current Issues:**
```yaml
# docker-compose.yml line 66
SECRET_KEY: 09bc0bd1c98d956279b430beb54303f69586e0d31323d88c638ce053f77c1d79  # Exposed!

# Database credentials exposed
POSTGRES_PASSWORD: postgres
RABBITMQ_DEFAULT_PASS: guest
```

**Recommendation:**
Create `.env` file (add to `.gitignore`):

```env
# Database
POSTGRES_DB=core_ledger_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password_here

# GlitchTip
GLITCHTIP_SECRET_KEY=your_secret_key_here

# RabbitMQ
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=your_rabbitmq_password_here
```

Update docker-compose.yml:
```yaml
postgres:
  environment:
    POSTGRES_DB: ${POSTGRES_DB}
    POSTGRES_USER: ${POSTGRES_USER}
    POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}

glitchtip:
  environment:
    SECRET_KEY: ${GLITCHTIP_SECRET_KEY}

rabbitmq:
  environment:
    RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER}
    RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
```

### 2. Resource Limits (RECOMMENDED)

**Current State:**
- Only postgres has CPU/memory limits
- Other services can consume unlimited resources

**Recommendation:**
Add resource limits to all services:

```yaml
api:
  deploy:
    resources:
      limits:
        cpus: '1.0'
        memory: 1G
      reservations:
        cpus: '0.5'
        memory: 512M

worker:
  deploy:
    resources:
      limits:
        cpus: '1.0'
        memory: 1G
      reservations:
        cpus: '0.5'
        memory: 512M

rabbitmq:
  deploy:
    resources:
      limits:
        cpus: '0.5'
        memory: 512M

redis:
  deploy:
    resources:
      limits:
        cpus: '0.25'
        memory: 256M
```

### 3. GlitchTip Database Configuration (ISSUE)

**Problem:**
```yaml
DATABASE_URL: postgres://postgres:postgres@core-ledger-db:5432/glitchtip_db
```

- GlitchTip is trying to use `glitchtip_db` database
- This database doesn't exist (only `core_ledger_db` is created)

**Solution Option 1:** Create separate database
```yaml
postgres:
  environment:
    POSTGRES_DB: core_ledger_db
    POSTGRES_MULTIPLE_DATABASES: glitchtip_db  # If supported by image
```

**Solution Option 2:** Use separate postgres instance for GlitchTip
```yaml
glitchtip-db:
  image: postgres:18
  container_name: glitchtip-db
  environment:
    POSTGRES_DB: glitchtip_db
    POSTGRES_USER: glitchtip
    POSTGRES_PASSWORD: ${GLITCHTIP_DB_PASSWORD}
  volumes:
    - glitchtip_data:/var/lib/postgresql/data
```

### 4. Logging Volume Permissions (POTENTIAL ISSUE)

**Current:**
```yaml
volumes:
  - ./logs:/var/tmp/coreledger
```

**Issue:**
- Container runs as user `1654` (APP_UID)
- Host `./logs` directory might not have correct permissions
- Could cause write failures

**Solution:**
Create logs directory with correct permissions before running:
```bash
mkdir -p logs
chmod 777 logs  # Or use specific UID 1654
```

### 5. PostgreSQL Volume Path (INCORRECT)

**Current:**
```yaml
volumes:
  - postgres_data:/var/lib/postgresql
```

**Should be:**
```yaml
volumes:
  - postgres_data:/var/lib/postgresql/data
```

PostgreSQL stores data in `/var/lib/postgresql/data`, not `/var/lib/postgresql`.

### 6. Development vs Production Configuration (RECOMMENDATION)

**Current:**
- Single docker-compose.yml for all environments
- BUILD_CONFIGURATION=Debug in compose file

**Recommendation:**
Create separate compose files:

- `docker-compose.yml` - Base configuration
- `docker-compose.override.yml` - Development overrides (auto-loaded)
- `docker-compose.prod.yml` - Production configuration

**docker-compose.override.yml (development):**
```yaml
version: '3.8'
services:
  api:
    build:
      args:
        BUILD_CONFIGURATION: Debug
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Serilog__MinimumLevel__Default=Debug

  worker:
    build:
      args:
        BUILD_CONFIGURATION: Debug
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

**docker-compose.prod.yml (production):**
```yaml
version: '3.8'
services:
  api:
    build:
      args:
        BUILD_CONFIGURATION: Release
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Serilog__MinimumLevel__Default=Information
    # Remove ports exposure (use reverse proxy)
```

## Docker Best Practices Checklist

- [x] Health checks configured for all services
- [x] Non-root user in containers (APP_UID=1654)
- [x] Multi-stage builds for smaller images
- [x] Minimal base images (aspnet vs sdk)
- [x] .dockerignore file (verify exists)
- [ ] Secrets in environment variables (not hardcoded)
- [ ] Resource limits on all services
- [ ] Separate development/production configs
- [ ] Named volumes for persistent data
- [x] Container restart policies
- [x] Service dependencies with health conditions
- [ ] Security scanning of images

## Quick Start After Fixes

```bash
# 1. Create environment file
cp .env.template .env
# Edit .env with your secrets

# 2. Create logs directory
mkdir -p logs
chmod 777 logs

# 3. Start services
docker-compose up -d

# 4. Check health
docker-compose ps

# 5. View logs
docker-compose logs -f api

# 6. Access services
# API: http://localhost:5071
# Swagger: http://localhost:5071/swagger
# RabbitMQ: http://localhost:15672 (guest/guest)
# GlitchTip: http://localhost:8000
```

## Accessing Services

| Service | External URL | Internal URL (from containers) |
|---------|-------------|--------------------------------|
| API | http://localhost:5071 | http://api:8080 |
| Worker | http://localhost:8080 | http://worker:8080 |
| PostgreSQL | localhost:5432 | postgres:5432 |
| RabbitMQ | localhost:5672 | rabbitmq:5672 |
| RabbitMQ UI | http://localhost:15672 | http://rabbitmq:15672 |
| Redis | localhost:6379 | redis:6379 |
| GlitchTip | http://localhost:8000 | http://glitchtip:8000 |

## Testing the Fix

```bash
# Rebuild and restart
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# Check API is healthy
curl http://localhost:5071/health

# Check Worker is healthy
curl http://localhost:8080/health

# View API logs
docker-compose logs -f api
```

## Production Deployment Recommendations

1. **HTTPS via Reverse Proxy**
   - Use nginx, Traefik, or cloud load balancer for TLS termination
   - Containers communicate via HTTP internally
   - Example: Traefik with Let's Encrypt

2. **Managed Services**
   - Use managed PostgreSQL (AWS RDS, Azure Database)
   - Use managed Redis (ElastiCache, Azure Cache)
   - Use managed message queue (AWS MQ, Azure Service Bus)

3. **Secrets Management**
   - Use Docker Secrets (Swarm) or Kubernetes Secrets
   - AWS Secrets Manager, Azure Key Vault
   - Never commit secrets to git

4. **Monitoring & Logging**
   - Centralized logging (ELK, Seq, CloudWatch)
   - APM tools (Application Insights, New Relic)
   - Container monitoring (Prometheus, Grafana)

5. **CI/CD Pipeline**
   - Build images in CI pipeline
   - Tag with git commit SHA or version
   - Push to container registry (Docker Hub, ECR, ACR)
   - Deploy using orchestration (Kubernetes, ECS, Azure Container Apps)

## Additional Files Needed

### .dockerignore
```
**/.git
**/.vs
**/.vscode
**/bin
**/obj
**/*.user
**/node_modules
**/logs
**/.env
**/.env.local
**/coverage
**/.DS_Store
```

### .env.template
```
# Database
POSTGRES_DB=core_ledger_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change_me_in_production

# GlitchTip
GLITCHTIP_SECRET_KEY=change_me_to_random_string

# RabbitMQ
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=change_me_in_production

# Redis (optional authentication)
REDIS_PASSWORD=

# Auth0 (if not using mock)
AUTH0_DOMAIN=your-tenant.auth0.com
AUTH0_AUDIENCE=your-api-audience
```
