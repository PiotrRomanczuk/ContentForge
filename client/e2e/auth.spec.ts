import { test, expect } from '@playwright/test'
import { TEST_API_KEY, login } from './helpers'

test.describe('Authentication', () => {
  test('shows login page when not authenticated', async ({ page }) => {
    await page.goto('/')
    // Should redirect to login
    await expect(page).toHaveURL('/login')
    await expect(page.getByText('ContentForge')).toBeVisible()
    await expect(page.getByPlaceholder('Enter API key')).toBeVisible()
  })

  test('logs in with valid API key and redirects to dashboard', async ({ page }) => {
    await login(page)
    // Dashboard should show
    await expect(page.getByText('Dashboard')).toBeVisible()
  })

  test('shows error on invalid API key', async ({ page }) => {
    await page.goto('/login')
    await page.getByPlaceholder('Enter API key').fill('wrong-key')
    await page.getByRole('button', { name: 'Connect' }).click()
    await expect(page.getByText('Invalid API key')).toBeVisible()
  })

  test('logout returns to login page', async ({ page }) => {
    await login(page)
    await page.getByRole('button', { name: 'Logout' }).click()
    await expect(page).toHaveURL('/login')
  })
})
