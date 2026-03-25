import { test, expect } from '@playwright/test'
import { login, navigateTo } from './helpers'

test.describe('Social Accounts', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
    await navigateTo(page, 'Social Accounts')
  })

  test('displays accounts or empty state', async ({ page }) => {
    const emptyState = page.getByText('No social accounts connected')
    const accountCount = page.getByText(/accounts/)

    await expect(emptyState.or(accountCount)).toBeVisible({ timeout: 5000 })
  })

  test('add account dialog opens with required fields', async ({ page }) => {
    await page.getByRole('button', { name: /Add Account/i }).click()
    await expect(page.getByText('Add Social Account')).toBeVisible()
    await expect(page.getByText('Name')).toBeVisible()
    await expect(page.getByText('Platform')).toBeVisible()
    await expect(page.getByText('External ID')).toBeVisible()
    await expect(page.getByText('Access Token')).toBeVisible()
  })
})
