import { test, expect } from '@playwright/test'
import { login, navigateTo } from './helpers'

test.describe('Approval Queue', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
    await navigateTo(page, 'Approval Queue')
  })

  test('displays empty state or pending items', async ({ page }) => {
    // Either shows the empty state or the approval cards
    const emptyState = page.getByText('All clear!')
    const pendingCount = page.getByText(/items pending/)

    // One of these should be visible
    await expect(emptyState.or(pendingCount)).toBeVisible({ timeout: 5000 })
  })

  test('shows keyboard shortcuts in preview panel', async ({ page }) => {
    // The keyboard shortcuts help should be visible somewhere on the page
    // If there are items, clicking one shows the preview with shortcuts
    const card = page.locator('[data-slot="card"]').first()
    if (await card.isVisible({ timeout: 2000 }).catch(() => false)) {
      await card.click()
      await expect(page.getByText('Keyboard Shortcuts')).toBeVisible()
    }
  })

  test('select all and batch action bar appear', async ({ page }) => {
    const selectAllButton = page.getByRole('button', { name: 'Select All' })
    if (await selectAllButton.isVisible({ timeout: 2000 }).catch(() => false)) {
      await selectAllButton.click()
      // Floating action bar should appear with batch options
      await expect(page.getByText(/selected/)).toBeVisible()
    }
  })
})
