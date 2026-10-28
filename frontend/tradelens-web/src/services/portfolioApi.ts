export interface PortfolioItem {
  portfolioId: string
  name: string
}

export interface PortfolioListResponse {
  items: PortfolioItem[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface PortfolioPositionValuation {
  instrumentId: string
  quantity: number
  costBasis: number
  averagePrice: number
  marketPrice: number | null
  marketValue: number | null
  unrealizedPnl: number | null
  unrealizedPnlPercentage: number | null
  priceStatus: string
}

export interface PortfolioValuation {
  portfolioId: string
  status: string
  totalCostBasis: number
  totalMarketValue: number | null
  totalUnrealizedPnl: number | null
  totalUnrealizedPnlPercentage: number | null
  positions: PortfolioPositionValuation[]
}

export interface PortfolioPnl {
  portfolioId: string
  asOf: string
  status: string
  realizedPnl: number
  unrealizedPnl: number | null
  totalPnl: number | null
}

const API_BASE_URL = '/api/v1'

export async function getPortfolios(page = 1, pageSize = 20): Promise<PortfolioListResponse> {
  const response = await fetch(`${API_BASE_URL}/portfolios?page=${page}&pageSize=${pageSize}`)

  if (!response.ok) {
    throw new Error(`Gagal mengambil portfolio (HTTP ${response.status}).`)
  }

  return (await response.json()) as PortfolioListResponse
}

export async function getPortfolioValuation(portfolioId: string): Promise<PortfolioValuation> {
  const response = await fetch(`${API_BASE_URL}/portfolios/${portfolioId}/valuation`)

  if (!response.ok) {
    throw new Error(`Gagal mengambil valuation (HTTP ${response.status}).`)
  }

  return (await response.json()) as PortfolioValuation
}

export async function getPortfolioPnl(portfolioId: string): Promise<PortfolioPnl> {
  const response = await fetch(`${API_BASE_URL}/portfolios/${portfolioId}/pnl`)

  if (!response.ok) {
    throw new Error(`Gagal mengambil P&L (HTTP ${response.status}).`)
  }

  return (await response.json()) as PortfolioPnl
}
