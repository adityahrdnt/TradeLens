# TradeLens

## Overview

TradeLens is a portfolio and P&L analytics platform for retail investors, designed to track stock transactions, calculate portfolio positions, and analyze realized and unrealized profit and loss.

TradeLens is an **analytics and portfolio tracking platform, not a trading execution system**.

The project is also designed as a practical demonstration of enterprise software engineering practices, including domain-driven design, layered architecture, transactional consistency, validation, error handling, security, resilience, testing, and production-readiness considerations.

---

## Problem Statement

Retail investors often maintain transaction records across multiple broker accounts and need a consistent way to understand:

* Current holdings
* Cost basis
* Average acquisition price
* Realized profit and loss
* Unrealized profit and loss
* Historical transaction changes
* Current portfolio valuation

TradeLens provides a domain-oriented application model for maintaining this information while keeping transaction history as the source of truth.

---

## Goals

* Track historical stock transactions
* Support multiple broker accounts
* Calculate current portfolio positions
* Calculate weighted-average cost basis
* Calculate realized P&L
* Calculate unrealized P&L
* Support transaction corrections
* Support transaction voiding
* Provide transaction history listing
* Provide deterministic recalculation
* Maintain transactional consistency
* Provide consistent API error contracts
* Support authentication and authorization
* Support market price integration
* Validate market price freshness
* Provide portfolio-level valuation
* Demonstrate layered architecture and DDD principles
* Provide automated tests
* Provide a foundation for production-ready observability and operations

---

## Key Features

### Completed

* [x] Transaction domain model
* [x] Position and P&L calculation
* [x] Weighted-average cost basis
* [x] PostgreSQL persistence
* [x] Entity Framework Core
* [x] Repository abstraction
* [x] Unit of Work
* [x] Application validation
* [x] Add Transaction use case
* [x] Transaction correction
* [x] Transaction void
* [x] Transaction history listing
* [x] Transaction API
* [x] Get Transaction API
* [x] Idempotency
* [x] Optimistic concurrency
* [x] Portfolio valuation
* [x] Portfolio-level valuation aggregation
* [x] Position-level valuation
* [x] Portfolio ownership authorization
* [x] Development authentication
* [x] Production JWT Bearer authentication
* [x] Global exception handling
* [x] Standardized API error contract
* [x] Market price integration foundation
* [x] External market price provider
* [x] Background market price synchronization
* [x] Market price freshness validation
* [x] Partial portfolio valuation handling
* [x] HTTP resilience and retry handling
* [x] Domain, application, and integration tests

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
│           Domain             │
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
│          PostgreSQL          │
└──────────────────────────────┘
```

### Dependency Direction

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

* Layered Architecture
* Domain-Driven Design principles
* Dependency Injection
* Dependency Inversion
* Repository Pattern
* Unit of Work Pattern
* Domain Services
* Lightweight CQRS-style separation between commands and queries
* DTOs and records for API boundaries
* Optimistic concurrency
* Idempotency
* Problem Details-based API error contract
* HTTP resilience

The project intentionally avoids unnecessary abstractions and infrastructure such as:

* Generic Repository
* MediatR
* AutoMapper
* Event Sourcing
* Separate CQRS databases
* Microservices
* Kafka
* Redis

These technologies may be considered when actual system requirements justify their introduction.

### Authorization

Portfolio access is enforced server-side based on the authenticated user identity.

The API does not trust the client to determine ownership. Application services use a centralized `IPortfolioAccessService` to verify that the current user owns the requested portfolio.

Unauthorized portfolio access returns:

* `403 Forbidden`
* Error code: `PORTFOLIO_ACCESS_DENIED`

Portfolio ownership authorization is enforced across protected portfolio-related use cases, including transaction operations and portfolio valuation.

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
 ├── Position
 └── MarketPrice

Portfolio Valuation
 ├── Portfolio
 ├── Position
 └── MarketPrice
```

### Transaction History

