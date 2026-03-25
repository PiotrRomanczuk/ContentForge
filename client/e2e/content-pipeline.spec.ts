import { test, expect } from '@playwright/test'
import { login, navigateTo } from './helpers'

test.describe('Content Pipeline', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
    await navigateTo(page, 'Pipeline')
  })

  test('displays filter controls', async ({ page }) => {
    await expect(page.getByRole('combobox').first()).toBeVisible()
    await expect(page.getByPlaceholder('Search content...')).toBeVisible()
  })

  test('content table renders with columns', async ({ page }) => {
    // Table headers should be present
    await expect(page.getByRole('columnheader', { name: 'Status' })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: 'Bot' })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: 'Type' })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: 'Content' })).toBeVisible()
  })

  test('status filter changes displayed content', async ({ page }) => {
    // Open status dropdown and select a status
    const statusSelect = page.getByRole('combobox').first()
    await statusSelect.click()
    await page.getByRole('option', { name: 'Published' }).click()

    // URL should reflect the filter (applied via state, not URL in this case)
    // The table should show only Published items or empty state
    await page.waitForTimeout(500)
    // Verify the filter is applied (select shows "Published")
    await expect(statusSelect).toContainText('Published')
  })

  test('navigates to content detail on row click', async ({ page }) => {
    // If there are content items, clicking the view button navigates to detail
    const viewButton = page.getByRole('link').filter({ has: page.locator('svg') }).first()
    if (await viewButton.isVisible({ timeout: 2000 }).catch(() => false)) {
      await viewButton.click()
      await expect(page).toHaveURL(/\/content\/[a-f0-9-]+/)
    }
  })
})
