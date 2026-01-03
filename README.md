# Core Ledger API

A production-ready .NET 10 REST API for **fund accounting ABOR (Accounting Book of Records)** designed for institutional financial clients, implementing Clean Architecture principles with a focus on safety, auditability, and maintainability. This system serves as the authoritative source of accounting data for investment funds.

## 🏗️ Architecture

This project follows **Clean Architecture** (Hexagonal Architecture) with clear separation of concerns:

```
CoreLedger.API/              # Presentation Layer (Controllers, Middleware, Extensions)
CoreLedger.Application/      # Application Layer (Use Cases, DTOs, Validators)
CoreLedger.Domain/           # Domain Layer (Entities, Value Objects, Interfaces)
CoreLedger.Infrastructure/   # Infrastructure Layer (EF Core, Repositories, Persistence)
CoreLedger.UnitTests/        # Unit Tests (xUnit + NSubstitute)
CoreLedger.IntegrationTests/ # Integration Tests (Testcontainers + xUnit)
```

### Key Architectural Patterns

- **CQRS**: Command Query Responsibility Segregation using MediatR
- **Direct DbContext Usage**: Entity Framework Core DbSet<T> as data access pattern
- **Query Services**: Infrastructure layer services for complex RFC-8040 filtering operations
- **Dependency Injection**: Constructor injection throughout
- **Domain-Driven Design**: Rich domain models with business logic
- **Middleware Pipeline**: Cross-cutting concerns (exception handling, logging, correlation IDs)

## 🛠️ Tech Stack

### Core Framework
- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **C# 13** - Latest language features with nullable reference types

### Database & ORM
- **PostgreSQL 18** - Primary database
- **Entity Framework Core 10** - ORM
- **Npgsql** - PostgreSQL provider

### Application Patterns
- **MediatR 14** - CQRS and mediator pattern
- **AutoMapper 16** - Object-to-object mapping
- **FluentValidation 12** - Input validation

### Messaging & Authentication
- **RabbitMQ** - Message broker for async processing and worker communication
- **Auth0** - JWT Bearer authentication and user management

### Logging & Monitoring
- **Serilog** - Structured logging
- **Health Checks** - Application health monitoring
- **Correlation IDs** - Request tracing

### API Documentation
- **Swagger/OpenAPI** - Interactive API documentation
- **XML Documentation** - Code-level documentation

