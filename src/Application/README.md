# Application

Business logic layer using CQRS with MediatR. Depends only on Domain.

## Structure

- **`Commands/{Entity}/Create|Update|Delete/`** — Command + Handler pairs for write operations
- **`Queries/Core/{Entity}/`** — Query + Handler pairs for single-entity reads
- **`Queries/Aggregates/`** — Complex queries joining multiple entities (e.g., `ReceiptWithItems`, `Trip`)
- **`Behaviors/`** — MediatR pipeline behaviors (e.g., `ValidationBehavior`)
- **`Interfaces/`** — Repository and service contracts implemented by Infrastructure
- **`Services/`** — Application-level services
- **`Exceptions/`** — Custom exception types
- **`Models/`** — DTOs and models used within the application layer

## Key Patterns

- **CQRS:** Commands and queries are separate types, each with a dedicated handler. Commands return the mutated entity; queries return read models.
- **Validation Pipeline:** `ValidationBehavior<TRequest, TResponse>` intercepts MediatR requests and runs all registered `IValidator<T>` instances before the handler executes.
- **Repository Interfaces:** Defined here, implemented in Infrastructure. The Application layer never references EF Core directly.
