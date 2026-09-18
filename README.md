# TradeLens

## Overview

TradeLens is a portfolio and P&L analytics platform for retail investors, designed to track stock transactions, calculate portfolio positions, and analyze realized and unrealized profit and loss.

TradeLens is an **analytics and portfolio tracking platform, not a trading execution system**.

The project is also designed as a practical demonstration of enterprise software engineering practices, including domain-driven design, layered architecture, transactional consistency, validation, error handling, security, testing, and production-readiness considerations.

---

## Problem Statement

Retail investors often maintain transaction records across multiple broker accounts and need a consistent way to understand:

- Current holdings
- Cost basis
- Average acquisition price
- Realized profit and loss
- Unrealized profit and loss
- Historical transaction changes
- Portfolio valuation

TradeLens provides a domain-oriented application model for maintaining this information while keeping transaction history as the source of truth.

---

## Goals

- Track historical stock transactions
- Support multiple broker accounts
- Calculate current portfolio positions
- Calculate weighted-average cost basis
- Calculate realized P&L
- Calculate unrealized P&L
- Support transaction corrections
- Provide deterministic recalculation
- Maintain transactional consistency
- Provide consistent API error contracts
- Support authentication and authorization
- Demonstrate layered architecture and DDD principles
- Provide automated tests
- Provide a foundation for production-ready observability and operations

---

## Key Features

### Completed

- [x] Transaction domain model
- [x] Position calculation
- [x] Realized P&L calculation
- [x] PostgreSQL persistence
- [x] Entity Framework Core
- [x] Repository abstraction
- [x] Unit of Work
- [x] Application validation with FluentValidation
- [x] Add Transaction use case
- [x] Transaction API
- [x] Get Transaction API
- [x] Development authentication
- [x] Global exception handling
- [x] Standardized API error contract
- [x] Validation error contract
- [x] Business-rule error contract
- [x] Transaction-not-found error contract
- [x] Domain and application unit tests

### In Progress

- [ ] Authentication hardening / JWT
- [ ] Authorization and portfolio ownership
- [ ] Transaction correction

### Planned

- [ ] Idempotency
- [ ] Optimistic concurrency
- [ ] Portfolio valuation
- [ ] Market price integration
- [ ] Integration tests
- [ ] Observability
- [ ] Operational monitoring
- [ ] Production deployment

---

## Architecture

TradeLens uses a layered architecture with DDD principles.

```text
┌──────────────────────────────┐
│        TradeLens API         │
│ Controllers / HTTP / Auth    │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│       Application Layer      │
│ Commands / Queries /         │
│ Validation / Use Cases       │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│          Domain              │
│ Entities / Domain Services   │
│ Business Rules / P&L         │
└──────────────────────────────┘


┌──────────────────────────────┐
│       Infrastructure         │
│ EF Core / Repository / DB    │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│         PostgreSQL           │
└──────────────────────────────┘
```

Dependency direction:

```text
TradeLens.Api
      ↓
TradeLens.Application
      ↓
TradeLens.Domain

TradeLens.Api
      ↓
TradeLens.Infrastructure
      ↓
TradeLens.Application
      ↓
TradeLens.Domain
```

The Domain layer does not depend on Infrastructure or API concerns.

Infrastructure implements persistence abstractions defined by the Application layer.

---

## Architecture Patterns

TradeLens currently uses:

- Layered Architecture
- Domain-Driven Design principles
- Dependency Injection
- Dependency Inversion
- Repository Pattern
- Unit of Work Pattern
- Domain Services
- Lightweight CQRS-style separation between commands and queries
- DTOs for API boundaries
- Optimistic concurrency foundation
- Problem Details-based API error contract

The project intentionally avoids unnecessary abstractions and infrastructure such as:

- Generic Repository
- MediatR
- AutoMapper
- Event Sourcing
- Separate CQRS databases
- Microservices
- Kafka
- Redis

These technologies may be considered when the actual system requirements justify their introduction.

---

## Domain Model

Core domain concepts:

```text
User
 └── Portfolio
      ├── BrokerAccount
      ├── Transaction
      └── Position

Instrument
 ├── Transaction
 └── Position

MarketPrice
 └── Portfolio Valuation
```

