import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Suspense, lazy } from 'react';
import { ProtectedRoute } from './ProtectedRoute';
import { ROUTES } from '@/constants/routes';

// Lazy-load pages for code splitting
const HomePage           = lazy(() => import('@/pages/public/HomePage'));
const CourtListPage      = lazy(() => import('@/pages/public/CourtListPage'));
const CourtDetailPage    = lazy(() => import('@/pages/public/CourtDetailPage'));
const LoginPage          = lazy(() => import('@/pages/auth/LoginPage'));
const RegisterPage       = lazy(() => import('@/pages/auth/RegisterPage'));
const VerifyEmailPage    = lazy(() => import('@/pages/auth/VerifyEmailPage'));
const ForgotPasswordPage = lazy(() => import('@/pages/auth/ForgotPasswordPage'));

const BookingPage       = lazy(() => import('@/pages/customer/BookingPage'));
const MyBookingsPage    = lazy(() => import('@/pages/customer/MyBookingsPage'));
const ProfilePage       = lazy(() => import('@/pages/customer/ProfilePage'));
const PaymentPage       = lazy(() => import('@/pages/customer/PaymentPage'));
const PaymentResultPage = lazy(() => import('@/pages/customer/PaymentResultPage'));
const NotificationsPage = lazy(() => import('@/pages/customer/NotificationsPage'));

const AdminLayout        = lazy(() => import('@/components/layout/AdminLayout'));
const AdminDashboardPage = lazy(() => import('@/pages/admin/DashboardPage'));
const ManageCourtsPage   = lazy(() => import('@/pages/admin/ManageCourtsPage'));
const ComplexDetailPage  = lazy(() => import('@/pages/admin/ComplexDetailPage'));
const ManageBookingsPage = lazy(() => import('@/pages/admin/ManageBookingsPage'));
const ManageUsersPage    = lazy(() => import('@/pages/admin/ManageUsersPage'));
const ReportsPage        = lazy(() => import('@/pages/admin/ReportsPage'));

const UnauthorizedPage = lazy(() => import('@/pages/public/UnauthorizedPage'));

/** Full-screen loading fallback */
function PageLoader() {
  return (
    <div className='min-h-screen flex items-center justify-center bg-surface'>
      <div className='w-12 h-12 border-4 border-primary-500 border-t-transparent rounded-full animate-spin' />
    </div>
  );
}

/**
 * Application Router — defines all routes with lazy loading and role protection.
 */
export function AppRouter() {
  return (
    <BrowserRouter>
      <Suspense fallback={<PageLoader />}>
        <Routes>
          {/* ─── Public Routes ─── */}
          <Route path={ROUTES.HOME}           element={<HomePage />} />
          <Route path={ROUTES.COURTS}         element={<CourtListPage />} />
          <Route path={ROUTES.COURT_DETAIL}   element={<CourtDetailPage />} />
          <Route path={ROUTES.LOGIN}           element={<LoginPage />} />
          <Route path={ROUTES.REGISTER}        element={<RegisterPage />} />
          <Route path='/verify-email'          element={<VerifyEmailPage />} />
          <Route path={ROUTES.FORGOT_PASSWORD} element={<ForgotPasswordPage />} />
          <Route path='/unauthorized'          element={<UnauthorizedPage />} />

          {/* ─── Customer Protected ─── */}
          <Route element={<ProtectedRoute allowedRoles={['Customer', 'Admin', 'Staff', 'Coach']} />}>
            <Route path={ROUTES.BOOKING}        element={<BookingPage />} />
            <Route path={ROUTES.MY_BOOKINGS}    element={<MyBookingsPage />} />
            <Route path={ROUTES.PROFILE}        element={<ProfilePage />} />
            <Route path={ROUTES.PAYMENT}        element={<PaymentPage />} />
            <Route path={ROUTES.PAYMENT_RESULT} element={<PaymentResultPage />} />
            <Route path={ROUTES.NOTIFICATIONS}  element={<NotificationsPage />} />
          </Route>

          {/* ─── Admin Protected (wrapped in AdminLayout) ─── */}
          <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
            <Route element={<AdminLayout />}>
              <Route path={ROUTES.ADMIN}          element={<AdminDashboardPage />} />
              <Route path={ROUTES.ADMIN_COURTS}   element={<ManageCourtsPage />} />
              <Route path={ROUTES.ADMIN_COMPLEX_DETAIL} element={<ComplexDetailPage />} />
              <Route path={ROUTES.ADMIN_BOOKINGS} element={<ManageBookingsPage />} />
              <Route path={ROUTES.ADMIN_USERS}    element={<ManageUsersPage />} />
              <Route path={ROUTES.ADMIN_REPORTS}  element={<ReportsPage />} />
            </Route>
          </Route>

          {/* ─── Fallback ─── */}
          <Route path='*' element={<Navigate to={ROUTES.HOME} replace />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}
