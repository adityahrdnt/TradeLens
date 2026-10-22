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

const API_BASE_URL = '/api/v1'

export async function getPortfolios(page = 1, pageSize = 20): Promise<PortfolioListResponse> {
  const response = await fetch(`${API_BASE_URL}/portfolios?page=${page}&pageSize=${pageSize}`)

  if (!response.ok) {
    throw new Error(`Gagal mengambil portfolio (HTTP ${response.status}).`)
  }

  return (await response.json()) as PortfolioListResponse
}
