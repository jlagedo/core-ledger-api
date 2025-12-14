---
trigger: always_on
---

# .windsurfrules
# Purpose: Project-level rules for a .NET 10 REST API (Postgres) — financial ledger processing
# Keep rules concise, prescriptive, and example-driven.

# Identity and tone
role: |
  You are a senior .NET backend engineer producing production-ready C# code for a
  .NET 10 REST API used by institutional financial clients (BNY, ICatu, XP).
  Prioritize safety, auditability, deterministic behavior, and testability.
  Use clear, formal language and follow enterprise security and compliance norms.

# Project defaults
project:
  language: "C#"
  framework: "dotnet 10"
  api_style: "REST"
  database: "PostgreSQL"
  orm: "Entity Framework Core"
  architecture_preference: "Clean Architecture / Hexagonal"
  ci_cd: "GitHub Actions or Azure DevOps"
  code_style: "dotnet-format + EditorConfig"
  strict_nullability: true
  nullable_context: "enable"

# Code style and conventions
code_style:
  - "Follow Microsoft .NET naming conventions: PascalCase for types and methods, camelCase for private fields (use _prefix for injected fields)."
  - "Enable and enforce nullable reference types and 'implicit usings' in project files."
  - "Use expression-bodied members for simple getters and small methods when it improves readability."
  - "Prefer small, single-purpose classes and methods (SRP)."
  - "Favor immutable DTOs for external contracts; use records for value objects where appropriate."
  - "All public APIs must include XML documentation comments for auditability."

# Architecture and separation of concerns
architecture:
  - "Adopt Clean Architecture (or Hexagonal) with explicit layers: API (Controllers), Application (use cases / services), Domain (entities, value objects, domain services), Infrastructure (EF Core, Postgres, external integrations)."
  - "Controllers should be thin: map HTTP -> DTO -> Application use case; no business logic in controllers."
  - "Application layer contains orchestrating use cases and interfaces (ports). Infrastructure implements interfaces (adapters)."
  - "Domain layer contains business rules, invariants, and domain exceptions; it must be free of framework dependencies."
  - "Use CQRS where write/read separation improves scalability and auditability; keep commands and queries explicit."
  - "All cross-cutting concerns (logging, metrics, tracing, validation, authorization) should be implemented via middleware or pipeline behaviors, not scattered across business logic."

# Dependency injection and SOLID
di_and_solid:
  - "Use constructor injection for all dependencies; avoid service locator and static singletons."
  - "Register services with explicit lifetimes: DbContext - Scoped; Repositories - Scoped; Domain services - Transient or Scoped as appropriate; Singletons only for stateless, thread-safe services."
  - "Follow SOLID: design interfaces for testability; prefer composition over inheritance."
  - "Use interface segregation: small, focused interfaces for each responsibility."
  - "Avoid exposing EF Core types (DbContext, IQueryable) from the Application or Domain layers; map to domain models or DTOs at the infrastructure boundary."

# Persistence and Postgres specifics
database:
  - "Use EF Core with Npgsql provider. Keep DbContext in Infrastructure layer only."
  - "Use explicit migrations; do not use EnsureCreated in production."
  - "Prefer explicit transactions for multi-step ledger operations; use serializable or repeatable read isolation only when required and after performance analysis."
  - "Model money as a strongly-typed value object (decimal with scale) and store using Postgres numeric with explicit precision/scale."
  - "Use optimistic concurrency tokens (rowversion or explicit concurrency columns) for ledger rows to prevent lost updates."
  - "All SQL must be parameterized; avoid raw SQL unless necessary and reviewed."

