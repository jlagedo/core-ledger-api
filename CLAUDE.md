# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Core Ledger API** is a production-ready .NET 10 REST API for fund accounting ABOR (Accounting Book of Records) designed for institutional financial clients. It implements Clean Architecture (Hexagonal Architecture) with strict separation of concerns across four layers.

This is a monolithic application (not a monorepo) with distinct projects for each architectural layer, plus a separate Worker service for background message processing via RabbitMQ.

## Essential Commands

### Running the Application

```bash
# Run API with hot reload (development)
dotnet watch run --project CoreLedger.API

# Run API (standard)
dotnet run --project CoreLedger.API

# Run Worker service
dotnet run --project CoreLedger.Worker
```

**Access Points:**
- API: https://localhost:7109
- Swagger UI: https://localhost:7109/swagger
- RabbitMQ Management: http://localhost:15672 (guest/guest)

### Testing

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test CoreLedger.UnitTests/CoreLedger.UnitTests.csproj

# Run integration tests only
dotnet test CoreLedger.IntegrationTests/CoreLedger.IntegrationTests.csproj

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run single test
dotnet test --filter "FullyQualifiedName~CoreLedger.UnitTests.Application.UseCases.GetToDoByIdQueryHandlerTests.Handle_WithValidId_ReturnsToDo"
```

### Database Migrations

```bash
# Apply migrations (always use this, never EnsureCreated())
dotnet ef database update --project CoreLedger.Infrastructure --startup-project CoreLedger.API

# Create new migration
dotnet ef migrations add MigrationName --project CoreLedger.Infrastructure --startup-project CoreLedger.API

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --project CoreLedger.Infrastructure --startup-project CoreLedger.API

# Remove last migration (if not applied)
dotnet ef migrations remove --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### Building

```bash
# Build solution
dotnet build

# Build release configuration
dotnet build --configuration Release

# Publish for production
dotnet publish CoreLedger.API/CoreLedger.API.csproj --configuration Release --output ./publish
```

## Architecture & Layer Responsibilities

### Clean Architecture Layers

```
CoreLedger.API/              → Presentation Layer
CoreLedger.Application/      → Application/Use Cases Layer
CoreLedger.Domain/           → Domain/Business Logic Layer
CoreLedger.Infrastructure/   → Data Access/External Services Layer
CoreLedger.Worker/           → Background Worker Service
```

**Dependency Rule (critical):** Dependencies point inward only. Domain has zero dependencies. Application depends only on Domain. Infrastructure and API depend on Application and Domain.

### CoreLedger.Domain (Domain Layer)

**What belongs here:**
- Business entities with rich behavior (`Entities/`)
- Value objects (immutable, no identity)
- Domain exceptions (business rule violations)
- Application interfaces (`Interfaces/` - minimal: `IApplicationDbContext` only)
- Domain enums and business logic
- Domain events (if using event sourcing)

**What doesn't belong here:**
- Infrastructure concerns (EF Core, HTTP clients, RabbitMQ)
- DTOs or API models
- MediatR handlers
- Database configurations
- Repository interfaces or implementations

**Key principle:** Domain layer has ZERO external dependencies except `IApplicationDbContext` (minimal abstraction). It represents pure business logic.

### CoreLedger.Application (Application/Use Cases Layer)

**What belongs here:**
- CQRS Commands and Queries (`UseCases/`)
- MediatR handlers for orchestrating use cases
- DTOs for API contracts (`DTOs/`)
- FluentValidation validators (`Validators/`)
- AutoMapper profiles (`Mappings/`)
- Application-level interfaces for external services
- MediatR pipeline behaviors (cross-cutting concerns)

**What doesn't belong here:**
- Controllers or HTTP-specific code
- Entity Framework configurations
- Direct database access
- Domain entity creation (use factory methods on entities)

**Key pattern:** Use cases are thin orchestration layers. Business logic belongs in Domain entities.

### CoreLedger.Infrastructure (Infrastructure Layer)

**What belongs here:**
- DbContext and entity configurations (`Persistence/`)
- EF Core migrations (`Persistence/Migrations/`)
- Query Services for complex RFC-8040 filtering operations (`Services/QueryServices/`)
- External service integrations (Auth0Service, RabbitMQ publishers)
- Database-specific configurations

**What doesn't belong here:**
- Business logic
- DTOs
- Controllers
- Repository implementations

**Critical rule:** Always use explicit migrations. Never call `EnsureCreated()` or `Migrate()` in application code.

**Data Access Pattern:** Use `IApplicationDbContext` directly in handlers instead of repositories. Query Services handle complex pagination, sorting, and filtering operations.