Transaction history is the **source of truth** for portfolio ownership and cost-basis calculation.

Transactions represent historical business events rather than the current materialized state of a position.

### Transaction Lifecycle

Transactions currently support three states:

```text
                 ┌──────────────┐
                 │    Active    │
                 └──────┬───────┘
                        │
             ┌──────────┴──────────┐
             │                     │
             ▼                     ▼
      ┌─────────────┐       ┌─────────────┐
      │ Superseded  │       │    Voided   │
      └─────────────┘       └─────────────┘
```

* `Active` transactions participate in position calculations.
* `Superseded` transactions are retained for historical traceability but are excluded from effective calculations.
* `Voided` transactions are retained for historical traceability but are excluded from effective calculations.

Posted transactions are not hard-deleted.

### Position

Position represents the derived/materialized current state of an instrument within a portfolio.

A position can be recalculated from transaction history when historical transactions change.

### Cost Basis

The current implementation uses weighted-average cost.

Key rules:

* BUY fee increases cost basis.
* SELL fee reduces net proceeds.
* Market price does not change cost basis.
* Realized P&L and unrealized P&L are calculated separately.
* Historical transaction corrections trigger deterministic recalculation.
* Voiding an effective transaction triggers deterministic recalculation.
* Posted transactions are not hard-deleted.

### Transaction Ordering

Transactions are processed deterministically using:

```text
TransactionDate
        +
Sequence
```

`TransactionDate` represents the business date.

`CreatedAt` represents the system timestamp when the transaction was recorded.

For transaction history listing, records are returned in deterministic reverse chronological order using:

```text
TransactionDate DESC
Sequence DESC
CreatedAt DESC
Id DESC
```

---

## Transaction Corrections and Voiding

TradeLens uses an append/supersede approach for transaction corrections.

```text
Original Transaction
        │
        │ correction
        ▼
Original → Superseded

New Corrected Transaction
        │
        ▼
Active
```

This preserves the historical record while allowing the portfolio position to be recalculated from the corrected transaction history.

### Transaction Void

An active transaction can also be voided when it should no longer affect portfolio calculations.

```text
Active Transaction
        │
        │ void
        ▼
Voided Transaction
```

The original transaction remains in transaction history with:

* `Status = Voided`
* `VoidReason`

Voided transactions are automatically excluded from effective position calculations.

Voiding a transaction recalculates the affected position from the remaining effective transaction history.

---

## Market Price Model

Market price is modeled independently from position ownership and cost basis.

```text
Instrument
├── Id
├── Symbol
├── Name
└── Currency
       │
       │ 1:N
       ▼
MarketPrice
├── Id
├── InstrumentId
├── Price
├── PriceTimestamp
└── Source
```

Market price represents an external market-data observation.

It does not modify:

* Position quantity
* Cost basis
* Average acquisition price
* Historical transaction records

This separation allows market prices to change independently from portfolio ownership.

---

## Portfolio Valuation

TradeLens supports both position-level and portfolio-level valuation.

### Position Valuation

Position valuation calculates:

* Current quantity
* Cost basis
* Average price
* Market price
* Market value
* Unrealized P&L
* Unrealized P&L percentage

Endpoint:

```text
GET /api/v1/portfolios/{portfolioId}/positions/{instrumentId}/valuation
```

A missing or stale market price results in a `400 Bad Request` for single-position valuation because a reliable valuation cannot be produced for that position.

### Portfolio Valuation

Portfolio valuation aggregates all current positions within a portfolio.

Endpoint:

```text
GET /api/v1/portfolios/{portfolioId}/valuation
```

The response contains:

* Portfolio status
* Total cost basis
* Total market value
* Total unrealized P&L
* Total unrealized P&L percentage
* Per-position valuation results
* Per-position market price status

### Portfolio Valuation Status

Portfolio valuation supports two states:

```text
COMPLETE
PARTIAL
```

`COMPLETE` is returned when every position has a fresh market price.

