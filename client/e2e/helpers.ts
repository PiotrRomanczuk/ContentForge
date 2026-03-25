import { type Page, expect } from '@playwright/test'

/**
 * Default API key from appsettings.Development.json.
 * E2E tests run against the real .NET API — no mocking.
 */
export const TEST_API_KEY = process.env.CF_API_KEY || 'CHANGE-ME-generate-a-strong-key'

/**
 * Authenticate by going through the real login flow.
 * Enters the API key, submits, and waits for redirect to dashboard.
 */
export async function login(page: Page) {
  await page.goto('/login')
  await page.getByPlaceholder('Enter API key').fill(TEST_API_KEY)
  await page.getByRole('button', { name: 'Connect' }).click()
  // Should redirect to dashboard
  await expect(page).toHaveURL('/', { timeout: 10_000 })
}

/**
 * Navigate via sidebar link text.
 */
export async function navigateTo(page: Page, label: string) {
  await page.getByRole('link', { name: label }).first().click()
}