### CoreLedger.API (Presentation Layer)

**What belongs here:**
- Controllers exposing HTTP endpoints (`Controllers/`)
- Middleware (exception handling, correlation IDs, security headers)
- Service configuration extensions (`Extensions/`)
- Program.cs with startup configuration
- API-specific configuration classes

**What doesn't belong here:**
- Business logic
- Direct database access
- Entity creation or validation logic

**Middleware pipeline order (critical):**
```
1. HttpsRedirection → SecurityHeaders → GlobalExceptionHandler
2. Authentication → CorrelationId (enriches with user context)
3. SerilogRequestLogging → Authorization → Controllers
```

### CoreLedger.Worker (Background Worker Service)

**What belongs here:**
- RabbitMQ message consumers (`Consumers/`)
- Background processing logic
- Message handling and deserialization
- Worker service configuration

**Message flow:** API publishes messages to RabbitMQ → Worker consumes and processes → Updates database via `IApplicationDbContext`.

**Data Access:** Use `IApplicationDbContext` injected via DI for database operations. Consumers should persist changes with single `SaveChangesAsync()` call per message.

## Key Architectural Patterns

### CQRS with MediatR

All business operations use Command/Query pattern:
- **Commands** modify state (CreateToDoCommand, UpdateAccountCommand)
- **Queries** read data (GetToDoByIdQuery, GetAllFundsQuery)
- Handlers live in `CoreLedger.Application/UseCases/`

**When creating new operations:**
1. Define Command/Query class with required properties
2. Create Handler implementing `IRequestHandler<TRequest, TResponse>`
3. Add FluentValidation validator in `Validators/`
4. Add AutoMapper profile if needed
5. Create controller endpoint that sends request via `IMediator`

### Direct DbContext Usage with Query Services

Data access follows Entity Framework Core patterns:
- **Simple Operations:** Inject `IApplicationDbContext` directly in handlers
  - Single-entity queries: `_context.Set<T>().FindAsync(id)`
  - Mutations: `.Add()`, `.Update()`, `.Remove()` followed by single `SaveChangesAsync()`
  - Always use `.AsNoTracking()` for queries in read-only handlers
- **Complex Operations:** Use Query Services in Infrastructure layer
  - RFC-8040 filtering, pagination, sorting: `_queryService.GetWithQueryAsync(parameters)`
  - Query Services handle dynamic SQL generation and optimization
  - Registered in DI as `IXxxQueryService` (e.g., `IAccountQueryService`)

### Domain-Driven Design

Entities are rich with behavior:
- Use factory methods for creation (e.g., `Account.Create()`)
- Business rules enforced in entity methods
- Exceptions thrown for rule violations
- Avoid anemic domain models (getters/setters only)

### Dependency Injection

Constructor injection everywhere:
- Services registered in extension methods (`AddApplication()`, `AddInfrastructure()`)
- No service locator pattern
- Scoped lifetime for `IApplicationDbContext` and Query Services
- Example registration:
  ```csharp
  services.AddScoped<IApplicationDbContext>(provider =>
      provider.GetRequiredService<ApplicationDbContext>());
  services.AddScoped<IAccountQueryService, AccountQueryService>();
  ```

## Critical Code Standards

### Compilation & Quality

- **Nullable Reference Types:** Enabled project-wide. Always handle null cases explicitly.
- **Warnings as Errors:** All warnings must be fixed. No suppression without justification.
- **XML Documentation:** Required for all public APIs (controllers, DTOs, public methods).
- **Test Coverage:** Minimum 80% for Application and Domain layers.

### Testing Conventions

```csharp
// Test naming: MethodName_Scenario_ExpectedBehavior
[Fact]
public async Task Handle_WithValidId_ReturnsToDo()
{
    // Arrange
    var expectedToDo = ToDo.Create("Test");

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
}
```

**Test structure:**
- Use AAA pattern (Arrange, Act, Assert)
- Use NSubstitute for mocking interfaces
- One assertion concept per test

### Financial Data Handling (Critical)

**Money and decimal precision:**
```csharp
// ALWAYS use decimal with explicit precision/scale for monetary values
[Column(TypeName = "decimal(18,2)")]
public decimal Amount { get; private set; }

// NEVER use double or float for money
```

