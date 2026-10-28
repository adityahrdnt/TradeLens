<script setup lang="ts">
  import { computed, onMounted, ref, watch } from 'vue'
  import {
    getPortfolios,
    getPortfolioValuation,
    getPortfolioPnl,
    type PortfolioItem,
    type PortfolioValuation,
    type PortfolioPnl,
  } from '@/services/portfolioApi'

  const portfolios = ref<PortfolioItem[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const errorMessage = ref('')
  const selectedPortfolioId = ref<string | null>(null)

  const valuation = ref<PortfolioValuation | null>(null)
  const pnl = ref<PortfolioPnl | null>(null)
  const isLoadingDetails = ref(false)
  const detailsErrorMessage = ref('')

  const selectedPortfolio = computed(
    () =>
      portfolios.value.find(
        (portfolio) =>
          portfolio.portfolioId === selectedPortfolioId.value,
      ) ?? null,
  )

  async function loadPortfolioDetails(portfolioId: string) {
    valuation.value = null
    pnl.value = null
    detailsErrorMessage.value = ''
    isLoadingDetails.value = true

    try {
      const [valuationResult, pnlResult] = await Promise.all([
        getPortfolioValuation(portfolioId),
        getPortfolioPnl(portfolioId),
      ])

      return { valuationResult, pnlResult, error: null }
    } catch (error) {
      return {
        valuationResult: null,
        pnlResult: null,
        error:
          error instanceof Error
            ? error.message
            : 'Terjadi kesalahan saat mengambil valuasi dan P&L.',
      }
    } finally {
      isLoadingDetails.value = false
    }
  }

  async function reloadPortfolioDetails() {
    const portfolioId = selectedPortfolioId.value
    if (!portfolioId) return

    const result = await loadPortfolioDetails(portfolioId)

    if (selectedPortfolioId.value !== portfolioId) return

    if (result.error) {
      detailsErrorMessage.value = result.error
      return
    }

    valuation.value = result.valuationResult
    pnl.value = result.pnlResult
  }

  async function loadPortfolios() {
    isLoading.value = true
    errorMessage.value = ''

    try {
      const result = await getPortfolios()
      portfolios.value = result.items
      totalCount.value = result.totalCount

      if (
        !portfolios.value.some(
          (portfolio) =>
            portfolio.portfolioId === selectedPortfolioId.value,
        )
      ) {
        selectedPortfolioId.value = null
      }
    } catch (error) {
      errorMessage.value =
        error instanceof Error
          ? error.message
          : 'Terjadi kesalahan saat mengambil data portfolio.'
    } finally {
      isLoading.value = false
    }
  }

  watch(selectedPortfolioId, async (portfolioId, _previousId, onCleanup) => {
    valuation.value = null
    pnl.value = null
    detailsErrorMessage.value = ''

    if (!portfolioId) {
      isLoadingDetails.value = false
      return
    }

    let requestIsCurrent = true
    onCleanup(() => {
      requestIsCurrent = false
    })

    const result = await loadPortfolioDetails(portfolioId)

    if (!requestIsCurrent) return

    if (result.error) {
      detailsErrorMessage.value = result.error
      return
    }

    valuation.value = result.valuationResult
    pnl.value = result.pnlResult
  })

  function formatCurrency(value: number | null | undefined): string {
    if (value == null) {
      return 'Belum tersedia'
    }

    return new Intl.NumberFormat('id-ID', {
      style: 'currency',
      currency: 'IDR',
      maximumFractionDigits: 2,
    }).format(value)
  }

  function formatPercentage(value: number | null | undefined): string {
    if (value == null) {
      return 'Belum tersedia'
    }

    return `${new Intl.NumberFormat('id-ID', {
      maximumFractionDigits: 2,
    }).format(value)}%`
  }

  onMounted(loadPortfolios)
</script>


<template>
  <main class="dashboard">
    <header class="page-header">
      <div>
        <p class="eyebrow">TRADELENS / PORTFOLIO ANALYTICS</p>
        <h1>Portfolio Dashboard</h1>
        <p class="subtitle">Pantau portfolio investasi saham Anda dalam satu tempat.</p>
      </div>

      <button class="refresh-button" type="button" :disabled="isLoading" @click="loadPortfolios">
        {{ isLoading ? 'Memuat...' : 'Refresh data' }}
      </button>
    </header>

    <section class="summary-grid" aria-label="Portfolio summary">
      <article class="summary-card">
        <span class="card-label">Total portfolio</span>
        <strong class="card-value">
          {{ isLoading ? '—' : totalCount }}
        </strong>
        <span class="card-note">Portfolio yang tersedia untuk akun ini</span>
      </article>

      <article class="summary-card">
        <span class="card-label">Portfolio dipilih</span>
        <strong class="selected-name">
          {{ selectedPortfolio?.name ?? 'Belum dipilih' }}
        </strong>
        <span class="card-note">Pilih portfolio untuk melihat detail</span>
      </article>

      <article class="summary-card">
        <span class="card-label">Status API</span>
        <strong
          class="card-value"
          :class="{
            'status-loading': isLoading,
            'status-error': errorMessage,
            'status-success': !isLoading && !errorMessage,
          }"
        >
          {{ isLoading ? 'Memuat' : errorMessage ? 'Error' : 'Terhubung' }}
        </strong>
        <span class="card-note">Koneksi ke backend TradeLens</span>
      </article>
    </section>

    <section class="portfolio-section">
      <div class="section-header">
        <div>
          <h2>Portfolio Anda</h2>
          <p>Pilih portfolio untuk melanjutkan analisis.</p>
        </div>
        <span class="count-label">{{ totalCount }} portfolio</span>
      </div>

      <div v-if="isLoading" class="state-panel" role="status">Memuat data portfolio...</div>

      <div v-else-if="errorMessage" class="state-panel error-panel" role="alert">
        <strong>Portfolio tidak dapat dimuat</strong>
        <p>{{ errorMessage }}</p>
        <button type="button" class="refresh-button" @click="loadPortfolios">Coba lagi</button>
      </div>

      <div v-else-if="portfolios.length === 0" class="state-panel">
        <strong>Belum ada portfolio</strong>
        <p>Belum ada portfolio yang tersedia untuk akun ini.</p>
      </div>

      <div v-else class="portfolio-list">
        <button
          v-for="portfolio in portfolios"
          :key="portfolio.portfolioId"
          class="portfolio-item"
          :class="{
            'portfolio-item-selected': selectedPortfolioId === portfolio.portfolioId,
          }"
          type="button"
          :aria-pressed="selectedPortfolioId === portfolio.portfolioId"
          @click="selectedPortfolioId = portfolio.portfolioId"
        >
          <span class="portfolio-icon" aria-hidden="true">
            {{ portfolio.name.trim().charAt(0).toUpperCase() || 'P' }}
          </span>

          <span class="portfolio-info">
            <strong>{{ portfolio.name }}</strong>
            <span class="portfolio-id"> ID: {{ portfolio.portfolioId }} </span>
          </span>

          <span class="select-indicator">
            {{ selectedPortfolioId === portfolio.portfolioId ? 'Dipilih' : 'Pilih' }}
          </span>
        </button>
      </div>

      <p v-if="selectedPortfolio" class="selection-notice" role="status">
        Portfolio "{{ selectedPortfolio.name }}" dipilih. Tampilan valuasi dan P&amp;L akan
        ditambahkan pada tahap berikutnya.
      </p>
    </section>

    <section
      v-if="selectedPortfolio"
      class="portfolio-section detail-section"
      aria-label="Portfolio valuation and profit and loss"
    >
      <div class="section-header">
        <div>
          <h2>Analisis Portfolio</h2>
          <p>{{ selectedPortfolio.name }}</p>
        </div>
        <span v-if="valuation" class="count-label">
          Status valuasi: {{ valuation.status }}
        </span>
      </div>

      <div v-if="isLoadingDetails" class="state-panel" role="status">
        Memuat valuasi dan P&amp;L...
      </div>

      <div v-else-if="detailsErrorMessage" class="state-panel error-panel" role="alert">
        <strong>Analisis portfolio tidak dapat dimuat</strong>
        <p>{{ detailsErrorMessage }}</p>
        <button
          type="button"
          class="refresh-button"
          @click="reloadPortfolioDetails"
        >
          Muat ulang analisis
        </button>
      </div>

      <template v-else-if="valuation && pnl">
        <p
          v-if="valuation.status.toUpperCase() === 'PARTIAL' ||
            pnl.status.toUpperCase() === 'PARTIAL'"
          class="partial-notice"
          role="status"
        >
          Data valuasi belum lengkap. Nilai pasar atau P&amp;L yang belum tersedia
          tidak ditampilkan sebagai nol.
        </p>

        <div class="summary-grid analysis-grid">
          <article class="summary-card">
            <span class="card-label">Total Cost Basis</span>
            <strong class="card-value">
              {{ formatCurrency(valuation.totalCostBasis) }}
            </strong>
            <span class="card-note">Total biaya perolehan posisi</span>
          </article>

          <article class="summary-card">
            <span class="card-label">Total Market Value</span>
            <strong class="card-value">
              {{ formatCurrency(valuation.totalMarketValue) }}
            </strong>
            <span class="card-note">Nilai pasar berdasarkan harga yang tersedia</span>
          </article>

          <article class="summary-card">
            <span class="card-label">Unrealized P&amp;L</span>
            <strong class="card-value">
              {{ formatCurrency(valuation.totalUnrealizedPnl) }}
            </strong>
            <span class="card-note">
              {{ formatPercentage(valuation.totalUnrealizedPnlPercentage) }}
            </span>
          </article>

          <article class="summary-card">
            <span class="card-label">Realized P&amp;L</span>
            <strong class="card-value">
              {{ formatCurrency(pnl.realizedPnl) }}
            </strong>
            <span class="card-note">P&amp;L yang telah direalisasikan</span>
          </article>

          <article class="summary-card">
            <span class="card-label">Total P&amp;L</span>
            <strong class="card-value">
              {{ formatCurrency(pnl.totalPnl) }}
            </strong>
            <span class="card-note">Nilai total dari hasil perhitungan backend</span>
          </article>
        </div>

        <p class="card-note">
          P&amp;L dihitung oleh backend TradeLens. Waktu data P&amp;L:
          {{ new Date(pnl.asOf).toLocaleString('id-ID') }}
        </p>
      </template>
    </section>

  </main>