### Transaction History

Transaction history is the **source of truth** for portfolio ownership and cost-basis calculation.

### Position

Position represents the derived/materialized current state of an instrument within a portfolio.

### Cost Basis

The current implementation uses weighted-average cost.

Key rules:

- BUY fee increases cost basis.
- SELL fee reduces net proceeds.
- Market price does not change cost basis.
- Realized P&L and unrealized P&L are calculated separately.
- Historical transaction corrections trigger deterministic recalculation.
- Posted transactions are not hard-deleted.

### Transaction Ordering

Transactions are processed deterministically using:

```text
TransactionDate
        +
Sequence
```

`TransactionDate` represents the business date.

`CreatedAt` represents the system timestamp when the transaction was recorded.

---

## API

Base path:

```text
/api/v1
```

### Implemented

```text
POST /api/v1/transactions

GET  /api/v1/transactions/{id}
```

### Planned

```text
GET  /api/v1/transactions

POST /api/v1/transactions/{id}/corrections

GET /api/v1/positions

GET /api/v1/pnl

GET /api/v1/portfolios
```

---

## API Error Handling

TradeLens uses a standardized error contract based on HTTP status codes and Problem Details.

Example:

```json
{
  "type": "https://api.tradelens.com/problems/position_insufficient_quantity",
  "title": "Insufficient Position Quantity",
  "status": 400,
  "code": "POSITION_INSUFFICIENT_QUANTITY",
  "detail": "Sell quantity cannot exceed current position.",
  "traceId": "..."
}
```

Common error categories:

| Category | HTTP Status | Example Code |
|---|---:|---|
| Validation | 400 | `VALIDATION_ERROR` |
| Business Rule | 400 | `POSITION_INSUFFICIENT_QUANTITY` |
| Authentication | 401 | `AUTHENTICATION_REQUIRED` |
| Authorization | 403 | `PORTFOLIO_ACCESS_DENIED` |
| Not Found | 404 | `TRANSACTION_NOT_FOUND` |
| Conflict | 409 | `TRANSACTION_CONFLICT` |
| Concurrency | 409 | `POSITION_CONCURRENCY_CONFLICT` |
| Unexpected Error | 500 | `INTERNAL_ERROR` |

The API uses stable machine-readable error codes so clients do not need to parse human-readable error messages.

---

## Technology Stack

- .NET 8
- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- Docker
- xUnit
- FluentAssertions
- FluentValidation
- Swagger / OpenAPI

---

## Project Structure

```text
TradeLens/
├── src/
│   ├── TradeLens.Api/
│   ├── TradeLens.Application/
│   ├── TradeLens.Domain/
│   └── TradeLens.Infrastructure/
│
├── tests/
│   ├── TradeLens.Domain.Tests/
│   ├── TradeLens.Application.Tests/
│   └── TradeLens.Integration.Tests/
│
├── docker/
│   └── docker-compose.yml
│
├── docs/
├── README.md
└── .gitignore
```

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- Git

### Clone

```bash
git clone <repository-url>
cd TradeLens
```

### Start PostgreSQL

```bash
docker compose -f docker/docker-compose.yml up -d
```

### Restore dependencies

```bash
dotnet restore
```

### Build

```bash
dotnet build
```

### Apply database migrations

```bash
dotnet ef database update
```

### Run the API

```bash
dotnet run --project src/TradeLens.Api
```

Swagger is available through the API's configured Swagger endpoint.

---

## Database

TradeLens uses PostgreSQL running in Docker for local development.

Current development configuration:

```text
Host:     localhost
Port:     5433
Database: tradelens
Username: tradelens
```

The Docker container exposes PostgreSQL's internal port `5432` through host port `5433` because another PostgreSQL instance may already use host port `5432`.

> Development credentials are intended only for local development and should not be used in production.

---

## Testing

Run all tests:

```powershell
dotnet test
```

Current automated tests cover:

- Domain transaction rules
- Position calculations
- Realized P&L calculations
- Add Transaction use case
- Validation behavior
- Oversell business rules

Integration tests for API and PostgreSQL interaction are planned.

---

## Design Decisions

### Why PostgreSQL?