**Transaction handling:**
```csharp
// Multi-step accounting operations MUST use explicit transactions
using var transaction = await _dbContext.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Concurrency:**
- Use `[Timestamp]` or `RowVersion` for optimistic concurrency on critical accounting data
- Handle `DbUpdateConcurrencyException` gracefully

### Security & Authentication

**Auth0 JWT Bearer Authentication:**
- All API endpoints require `[Authorize]` attribute (except health checks)
- User identity extracted from JWT `sub` claim
- Development mode supports mock auth via `Auth:UseMock` configuration

**User context in logs:**
- CorrelationIdMiddleware enriches logs with UserId, UserEmail, UserName
- All logs automatically include user information via Serilog LogContext
- Never log sensitive data (passwords, tokens, PII without masking)

### Logging & Correlation IDs

**Structured logging with Serilog:**
```csharp
// Use structured logging, not string interpolation
_logger.LogInformation("Creating ToDo with description {Description}", description);

// NOT: _logger.LogInformation($"Creating ToDo with description {description}");
```

**Correlation IDs:**
- Every request gets `X-Correlation-ID` header (auto-generated or from client)
- Flows through API → RabbitMQ → Worker
- Use `LogContext.PushProperty("CorrelationId", correlationId)` for distributed tracing

**Log levels:**
- **Information:** High-level business events (transaction recorded, NAV calculated)
- **Warning:** Recoverable anomalies
- **Error:** Failures requiring attention
- **Critical:** System-level failures

## Database & Migrations

### Migration Workflow

**Always use explicit migrations:**
```bash
# 1. Create migration
dotnet ef migrations add AddNewFeature --project CoreLedger.Infrastructure --startup-project CoreLedger.API

# 2. Review generated migration code

# 3. Apply migration
dotnet ef database update --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

**Never use:**
- `context.Database.EnsureCreated()` (bypasses migrations)
- `context.Database.Migrate()` in application code (use CLI or deployment scripts)

### Entity Configuration

All entity configurations live in `CoreLedger.Infrastructure/Persistence/Configuration/`:
```csharp
public class ToDoConfiguration : IEntityTypeConfiguration<ToDo>
{
    public void Configure(EntityTypeBuilder<ToDo> builder)
    {
        builder.ToTable("todos"); // snake_case table names

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Description)
            .HasColumnName("description") // snake_case column names
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");
    }
}
```

**Naming conventions:**
- Tables: snake_case (e.g., `account_transactions`)
- Columns: snake_case (e.g., `created_at`, `is_completed`)
- Indexes: `ix_{table}_{column}` (e.g., `ix_accounts_fund_id`)

## RabbitMQ Message Processing

### Publishing Messages (API)

```csharp
// 1. Define message class in CoreLedger.Application/DTOs/Messages/
public record MyMessage(string Data, Guid CorrelationId);

// 2. Publish via IMessagePublisher
await _messagePublisher.PublishAsync(
    queueName: "my.queue",
    message: myMessage,
    correlationId: correlationId
);
```

### Consuming Messages (Worker)

```csharp
// 1. Create consumer in CoreLedger.Worker/Consumers/
public class MyMessageConsumer : IHostedService
{
    // 2. Extract correlation ID from message headers
    var correlationId = basicProperties.Headers?["X-Correlation-ID"]?.ToString();

    // 3. Set in LogContext for distributed tracing
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        _logger.LogInformation("Processing message");
        // Handle message
    }
}
```

**Message flow with correlation:**
1. API receives request with `X-Correlation-ID` (or generates one)
2. API publishes message to RabbitMQ with correlation ID in headers
3. Worker extracts correlation ID from message
4. Worker sets correlation ID in LogContext
5. All logs across API and Worker include same correlation ID for tracing

## Configuration & Secrets

### Configuration Files

- `appsettings.json` - Production defaults
- `appsettings.Development.json` - Development overrides
- `.env` - Local environment variables (gitignored)
- `.env.template` - Template for required environment variables

### Secrets Management

**Development:**
```bash
# Use User Secrets (NOT appsettings.json)
dotnet user-secrets set "Auth0:ClientSecret" "your-secret"
```

**Production:**
- Never commit secrets to source control
- Use environment variables for configuration

### Important Configuration Sections

**Database:**
- Connection string: `ConnectionStrings:DefaultConnection`
- Retry policy: `Database:MaxRetryCount`, `Database:MaxRetryDelaySeconds`

**Auth0:**
- Domain: `Auth0:Domain`
- Audience: `Auth0:Audience`
- Mock auth for development: `Auth:UseMock` (bypasses Auth0)

**RabbitMQ:**
- Connection: `RabbitMQ:Hostname`, `RabbitMQ:Port`, `RabbitMQ:Username`, `RabbitMQ:Password`
- Consumer settings: `RabbitMQ:PrefetchCount`, `RabbitMQ:QueueDurable`

