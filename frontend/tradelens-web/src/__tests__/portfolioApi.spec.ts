import { afterEach, describe, expect, it, vi } from 'vitest'
import { getPortfolioPositions } from '../services/portfolioApi'

describe('getPortfolioPositions', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('requests positions using the portfolio ID and pagination parameters', async () => {
    const responseBody = {
      portfolioId: 'portfolio-123',
      items: [
        {
          positionId: 'position-123',
          instrumentId: 'instrument-456',
          quantity: 100,
          costBasis: 1_000_000,
          averagePrice: 10_000,
        },
      ],
      page: 2,
      pageSize: 10,
      totalCount: 15,
      totalPages: 2,
    }

    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue({
      ok: true,
      json: async () => responseBody,
    } as Response)

    vi.stubGlobal('fetch', fetchMock)

    const result = await getPortfolioPositions('portfolio-123', 2, 10)

    expect(fetchMock).toHaveBeenCalledExactlyOnceWith(
      '/api/v1/portfolios/portfolio-123/positions?page=2&pageSize=10',
    )
    expect(result).toEqual(responseBody)
    expect(result.items).toHaveLength(1)
    expect(result.items[0]?.instrumentId).toBe('instrument-456')
    expect(result.totalPages).toBe(2)
  })

  it('uses the default pagination parameters when omitted', async () => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue({
      ok: true,
      json: async () => ({
        portfolioId: 'portfolio-123',
        items: [],
        page: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      }),
    } as Response)

    vi.stubGlobal('fetch', fetchMock)

    await getPortfolioPositions('portfolio-123')

    expect(fetchMock).toHaveBeenCalledExactlyOnceWith(
      '/api/v1/portfolios/portfolio-123/positions?page=1&pageSize=20',
    )
  })

  it('throws an error when the API responds with a non-success status', async () => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue({
      ok: false,
      status: 500,
    } as Response)

    vi.stubGlobal('fetch', fetchMock)

    await expect(getPortfolioPositions('portfolio-123')).rejects.toThrow(
      'Gagal mengambil posisi portfolio (HTTP 500).',
    )
  })
})
