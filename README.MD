# Inventory Management API

A RESTful API for managing inventory — products, categories and suppliers — built as a technical assessment.

---

## Tech Stack

| | |
|---|---|
| Runtime | .NET 10 |
| Web Framework | ASP.NET Core Minimal APIs |
| ORM | Entity Framework Core |
| Database | PostgreSQL 16 |
| Validation | FluentValidation |
| API Docs | Scalar (OpenAPI) |
| Testing | xUnit + FluentAssertions + NSubstitute |
| Containerization | Docker + Docker Compose |

---

## Architecture

The project uses **Vertical Slice Architecture** — code is organized by feature, not by technical layer.

Each feature folder is self-contained: entity, configuration, use case and endpoint live together. There are no cross-cutting layers like `Controllers/`, `Services/` or `Repositories/`.

```
Features/
├── Categories/
│   ├── Category.cs               ← entity + domain rules
│   ├── CategoryConfiguration.cs  ← EF Core mapping
│   ├── CreateCategory.cs         ← contract + validator + use case + endpoint
│   ├── ListCategories.cs
│   └── DeleteCategory.cs
│
├── Suppliers/
│   ├── Supplier.cs
│   ├── SupplierConfiguration.cs
│   ├── CreateSupplier.cs
│   └── ListSuppliers.cs
│
└── Products/
    ├── Product.cs                 ← entity + status state machine
    ├── ProductStatus.cs
    ├── ProductConfiguration.cs
    ├── CreateProduct.cs
    └── ChangeProductStatus.cs

Infrastructure/
├── AppDbContext.cs
└── ExternalServices/
    ├── IExternalServices.cs       ← IWmsService, IAuditService, IEmailService
    └── ExternalServices.cs        ← mock implementations + LoggerMessage source generator

Shared/
├── DomainException.cs
├── DomainExceptionHandler.cs
└── GlobalExceptionHandler.cs
```

### Request Flow

```
HTTP Request
    │
    ▼
Endpoint          → validates input (FluentValidation)
    │
    ▼
Use Case          → checks pre-conditions, calls domain, persists via EF Core
    │
    ▼
Domain Entity     → enforces business rules, transitions state
    │
    ▼
External Services → WMS / Audit / Email (mock adapters, swappable)
```

### Why Vertical Slice?

Organizing by feature instead of layer means:

- **High cohesion** — everything related to "create product" is in `CreateProduct.cs`
- **Low coupling** — features don't depend on each other
- **Easy to navigate** — bug in `CreateProduct`? Open `CreateProduct.cs`
- **Easy to delete** — removing a feature means deleting one file

### Domain Entities

Entities use **private setters** and **static factory methods** to ensure they can never be in an invalid state. All business rules live inside the entity itself — no anemic models, no logic leaking into use cases.

```csharp
// The entity owns its state transitions
product.MarkAsSold();   // throws DomainException if cancelled or returned
product.Cancel();
product.Return();
```

### Product State Machine

```
         ┌───────────┐
         │  Created  │
         └─────┬─────┘
               │ MarkAsSold
         ┌─────▼─────┐
         │   Sold    │
         └─────┬─────┘
       ┌───────┴────────┐
    Return            Cancel
       │                │
┌──────▼──────┐  ┌──────▼──────┐
│  Returned   │  │  Cancelled  │
└─────────────┘  └─────────────┘

Rules:
  ✓ Created  → Sold
  ✓ Created  → Cancelled
  ✓ Sold     → Cancelled
  ✓ Sold     → Returned
  ✗ Cancelled → Sold      (DomainException)
  ✗ Returned  → Sold      (DomainException)
```

### EF Core — No Repository Pattern

`DbContext` is a Unit of Work and `DbSet<T>` is already a Repository. Adding another abstraction layer on top would be indirection without value. Use cases query `AppDbContext` directly.

### External Services

WMS, Audit and Email are **interface-backed mock adapters** using the `[LoggerMessage]` source generator — zero string interpolation allocations at runtime. Replacing a mock with a real HTTP client means adding one class in `ExternalServices/`, nothing else changes.

---

## Running the Project