`PARTIAL` is returned when one or more positions have either:

* No market price
* A stale market price

For partial valuation:

* Total cost basis remains available.
* Per-position results with fresh prices remain available.
* Positions with missing or stale prices contain nullable valuation fields.
* Portfolio-level market value and P&L totals are returned as `null`.

This prevents incomplete partial sums from being presented as a complete portfolio valuation.

### Market Price Status

Each portfolio position valuation contains one of:

```text
FRESH
STALE
NOT_AVAILABLE
```

The same reference time is used for all freshness checks during a single portfolio valuation request.

Portfolio-level unrealized P&L percentage is calculated from total portfolio cost basis:

```text
Total Unrealized P&L
-------------------- × 100
Total Cost Basis
```

It is not calculated as the arithmetic average of individual position percentages.

---

## Market Price Integration

TradeLens uses an abstraction-based market price provider architecture so that market data sources can be replaced without changing application-level business logic.

```text
                    IMarketPriceProvider
                           │
             ┌─────────────┴─────────────┐
             ▼                           ▼
YahooFinanceMarketPriceProvider    IdxMarketPriceProvider
       Development / Demo                 Future
```

The current Yahoo Finance provider is intended for development and demonstration purposes. It should not be considered a production commercial market-data source without verifying the applicable licensing and usage terms.

The application layer depends only on `IMarketPriceProvider`, keeping the domain and application layers independent from the external market-data source.

### Market Price Synchronization

Market prices are synchronized through:

```text
IMarketPriceProvider
        ↓
MarketPriceSyncService
        ↓
MarketPriceRepository
        ↓
PostgreSQL
```

A background service periodically triggers synchronization.

Current development configuration:

```text
Sync interval: 5 minutes
Maximum market price age: 30 minutes
```

The freshness threshold is configurable and enforced at the application layer.

### HTTP Resilience

The Yahoo Finance HTTP client uses the standard .NET resilience handler for transient HTTP failures, including retry behavior.

Example scenario covered by integration tests:

```text
HTTP 503
   ↓ retry
HTTP 503
   ↓ retry
HTTP 200
   ↓
Market price successfully retrieved
```

Provider failures for individual symbols are handled without preventing other symbols from being processed.

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

POST /api/v1/transactions/{id}/corrections

POST /api/v1/transactions/{id}/void

GET  /api/v1/portfolios/{portfolioId}/transactions

GET  /api/v1/portfolios/{portfolioId}/positions/{instrumentId}/valuation

GET  /api/v1/portfolios/{portfolioId}/valuation
```

### Transaction Listing

Transaction history is exposed in portfolio context:

```text
GET /api/v1/portfolios/{portfolioId}/transactions
```

The endpoint supports pagination:

```text
page
pageSize
```

`pageSize` is limited to a maximum of 100.

The history endpoint includes active, superseded, and voided transactions because it represents historical records rather than only effective transactions.

### Planned

```text
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

| Category         | HTTP Status | Example Code                     |
| ---------------- | ----------: | -------------------------------- |
| Validation       |         400 | `VALIDATION_ERROR`               |
| Business Rule    |         400 | `POSITION_INSUFFICIENT_QUANTITY` |
| Market Price     |         400 | `MARKET_PRICE_NOT_AVAILABLE`     |
| Market Price     |         400 | `MARKET_PRICE_STALE`             |
| Authentication   |         401 | `AUTHENTICATION_REQUIRED`        |
| Authorization    |         403 | `PORTFOLIO_ACCESS_DENIED`        |
| Not Found        |         404 | `TRANSACTION_NOT_FOUND`          |
| Conflict         |         409 | `IDEMPOTENCY_CONFLICT`           |
| Concurrency      |         409 | `POSITION_CONCURRENCY_CONFLICT`  |
| Unexpected Error |         500 | `INTERNAL_ERROR`                 |

The API uses stable machine-readable error codes so clients do not need to parse human-readable error messages.

---

## Technology Stack

