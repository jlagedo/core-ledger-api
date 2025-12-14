# Core Ledger API - Migration Guide
## Step-by-Step Refactoring to Production-Ready Architecture

**Document Version:** 1.0  
**Last Updated:** December 14, 2025  
**Target Framework:** .NET 10  
**Estimated Timeline:** 4-6 weeks  

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Migration Strategy](#migration-strategy)
4. [Phase 1: Foundation & Security (Week 1)](#phase-1-foundation--security)
5. [Phase 2: Clean Architecture (Week 2)](#phase-2-clean-architecture)
6. [Phase 3: Testing & Quality (Week 3)](#phase-3-testing--quality)
7. [Phase 4: Production Readiness (Week 4)](#phase-4-production-readiness)
8. [Rollback Procedures](#rollback-procedures)
9. [Validation Checkpoints](#validation-checkpoints)

---

## Overview

This guide provides a step-by-step migration path from the current prototype to a production-ready financial ledger API that complies with enterprise standards for institutional clients.

**Current State:** 12% compliance  
**Target State:** 95%+ compliance with rules  

**Key Principles:**
- Incremental migration (no big-bang rewrites)
- Maintain backward compatibility where possible
- Test after each phase
- Each phase delivers working, deployable code

---

## Prerequisites

### Required Tools
```bash
# Ensure .NET 10 SDK installed
dotnet --version  # Should be 10.0.x

# Docker for local Postgres and testing
docker --version

# Git for version control
git --version
```

### Required Knowledge
- Clean Architecture / Hexagonal Architecture
- SOLID principles
- Entity Framework Core
- xUnit or NUnit testing
- OAuth2 / OpenID Connect basics

### Backup Current State
```bash
# Create backup branch
git checkout -b backup/pre-migration
git push origin backup/pre-migration

# Tag current state
git tag v0.1.0-prototype
git push origin v0.1.0-prototype

# Create migration branch
git checkout -b feature/enterprise-migration
```

---

## Migration Strategy

### Approach: Strangler Fig Pattern
We'll gradually replace the old implementation while keeping the system running:

1. **Foundation First:** Security and observability (no breaking changes)
2. **Architecture Next:** Introduce new layers alongside old code
3. **Migrate Gradually:** Move features one-by-one to new architecture
4. **Remove Old Code:** Once all features migrated, remove legacy code

### Risk Mitigation
- Feature flags for new implementations
- Parallel running of old/new code during transition
- Comprehensive testing at each checkpoint
- Easy rollback procedures documented

---

## Phase 1: Foundation & Security

**Duration:** 5-7 days  
**Goal:** Secure the application and add observability without breaking changes  
**Risk Level:** Low (additive changes only)

---

### Step 1.1: Environment Configuration & Secrets Management
**Day 1 - Morning (2 hours)**

#### 1.1.1 Create User Secrets for Development

```bash
cd /Users/jlagedo/Developer/angular/core-ledger-api

# Initialize user secrets
dotnet user-secrets init

# Add connection string to user secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=core_ledger_db;Username=postgres;Password=YOUR_DEV_PASSWORD"
```

#### 1.1.2 Update appsettings.json

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### 1.1.3 Update appsettings.Development.json

Remove the password from `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

#### 1.1.4 Create Environment Variables Template

Create `.env.template`:
```bash
# Database Configuration
DB_HOST=localhost
DB_PORT=5432
DB_NAME=core_ledger_db
DB_USER=postgres
DB_PASSWORD=<your-password>

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7001;http://localhost:5001

# Azure Key Vault (Production)
# AZURE_KEYVAULT_URI=https://your-vault.vault.azure.net/
```

#### 1.1.5 Update .gitignore

Add to `.gitignore`:
```
# Secrets
.env
.env.*
!.env.template
appsettings.*.json
!appsettings.json
```

**Checkpoint 1.1:**
```bash
# Verify secrets are not in source
git status
# Should NOT show appsettings.Development.json as modified with secrets

# Test application starts
dotnet run
# Should connect to database successfully
```

---

### Step 1.2: Add Structured Logging with Serilog
**Day 1 - Afternoon (3 hours)**

#### 1.2.1 Add Required Packages

Update `core-ledger-api.csproj`:
```xml
<ItemGroup>
  <!-- Existing packages -->
  <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
  <PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="10.0.1" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.1" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.1">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  
  <!-- NEW: Logging -->
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  <PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
  <PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
  <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
  <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
</ItemGroup>
```

```bash
dotnet restore
```

#### 1.2.2 Configure Serilog in Program.cs

Replace `Program.cs` content:
```csharp
using Microsoft.EntityFrameworkCore;
using coreledger.model;
using core_ledger_api.Infra;
using Serilog;
using Serilog.Events;

// Configure Serilog before building the app
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "CoreLedgerApi")
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/core-ledger-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Core Ledger API");

    var builder = WebApplication.CreateBuilder(args);
    
    // Use Serilog for all logging
    builder.Host.UseSerilog();
    
    // Database configuration
    builder.Services.AddDbContext<ToDoDbContext>(options =>
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"), 
            npgsqlOptions => npgsqlOptions
                .EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null)
                .CommandTimeout(30))
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
        .EnableDetailedErrors(builder.Environment.IsDevelopment());
    });
    
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddAutoMapper(typeof(Program));
    builder.Services.AddControllers();
    
    var app = builder.Build();
    
    // Ensure request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
        };
    });
    
    app.MapControllers();
    
    Log.Information("Core Ledger API started successfully");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
