# Docker Compose Setup

This document describes how to run the Core Ledger API using Docker Compose with development configuration and mocked authentication.

## Services

The `docker-compose.yml` file defines the following services:

- **postgres**: PostgreSQL 18 database
- **rabbitmq**: RabbitMQ message broker with management UI
- **redis**: Redis cache
- **glitchtip**: Error tracking and monitoring
- **api**: Core Ledger API (ASP.NET Core)
- **worker**: Background worker service for processing RabbitMQ messages

## Prerequisites

- Docker Desktop installed and running
- At least 4GB of available RAM

## Quick Start

### 1. Start all services

```bash
docker-compose up -d
```

This will start all services in detached mode (background).

### 2. View logs

```bash
# View all logs
docker-compose logs -f

# View API logs only
docker-compose logs -f api

# View Worker logs only
docker-compose logs -f worker
```

### 3. Run database migrations

After the API container is running, apply database migrations:

```bash
docker-compose exec api dotnet ef database update --project /src/CoreLedger.Infrastructure --startup-project /src/CoreLedger.API
```

Or from your host machine:

```bash
dotnet ef database update --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### 4. Access the services

**Ports match development configuration for seamless experience:**

- **API (HTTP)**: http://localhost:5071
- **API (HTTPS)**: https://localhost:7109
- **API Swagger UI**: https://localhost:7109/swagger
- **Worker Health Check**: http://localhost:8080/health
- **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)
- **GlitchTip**: http://localhost:8000
- **PostgreSQL**: localhost:5432 (postgres/postgres)
- **Redis**: localhost:6379

## Configuration

### Development Environment

The API and Worker services are configured to run in **Development** mode with the following settings:

- **ASPNETCORE_ENVIRONMENT**: Development
- **Auth__UseMock**: true (mocked authentication enabled - no Auth0 required)
- **Debug logging**: Enabled
- **Ports**: Match local development ports exactly (API: 7109/5071, Worker: 8080)

### Mocked Authentication

Authentication is mocked for development, which means:
- You don't need Auth0 credentials
- All endpoints that require `[Authorize]` will accept requests without JWT tokens
- User context is still available in logs and middleware

### Environment Variables

Environment variables are managed using a `.env` file following Microsoft Learn best practices.

**Setup:**
1. The `.env` file is already configured for development
2. For production or custom environments, copy `.env.example` to `.env` and update values
3. The `.env` file is gitignored for security

**Key variables (defined in `.env`):**

**Database:**
- `POSTGRES_DB`: Database name
- `POSTGRES_USER`: Database username
- `POSTGRES_PASSWORD`: Database password
- `DB_CONNECTION_STRING`: Full connection string

**Authentication:**
- `Auth__UseMock=true`: Enables mock authentication (bypasses Auth0)
- `AUTH0_DOMAIN`: Auth0 domain (for reference)
- `AUTH0_AUDIENCE`: Auth0 API audience

**RabbitMQ:**
- `RABBITMQ_HOSTNAME`: RabbitMQ host
- `RABBITMQ_USERNAME`: RabbitMQ username
- `RABBITMQ_PASSWORD`: RabbitMQ password

**Redis:**
- `REDIS_CONNECTION_STRING`: Redis connection

**Ports:**
- `API_HTTP_PORT`: API HTTP port (5071)
- `API_HTTPS_PORT`: API HTTPS port (7109)
- `WORKER_HTTP_PORT`: Worker health port (8080)

## Management Commands

### Stop all services

```bash
docker-compose down
```

### Stop and remove volumes (clears database)

```bash
docker-compose down -v
```

### Rebuild services

```bash
docker-compose build

# Or rebuild and start
docker-compose up -d --build
```

### Restart a specific service

```bash
docker-compose restart api
docker-compose restart worker
```

### Check service health

```bash
docker-compose ps
```

### Execute commands in containers

```bash
# Open shell in API container
docker-compose exec api /bin/bash

# Open shell in Worker container
docker-compose exec worker /bin/bash

# Connect to PostgreSQL
docker-compose exec postgres psql -U postgres -d core_ledger_db
```

## Debugging

### View real-time logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f worker
```

### Check container status