### Prerequisites

- Docker Desktop

### Full stack (recommended)

```bash
docker-compose up --build
```

| | |
|---|---|
| API | http://localhost:5035 |
| Scalar docs | http://localhost:5035/scalar/v1 |

Database migrations are applied automatically on startup.

### Local development

```bash
# Start only the database
docker-compose up db -d

# Run the API
cd InventoryManagement.Api
dotnet run
```

---

## Running Tests

```bash
dotnet test
```

| Suite | Strategy | What's covered |
|---|---|---|
| `ProductTests` | Pure unit tests | State machine — all valid and invalid transitions |
| `CreateProductUseCaseTests` | EF InMemory + NSubstitute | Persistence, WMS/Audit integration, not-found cases |
| `ChangeProductStatusUseCaseTests` | EF InMemory + NSubstitute | Status transitions, side effects per status |
| `CategoryTests` | Pure unit tests | Creation rules, shortcode normalization |
| `SupplierTests` | Pure unit tests | Creation rules, email/currency normalization |

Use cases are tested with **EF Core InMemory** (real query behavior, isolated per test) and **NSubstitute** for external services (WMS, Audit, Email).

---

## API Reference

### Categories

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/categories` | Create a category (supports parent category) |
| `GET` | `/categories` | List all categories |
| `DELETE` | `/categories/{id}` | Delete a category |

**Create category request:**
```json
{
  "name": "Shirts",
  "shortcode": "SHI",
  "parentCategoryId": null
}
```

### Suppliers

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/suppliers` | Create a supplier |
| `GET` | `/suppliers` | List all suppliers |

**Create supplier request:**
```json
{
  "name": "Farfetch LTDA",
  "email": "supplier@farfetch.com",
  "currency": "EUR",
  "country": "Portugal"
}
```

### Products

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/products` | Create a product — triggers WMS sync + audit log |
| `PATCH` | `/products/{id}/status` | Change product status — Sold triggers email + WMS dispatch + audit log |

**Create product request:**
```json
{
  "description": "Coffe Bag",
  "acquisitionCostInSupplierCurrency": 1200.00,
  "acquisitionCostInUsd": 1000.00,
  "acquireDate": "2026-01-15",
  "supplierId": "019123ab-...",
  "categoryId": "019123cd-..."
}
```

**Change status request:**
```json
{
  ...
  "newStatus": "Sold"
  ...
}
```

Allowed values: `Sold`, `Cancelled`, `Returned`.

### Error responses

All errors follow [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457):

```json
{
  "title": "Business Rule Violation",
  "status": 422,
  "detail": "A cancelled product cannot be sold.",
  "instance": "PATCH /products/019123ab-.../status"
}
```

| Status | When |
|---|---|
| `400` | Validation errors (missing fields, wrong format) |
| `422` | Business rule violations (DomainException) |
| `500` | Unexpected errors |

---

## Design Decisions

**Vertical Slice over layered architecture** — features are cohesive units. Adding a new feature means adding one folder with a few files. No existing code needs to change.

**EF Core directly in use cases** — `DbSet<T>` is already a Repository pattern. A custom `IRepository<T>` on top adds abstraction without value at this scale.

**Feature-colocated EF configurations** — each `IEntityTypeConfiguration<T>` lives in its feature folder. `AppDbContext` stays clean with a single `ApplyConfigurationsFromAssembly` call.

**`[LoggerMessage]` source generator** — mock adapters use compile-time generated log methods instead of `logger.LogInformation(...)`. No string allocations on the logging hot path.

**UUID v7 identifiers** — time-sortable, globally unique, no sequential ID leakage. Maintains good B-tree index performance in PostgreSQL without exposing business information.

**String enum storage** — `ProductStatus` is stored as a string (`"Created"`, `"Sold"`...). Human-readable in the database, resilient to enum reordering, no magic numbers.

**`IExceptionHandler` over custom middleware** — registration order defines the handler chain explicitly. `DomainExceptionHandler` returns `false` for non-domain exceptions, falling through to `GlobalExceptionHandler`. Both return structured `ProblemDetails` (RFC 9457).