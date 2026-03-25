import { test, expect } from '@playwright/test'
import { login, navigateTo } from './helpers'

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
  })

  test('displays status stat cards', async ({ page }) => {
    // The 7 status cards should be visible
    await expect(page.getByText('Draft')).toBeVisible()
    await expect(page.getByText('Generated')).toBeVisible()
    await expect(page.getByText('Published')).toBeVisible()
    await expect(page.getByText('Failed')).toBeVisible()
  })

  test('displays pipeline chart', async ({ page }) => {
    await expect(page.getByText('Content Pipeline')).toBeVisible()
  })

  test('quick actions navigate correctly', async ({ page }) => {
    await page.getByRole('link', { name: /Review Queue/i }).click()
    await expect(page).toHaveURL('/approval')
  })

  test('shows connected bots section', async ({ page }) => {
    await expect(page.getByText('Connected Bots')).toBeVisible()
  })
})