**Pagination:**
- Defaults: `Pagination:DefaultPageSize`, `Pagination:MaxPageSize`

**Logging:**
- File path: `Serilog:WriteTo[0].Args.path` (default: `/var/tmp/coreledger/`)
- Retention: `Serilog:WriteTo[0].Args.retainedFileCountLimit` (default: 30 days)

## Development Workflow

### Adding a New Feature

1. **Domain First:** Create entity in `CoreLedger.Domain/Entities/` with factory methods and business logic
2. **Use Case:** Create Command/Query in `CoreLedger.Application/UseCases/` with MediatR handler
3. **DbContext Usage in Handler:** Inject `IApplicationDbContext` for data access
   - For simple queries: Use `_context.Set<T>().FindAsync(id)`, `FirstOrDefaultAsync()`, etc.
   - For complex queries with filtering/pagination: Create Query Service interface + implementation
   - For mutations: Use `.Add()`, `.Update()`, `.Remove()` with single `SaveChangesAsync()` at end
4. **Query Service (if needed):** Add `IXxxQueryService` interface in `CoreLedger.Application/Interfaces/QueryServices/`, implement in `CoreLedger.Infrastructure/Services/QueryServices/`
5. **DTO:** Add request/response DTOs in `CoreLedger.Application/DTOs/`
6. **Validation:** Add FluentValidation validator in `CoreLedger.Application/Validators/`
7. **Mapping:** Add AutoMapper profile in `CoreLedger.Application/Mappings/`
8. **Controller:** Create endpoint in `CoreLedger.API/Controllers/` that uses MediatR
9. **Migration:** Create and apply database migration
10. **Tests:** Write unit tests (Application, Domain) and integration tests

### Handler Patterns with Direct DbContext Usage

**Query Handler (read-only):**
```csharp
public class GetMyEntityByIdQueryHandler : IRequestHandler<GetMyEntityByIdQuery, MyDto>
{
    private readonly IApplicationDbContext _context;

    public GetMyEntityByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<MyDto> Handle(GetMyEntityByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.MyEntities
            .AsNoTracking() // Critical: use AsNoTracking() for queries
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        return entity == null ? throw new NotFoundException(...) : MapToDto(entity);
    }
}
```

**Command Handler (mutation):**
```csharp
public class CreateMyEntityCommandHandler : IRequestHandler<CreateMyEntityCommand, MyDto>
{
    private readonly IApplicationDbContext _context;

    public CreateMyEntityCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<MyDto> Handle(CreateMyEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = MyEntity.Create(request.Name, request.Description);

        _context.MyEntities.Add(entity); // Add entity
        await _context.SaveChangesAsync(cancellationToken); // Single SaveChangesAsync() at end

        return MapToDto(entity);
    }
}
```

**Query Handler with Filtering/Pagination:**
```csharp
public class GetMyEntitiesQueryHandler : IRequestHandler<GetMyEntitiesQuery, PagedResult<MyDto>>
{
    private readonly IMyEntityQueryService _queryService; // Use Query Service for complex queries

    public GetMyEntitiesQueryHandler(IMyEntityQueryService queryService) => _queryService = queryService;

    public async Task<PagedResult<MyDto>> Handle(GetMyEntitiesQuery request, CancellationToken cancellationToken)
    {
        var parameters = new QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter
        };

        var (entities, totalCount) = await _queryService.GetWithQueryAsync(parameters, cancellationToken);
        var dtos = entities.Select(MapToDto).ToList();

        return new PagedResult<MyDto>(dtos, totalCount, parameters.Limit, parameters.Offset);
    }
}
```

### Adding a New API Endpoint

```csharp
// 1. Create controller in CoreLedger.API/Controllers/
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class MyController : ControllerBase
{
    private readonly IMediator _mediator;

    public MyController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// XML documentation for Swagger
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetMyEntityByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

### Error Handling

**GlobalExceptionMiddleware handles:**
- `DomainException` → 400 Bad Request
- `NotFoundException` → 404 Not Found
- `ValidationException` → 400 Bad Request with errors
- `ExternalServiceException` → 503 Service Unavailable
- `UnauthorizedAccessException` → 401 Unauthorized
- All others → 500 Internal Server Error (with correlation ID)

**Never expose internal exception details to clients in production.**

## Project-Specific Domain Knowledge

### Data Integrity

- Use database constraints (unique indexes, foreign keys)
- Validate business rules in domain entities
- Use transactions for multi-step operations
- Audit all changes via audit log table
- Correlation IDs enable full request tracing
