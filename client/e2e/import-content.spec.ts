import { test, expect } from '@playwright/test'
import { login, navigateTo } from './helpers'

test.describe('Import Content', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
    await navigateTo(page, 'Import')
  })

  test('displays form and JSON batch tabs', async ({ page }) => {
    await expect(page.getByRole('tab', { name: 'Form' })).toBeVisible()
    await expect(page.getByRole('tab', { name: 'JSON Batch' })).toBeVisible()
  })

  test('form mode shows all required fields', async ({ page }) => {
    await expect(page.getByText('Bot')).toBeVisible()
    await expect(page.getByText('Category')).toBeVisible()
    await expect(page.getByText('Content Type')).toBeVisible()
    await expect(page.getByText('Text Content')).toBeVisible()
  })

  test('can add and remove form items', async ({ page }) => {
    // Click "Add Item" to add a second form
    await page.getByRole('button', { name: 'Add Item' }).click()
    // There should now be 2 form cards
    const cards = page.locator('[data-slot="card"]')
    await expect(cards).toHaveCount(2)

    // Remove the second one
    const deleteButtons = page.getByRole('button').filter({ has: page.locator('svg.text-destructive') })
    if (await deleteButtons.first().isVisible()) {
      await deleteButtons.first().click()
      await expect(cards).toHaveCount(1)
    }
  })

  test('JSON batch tab shows textarea with placeholder', async ({ page }) => {
    await page.getByRole('tab', { name: 'JSON Batch' }).click()
    await expect(page.getByPlaceholder(/items/)).toBeVisible()
  })
})