```

#### 1.2.3 Add Correlation ID Middleware

Create `Middleware/CorrelationIdMiddleware.cs`:
```csharp
namespace core_ledger_api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
```

Update `Program.cs` to use middleware (add after `var app = Builder.Build();`):
```csharp
app.UseCorrelationId();
app.UseSerilogRequestLogging(...);
```

#### 1.2.4 Add Logging to TodosController

Update `Controllers/TodosController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using coreledger.model;
using core_ledger_api.Infra;
using AutoMapper;
using core_ledger_api.Dtos;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ToDoDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TodosController> _logger;

    public TodosController(
        ToDoDbContext context, 
        IMapper mapper,
        ILogger<TodosController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IResult> GetAll()
    {
        _logger.LogInformation("Retrieving all ToDo items");
        
        var todos = await _context.ToDos.AsNoTracking().ToListAsync();
        var todoDtos = _mapper.Map<List<ToDoDto>>(todos);
        
        _logger.LogInformation("Retrieved {Count} ToDo items", todoDtos.Count);
        
        return TypedResults.Ok(todoDtos);
    }

    [HttpGet("{id}", Name = "GetTodoById")]
    public async Task<IResult> GetById(int id)
    {
        _logger.LogInformation("Retrieving ToDo item {TodoId}", id);
        
        var todo = await _context.ToDos.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        
        if (todo == null)
        {
            _logger.LogWarning("ToDo item {TodoId} not found", id);
            return TypedResults.NotFound();
        }
        
        var todoDto = _mapper.Map<ToDoDto>(todo);
        return TypedResults.Ok(todoDto);
    }

    [HttpPost]
    public async Task<IResult> Create(CreateToDoDto createToDoDto)
    {
        _logger.LogInformation("Creating new ToDo item with description: {Description}", 
            createToDoDto.Description);
        
        var todo = _mapper.Map<ToDo>(createToDoDto);
        _context.ToDos.Add(todo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created ToDo item {TodoId}", todo.Id);
        
        var todoDto = _mapper.Map<ToDoDto>(todo);
        return TypedResults.CreatedAtRoute(todoDto, "GetTodoById", new { id = todoDto.Id });
    }

    [HttpPut("{id}")]
    public async Task<IResult> Update(int id, UpdateToDoDto updateToDoDto)
    {
        _logger.LogInformation("Updating ToDo item {TodoId}", id);
        
        var todo = await _context.ToDos.FindAsync(id);

        if (todo == null)
        {
            _logger.LogWarning("ToDo item {TodoId} not found for update", id);
            return TypedResults.NotFound();
        }

        _mapper.Map(updateToDoDto, todo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated ToDo item {TodoId}", id);
        
        return TypedResults.NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IResult> Delete(int id)
    {
        _logger.LogInformation("Deleting ToDo item {TodoId}", id);
        
        var todo = await _context.ToDos.FindAsync(id);
        
        if (todo == null)
        {
            _logger.LogWarning("ToDo item {TodoId} not found for deletion", id);
            return TypedResults.NotFound();
        }

        _context.ToDos.Remove(todo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted ToDo item {TodoId}", id);
        
        return TypedResults.NoContent();
    }
}
```

**Checkpoint 1.2:**
```bash
# Build and run
dotnet build
dotnet run

# Test logging - check console output and logs/core-ledger-*.log
# Make API request
curl http://localhost:5001/api/todos

# Verify:
# 1. Logs show correlation ID
# 2. Structured log properties present
# 3. Request/response logged
# 4. Log file created in logs/
```

---

### Step 1.3: Global Exception Handling
**Day 2 - Morning (3 hours)**

#### 1.3.1 Create Domain Exceptions

Create `Exceptions/DomainException.cs`:
```csharp
namespace core_ledger_api.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; }
    
    public DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object id) 
        : base($"{entityName} with id {id} not found", "ERR-NOTFOUND-001")
    {
    }
}

public class ValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(Dictionary<string, string[]> errors) 
        : base("One or more validation errors occurred", "ERR-VALIDATION-001")
    {
        Errors = errors;
    }
}

