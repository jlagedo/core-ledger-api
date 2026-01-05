# Health Check Implementation

This document describes the health check system implemented for the Core Ledger Worker service.

## Overview

Both the API and Worker services now include comprehensive health check endpoints that monitor:
- **Self**: Basic application liveness
- **Database**: PostgreSQL connection and availability
- **RabbitMQ**: Message broker connection (Worker only)

## Health Check Endpoints

### API Service

**Base URL**: http://localhost:7109

| Endpoint | Purpose | Checks |
|----------|---------|--------|
| `/health` | Overall health | All checks (self, database, dependencies) |
| `/health/ready` | Readiness probe | Database and external dependencies |
| `/health/live` | Liveness probe | Self check only |

### Worker Service

**Base URL**: http://localhost:8080

| Endpoint | Purpose | Checks |
|----------|---------|--------|
| `/health` | Overall health | All checks (self, database, RabbitMQ) |
| `/health/ready` | Readiness probe | Database and RabbitMQ connectivity |
| `/health/live` | Liveness probe | Self check only |

## Health Check Configuration

### Worker Service Changes

#### 1. Project SDK Change
Changed from `Microsoft.NET.Sdk.Worker` to `Microsoft.NET.Sdk.Web` to support HTTP endpoints:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
```

#### 2. Added NuGet Package
Added RabbitMQ health check package:

```xml
<PackageReference Include="AspNetCore.HealthChecks.RabbitMQ" Version="9.0.0" />
```

#### 3. Program.cs Updates

**Before:**
```csharp
var builder = Host.CreateApplicationBuilder(args);
// ...
var host = builder.Build();
host.Run();
```

**After:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database", tags: ["db", "sql", "postgres"])
    .AddRabbitMQ(rabbitConnectionString, name: "rabbitmq", tags: ["messaging", "rabbitmq"])
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["self"]);

var app = builder.Build();

// Map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("messaging")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("self")
});

app.Run();
```

#### 4. Dockerfile Updates

**Added curl for health checks:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
ARG APP_UID=1654
USER $APP_UID
WORKDIR /app
EXPOSE 8080
```

**Changed base image from runtime to aspnet:**
- Before: `FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base`
- After: `FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base`

#### 5. Docker Compose Configuration

**Added port exposure:**
```yaml
ports:
  - "8080:8080"  # Health check endpoint
```

**Added environment variables:**
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ASPNETCORE_URLS=http://+:8080
```

**Added health check:**
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

## Health Check Tags

Health checks are organized with tags for flexible filtering:

| Tag | Purpose | Used By |
|-----|---------|---------|
| `self` | Basic application liveness | `/health/live` endpoint |
| `db`, `sql`, `postgres` | Database connectivity | `/health/ready` endpoint |
| `messaging`, `rabbitmq` | Message broker connectivity | `/health/ready` endpoint (Worker only) |

## Health Check Responses

### Healthy Response
**Status Code**: 200 OK
```json
{
  "status": "Healthy",
  "results": {
    "self": {
      "status": "Healthy"
    },
    "database": {
      "status": "Healthy"
    },
    "rabbitmq": {
      "status": "Healthy"
    }
  }
}
```

### Unhealthy Response
**Status Code**: 503 Service Unavailable
```json
{
  "status": "Unhealthy",
  "results": {
    "self": {
      "status": "Healthy"
    },
    "database": {
      "status": "Unhealthy",
      "description": "Connection failed",
      "exception": "..."
    },
    "rabbitmq": {
      "status": "Healthy"
    }
  }
}
```

## Testing Health Checks

### Using curl

```bash
# Test all health checks
curl -i http://localhost:8080/health

# Test readiness (database + RabbitMQ)
curl -i http://localhost:8080/health/ready

# Test liveness (self check only)
curl -i http://localhost:8080/health/live
```

### Using Docker

```bash
# Check health status in Docker Compose
docker-compose ps

# View health check logs
docker inspect core-ledger-worker | jq '.[0].State.Health'

# Manual health check
docker-compose exec worker curl -f http://localhost:8080/health
```

## Kubernetes Integration

These health checks are designed to work with Kubernetes probes:

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: core-ledger-worker
spec:
  containers:
  - name: worker
    image: core-ledger-worker:latest
    livenessProbe:
      httpGet:
        path: /health/live
        port: 8080
      initialDelaySeconds: 40
      periodSeconds: 30
      timeoutSeconds: 10
      failureThreshold: 3
    readinessProbe:
      httpGet:
        path: /health/ready
        port: 8080
      initialDelaySeconds: 40
      periodSeconds: 30
      timeoutSeconds: 10
      failureThreshold: 3
```

## Startup Orchestration

Docker Compose uses health checks to ensure proper startup order:

1. **postgres** becomes healthy (PostgreSQL ready)
2. **rabbitmq** becomes healthy (RabbitMQ ready)
3. **redis** becomes healthy (Redis ready)
4. **api** starts and becomes healthy (API ready)
5. **worker** starts and becomes healthy (Worker ready)

This ensures that:
- Database migrations can run successfully
- Services don't fail due to missing dependencies
- The system starts in the correct order

## Monitoring and Alerts

Health check endpoints can be used for:

1. **Container Orchestration**: Docker, Kubernetes auto-restart unhealthy containers
2. **Load Balancers**: Remove unhealthy instances from rotation
3. **Monitoring Systems**: Prometheus, Grafana, DataDog integration
4. **Alerting**: Trigger alerts when services become unhealthy
5. **Debugging**: Quick diagnostic tool during development

## Troubleshooting

### Health Check Failing

1. **Check logs:**
   ```bash
   docker-compose logs worker
   ```

2. **Inspect health status:**
   ```bash
   docker inspect core-ledger-worker | jq '.[0].State.Health'
   ```

3. **Manual health check:**
   ```bash
   docker-compose exec worker curl -v http://localhost:8080/health
   ```

### Common Issues

| Issue | Possible Cause | Solution |
|-------|---------------|----------|
| Database unhealthy | PostgreSQL not ready | Check postgres container health |
| RabbitMQ unhealthy | Wrong credentials or connection | Verify RabbitMQ__* environment variables |
| 404 Not Found | Endpoints not mapped | Verify Program.cs has MapHealthChecks() |
| Connection refused | Port not exposed | Check ASPNETCORE_URLS and Dockerfile EXPOSE |

## Performance Considerations

- Health checks run every 30 seconds by default
- Timeout is set to 10 seconds
- Database and RabbitMQ checks create connections, monitor frequency
- Liveness checks (`/health/live`) are lightweight and can be frequent
- Readiness checks (`/health/ready`) should be used for orchestration decisions

## Security Considerations

1. **No Authentication**: Health endpoints are public for container orchestration
2. **Minimal Information**: Only expose necessary status information
3. **Network Isolation**: In production, restrict health endpoints to internal networks
4. **Rate Limiting**: Consider rate limiting health endpoints if exposed publicly

## Next Steps

Consider adding:
- **Custom health checks** for specific business logic
- **Detailed metrics** in health responses
- **Health UI** dashboard for visualization
- **Notification integration** for health status changes
- **Redis connectivity check** in Worker
- **SignalR hub connectivity check** in API
