import { test, expect } from '@playwright/test'
import { login, navigateTo } from './helpers'

test.describe('Bot Explorer', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
    await navigateTo(page, 'Bot Explorer')
  })

  test('displays registered bots', async ({ page }) => {
    // Should show bot cards or empty state
    const botCard = page.getByText('EnglishFactsBot').or(page.getByText('No bots registered'))
    await expect(botCard).toBeVisible({ timeout: 5000 })
  })

  test('clicking a bot shows prompt template viewer', async ({ page }) => {
    const botCard = page.getByText('EnglishFactsBot')
    if (await botCard.isVisible({ timeout: 3000 }).catch(() => false)) {
      await botCard.click()
      // Prompt template panel should appear
      await expect(page.getByText('Prompt Template')).toBeVisible()
      // Language selector should be visible
      await expect(page.getByText('EN')).toBeVisible()
    }
  })
})