* .NET 8
* ASP.NET Core Web API
* C#
* Entity Framework Core
* PostgreSQL
* Docker
* Microsoft.Extensions.Http.Resilience
* xUnit
* FluentAssertions
* FluentValidation
* Swagger / OpenAPI
* JWT Bearer Authentication

---

## Reliability and Security

TradeLens includes several reliability and security mechanisms.

### Idempotency

Transaction creation supports idempotency through:

* `Idempotency-Key`
* Request hash validation
* Unique database constraint
* Atomic idempotency persistence
* Concurrent duplicate request handling
* Idempotency conflict detection

The idempotency record and business transaction are persisted atomically.

The implementation handles both:

* Sequential replay using the same idempotency key and payload
* Concurrent requests using the same idempotency key

A repeated request with the same key and same payload returns the original transaction result.

A request using an existing key with a different payload is rejected as an idempotency conflict.

### Optimistic Concurrency

Positions use an EF Core concurrency token.

Concurrent updates are detected and mapped to an application-level concurrency exception and HTTP `409 Conflict`.

### Authentication

The application supports:

* Development authentication for local testing
* Production JWT Bearer authentication
* JWT issuer validation
* JWT audience validation
* JWT signing-key validation
* JWT lifetime validation

Invalid JWTs result in `401 Unauthorized`.

A valid authenticated user attempting to access another user's portfolio receives `403 Forbidden`.

### HTTP Resilience

External market price requests use standard .NET HTTP resilience capabilities to handle transient failures.

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

* .NET 8 SDK
* Docker Desktop
* Git

### Clone

```bash
git clone <repository-url>
cd TradeLens
```

### Start PostgreSQL

```bash
docker compose -f docker/docker-compose.yml up -d
```

### Restore Dependencies

```bash
dotnet restore
```

### Build

```bash
dotnet build
```

### Apply Database Migrations

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

The automated test suite currently covers:

* Domain transaction rules
* Position calculations
* Realized P&L calculations
* Add Transaction use case
* Transaction correction
* Transaction void
* Transaction history listing
* Validation behavior
* Oversell business rules
* Idempotency
* Sequential idempotency replay
* Concurrent duplicate requests
* Optimistic concurrency
* Portfolio ownership authorization
* JWT authentication
* Position valuation
* Portfolio-level valuation
* Complete portfolio valuation
* Partial portfolio valuation
* Missing market price handling
* Stale market price handling
* Market price persistence
* Market price synchronization
* External market price provider mapping
* Market price provider error handling
* HTTP resilience and retry behavior
* API integration
* PostgreSQL integration

### Current Test Suite

```text
Domain:       32 tests
Application:  86 tests
Integration:  41 tests
------------------------
Total:       159 tests
```

The full solution test suite is currently passing.

---

## Design Decisions

### Why PostgreSQL?

PostgreSQL provides strong relational consistency and is sufficient for the transactional workload of the MVP without introducing unnecessary infrastructure complexity.

### Why Repository Pattern?

Repositories provide business-oriented persistence abstractions and prevent the Application and Domain layers from depending directly on EF Core.

### Why No Generic Repository?

A generic CRUD abstraction can hide business-oriented access patterns.

TradeLens uses repositories designed around actual use-case requirements.

### Why No Event Sourcing?

TradeLens requires deterministic recalculation from transaction history, but does not require the operational and architectural complexity of full event sourcing for the MVP.

### Why Is Transaction History the Source of Truth?

Transaction history represents the historical business events that determine portfolio ownership and cost basis.

Position is therefore treated as a materialized current state rather than the authoritative historical record.

This allows positions to be recalculated when historical transactions are corrected or voided.

### Why Is Position Derived?

Position represents current ownership and cost-basis state for an instrument within a portfolio.

Keeping position derived from transaction history provides:

* Deterministic recalculation
* Correct handling of historical corrections
* Correct handling of transaction voids
* Efficient access to current holdings
* Separation between historical facts and current materialized state