public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message = "The record was modified by another user") 
        : base(message, "ERR-CONCURRENCY-001")
    {
    }
}
```

#### 1.3.2 Create Error Response Model

Create `Models/ErrorResponse.cs`:
```csharp
namespace core_ledger_api.Models;

public record ErrorResponse(
    string ErrorCode,
    string Message,
    string? CorrelationId,
    Dictionary<string, string[]>? Errors = null,
    string? TraceId = null
);
```

#### 1.3.3 Create Global Exception Middleware

Create `Middleware/GlobalExceptionMiddleware.cs`:
```csharp
using core_ledger_api.Exceptions;
using core_ledger_api.Models;
using System.Net;
using System.Text.Json;

namespace core_ledger_api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();
        var traceId = context.TraceIdentifier;

        _logger.LogError(exception, 
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, TraceId: {TraceId}",
            correlationId, traceId);

        var (statusCode, errorCode, message, errors) = exception switch
        {
            NotFoundException notFound => 
                (HttpStatusCode.NotFound, notFound.ErrorCode, notFound.Message, null),
            
            ValidationException validation => 
                (HttpStatusCode.BadRequest, validation.ErrorCode, validation.Message, validation.Errors),
            
            ConcurrencyException concurrency => 
                (HttpStatusCode.Conflict, concurrency.ErrorCode, concurrency.Message, null),
            
            DomainException domain => 
                (HttpStatusCode.BadRequest, domain.ErrorCode, domain.Message, null),
            
            _ => (HttpStatusCode.InternalServerError, "ERR-INTERNAL-001", 
                  "An internal server error occurred", null)
        };

        var response = new ErrorResponse(
            ErrorCode: errorCode,
            Message: _environment.IsDevelopment() ? message : GetSafeMessage(exception),
            CorrelationId: correlationId,
            Errors: errors,
            TraceId: _environment.IsDevelopment() ? traceId : null
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private static string GetSafeMessage(Exception exception)
    {
        return exception is DomainException 
            ? exception.Message 
            : "An error occurred while processing your request. Please contact support with the correlation ID.";
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
```

#### 1.3.4 Update Program.cs

Add exception middleware (add as FIRST middleware):
```csharp
var app = builder.Build();

// FIRST middleware - catches all exceptions
app.UseGlobalExceptionHandler();

app.UseCorrelationId();
app.UseSerilogRequestLogging(...);
// ... rest of pipeline
```

**Checkpoint 1.3:**
```bash
# Test exception handling
# 1. Request non-existent ID
curl http://localhost:5001/api/todos/99999

# Should return:
# {
#   "errorCode": "ERR-NOTFOUND-001",
#   "message": "...",
#   "correlationId": "...",
#   "traceId": "..."
# }

# 2. Check logs for exception with correlation ID
tail -f logs/core-ledger-*.log
```

---

### Step 1.4: HTTPS & Security Headers
**Day 2 - Afternoon (2 hours)**

#### 1.4.1 Configure HTTPS Redirection

Update `Program.cs`:
```csharp
var app = builder.Build();

// Security middleware
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseGlobalExceptionHandler();
app.UseCorrelationId();
// ... rest
```

#### 1.4.2 Add Security Headers Middleware

Create `Middleware/SecurityHeadersMiddleware.cs`:
```csharp
namespace core_ledger_api.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "no-referrer");
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; frame-ancestors 'none'");

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
```

Update `Program.cs`:
```csharp
app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseGlobalExceptionHandler();
// ... rest
```

**Checkpoint 1.4:**
```bash
# Test HTTPS redirect
curl -I http://localhost:5001/api/todos
# Should see 307 redirect to https://

# Test security headers
curl -I https://localhost:7001/api/todos
# Should see all security headers in response
```

---

### Step 1.5: Input Validation with FluentValidation
**Day 3 (4 hours)**

#### 1.5.1 Add FluentValidation Package

Update `core-ledger-api.csproj`:
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
```

```bash
dotnet restore
```

#### 1.5.2 Create Validators

Create `Validators/CreateToDoDtoValidator.cs`:
```csharp
using FluentValidation;
using core_ledger_api.Dtos;

namespace core_ledger_api.Validators;

public class CreateToDoDtoValidator : AbstractValidator<CreateToDoDto>
{
    public CreateToDoDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");
    }
}
```

Create `Validators/UpdateToDoDtoValidator.cs`:
```csharp
using FluentValidation;
using core_ledger_api.Dtos;

namespace core_ledger_api.Validators;

public class UpdateToDoDtoValidator : AbstractValidator<UpdateToDoDto>
{
    public UpdateToDoDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");
    }
}
```

#### 1.5.3 Register FluentValidation

Update `Program.cs`:
```csharp
using FluentValidation;
using FluentValidation.AspNetCore;

// ... after builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

#### 1.5.4 Add Validation Filter

Create `Filters/ValidationFilter.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using core_ledger_api.Exceptions;

namespace core_ledger_api.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            throw new ValidationException(errors);
        }

        await next();
    }
}
```

Register filter in `Program.cs`:
```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
```

**Checkpoint 1.5:**
```bash
# Test validation
curl -X POST https://localhost:7001/api/todos \
  -H "Content-Type: application/json" \
  -d '{"description":"","isCompleted":false}'

