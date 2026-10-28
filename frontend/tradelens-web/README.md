# TradeLens Web

The web frontend for **TradeLens**, an API-first portfolio tracking application built with Vue 3 and TypeScript.

TradeLens Web provides a user interface for viewing investment portfolios and reviewing portfolio valuation and profit and loss (P&L) information provided by the TradeLens backend API.

> **Development status:** The application is under active development. Portfolio listing, portfolio selection, valuation summaries, and P&L summaries are implemented. Portfolio positions, additional portfolio management workflows, and production readiness remain part of the roadmap.

## Current Features

- **Portfolio Dashboard** — Retrieve and display portfolios from the backend API.
- **Portfolio Selection** — Select a portfolio for analysis.
- **Portfolio Valuation** — Display cost basis and available market valuation data.
- **Profit and Loss (P&L)** — Display realized P&L, unrealized P&L, and total P&L when available.
- **Loading and Error Handling** — Display loading indicators, error messages, and retry actions.
- **Unavailable Data Handling** — Distinguish unavailable financial values from numeric zero.

Portfolio calculations and business rules remain the responsibility of the backend. The frontend consumes API responses and presents the results to users.

## Technology Stack

- Vue 3
- TypeScript
- Vue Router
- Vite
- Vitest
- ESLint
- Oxlint

## Project Structure

```text
tradelens-web/
├── public/
├── src/
│   ├── router/
│   │   └── index.ts
│   ├── services/
│   │   └── portfolioApi.ts
│   ├── views/
│   │   └── DashboardView.vue
│   ├── __tests__/
│   │   └── App.spec.ts
│   ├── App.vue
│   └── main.ts
├── index.html
├── package.json
├── vite.config.ts
└── README.md
```

The structure above highlights the main application areas. Additional configuration files and directories may exist in the repository.

### Key Areas

- `src/router/` — Application routing configuration.
- `src/services/` — Backend API communication and response types.
- `src/views/` — Application pages and views.
- `src/__tests__/` — Frontend unit tests.
- `App.vue` — Root application component.
- `main.ts` — Application entry point.
- `vite.config.ts` — Vite development and build configuration.

## Prerequisites

Install the following tools before starting development:

- Node.js `^22.18.0` or `>=24.12.0`, as specified in `package.json`.
- npm.
- Git.
- A running TradeLens backend API for testing API-dependent functionality.

## Getting Started

Run the following commands from the `frontend/tradelens-web` directory.

### 1. Install Dependencies

```bash
npm install
```

### 2. Start the Development Server

```bash
npm run dev
```

Vite will display the local development URL in the terminal.

### 3. Build for Production

```bash
npm run build
```

This command runs TypeScript project checking and creates an optimized production build in the `dist/` directory.

### 4. Preview the Production Build

```bash
npm run preview
```

This starts a local server for previewing the production build.

## Testing and Code Quality

Run the following checks from the `frontend/tradelens-web` directory.

### Unit Tests

```bash
npm run test:unit -- --run
```

### ESLint

```bash
npm exec -- eslint .
```

### Oxlint

```bash
npm exec -- oxlint .
```

### Production Build and Type Checking

```bash
npm run build
```

The production build includes TypeScript project checking before creating the build output.

The frontend test suite is being expanded alongside feature development. Build and lint checks complement unit tests but do not replace functional or integration testing.

## Backend Integration

TradeLens Web communicates with the backend through versioned API endpoints.

The current dashboard uses the following endpoints:

| Endpoint                                         | Purpose                      |
| ------------------------------------------------ | ---------------------------- |
| `GET /api/v1/portfolios`                         | Retrieve the portfolio list  |
| `GET /api/v1/portfolios/{portfolioId}/valuation` | Retrieve portfolio valuation |
| `GET /api/v1/portfolios/{portfolioId}/pnl`       | Retrieve portfolio P&L       |

The frontend expects the backend API to return response fields compatible with the corresponding TypeScript interfaces in `src/services/portfolioApi.ts`.

### API Configuration

The frontend currently uses the `/api/v1` API path. Check `vite.config.ts` for the development server proxy configuration and the backend URL used during local development.

For production deployment, configure frontend and backend routing appropriately for the target environment. Do not place secrets or private credentials in frontend source code.

## Development Roadmap

### Completed

- [x] Initialize the Vue 3 and TypeScript application with Vite.
- [x] Establish the frontend project structure and development tooling.
- [x] Configure unit testing and linting tools.
- [x] Retrieve and display portfolios from the backend API.
- [x] Implement portfolio selection.
- [x] Integrate portfolio valuation and P&L APIs.
- [x] Display valuation and P&L summary information.
- [x] Implement loading, error, and retry states.
- [x] Handle unavailable financial values without treating them as zero.
- [x] Validate dashboard behavior through manual testing.

### Planned

- [ ] Display portfolio positions and instrument-level valuation.
- [ ] Handle empty positions and unavailable market prices.
- [ ] Implement instrument discovery and selection.
- [ ] Implement transaction management workflows.
- [ ] Implement the corporate actions interface.
- [ ] Add authentication and complete navigation workflows.
- [ ] Expand unit and integration test coverage.
- [ ] Configure production deployment.
- [ ] Perform launch verification.

The roadmap reflects the current development plan and may change as implementation progresses.

## Development Guidelines

- Keep portfolio calculations and business rules in the backend.
- Use typed API response interfaces in the frontend.
- Handle loading, empty, error, and unavailable-data states explicitly.
- Validate changes with the production build, unit tests, and linting tools.
- Keep changes scoped to a feature and commit them through the project's Git workflow.

## License

Refer to the repository's root README and license file for project-level licensing information.