</template>

<style scoped>
.dashboard {
  max-width: 1200px;
  margin: 0 auto;
  padding: 40px 28px 64px;
  color: #182230;
}

.page-header,
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
}

.page-header {
  margin-bottom: 30px;
}

.eyebrow {
  margin: 0 0 12px;
  color: #526a82;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.12em;
}

h1 {
  margin: 0;
  font-size: clamp(28px, 4vw, 36px);
  letter-spacing: -0.04em;
}

.subtitle,
.section-header p,
.card-note,
.state-panel p {
  color: #687789;
  line-height: 1.6;
}

.subtitle {
  margin: 10px 0 0;
}

.refresh-button {
  flex-shrink: 0;
  padding: 10px 16px;
  border: 1px solid #cbd5e1;
  border-radius: 9px;
  background: #fff;
  color: #182230;
  font: inherit;
  font-weight: 600;
  cursor: pointer;
}

.refresh-button:hover:not(:disabled) {
  background: #f1f5f9;
}

.refresh-button:disabled {
  cursor: wait;
  opacity: 0.6;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 18px;
  margin-bottom: 32px;
}

.summary-card,
.portfolio-section {
  border: 1px solid #e1e7ef;
  border-radius: 14px;
  background: #fff;
}

.summary-card {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 10px;
  padding: 22px;
}

