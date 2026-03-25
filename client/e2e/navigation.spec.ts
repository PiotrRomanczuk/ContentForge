import { test, expect } from '@playwright/test'
import { login, navigateTo } from './helpers'

test.describe('Sidebar Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
  })

  test('navigates to all main pages from sidebar', async ({ page }) => {
    // Pipeline
    await navigateTo(page, 'Pipeline')
    await expect(page).toHaveURL('/content')

    // Import
    await navigateTo(page, 'Import')
    await expect(page).toHaveURL('/content/import')

    // Approval Queue
    await navigateTo(page, 'Approval Queue')
    await expect(page).toHaveURL('/approval')

    // Social Accounts
    await navigateTo(page, 'Social Accounts')
    await expect(page).toHaveURL('/social-accounts')

    // Schedules
    await navigateTo(page, 'Schedules')
    await expect(page).toHaveURL('/schedules')

    // Bot Explorer
    await navigateTo(page, 'Bot Explorer')
    await expect(page).toHaveURL('/bots')

    // Logs
    await navigateTo(page, 'Logs')
    await expect(page).toHaveURL('/logs')

    // Back to Dashboard
    await navigateTo(page, 'Dashboard')
    await expect(page).toHaveURL('/')
  })

  test('shows 404 page for unknown routes', async ({ page }) => {
    await page.goto('/nonexistent-page')
    await expect(page.getByText('Page not found')).toBeVisible()
    await expect(page.getByRole('link', { name: 'Back to Dashboard' })).toBeVisible()
  })
})
