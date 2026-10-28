import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import DashboardView from '../views/DashboardView.vue'
import {
  getPortfolios,
  getPortfolioPositions,
  getPortfolioValuation,
  getPortfolioPnl,
} from '../services/portfolioApi'

vi.mock('../services/portfolioApi', () => ({
  getPortfolios: vi.fn<typeof getPortfolios>(),
  getPortfolioPositions: vi.fn<typeof getPortfolioPositions>(),
  getPortfolioValuation: vi.fn<typeof getPortfolioValuation>(),
  getPortfolioPnl: vi.fn<typeof getPortfolioPnl>(),
}))

const portfolioA = {
  portfolioId: 'portfolio-a',
  name: 'Portfolio A',
}

const portfolioB = {
  portfolioId: 'portfolio-b',
  name: 'Portfolio B',
}

const positionsA = {
  portfolioId: 'portfolio-a',
  items: [
    {
      positionId: 'position-a1',
      instrumentId: 'instrument-a1',
      quantity: 100,
      costBasis: 1_000_000,
      averagePrice: 10_000,
    },
  ],
  page: 1,
  pageSize: 10,
  totalCount: 1,
  totalPages: 1,
}

const emptyValuation = {
  portfolioId: 'portfolio-a',
  status: 'COMPLETE',
  totalCostBasis: 0,
  totalMarketValue: 0,
  totalUnrealizedPnl: 0,
  totalUnrealizedPnlPercentage: 0,
  positions: [],
}

const emptyPnl = {
  portfolioId: 'portfolio-a',
  asOf: '2026-10-28T00:00:00Z',
  status: 'COMPLETE',
  realizedPnl: 0,
  unrealizedPnl: 0,
  totalPnl: 0,
}

function mountDashboard() {
  return mount(DashboardView)
}

describe('DashboardView portfolio positions', () => {
  beforeEach(() => {
    vi.resetAllMocks()

    vi.mocked(getPortfolios).mockResolvedValue({
      items: [portfolioA, portfolioB],
      page: 1,
      pageSize: 20,
      totalCount: 2,
      totalPages: 1,
    })

    vi.mocked(getPortfolioPositions).mockImplementation(
      async (portfolioId, page = 1, pageSize = 20) => ({
        ...(portfolioId === 'portfolio-a'
          ? positionsA
          : {
              portfolioId,
              items: [],
              page,
              pageSize,
              totalCount: 0,
              totalPages: 0,
            }),
        page,
        pageSize,
      }),
    )

    vi.mocked(getPortfolioValuation).mockResolvedValue(emptyValuation)
    vi.mocked(getPortfolioPnl).mockResolvedValue(emptyPnl)
  })

  it('loads and displays positions after a portfolio is selected', async () => {
    const wrapper = mountDashboard()
    await flushPromises()

    await wrapper.get('.portfolio-item').trigger('click')
    await flushPromises()

    expect(getPortfolioPositions).toHaveBeenCalledWith('portfolio-a', 1, 10)
    expect(wrapper.text()).toContain('Portfolio Positions')
    expect(wrapper.text()).toContain('instrument-a1')
    expect(wrapper.text()).toContain('1.000.000')
    expect(wrapper.text()).toContain('10.000')
  })

  it('shows an empty state when the selected portfolio has no positions', async () => {
    const wrapper = mountDashboard()
    await flushPromises()

    await wrapper.findAll('.portfolio-item')[1]!.trigger('click')
    await flushPromises()

    expect(wrapper.text()).toContain('Belum ada posisi')
    expect(wrapper.text()).toContain('Portfolio B')
  })

  it('shows an error and allows retrying the positions request', async () => {
    vi.mocked(getPortfolioPositions)
      .mockRejectedValueOnce(new Error('Positions API unavailable'))
      .mockResolvedValueOnce(positionsA)

    const wrapper = mountDashboard()
    await flushPromises()

    await wrapper.get('.portfolio-item').trigger('click')
    await flushPromises()

    expect(wrapper.text()).toContain('Positions API unavailable')

    await wrapper.get('.error-panel .refresh-button').trigger('click')
    await flushPromises()

    expect(getPortfolioPositions).toHaveBeenCalledTimes(2)
    expect(wrapper.text()).toContain('instrument-a1')
  })

  it('requests the next page when the next button is clicked', async () => {
    vi.mocked(getPortfolioPositions).mockResolvedValue({
      ...positionsA,
      page: 1,
      pageSize: 10,
      totalCount: 11,
      totalPages: 2,
    })

    const wrapper = mountDashboard()
    await flushPromises()

    await wrapper.get('.portfolio-item').trigger('click')
    await flushPromises()

    await wrapper.get('.pagination-buttons button:last-child').trigger('click')
    await flushPromises()

    expect(getPortfolioPositions).toHaveBeenLastCalledWith('portfolio-a', 2, 10)
    expect(wrapper.text()).toContain('Halaman 2 dari 2')
  })

  it('loads positions for the newly selected portfolio', async () => {
    const wrapper = mountDashboard()
    await flushPromises()

    await wrapper.get('.portfolio-item').trigger('click')
    await flushPromises()

    await wrapper.findAll('.portfolio-item')[1]!.trigger('click')
    await flushPromises()

    expect(getPortfolioPositions).toHaveBeenLastCalledWith('portfolio-b', 1, 10)
    expect(wrapper.text()).not.toContain('instrument-a1')
  })
})
