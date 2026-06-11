import { lazy, Suspense } from 'react'
import { Spin } from 'antd'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AppLayout } from './shared/components/AppLayout'
import { ApiErrorBridge } from './shared/components/ApiErrorBridge'

const DashboardPage = lazy(() =>
  import('./features/dashboard/components/DashboardPage').then((m) => ({ default: m.DashboardPage })),
)
const CompaniesPage = lazy(() =>
  import('./features/companies/components/CompaniesPage').then((m) => ({ default: m.CompaniesPage })),
)
const CalendarPage = lazy(() =>
  import('./features/calendar/components/CalendarPage').then((m) => ({ default: m.CalendarPage })),
)
const AlertsPage = lazy(() =>
  import('./features/alerts/components/AlertsPage').then((m) => ({ default: m.AlertsPage })),
)

function PageFallback() {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', padding: 64 }}>
      <Spin size="large" />
    </div>
  )
}

function App() {
  return (
    <BrowserRouter>
      <ApiErrorBridge />
      <Routes>
        <Route element={<AppLayout />}>
          <Route
            index
            element={
              <Suspense fallback={<PageFallback />}>
                <DashboardPage />
              </Suspense>
            }
          />
          <Route
            path="empresas"
            element={
              <Suspense fallback={<PageFallback />}>
                <CompaniesPage />
              </Suspense>
            }
          />
          <Route
            path="calendario"
            element={
              <Suspense fallback={<PageFallback />}>
                <CalendarPage />
              </Suspense>
            }
          />
          <Route
            path="alertas"
            element={
              <Suspense fallback={<PageFallback />}>
                <AlertsPage />
              </Suspense>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