### Why Is MarketPrice Independent from Position?

Market prices change independently from ownership and cost basis.

Keeping market price as a separate domain concept prevents market-data updates from mutating portfolio accounting state.

This also allows the market-data provider to change without changing the core valuation model.

### Why Partial Portfolio Valuation?

A portfolio can contain multiple positions, and market data availability may differ between instruments.

Instead of failing the entire portfolio valuation when one price is unavailable or stale, TradeLens returns a `PARTIAL` result.

However, incomplete market-value and P&L totals are not presented as complete portfolio totals.

This allows clients to display useful per-position information while clearly communicating that the portfolio-level valuation is incomplete.

### Why No Microservices?

The current domain and workload do not justify distributed-system complexity.

TradeLens is intentionally designed as a modular monolith that can evolve if future requirements require service decomposition.

### Why Lightweight CQRS?

Commands and queries have different responsibilities, but a full CQRS infrastructure is unnecessary for the current scope.

TradeLens therefore separates command and query use cases without introducing separate databases or messaging infrastructure.

### Why External Market Price Provider Abstraction?

Market data providers may change because of:

* Data availability
* Licensing
* Cost
* Reliability
* Market coverage

TradeLens therefore depends on `IMarketPriceProvider` rather than coupling application logic directly to a specific external provider.

---

## Current Progress

### Completed

#### Core Domain

* [x] Transaction domain model
* [x] Position domain model
* [x] Weighted-average cost basis
* [x] Realized P&L calculation
* [x] Unrealized P&L calculation
* [x] Deterministic transaction processing
* [x] Historical transaction correction
* [x] Transaction void lifecycle

#### Persistence

* [x] EF Core persistence
* [x] PostgreSQL setup
* [x] Database migrations
* [x] Repository pattern
* [x] Unit of Work

#### Transaction Use Cases

* [x] Add Transaction
* [x] Get Transaction
* [x] Transaction listing
* [x] Transaction correction
* [x] Transaction void
* [x] Transaction validation
* [x] Oversell business rule
* [x] Deterministic position recalculation

#### Reliability

* [x] Idempotency-Key support
* [x] Request hash validation
* [x] Atomic idempotency persistence
* [x] Sequential idempotency replay
* [x] Concurrent duplicate request handling
* [x] Idempotency conflict handling
* [x] Optimistic concurrency
* [x] EF Core concurrency token
* [x] Concurrent update detection
* [x] HTTP 409 conflict mapping

#### Authentication and Authorization

* [x] Development authentication handler
* [x] Production JWT Bearer authentication
* [x] JWT issuer validation
* [x] JWT audience validation
* [x] JWT signing-key validation
* [x] JWT lifetime validation
* [x] Portfolio ownership authorization
* [x] Server-side ownership verification
* [x] HTTP 401 authentication handling
* [x] HTTP 403 authorization handling

#### Market Price

* [x] MarketPrice domain entity
* [x] EF Core persistence
* [x] Market price migration
* [x] Market price repository
* [x] Latest market price query
* [x] Market price provider abstraction
* [x] Yahoo Finance development/demo provider
* [x] IDX symbol mapping
* [x] Latest available price extraction
* [x] Provider-level error handling
* [x] Application-level synchronization service
* [x] Market price synchronization job
* [x] Background synchronization service
* [x] Configurable synchronization interval
* [x] Unknown instrument handling
* [x] HTTP resilience and retry strategy
* [x] Market price freshness policy
* [x] Configurable maximum market price age
* [x] Future timestamp rejection
* [x] Missing market price handling
* [x] Stale market price handling

#### Valuation

* [x] Position valuation
* [x] Position valuation API
* [x] Portfolio-level valuation
* [x] Portfolio valuation aggregation
* [x] Portfolio valuation API
* [x] Complete valuation
* [x] Partial valuation
* [x] Per-position price status
* [x] Total cost basis
* [x] Total market value
* [x] Total unrealized P&L
* [x] Total unrealized P&L percentage
* [x] Same reference time for portfolio freshness checks
* [x] Portfolio ownership enforcement for valuation