.card-label,
.card-note {
  font-size: 13px;
}

.card-label {
  color: #687789;
  font-weight: 600;
}

.card-value {
  font-size: 28px;
  overflow-wrap: anywhere;
}

.selected-name {
  overflow-wrap: anywhere;
  font-size: 18px;
  line-height: 1.4;
}

.card-note {
  line-height: 1.5;
}

.status-success {
  color: #177245;
}

.status-error {
  color: #b42318;
}

.status-loading {
  color: #526a82;
}

.portfolio-section {
  padding: 24px;
}

.section-header {
  margin-bottom: 20px;
}

.section-header h2 {
  margin: 0;
  font-size: 20px;
}

.section-header p {
  margin: 6px 0 0;
  font-size: 14px;
}

.count-label {
  color: #687789;
  font-size: 13px;
  white-space: nowrap;
}

.portfolio-list {
  display: grid;
  gap: 10px;
}

.portfolio-item {
  display: flex;
  width: 100%;
  align-items: center;
  gap: 14px;
  padding: 16px;
  border: 1px solid #e1e7ef;
  border-radius: 10px;
  background: #fff;
  color: inherit;
  text-align: left;
  cursor: pointer;
}

.portfolio-item:hover {
  border-color: #94a3b8;
  background: #f8fafc;
}

.portfolio-item-selected {
  border-color: #426b99;
  background: #f0f6fc;
}

.portfolio-icon {
  display: grid;
  width: 42px;
  height: 42px;
  flex-shrink: 0;
  place-items: center;
  border-radius: 10px;
  background: #eaf0f7;
  color: #294f78;
  font-weight: 700;
}

.portfolio-info {
  display: flex;
  min-width: 0;
  flex: 1;
  flex-direction: column;
  gap: 5px;
}

.portfolio-info strong {
  overflow-wrap: anywhere;
}

.portfolio-id {
  overflow-wrap: anywhere;
  color: #687789;
  font-size: 12px;
}

.select-indicator {
  flex-shrink: 0;
  color: #426b99;
  font-size: 13px;
  font-weight: 600;
}

.state-panel {
  padding: 28px 20px;
  border: 1px dashed #cbd5e1;
  border-radius: 10px;
  color: #334155;
  text-align: center;
}

.state-panel p {
  margin: 8px 0 16px;
  font-size: 14px;
}

.error-panel {
  border-color: #e4aaa6;
  background: #fffafa;
}

.error-panel strong {
  color: #b42318;
}

.selection-notice {
  margin: 18px 0 0;
  padding: 12px 14px;
  border-radius: 8px;
  background: #f0f6fc;
  color: #294f78;
  font-size: 14px;
  line-height: 1.6;
}

.detail-section {
  margin-top: 24px;
}

.analysis-grid {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.partial-notice {
  margin: 0 0 18px;
  padding: 12px 14px;
  border-radius: 8px;
  background: #fff7e6;
  color: #8a5a00;
  font-size: 14px;
  line-height: 1.6;
}

@media (max-width: 760px) {
  .analysis-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 760px) {
  .dashboard {
    padding: 26px 16px 40px;
  }

  .page-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .summary-grid {
    grid-template-columns: 1fr;
    gap: 12px;
  }

  .portfolio-section {
    padding: 18px;
  }

  .section-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .portfolio-item {
    align-items: flex-start;
    padding: 12px;
  }

  .select-indicator {
    align-self: center;
  }
}
</style>