# Should return 400 with validation errors:
# {
#   "errorCode": "ERR-VALIDATION-001",
#   "message": "One or more validation errors occurred",
#   "errors": {
#     "Description": ["Description is required"]
#   }
# }
```

---

### Phase 1 Checkpoint

**Before proceeding to Phase 2, verify:**

✅ All secrets removed from appsettings files  
✅ Structured logging working (console + file)  
✅ Correlation IDs in all logs and responses  
✅ Global exception handling returns safe errors  
✅ HTTPS enforced  
✅ Security headers present  
✅ Input validation working  

```bash
# Run full test suite (when created in Phase 3)
dotnet test

# Commit Phase 1
git add .
git commit -m "Phase 1: Foundation & Security complete"
git push origin feature/enterprise-migration
```

---

## Phase 2: Clean Architecture

**Duration:** 7-10 days  
**Goal:** Restructure codebase into Clean Architecture layers  
**Risk Level:** Medium (requires code reorganization)

---

### Step 2.1: Create Project Structure
**Day 1 (4 hours)**

#### 2.1.1 Create Solution and Projects

```bash
cd /Users/jlagedo/Developer/angular/core-ledger-api

# Create solution file
dotnet new sln -n CoreLedger

# Create projects
dotnet new classlib -n CoreLedger.Domain -f net10.0
dotnet new classlib -n CoreLedger.Application -f net10.0
dotnet new classlib -n CoreLedger.Infrastructure -f net10.0
dotnet new webapi -n CoreLedger.API -f net10.0

# Add projects to solution
dotnet sln add CoreLedger.Domain/CoreLedger.Domain.csproj
dotnet sln add CoreLedger.Application/CoreLedger.Application.csproj
dotnet sln add CoreLedger.Infrastructure/CoreLedger.Infrastructure.csproj
dotnet sln add CoreLedger.API/CoreLedger.API.csproj

# Set up project references
cd CoreLedger.Application
dotnet add reference ../CoreLedger.Domain/CoreLedger.Domain.csproj

cd ../CoreLedger.Infrastructure
dotnet add reference ../CoreLedger.Domain/CoreLedger.Domain.csproj
dotnet add reference ../CoreLedger.Application/CoreLedger.Application.csproj

cd ../CoreLedger.API
dotnet add reference ../CoreLedger.Application/CoreLedger.Application.csproj
dotnet add reference ../CoreLedger.Infrastructure/CoreLedger.Infrastructure.csproj
```

#### 2.1.2 Configure Project Settings

Update each `.csproj` to include:
```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

#### 2.1.3 Directory Structure

