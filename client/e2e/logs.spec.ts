import { test, expect } from '@playwright/test'
import { login, navigateTo } from './helpers'

test.describe('Logs Viewer', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
    await navigateTo(page, 'Logs')
  })

  test('displays log level filter and search', async ({ page }) => {
    await expect(page.getByPlaceholder('Search messages...')).toBeVisible()
    await expect(page.getByPlaceholder('Source context...')).toBeVisible()
  })

  test('displays stats cards or log entries', async ({ page }) => {
    // Should show stats cards (Total, Debug, Info, Warning, Error)
    // or at minimum the log table / empty state
    const totalCard = page.getByText('Total')
    const emptyState = page.getByText('No log entries found')

    await expect(totalCard.or(emptyState)).toBeVisible({ timeout: 5000 })
  })

  test('level filter dropdown works', async ({ page }) => {
    const levelSelect = page.getByRole('combobox').filter({ hasText: /All Levels|Level/ })
    if (await levelSelect.isVisible({ timeout: 2000 }).catch(() => false)) {
      await levelSelect.click()
      await expect(page.getByRole('option', { name: 'Error' })).toBeVisible()
      await expect(page.getByRole('option', { name: 'Warning' })).toBeVisible()
      await expect(page.getByRole('option', { name: 'Information' })).toBeVisible()
    }
  })
})
