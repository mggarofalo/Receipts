# Receipts Project Constitution

This constitution establishes non-negotiable principles that govern all specification-driven development in the Receipts application. Every feature specification, technical plan, and implementation must comply with these articles.

---

## Article I: Clean Architecture Compliance

All code MUST adhere to Clean Architecture principles with strict layer separation:

1. **Domain Layer**: Contains only domain entities, aggregates, and business rules. MUST NOT reference Application, Infrastructure, or Presentation layers.
2. **Application Layer**: Contains business logic via CQRS (Commands/Queries). MAY reference Domain. MUST NOT reference Infrastructure or Presentation.
3. **Infrastructure Layer**: Implements data access and external services. MAY reference Domain and Application.
4. **Presentation Layer**: Contains API, Client, and Shared projects. MAY reference all layers except Infrastructure directly (via dependency injection only).

Dependencies flow inward only. Violations of layer boundaries are not permitted.

---

## Article II: CQRS Pattern Enforcement

All business operations MUST follow the Command Query Responsibility Segregation pattern:

1. **Queries**: Read operations that return data without side effects. Located in `Application/Queries/`.
2. **Commands**: Write operations that modify state. Located in `Application/Commands/`.
3. Every Query and Command MUST have a corresponding Handler.
4. MediatR MUST be used to dispatch all Commands and Queries.
5. Direct service calls that bypass MediatR are not permitted in Presentation layer.

---

## Article III: Test-First Development

No implementation code shall be written before tests are written:

1. Unit tests MUST use xUnit with Arrange/Act/Assert structure.
2. Use `expected` and `actual` as variable names in test assertions.
3. Test data MUST be generated using SampleData generators, not inline instantiation.
4. Every new Query/Command handler MUST have corresponding handler tests.
5. Domain entities MUST have validation and equality tests.
6. AVOID testing implementation details; focus on behavior.

Tests that are expected to pass MUST pass before code is considered complete.

---

## Article IV: Explicit Type Declarations

Code MUST use explicit type declarations:

1. Use explicit types instead of `var` except when the type is obvious and enhances readability.
2. Use primary constructors for dependency injection where appropriate.
3. Use `new(..)` target-typed syntax: `DatabaseMigratorService service = new(...);`
4. Nullable reference types MUST be enabled and respected.

---

## Article V: Domain Separation

Domain models, Infrastructure entities, and ViewModels MUST remain separate:

1. **Domain models** (`Domain/Core/`, `Domain/Aggregates/`): Business logic and validation.
2. **Infrastructure entities** (`Infrastructure/Entities/`): EF Core database mapping only.
3. **ViewModels** (`Shared/ViewModels/`): API contracts and DTOs.
4. AutoMapper MUST handle all conversions between these three representations.
5. Never expose Infrastructure entities or Domain models directly via API endpoints.

---

## Article VI: Database Access Patterns

All database operations MUST follow established patterns:

1. EF Core with PostgreSQL is the only permitted ORM/database combination.
2. Repository pattern MUST be used; direct DbContext access outside Infrastructure is forbidden.
3. Migrations are managed via EF Core and run automatically on API startup.
4. Connection configuration MUST use environment variables, never hardcoded strings.
5. No raw SQL queries unless absolutely necessary and documented with rationale.

---

## Article VII: API Design Standards

All API endpoints MUST follow REST conventions:

1. Controllers MUST be organized under `Controllers/Core/` or `Controllers/Aggregates/`.
2. FluentValidation MUST validate all incoming ViewModels.
3. SignalR hub updates MUST be sent after successful write operations for real-time sync.
4. API endpoints MUST return appropriate HTTP status codes (200, 201, 204, 400, 404, 500).
5. No business logic in controllers; delegate to MediatR handlers.

---

## Article VIII: Simplicity Over Cleverness

Every feature MUST prioritize simplicity:

1. Do not add features beyond what is explicitly specified.
2. Do not create abstractions for single-use operations.
3. Three similar lines of code are better than a premature abstraction.
4. Do not design for hypothetical future requirements.
5. Avoid over-engineering; solve the current problem with minimal complexity.

---

## Article IX: Security and Configuration

Security MUST be considered in all implementations:

1. Never commit secrets, connection strings, or credentials to source control.
2. All configuration MUST use environment variables or secure configuration providers.
3. Input validation MUST occur at API boundaries using FluentValidation.
4. Be vigilant against OWASP Top 10 vulnerabilities (XSS, SQL injection, etc.).
5. Authentication and authorization checks MUST be enforced where applicable.

---

## Article X: Conventional Commits

All commits MUST follow Conventional Commits format:

```
<type>(<scope>): <description>
```

**Types**: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

**Scopes**: `api`, `client`, `domain`, `application`, `infrastructure`, `common`, `shared`

Commit messages MUST be descriptive and explain the "why" behind changes.

---

## Governance

Any modification to this constitution requires:

1. A clear rationale documented in the PR description.
2. Review and approval from project maintainers.
3. An update to this document's revision history.

**Revision History**:
- v1.0.0 (2026-01-25): Initial constitution established for SpecKit integration.