### Testing
- **xUnit** - Test framework
- **NSubstitute** - Mocking library
- **Testcontainers** - Integration testing with disposable Docker containers

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Local development environment

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for local PostgreSQL and RabbitMQ)
- [PostgreSQL 18](https://www.postgresql.org/download/) (or use Docker)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (or use Docker)
- IDE: [Visual Studio 2025](https://visualstudio.microsoft.com/), [Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/core-ledger-api.git
cd core-ledger-api
```

### 2. Set Up Environment Variables

```bash
# Copy the environment template
cp .env.template .env

# Edit .env with your configuration
# For local development, the defaults should work with Docker Compose
```

### 3. Start PostgreSQL and RabbitMQ with Docker

```bash
# Start PostgreSQL and RabbitMQ containers
docker-compose up -d

# Verify containers are running
docker ps
```

The following services will be available:
- **PostgreSQL**: localhost:5432
- **RabbitMQ AMQP**: localhost:5672
- **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)

### 4. Apply Database Migrations

```bash
# From the solution root
dotnet ef database update --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### 5. Run the Application

```bash
# Development mode with hot reload
dotnet watch run --project CoreLedger.API

# Or standard run
dotnet run --project CoreLedger.API
```

The API will be available at:
- **HTTPS**: https://localhost:7001
- **HTTP**: http://localhost:5001
- **Swagger UI**: https://localhost:7001/swagger

## 🧪 Testing

### Run All Tests

```bash
dotnet test
```

### Run Unit Tests Only

```bash
dotnet test CoreLedger.UnitTests/CoreLedger.UnitTests.csproj
```

### Run Integration Tests Only

```bash
dotnet test CoreLedger.IntegrationTests/CoreLedger.IntegrationTests.csproj
```

### Code Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 📚 API Documentation

### Swagger UI

When running in Development mode, access the interactive API documentation at:

**https://localhost:7001/swagger**

### Health Checks

- **General Health**: `GET /health`
- **Readiness**: `GET /health/ready`
- **Liveness**: `GET /health/live`

### Example Endpoints

#### ToDos API

```bash
# Get all ToDos
GET /api/todos

# Get ToDo by ID
GET /api/todos/{id}

# Create new ToDo
POST /api/todos
Content-Type: application/json
{
  "description": "Complete the project documentation"
}

# Update ToDo
PUT /api/todos/{id}
Content-Type: application/json
{
  "description": "Updated description",
  "isCompleted": true
}

# Delete ToDo
DELETE /api/todos/{id}
```

## 🗂️ Project Structure

```
core-ledger-api/
├── CoreLedger.API/
│   ├── Controllers/          # API Controllers
│   ├── Extensions/           # Service configuration extensions
│   ├── Middleware/           # Custom middleware
│   └── Program.cs            # Application entry point
├── CoreLedger.Application/
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Mappings/             # AutoMapper profiles
│   ├── UseCases/             # Commands and Queries (CQRS)
│   └── Validators/           # FluentValidation validators
├── CoreLedger.Domain/
│   ├── Entities/             # Domain entities
│   ├── Exceptions/           # Domain exceptions
│   ├── Interfaces/           # Application interfaces (IApplicationDbContext)
│   └── ValueObjects/         # Value objects
├── CoreLedger.Infrastructure/
│   ├── Persistence/          # DbContext and entity configurations
│   │   └── Migrations/       # EF Core migrations
│   └── Services/
│       ├── QueryServices/    # RFC-8040 filtering and pagination
│       └── External/         # External service integrations
├── CoreLedger.Worker/
│   ├── Consumers/            # RabbitMQ message consumers
│   └── Program.cs            # Worker service entry point
├── CoreLedger.UnitTests/
│   ├── Application/          # Application layer tests
│   └── Domain/               # Domain layer tests
├── CoreLedger.IntegrationTests/
│   └── API/                  # API integration tests
└── docs/
    └── archive/              # Archived documentation
```

## 💻 Development Guidelines

### Code Standards

- **Nullable Reference Types**: Enabled project-wide (`<Nullable>enable</Nullable>`)
- **Warnings as Errors**: Strict compilation (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- **XML Documentation**: Required for all public APIs
- **Naming Conventions**: Follow Microsoft .NET guidelines

### Testing Standards

- **Unit Test Coverage**: Minimum 80% for Application and Domain layers
- **Test Naming**: `MethodName_Scenario_ExpectedBehavior`
- **Arrange-Act-Assert**: Follow AAA pattern in all tests
- **Mocking**: Use NSubstitute for interface-based mocking

### Security Best Practices

- **Secrets Management**: Use User Secrets (dev) or Azure Key Vault (prod)
- **Input Validation**: FluentValidation for all commands and queries
- **Error Handling**: Global exception middleware prevents leaking internal details
- **HTTPS**: Enforced in non-development environments
- **Audit Trails**: Structured logging with correlation IDs

### Database Guidelines

- **Migrations**: Always use explicit migrations; never `EnsureCreated()`
- **Transactions**: Explicit transactions for multi-step accounting operations
- **Concurrency**: Optimistic concurrency tokens for critical accounting data
- **Money Handling**: Use `decimal` with explicit precision/scale for all monetary values

## 🔧 Database Migrations

### Create a New Migration

```bash
dotnet ef migrations add MigrationName --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### Apply Migrations

```bash
dotnet ef database update --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### Rollback Migration

```bash
dotnet ef database update PreviousMigrationName --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

## 📦 Building for Production

### Build Release

```bash
dotnet build --configuration Release
```

### Publish

```bash
dotnet publish CoreLedger.API/CoreLedger.API.csproj --configuration Release --output ./publish
```

### Docker Build (Future)

```bash
# TODO: Add Dockerfile
docker build -t core-ledger-api:latest .
```

## 🔍 Logging

Logs are written to:
- **Console**: Structured output with timestamps
- **File**: `logs/core-ledger-{Date}.log` (retained for 30 days)

### Log Levels
- **Information**: High-level events (transaction recorded, journal entry posted, NAV calculated)
- **Warning**: Recoverable anomalies
- **Error**: Failures requiring attention
- **Critical**: System-level failures

### Correlation IDs

Every request receives a correlation ID (via `X-Correlation-ID` header or auto-generated) for end-to-end tracing.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### PR Requirements

- ✅ All tests pass (`dotnet test`)
- ✅ Code follows project conventions
- ✅ XML documentation for public APIs
- ✅ Unit tests for new features
- ✅ No warnings or errors

## 📄 License

[Specify your license here - MIT, Apache 2.0, Proprietary, etc.]

## 👥 Authors

- **Development Team** - [Your Organization]

## 🙏 Acknowledgments

- Built as a fund accounting ABOR system for institutional financial clients
- Follows enterprise-grade security and compliance standards
- Designed for auditability, data integrity, and deterministic behavior
- Serves as the authoritative accounting book of records for investment funds

---

**For support or questions, please open an issue on GitHub.**