#### Testing

* [x] Domain unit tests
* [x] Application unit tests
* [x] Integration tests
* [x] PostgreSQL integration
* [x] Authentication integration tests
* [x] Authorization integration tests
* [x] Transaction listing integration tests
* [x] Transaction correction integration tests
* [x] Transaction void integration tests
* [x] Idempotency integration tests
* [x] Market price integration tests
* [x] HTTP resilience integration tests
* [x] Portfolio valuation integration tests
* [x] Full solution regression testing

---

## In Progress

* Structured logging
* Traceability and correlation IDs
* Production observability
* Additional API completeness
* Portfolio analytics expansion

---

## Planned

* Market-hours-aware synchronization strategy
* Health checks
* Monitoring
* Production Docker deployment
* Configuration management
* CI/CD
* Operational documentation
* Performance and scalability improvements
* Additional portfolio analytics
* Web dashboard
* Portfolio performance history
* Portfolio allocation analytics
* P&L analytics API

---

## Roadmap

### Phase 1 — Core Domain

* [x] Transaction
* [x] Position
* [x] Cost basis
* [x] Realized P&L
* [x] Unrealized P&L

### Phase 2 — Transaction Use Cases

* [x] Add transaction
* [x] Query transaction
* [x] Transaction correction
* [x] Transaction void
* [x] Deterministic recalculation

### Phase 3 — API

* [x] REST API
* [x] Validation
* [x] Error contract
* [x] Problem Details
* [x] Authentication
* [x] Authorization
* [x] Transaction listing API

### Phase 4 — Market Price and Valuation

* [x] Market price persistence
* [x] Market price repository
* [x] Latest market price retrieval
* [x] Market price provider abstraction
* [x] External market price provider
* [x] Market price synchronization service
* [x] Background market price synchronization
* [x] HTTP resilience
* [x] Market price freshness policy
* [x] Position valuation
* [x] Portfolio-level valuation
* [x] Partial portfolio valuation
* [x] Portfolio valuation API

### Phase 5 — Reliability, Security & Observability

* [x] Idempotency
* [x] Optimistic concurrency
* [x] Portfolio ownership authorization
* [x] Authentication hardening
* [x] HTTP resilience
* [x] Integration testing
* [ ] Structured logging
* [ ] Traceability
* [ ] Health checks
* [ ] Monitoring

### Phase 6 — API Completeness

* [x] Transaction creation API
* [x] Transaction retrieval API
* [x] Transaction correction API
* [x] Transaction void API
* [x] Transaction listing API
* [x] Position valuation API
* [x] Portfolio valuation API
* [ ] Position listing API
* [ ] Portfolio API
* [ ] P&L analytics API

### Phase 7 — Web Portfolio Dashboard

* [ ] Login
* [ ] Portfolio dashboard
* [ ] Portfolio valuation summary
* [ ] Position table
* [ ] Market price freshness indicators
* [ ] Position detail
* [ ] Transaction history

### Phase 8 — Portfolio Analytics

* [ ] Performance history
* [ ] Daily portfolio change
* [ ] Allocation analysis
* [ ] Realized P&L analytics
* [ ] Total P&L analytics
* [ ] Historical portfolio valuation

### Phase 9 — Production Readiness

* [ ] Production Docker deployment
* [ ] Configuration management
* [ ] Health checks
* [ ] Monitoring
* [ ] CI/CD
* [ ] Operational documentation
* [ ] Production observability

### Phase 10 — Advanced Portfolio Features

* [ ] Multiple broker account consolidation
* [ ] Corporate actions
* [ ] Stock split
* [ ] Reverse split
* [ ] Rights issue
* [ ] Bonus shares
* [ ] Dividend
* [ ] Corporate action cancellation and delay handling

---

## License

TBD
