# TradeLens

## Overview
TradeLens is a portfolio and P&L analytics platform for retail investors, designed to track stock transactions, calculate portfolio positions, and analyze realized and unrealized profit and loss.
TradeLens is an analytics platform, not a trading execution system.

## Problem Statement

## Goals
- Track historical stock transactions
- Calculate current portfolio positions
- Calculate weighted-average cost basis
- Calculate realized P&L
- Calculate unrealized P&L
- Support transaction corrections
- Support multiple broker accounts
- Provide deterministic recalculation
- Demonstrate clean architecture and domain-driven design

## Key Features
- [x] Transaction domain model
- [x] Position calculation
- [x] Realized P&L calculation
- [x] PostgreSQL persistence
- [x] EF Core
- [x] Repository abstraction
- [x] Unit of Work
- [x] Transaction validation
- [ ] Add Transaction API
- [ ] Transaction correction
- [ ] Portfolio valuation
- [ ] Authentication

## Architecture
                ┌─────────────────┐
                │   TradeLens API │
                └────────┬────────┘
                         │
                         ▼
                ┌─────────────────┐
                │   Application   │
                │ Commands/Query  │
                └────────┬────────┘
                         │
                         ▼
                ┌─────────────────┐
                │     Domain      │
                │ Position / P&L  │
                └────────┬────────┘
                         │
                         ▼
                ┌─────────────────┐
                │ Infrastructure  │
                │ EF Core / DB    │
                └────────┬────────┘
                         │
                         ▼
                ┌─────────────────┐
                │   PostgreSQL    │
                └─────────────────┘
        
API
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application
 ↓
Domain    

The Domain layer does not depend on Infrastructure or API concerns.

## Domain Model
User
Portfolio
BrokerAccount
Instrument
Transaction
Position
MarketPrice

Portfolio
 ├── BrokerAccount
 ├── Transaction
 └── Position

Instrument
 ├── Transaction
 └── Position

Transaction History = Source of Truth

Position = Derived / Materialized Current State

Market Price does not change Cost Basis

BUY fee increases Cost Basis

SELL fee reduces Net Proceeds

Realized P&L and Unrealized P&L are calculated separately

Historical corrections trigger deterministic recalculation

Posted transactions are not hard-deleted

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

## Project Structure
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
├── docs/
├── README.md
└── .gitignore

## Getting Started
Prerequisites

- .NET 8 SDK
- Docker Desktop
- Git

git clone <repository>
cd TradeLens/Implementation

docker compose -f docker/docker-compose.yml up -d

dotnet restore
dotnet build

dotnet ef database update

dotnet run --project src/TradeLens.Api

## Database
PostgreSQL run on docker, with setup :
Host: localhost
Port: 5433
Database: tradelens

## API
POST   /api/v1/transactions
GET    /api/v1/transactions
GET    /api/v1/transactions/{id}

POST   /api/v1/transactions/{id}/corrections

GET    /api/v1/positions
GET    /api/v1/pnl
GET    /api/v1/portfolios

## Testing
Powershell command
    dotnet test

The project uses unit tests for domain calculations and application validation, with integration tests planned for API and PostgreSQL interaction.

## Design Decisions

**Why PostgreSQL?**

PostgreSQL provides relational consistency and is sufficient for the transactional workload of the MVP without introducing unnecessary infrastructure complexity.

**Why no Generic Repository?**

Repositories are defined around business-oriented access patterns rather than exposing a generic CRUD abstraction.

**Why no Event Sourcing?**

TradeLens requires deterministic recalculation from transaction history, but does not require the operational and architectural complexity of full event sourcing for the MVP.

**Why Position is derived?**

Transaction history represents the source of truth. Position is a materialized current state that can be recalculated when historical transactions are corrected.

## Current Status
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

- Unit tests for domain calculations

- Unit tests for Add Transaction use case

### In Progress

- Transaction API

### Planned

- Transaction correction
- Idempotency
- Optimistic concurrency
- Portfolio valuation
- Authentication / authorization
- Integration tests

## Roadmap
- Phase 1 — Core Domain
- Phase 2 — Transaction Use Cases
- Phase 3 — API
- Phase 4 — Valuation
- Phase 5 — Security
- Phase 6 — Integration & Observability
- Phase 7 — Production Readiness

## License