Create the following structure:

```
CoreLedger.Domain/
  ├── Entities/
  ├── ValueObjects/
  ├── Enums/
  ├── Exceptions/
  └── Interfaces/

CoreLedger.Application/
  ├── UseCases/
  │   ├── ToDos/
  │   │   ├── Commands/
  │   │   └── Queries/
  ├── DTOs/
  ├── Interfaces/
  ├── Mappings/
  └── Validators/

CoreLedger.Infrastructure/
  ├── Persistence/
  │   ├── Configurations/
  │   ├── Repositories/
  │   └── Migrations/
  └── Services/

CoreLedger.API/
  ├── Controllers/
  ├── Middleware/
  ├── Filters/
  └── Extensions/
```

**Checkpoint 2.1:**
```bash
# Build solution
dotnet build

# Should compile successfully
```

---

### Step 2.2: Migrate Domain Layer
**Day 2-3 (8 hours)**

#### 2.2.1 Create Domain Entities

Create `CoreLedger.Domain/Entities/BaseEntity.cs`:
```csharp
namespace CoreLedger.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public byte[] RowVersion { get; protected set; } = Array.Empty<byte>();

    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void SetUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
```

Create `CoreLedger.Domain/Entities/ToDo.cs`:
```csharp
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

public class ToDo : BaseEntity
{
    public string Description { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private ToDo() { }

    public static ToDo Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException("Description cannot be empty");

        if (description.Length > 500)
            throw new DomainValidationException("Description cannot exceed 500 characters");

        return new ToDo
        {
            Description = description,
            IsCompleted = false
        };
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException("Description cannot be empty");

        if (description.Length > 500)
            throw new DomainValidationException("Description cannot exceed 500 characters");

        Description = description;
        SetUpdated();
    }

    public void MarkAsCompleted()
    {
        if (IsCompleted)
            throw new DomainValidationException("ToDo is already completed");

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void MarkAsIncomplete()
    {
        if (!IsCompleted)
            throw new DomainValidationException("ToDo is already incomplete");

        IsCompleted = false;
        CompletedAt = null;
        SetUpdated();
    }
}
```

#### 2.2.2 Create Domain Exceptions

Create `CoreLedger.Domain/Exceptions/DomainException.cs`:
```csharp
namespace CoreLedger.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) 
        : base(message, "ERR-DOMAIN-001")
    {
    }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id) 
        : base($"{entityName} with id {id} not found", "ERR-NOTFOUND-001")
    {
    }
}
```

#### 2.2.3 Create Repository Interfaces

Create `CoreLedger.Domain/Interfaces/IRepository.cs`:
```csharp
using CoreLedger.Domain.Entities;

namespace CoreLedger.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
```

Create `CoreLedger.Domain/Interfaces/IToDoRepository.cs`:
```csharp
using CoreLedger.Domain.Entities;

namespace CoreLedger.Domain.Interfaces;

public interface IToDoRepository : IRepository<ToDo>
{
    Task<IReadOnlyList<ToDo>> GetCompletedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ToDo>> GetIncompleteAsync(CancellationToken cancellationToken = default);
}
```

**Checkpoint 2.2:**
```bash
cd CoreLedger.Domain
dotnet build
# Should compile with no errors
```

---

### Step 2.3: Migrate Application Layer
**Day 4-5 (10 hours)**

#### 2.3.1 Create DTOs

Create `CoreLedger.Application/DTOs/ToDoDto.cs`:
```csharp
namespace CoreLedger.Application.DTOs;

public record ToDoDto(
    int Id,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record CreateToDoDto(string Description);

public record UpdateToDoDto(string Description, bool IsCompleted);
```

#### 2.3.2 Install MediatR

Update `CoreLedger.Application/CoreLedger.Application.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="MediatR" Version="12.2.0" />
  <PackageReference Include="AutoMapper" Version="12.0.1" />
  <PackageReference Include="FluentValidation" Version="11.9.0" />
</ItemGroup>
```

#### 2.3.3 Create Use Cases (Commands)

Create `CoreLedger.Application/UseCases/ToDos/Commands/CreateToDoCommand.cs`:
```csharp
using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.ToDos.Commands;

public record CreateToDoCommand(string Description) : IRequest<ToDoDto>;
```