```bash
docker-compose ps
```

### Inspect service health

```bash
# API health check (HTTP)
curl http://localhost:5071/health
curl http://localhost:5071/health/ready  # Database + dependencies
curl http://localhost:5071/health/live   # Liveness probe

# API health check (HTTPS)
curl https://localhost:7109/health --insecure
curl https://localhost:7109/health/ready --insecure
curl https://localhost:7109/health/live --insecure

# Worker health check
curl http://localhost:8080/health
curl http://localhost:8080/health/ready  # Database + RabbitMQ
curl http://localhost:8080/health/live   # Liveness probe

# RabbitMQ status
curl http://localhost:15672/api/overview -u guest:guest
```

### Access log files

Logs are written to `./logs` directory on your host machine, mapped to `/var/tmp/coreledger` in containers.

## Troubleshooting

### Services won't start

1. Check if ports are already in use:
   ```bash
   lsof -i :7109  # API HTTP
   lsof -i :5432  # PostgreSQL
   lsof -i :5672  # RabbitMQ
   ```

2. Check Docker resources (ensure enough memory is allocated)

3. View logs for errors:
   ```bash
   docker-compose logs api
   docker-compose logs worker
   ```

### Database connection errors

1. Ensure PostgreSQL is healthy:
   ```bash
   docker-compose ps postgres
   ```

2. Check if migrations have been applied:
   ```bash
   docker-compose exec api dotnet ef migrations list --project /src/CoreLedger.Infrastructure --startup-project /src/CoreLedger.API
   ```

### RabbitMQ connection errors

1. Ensure RabbitMQ is healthy:
   ```bash
   docker-compose ps rabbitmq
   ```

2. Check RabbitMQ logs:
   ```bash
   docker-compose logs rabbitmq
   ```

3. Access RabbitMQ management UI: http://localhost:15672 (guest/guest)

### Worker not processing messages

1. Check Worker logs:
   ```bash
   docker-compose logs -f worker
   ```

2. Verify RabbitMQ queues in management UI

3. Ensure API is publishing messages successfully

## Best Practices Applied

This Docker setup follows **Microsoft Learn best practices**:

### 1. **Port Consistency**
- Container ports match local development exactly (7109, 5071, 8080)
- Eliminates confusion when switching between Docker and local development
- Format: `"HOST_PORT:CONTAINER_PORT"` where both are identical

### 2. **Environment Variable Management**
- Uses `.env` file for centralized configuration (Microsoft recommended pattern)
- `.env.example` template for team onboarding
- Sensitive values gitignored for security

### 3. **Multi-Stage Dockerfile**
- Separate build and runtime stages reduce image size
- Build artifacts not included in final image
- Optimized for both Debug and Release configurations

### 4. **Build Optimization**
- `--no-restore` flag prevents redundant package restoration
- Layer caching for faster rebuilds
- Explicit `APP_UID` for security compliance

### 5. **Health Checks**
- Kubernetes-compatible liveness and readiness probes
- Automatic container restart on failure
- Proper startup orchestration with dependencies

### 6. **ASPNETCORE_URLS Configuration**
- Explicit port binding using `https://+:7109;http://+:5071` format
- Matches Microsoft documentation examples
- Supports both HTTP and HTTPS in development

## Production Considerations

This Docker Compose setup is configured for **development only**. For production:

1. Remove `Auth__UseMock=true` and configure real Auth0 credentials
2. Use production-grade PostgreSQL with persistent volumes
3. Configure proper secrets management (Azure Key Vault, not .env files)
4. Use proper HTTPS certificates (not self-signed)
5. Configure proper resource limits and auto-scaling
6. Set up monitoring and alerting (Application Insights, Prometheus)
7. Use `BUILD_CONFIGURATION=Release` instead of `Debug`
8. Review security settings for all services
9. Use image digests (SHA256) for reproducibility
10. Implement proper logging aggregation

## Notes

- The API healthcheck endpoint is `/health`
- Both API and Worker use Debug build configuration for faster development
- Logs are persisted to `./logs` directory
- PostgreSQL data is persisted in a Docker volume `postgres_data`
- All services restart automatically unless stopped manually