PostgreSQL provides strong relational consistency and is sufficient for the transactional workload of the MVP without introducing unnecessary infrastructure complexity.

### Why Repository Pattern?

Repositories provide business-oriented persistence abstractions and prevent the Application and Domain layers from depending directly on EF Core.

### Why no Generic Repository?

A generic CRUD abstraction can hide business-oriented access patterns. TradeLens uses repositories designed around actual use-case requirements.

### Why no Event Sourcing?

TradeLens requires deterministic recalculation from transaction history, but does not require the operational and architectural complexity of full event sourcing for the MVP.

### Why is Position derived?

Transaction history represents the source of truth.

Position is a materialized current state that can be recalculated when historical transactions change.

This provides a balance between calculation performance and data correctness.

### Why not microservices?

The current domain and workload do not justify distributed-system complexity. TradeLens is intentionally designed as a modular monolith that can evolve if future requirements require service decomposition.

### Why lightweight CQRS?

Commands and queries have different responsibilities, but a full CQRS infrastructure is unnecessary for the current scope.

TradeLens therefore separates command and query use cases without introducing separate databases or messaging infrastructure.

---

### Completed

- Domain transaction model

- Position calculation

- P&L calculation

- EF Core persistence

- PostgreSQL setup

- Repository foundation

- Unit of Work

- Application validation

- Add Transaction use case

- Transaction API

- Get Transaction API

- Development authentication

- Portfolio ownership authorization

- Global exception handling

- Standardized API error contract

- Transaction correction domain behavior
  - Supersede original transaction
  - Create corrected transaction
  - Preserve transaction history

- Transaction correction application workflow
  - CorrectTransactionService
  - Portfolio ownership validation
  - Position recalculation
  - Application unit tests

- Transaction correction API

- Integration test coverage
  - POST transaction → 201 Created
  - GET transaction → 200 OK
  - Position persistence verification
  - Validation error → 400 Bad Request
  - Transaction not found → 404 Not Found
  - Portfolio ownership violation → 403 Forbidden
  - Transaction correction → 200 OK
  - Concurrent idempotent requests
  - Idempotency conflict handling
  - Position optimistic concurrency conflict

- Idempotency
  - Required `Idempotency-Key` header
  - Request payload hashing
  - Duplicate request detection
  - Idempotency conflict detection
  - Persistent idempotency records
  - Unique `(UserId, IdempotencyKey)` constraint
  - Atomic transaction handling
  - Concurrent request handling
  - Idempotency concurrency exception handling

- Optimistic concurrency
  - EF Core concurrency token for Position
  - Concurrent update detection
  - Application-level concurrency exception
  - HTTP 409 conflict mapping
  - Integration test coverage

- Automated test coverage
  - Domain unit tests
  - Application unit tests
  - Integration tests
  - Application tests: 24 passing
  - Integration tests: 8 passing

### In Progress

- API-level concurrency testing

### Planned

- Portfolio valuation
- Market price integration
- Authentication hardening
- Observability
- Production readiness
- Final integration test coverage and cleanup
- Architecture and implementation documentation

---

## Roadmap

### Phase 1 — Core Domain

- [x] Transaction
- [x] Position
- [x] Cost basis
- [x] P&L

### Phase 2 — Transaction Use Cases

- [x] Add transaction
- [x] Query transaction
- [x] Transaction correction
- [x] Deterministic recalculation

### Phase 3 — API

- [x] REST API
- [x] Validation
- [x] Error contract
- [x] Problem Details

### Phase 4 — Valuation

- [ ] Market prices
- [ ] Portfolio valuation
- [ ] Unrealized P&L

### Phase 5 — Security

- [x] Authentication
- [x] Authorization
- [x] Portfolio ownership
- [x] Secure API boundaries

### Phase 6 — Reliability & Observability

- [x] Idempotency
- [x] Optimistic concurrency
- [ ] Structured logging
- [ ] Traceability
- [x] Integration testing

### Phase 7 — Production Readiness

- [x] Containerized development environment
- [ ] Production Docker deployment
- [ ] Configuration management
- [ ] Health checks
- [ ] Monitoring
- [ ] Operational documentation
- [ ] CI/CD

---

## License

TBD