Create `CoreLedger.Application/UseCases/ToDos/Commands/CreateToDoCommandHandler.cs`:
```csharp
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.ToDos.Commands;

public class CreateToDoCommandHandler : IRequestHandler<CreateToDoCommand, ToDoDto>
{
    private readonly IToDoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateToDoCommandHandler> _logger;

    public CreateToDoCommandHandler(
        IToDoRepository repository,
        IMapper mapper,
        ILogger<CreateToDoCommandHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ToDoDto> Handle(
        CreateToDoCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new ToDo with description: {Description}", 
            request.Description);

        var todo = ToDo.Create(request.Description);
        var created = await _repository.AddAsync(todo, cancellationToken);

        _logger.LogInformation("Created ToDo with ID: {TodoId}", created.Id);

        return _mapper.Map<ToDoDto>(created);
    }
}
```

#### 2.3.4 Create Use Cases (Queries)

Create `CoreLedger.Application/UseCases/ToDos/Queries/GetAllToDosQuery.cs`:
```csharp
using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.ToDos.Queries;

public record GetAllToDosQuery : IRequest<IReadOnlyList<ToDoDto>>;
```

Create handler similar to command handler.

#### 2.3.5 Create Mapping Profile

Create `CoreLedger.Application/Mappings/ToDoMappingProfile.cs`:
```csharp
using AutoMapper;
using CoreLedger.Domain.Entities;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.Mappings;

public class ToDoMappingProfile : Profile
{
    public ToDoMappingProfile()
    {
        CreateMap<ToDo, ToDoDto>();
    }
}
```

#### 2.3.6 Create Validators

Create `CoreLedger.Application/Validators/CreateToDoCommandValidator.cs`:
```csharp
using FluentValidation;
using CoreLedger.Application.UseCases.ToDos.Commands;

namespace CoreLedger.Application.Validators;

public class CreateToDoCommandValidator : AbstractValidator<CreateToDoCommand>
{
    public CreateToDoCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");
    }
}
```

#### 2.3.7 Register Application Services

Create `CoreLedger.Application/DependencyInjection.cs`:
```csharp
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace CoreLedger.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
```

**Checkpoint 2.3:**
```bash
cd CoreLedger.Application
dotnet build
```

---

### Step 2.4: Migrate Infrastructure Layer
**Day 6-7 (10 hours)**

#### 2.4.1 Add Required Packages

Update `CoreLedger.Infrastructure/CoreLedger.Infrastructure.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.1" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.1">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

#### 2.4.2 Create DbContext and Repository Implementation

See detailed implementation in Phase 2.4 sections covering:
- ApplicationDbContext with proper configuration
- Entity configurations with concurrency tokens
- Repository pattern implementation
- Infrastructure dependency injection

**Checkpoint 2.4:**
```bash
cd CoreLedger.Infrastructure
dotnet build
```

---

### Step 2.5: Migrate API Layer
**Day 8-9 (10 hours)**

#### 2.5.1 Create New Controllers with MediatR

Migrate controllers to use CQRS pattern with thin controllers delegating to MediatR.

**Checkpoint 2.5:**
```bash
dotnet build
dotnet ef database update --project CoreLedger.Infrastructure
dotnet run --project CoreLedger.API
curl https://localhost:7001/api/todos
```

### Phase 2 Complete Checkpoint

**Verify before proceeding:**

✅ Solution builds successfully  
✅ All 4 projects compile  
✅ Database migrations applied  
✅ API starts without errors  
✅ All CRUD endpoints functional  
✅ Logging with correlation IDs working  
✅ Exception handling working  

```bash
git add .
git commit -m "Phase 2: Clean Architecture complete"
git push origin feature/enterprise-migration
```

---

## Step-by-Step Summary

This migration guide continues with:

- **Phase 2 remaining:** Infrastructure layer, API controllers, database configuration
- **Phase 3:** Unit tests, integration tests, test containers
- **Phase 4:** Swagger/OpenAPI, authentication, metrics, CI/CD

Each phase includes:
- Detailed step-by-step instructions
- Code examples
- Checkpoints for validation
- Rollback procedures
- Estimated time per task

**Total Migration Timeline:** 4-6 weeks for complete production readiness

**Next Steps:**
1. Complete Phase 1 (Foundation & Security)
2. Review and validate each checkpoint
3. Proceed to Phase 2 once Phase 1 is stable
4. Continue incrementally through all phases

Would you like me to continue with the remaining phases in detail?
