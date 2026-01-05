# HTTP/HTTPS Configuration Guide

## Summary of Changes

All configuration files have been updated to use HTTP for local development while maintaining HTTPS capability for production deployments.

## Changes Made

### 1. API Configuration

#### `CoreLedger.API/Properties/launchSettings.json`
**Before:**
- Profiles: `https-auth`, `https-noauth`
- URLs: `https://localhost:7109;http://localhost:5071`

**After:**
- Profiles: `http`, `http-auth`
- URLs: `http://localhost:5071` (HTTP only)
- Default profile `http` includes `Auth__UseMock=true` for easy development

#### `CoreLedger.API/Program.cs`
**Before:**
```csharp
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
```

**After:**
```csharp
// HTTPS and HSTS only in production (development uses HTTP only)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

**Impact:** Development mode now runs HTTP only, production automatically enables HTTPS redirection and HSTS.

### 2. Worker Configuration

#### `CoreLedger.Worker/appsettings.json`
**Before:**
```json
"ApiBaseUrl": "https://localhost:7109"
```

**After:**
```json
"ApiBaseUrl": "http://localhost:5071"
```

#### `CoreLedger.Worker/Configuration/WorkerHttpClientOptions.cs`
**Before:**
```csharp
public string ApiBaseUrl { get; set; } = "https://localhost:7109";
```

**After:**
```csharp
public string ApiBaseUrl { get; set; } = "http://localhost:5071";
```

### 3. Docker Configuration

#### `docker-compose.yml`
**Before:**
- API ports: `5071:5071`, `7109:7109`
- URLs: `https://+:7109;http://+:5071`
- Certificate configuration and volume mounts

**After:**
- API ports: `5071:8080` (single HTTP port)
- URLs: `http://+:8080`
- No certificate configuration
- Health check: `http://localhost:8080/health`

#### `CoreLedger.API/Dockerfile`
**Before:**
```dockerfile
EXPOSE 5071
EXPOSE 7109
```

**After:**
```dockerfile
EXPOSE 8080
```

### 4. Postman Collection

#### `CoreLedger.API/CoreLedger.postman_collection.json`
**Before:**
```json
"value": "https://localhost:7057"
```

**After:**
```json
"value": "http://localhost:5071"
```

## Development Usage

### Running Locally (Outside Docker)

```bash
# API (default profile uses mock auth)
dotnet run --project CoreLedger.API

# API with Auth0 authentication
dotnet run --project CoreLedger.API --launch-profile http-auth

# Worker
dotnet run --project CoreLedger.Worker
```

**Access Points:**
- API: http://localhost:5071
- Swagger: http://localhost:5071/swagger
- Worker health: http://localhost:8080/health (when running in Docker)

### Running with Docker

```bash
# Start all services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f api
docker-compose logs -f worker
```

**Access Points:**
- API: http://localhost:5071
- Swagger: http://localhost:5071/swagger
- Worker: http://localhost:8080
- RabbitMQ: http://localhost:15672
- PostgreSQL: localhost:5432
- Redis: localhost:6379

## Production Configuration

### Environment-Specific Configuration Files

For production deployments, create environment-specific configuration:

#### `appsettings.Production.json` (API)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db-server;Port=5432;Database=core_ledger_db;Username=app_user;Password=***"
  },
  "Auth0": {
    "Domain": "https://your-production-tenant.auth0.com/",
    "Audience": "https://api.your-domain.com"
  },
  "RabbitMQ": {
    "Hostname": "prod-rabbitmq-server",
    "Port": "5672",
    "Username": "prod_user",
    "Password": "***"
  },
  "Redis": {
    "ConnectionString": "prod-redis-server:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### `appsettings.Production.json` (Worker)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db-server;Port=5432;Database=core_ledger_db;Username=app_user;Password=***"
  },
  "WorkerHttpClient": {
    "ApiBaseUrl": "https://api.your-domain.com",
    "TimeoutSeconds": 30,
    "UserAgent": "CoreLedgerWorker/1.0"
  },
  "RabbitMQ": {
    "Hostname": "prod-rabbitmq-server",
    "Port": "5672",
    "Username": "prod_user",
    "Password": "***"
  },
  "Redis": {
    "ConnectionString": "prod-redis-server:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

### Production Deployment Options

#### Option 1: Reverse Proxy with HTTPS (Recommended)

Use a reverse proxy (nginx, Traefik, cloud load balancer) to handle HTTPS termination:

```
Internet (HTTPS) → Reverse Proxy (HTTPS → HTTP) → API Container (HTTP)
```

**nginx example:**
```nginx
server {
    listen 443 ssl http2;
    server_name api.your-domain.com;

    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;

    location / {
        proxy_pass http://api-container:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

**Traefik example (docker-compose):**
```yaml
services:
  traefik:
    image: traefik:v2.10
    command:
      - "--providers.docker=true"
      - "--entrypoints.web.address=:80"
      - "--entrypoints.websecure.address=:443"
      - "--certificatesresolvers.letsencrypt.acme.tlschallenge=true"
      - "--certificatesresolvers.letsencrypt.acme.email=admin@your-domain.com"
    ports:
      - "80:80"
      - "443:443"

  api:
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.api.rule=Host(`api.your-domain.com`)"
      - "traefik.http.routers.api.entrypoints=websecure"
      - "traefik.http.routers.api.tls.certresolver=letsencrypt"
```

#### Option 2: Application-Level HTTPS

If you need the application to handle HTTPS directly (not recommended for containers):

**Production launchSettings.json:**
```json
{
  "profiles": {
    "Production": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "applicationUrl": "https://+:443;http://+:80",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "ASPNETCORE_Kestrel__Certificates__Default__Path": "/app/certs/cert.pfx",
        "ASPNETCORE_Kestrel__Certificates__Default__Password": "***"
      }
    }
  }
}
```

**Note:** This approach requires:
1. Valid SSL certificate
2. Certificate mounted into container
3. Certificate password in environment variables or secrets

### Cloud Platform Deployment

#### Azure App Service
- Configure HTTPS at platform level (App Service handles SSL/TLS)
- Set `ASPNETCORE_ENVIRONMENT=Production`
- Configure connection strings in Application Settings
- Use Azure Key Vault for secrets

#### AWS ECS/Fargate
- Use Application Load Balancer (ALB) with HTTPS listener
- Containers run HTTP internally
- Configure environment variables via task definition
- Use AWS Secrets Manager or Parameter Store

#### Google Cloud Run
- HTTPS is automatic and managed by Cloud Run
- Containers listen on HTTP (Cloud Run handles HTTPS)
- Set environment variables in service configuration
- Use Secret Manager for sensitive data

### Production Docker Compose Example

```yaml
version: '3.8'