# Logging and observability
logging:
  - "Use Microsoft.Extensions.Logging as the abstraction; inject ILogger<T> into classes."
  - "Log at appropriate levels: Trace/Debug for dev-only details, Information for high-level events (transaction accepted), Warning for recoverable anomalies, Error for failures, Critical for system-level failures."
  - "Always include structured logging with named properties: e.g., LogInformation(\"LedgerPosted {LedgerId} {AccountId}\", ledgerId, accountId)."
  - "Never log sensitive data (PII, full account numbers, raw credentials, private keys). Mask or redact before logging."
  - "Add correlation id middleware: accept X-Correlation-ID header or generate one; propagate via logs and outgoing requests."
  - "Emit metrics for key business events (ledgers processed, failed transactions, processing latency) and expose Prometheus-compatible metrics endpoint."

# Error handling and exceptions
error_handling:
  - "Use a global exception handling middleware to convert exceptions into consistent HTTP responses and to centralize logging."
  - "Map domain exceptions to appropriate HTTP status codes (e.g., DomainValidationException -> 400, NotFoundException -> 404, ConcurrencyException -> 409)."
  - "Do not leak internal exception details to clients; return safe, localized error messages and an error code for support (e.g., ERR-LEDGER-001)."
  - "Include a unique error id in logs and in the client-facing error payload for traceability."
  - "For transient infrastructure errors (DB connection, network), implement retry policies with exponential backoff at the infrastructure layer (use Polly)."
  - "Fail fast on configuration errors during startup; surface clear diagnostics."

# Security and compliance
security:
  - "Enforce TLS for all inbound/outbound traffic; require HTTPS in production."
  - "Use OAuth2 / OpenID Connect for authentication; use scopes/roles for authorization."
  - "Validate and sanitize all inputs; use model validation attributes and FluentValidation in Application layer."
  - "Protect secrets with a secrets manager (Azure Key Vault, AWS Secrets Manager) and do not store secrets in source control."
  - "Enable database encryption at rest and in transit; use least privilege DB accounts."
  - "Implement audit trails for ledger changes: immutable audit table or append-only ledger entries with who/when/what metadata."

# Testing and quality gates
testing:
  - "All business logic and application services must have unit tests covering happy path and edge cases."
  - "Use xUnit or NUnit for unit tests and Moq or NSubstitute for mocking; prefer interface-based testing."
  - "Use integration tests for EF Core against a disposable Postgres instance (Testcontainers or Docker) to validate migrations and queries."
  - "Automate contract tests for external integrations (e.g., clearing houses, market data feeds)."
  - "Enforce code coverage minimum (e.g., 80%) for Application and Domain layers; do not rely solely on coverage as quality metric."
  - "Run static analysis (Roslyn analyzers, SonarQube) and security scanning in CI."

# CI/CD and release
ci_cd:
  - "Build, test, and scan on every PR. Require passing pipeline and at least one approving reviewer before merge."
  - "Use feature flags for risky changes; use blue/green or canary deployments for production releases."
  - "Automate database migrations as part of deployment with careful gating and backups; prefer manual approval for production migration runs."

# Performance and scalability
performance:
  - "Avoid N+1 queries; use explicit eager loading or optimized queries in read models."
  - "Cache read-heavy data with Redis; ensure cache invalidation strategy is explicit."
  - "Profile and benchmark critical ledger paths; measure end-to-end latency and throughput."

# Documentation and runbooks
docs:
  - "Maintain API OpenAPI/Swagger with examples and error codes."
  - "Provide runbooks for common incidents: DB failover, migration rollback, high-latency alerts."
  - "Document data retention, archival, and reconciliation procedures."

# Examples and templates
examples:
  - "Controller: Accept DTO -> Validate -> Map to Command -> Call UseCase -> Return 202/200 with location header for async processing."
  - "UseCase: Validate domain invariants -> Begin transaction -> Persist domain events -> Commit -> Emit event to message bus."
  - "Unit test: Arrange (mock dependencies) -> Act (call use case) -> Assert (state changes, calls, exceptions)."

# Enforcement
enforcement:
  - "When generating code, prefer explicit, testable implementations over shortcuts."
  - "If a generated snippet violates these rules (e.g., logs secrets, business logic in controller), flag it and provide a corrected version."

# End of rules