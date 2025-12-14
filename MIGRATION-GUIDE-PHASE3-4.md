# Migration Guide - Phase 3 & 4 (Continuation)

## Phase 3: Testing & Quality

**Duration:** 7-10 days  
**Goal:** Achieve 80%+ code coverage with comprehensive tests  
**Risk Level:** Low (adding tests, not changing functionality)

---

### Step 3.1: Create Test Projects
**Day 1 (3 hours)**

```bash
cd /Users/jlagedo/Developer/angular/core-ledger-api

# Create test projects
dotnet new xunit -n CoreLedger.UnitTests -f net10.0
dotnet new xunit -n CoreLedger.IntegrationTests -f net10.0

# Add to solution
dotnet sln add CoreLedger.UnitTests/CoreLedger.UnitTests.csproj
dotnet sln add CoreLedger.IntegrationTests/CoreLedger.IntegrationTests.csproj
```

Add testing packages (Moq, FluentAssertions, Testcontainers).

---

### Step 3.2: Write Unit Tests
**Day 2-5 (16 hours)**

Create comprehensive unit tests for:
- **Domain entities** - Test business rules, invariants, domain exceptions
- **Application use cases** - Test command/query handlers with mocked dependencies
- **Validators** - Test FluentValidation rules

Example test structure:
```csharp
public class CreateToDoCommandHandlerTests
{
    private readonly Mock<IToDoRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreateToDoCommandHandler _handler;

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateToDo()
    {
        // Arrange, Act, Assert with FluentAssertions
    }
}
```

---

### Step 3.3: Write Integration Tests
**Day 6-7 (10 hours)**

Create integration tests using:
- **Testcontainers** for real Postgres database
- **WebApplicationFactory** for API testing
- **Respawn** for database cleanup between tests

```csharp
public class ToDosControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreatedToDo()
    {
        // End-to-end API testing
    }
}
```

---

### Phase 3 Checkpoint

✅ All unit tests passing (≥80% coverage)  
✅ All integration tests passing with Testcontainers  
✅ Tests run in CI pipeline  
✅ Code coverage reports generated  

```bash
dotnet test
git commit -m "Phase 3: Testing complete"
```

---

## Phase 4: Production Readiness

**Duration:** 7-10 days  
**Goal:** Add authentication, documentation, metrics, CI/CD  
**Risk Level:** Medium

---

### Step 4.1: Add Swagger/OpenAPI
**Day 1 (4 hours)**

