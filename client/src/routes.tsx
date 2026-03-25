import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from '@/components/layout/AppLayout'
import { LoginPage } from '@/pages/LoginPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { ContentListPage } from '@/pages/ContentListPage'
import { ContentDetailPage } from '@/pages/ContentDetailPage'
import { ImportContentPage } from '@/pages/ImportContentPage'
import { ApprovalQueuePage } from '@/pages/ApprovalQueuePage'
import { SocialAccountsPage } from '@/pages/SocialAccountsPage'
import { SchedulesPage } from '@/pages/SchedulesPage'
import { BotExplorerPage } from '@/pages/BotExplorerPage'
import { LogsPage } from '@/pages/LogsPage'
import { NotFoundPage } from '@/pages/NotFoundPage'

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    element: <AppLayout />,
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'content', element: <ContentListPage /> },
      { path: 'content/import', element: <ImportContentPage /> },
      { path: 'content/:id', element: <ContentDetailPage /> },
      { path: 'approval', element: <ApprovalQueuePage /> },
      { path: 'social-accounts', element: <SocialAccountsPage /> },
      { path: 'schedules', element: <SchedulesPage /> },
      { path: 'bots', element: <BotExplorerPage /> },
      { path: 'logs', element: <LogsPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