services:
  api:
    image: your-registry/core-ledger-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - Auth0__Domain=${AUTH0_DOMAIN}
      - Auth0__Audience=${AUTH0_AUDIENCE}
      - RabbitMQ__Hostname=${RABBITMQ_HOST}
      - RabbitMQ__Username=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - Redis__ConnectionString=${REDIS_CONNECTION}
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
      restart_policy:
        condition: on-failure
        max_attempts: 3

  worker:
    image: your-registry/core-ledger-worker:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - WorkerHttpClient__ApiBaseUrl=https://api.your-domain.com
      - RabbitMQ__Hostname=${RABBITMQ_HOST}
      - RabbitMQ__Username=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - Redis__ConnectionString=${REDIS_CONNECTION}
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
```

## Environment Variables for Production

Create a `.env.production` file (DO NOT commit to git):

```env
# Database
DB_CONNECTION_STRING=Host=prod-db;Port=5432;Database=core_ledger_db;Username=app;Password=***

# Auth0
AUTH0_DOMAIN=https://your-production-tenant.auth0.com/
AUTH0_AUDIENCE=https://api.your-domain.com

# RabbitMQ
RABBITMQ_HOST=prod-rabbitmq-server
RABBITMQ_USER=prod_user
RABBITMQ_PASSWORD=***

# Redis
REDIS_CONNECTION=prod-redis:6379

# Worker
WORKER_API_BASE_URL=https://api.your-domain.com
```

## Security Checklist for Production

- [ ] HTTPS enforced at reverse proxy or cloud platform level
- [ ] HSTS enabled (automatic when ASPNETCORE_ENVIRONMENT=Production)
- [ ] All secrets in environment variables or secret management service
- [ ] Auth0 configured with production tenant (Auth__UseMock=false)
- [ ] Database using strong credentials and restricted network access
- [ ] RabbitMQ using dedicated user with minimal permissions
- [ ] Redis protected with authentication if exposed
- [ ] CORS configured for production domains only
- [ ] Rate limiting configured
- [ ] Health checks configured in load balancer
- [ ] Logging configured to centralized service
- [ ] Certificate auto-renewal configured (Let's Encrypt via Traefik or cloud provider)

## Troubleshooting

### API won't start in development
**Error:** Cannot bind to address
**Solution:** Check nothing else is using port 5071
```bash
lsof -i :5071  # macOS/Linux
netstat -ano | findstr :5071  # Windows
```

### Worker can't connect to API
**Error:** Connection refused
**Solution:**
1. Verify API is running: `curl http://localhost:5071/health`
2. Check Worker configuration points to correct URL
3. Ensure Docker network is configured correctly

### HTTPS redirect loops in production
**Cause:** Reverse proxy and application both trying to handle HTTPS
**Solution:** Ensure `ASPNETCORE_ENVIRONMENT=Production` OR configure reverse proxy to set `X-Forwarded-Proto` header

### Certificate errors in Docker
**Cause:** Trying to use HTTPS in container without certificate
**Solution:** Use HTTP in containers, handle HTTPS at reverse proxy level

## Migration from HTTPS to HTTP (Development)

If you have existing development databases or configurations:

1. **Clear browser cache** - Old HTTPS redirects may be cached
2. **Update Postman collection** - Import updated collection or change base URL
3. **Update environment variables** - Remove HTTPS-related variables
4. **Restart all services** - `docker-compose down && docker-compose up -d`

## Further Reading

- [ASP.NET Core HTTPS Configuration](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl)
- [Kestrel HTTPS Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints)
- [Docker Secrets](https://docs.docker.com/engine/swarm/secrets/)
- [Traefik with Let's Encrypt](https://doc.traefik.io/traefik/https/acme/)
- [nginx SSL Configuration](https://nginx.org/en/docs/http/configuring_https_servers.html)