Install Swashbuckle and configure:
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Core Ledger API",
        Version = "v1",
        Description = "Financial Ledger REST API"
    });
    // Add JWT security definition
    // Include XML comments
});
```

**Result:** API documentation at `/api-docs`

---

### Step 4.2: Add Health Checks
**Day 1 (3 hours)**

Configure health checks for database and application:
```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddCheck("self", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");
```

---

### Step 4.3: Add Authentication (JWT)
**Day 2-3 (10 hours)**

Implement OAuth2/JWT authentication:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});
```

Store JWT secret in Key Vault:
```bash
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR-256-BIT-KEY"
```

Protect controllers:
```csharp
[Authorize]
public class ToDosController : ControllerBase
```

---

### Step 4.4: Add Metrics (Prometheus)
**Day 4 (6 hours)**

Install prometheus-net and expose metrics:
```csharp
app.UseMetricServer("/metrics");
app.UseHttpMetrics();
```

Create custom business metrics:
```csharp
public static class ApplicationMetrics
{
    public static readonly Counter TodosCreated = 
        Metrics.CreateCounter("todos_created_total", "Total todos created");
        
    public static readonly Histogram TodoCreationDuration = 
        Metrics.CreateHistogram("todo_creation_duration_seconds", "Creation time");
}
```

---

### Step 4.5: Create CI/CD Pipeline
**Day 5-6 (10 hours)**

#### GitHub Actions Workflow

Create `.github/workflows/ci-cd.yml`:
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_PASSWORD: postgres
        ports:
          - 5432:5432

    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Run tests
      run: dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage"
    
    - name: Upload coverage
      uses: codecov/codecov-action@v3
      with:
        files: '**/coverage.cobertura.xml'
    
    - name: Security scan
      run: dotnet list package --vulnerable --include-transitive

  build-docker:
    runs-on: ubuntu-latest
    needs: build-and-test
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: yourorg/core-ledger-api:latest
```

#### Dockerfile

Create `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CoreLedger.API/CoreLedger.API.csproj", "CoreLedger.API/"]
COPY ["CoreLedger.Application/CoreLedger.Application.csproj", "CoreLedger.Application/"]
COPY ["CoreLedger.Domain/CoreLedger.Domain.csproj", "CoreLedger.Domain/"]
COPY ["CoreLedger.Infrastructure/CoreLedger.Infrastructure.csproj", "CoreLedger.Infrastructure/"]

RUN dotnet restore "CoreLedger.API/CoreLedger.API.csproj"
COPY . .

WORKDIR "/src/CoreLedger.API"
RUN dotnet build "CoreLedger.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoreLedger.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "CoreLedger.API.dll"]
```

---

### Step 4.6: Add Code Quality Tools
**Day 7 (4 hours)**

#### .editorconfig

Create `.editorconfig` with C# naming conventions and style rules.

#### Enable Nullable Reference Types

Ensure all projects have:
```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

---

### Phase 4 Checkpoint

✅ Swagger documentation available  
✅ Health checks functional  
✅ JWT authentication working  
✅ Metrics exposed at `/metrics`  
✅ CI/CD pipeline running  
✅ Docker image builds successfully  
✅ Code quality enforced  

```bash
git commit -m "Phase 4: Production readiness complete"
```

---

## Rollback Procedures

### Phase 1 Rollback
If issues arise after Phase 1:
```bash
git revert HEAD
dotnet restore
dotnet build
```

### Phase 2 Rollback
If Clean Architecture causes issues:
```bash
# Switch back to old implementation
git checkout backup/pre-migration
# Or keep new structure but revert specific changes
git revert <commit-hash>
```

### Phase 3 Rollback
Tests don't affect runtime - safe to iterate.

### Phase 4 Rollback
```bash
# Disable authentication temporarily
# Comment out app.UseAuthentication() in Program.cs
# Redeploy without auth middleware
```

### Database Rollback
```bash
# Revert to previous migration
dotnet ef database update PreviousMigrationName --project CoreLedger.Infrastructure

# Or restore from backup
pg_restore -d core_ledger_db backup.sql
```

---

## Validation Checkpoints

### After Each Phase

Run this validation checklist:

#### Phase 1: Foundation
- [ ] Application starts without errors
- [ ] Logs show structured output with correlation IDs
- [ ] Exceptions return safe error messages
- [ ] HTTPS enforced
- [ ] Input validation rejects invalid data
- [ ] No secrets in source control

#### Phase 2: Architecture
- [ ] Solution builds successfully
- [ ] All 4 projects compile independently
- [ ] Database migrations apply cleanly
- [ ] All endpoints return expected responses
- [ ] Repositories isolate data access
- [ ] Controllers are thin (< 50 lines per action)

#### Phase 3: Testing
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Code coverage ≥ 80% for Domain/Application
- [ ] Tests run in < 60 seconds
- [ ] CI pipeline executes tests automatically

#### Phase 4: Production
- [ ] Swagger UI loads at /api-docs
- [ ] /health endpoint returns 200
- [ ] JWT authentication blocks unauthenticated requests
- [ ] /metrics endpoint exposes Prometheus data
- [ ] Docker image builds and runs
- [ ] CI/CD pipeline passes all stages

---

## Post-Migration Tasks

After completing all 4 phases:

### 1. Performance Testing
```bash
# Load test with k6 or Artillery
k6 run load-test.js
```

### 2. Security Audit
- Run OWASP ZAP scan
- Review dependency vulnerabilities
- Penetration testing by security team

### 3. Documentation
- [ ] Update README with new architecture
- [ ] Create API consumer guide
- [ ] Document deployment procedures
- [ ] Create runbooks for common incidents

### 4. Training
- [ ] Team walkthrough of new architecture
- [ ] Code review standards updated
- [ ] Onboarding docs updated

### 5. Monitoring Setup
- [ ] Configure Application Insights or similar
- [ ] Set up alerts for errors/latency
- [ ] Dashboard for business metrics
- [ ] Log aggregation (ELK/Splunk)

---

## Success Criteria

The migration is complete when:

✅ **Code Quality**
- 80%+ test coverage
- All CI checks passing
- No critical security vulnerabilities
- Code follows naming conventions

✅ **Architecture**
- Clean Architecture implemented
- SOLID principles followed
- Proper layer separation
- Repository pattern in place

✅ **Security**
- JWT authentication enabled
- All secrets in Key Vault/environment
- HTTPS enforced
- Input validation comprehensive

✅ **Observability**
- Structured logging throughout
- Correlation IDs in all logs
- Metrics exposed
- Health checks working

✅ **Production Ready**
- Docker image available
- CI/CD pipeline functional
- Documentation complete
- Team trained

---

## Timeline Summary

| Phase | Duration | Key Deliverables |
|-------|----------|------------------|
| Phase 1 | 5-7 days | Security, logging, validation |
| Phase 2 | 7-10 days | Clean Architecture, CQRS |
| Phase 3 | 7-10 days | Unit + integration tests |
| Phase 4 | 7-10 days | Auth, metrics, CI/CD |
| **Total** | **4-6 weeks** | Production-ready enterprise API |

---

## Support and Resources

### Documentation
- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET 10 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [EF Core Best Practices](https://docs.microsoft.com/en-us/ef/core/)

### Tools
- **Testing:** xUnit, Moq, FluentAssertions, Testcontainers
- **Logging:** Serilog
- **Metrics:** Prometheus
- **CI/CD:** GitHub Actions
- **Container:** Docker

### Team Contacts
- Architecture questions: [Your architecture team]
- Security concerns: [Your security team]
- DevOps support: [Your DevOps team]

---

## Conclusion

This migration transforms the prototype into a production-ready financial ledger API that:
- Follows enterprise architectural patterns
- Implements comprehensive security
- Provides full observability
- Has extensive test coverage
- Can be deployed via automated CI/CD

**Next Action:** Start with Phase 1, validate each checkpoint, and proceed incrementally.

**Remember:** Quality over speed. Each phase must be stable before proceeding to the next.

Good luck with your migration! 🚀
