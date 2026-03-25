import { defineConfig, devices } from '@playwright/test'

/**
 * E2E tests for ContentForge frontend.
 *
 * Prerequisites:
 *   1. .NET API running on port 8080: dotnet run --project src/ContentForge.API
 *   2. Tests start the Vite dev server automatically via webServer below
 *
 * Run:
 *   npm run test:e2e           # headless
 *   npm run test:e2e:ui        # Playwright UI mode
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  timeout: 30_000,

  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'mobile-chrome',
      use: { ...devices['Pixel 7'] },
    },
  ],

  webServer: {
    command: 'npm run dev',
    port: 5173,
    reuseExistingServer: !process.env.CI,
    timeout: 15_000,
  },
})
