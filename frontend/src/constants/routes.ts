/** Application routes constants */
export const ROUTES = {
  // Public
  HOME:            '/',
  COURTS:          '/courts',
  COURT_DETAIL:    '/courts/:id',
  PROMOTIONS:      '/promotions',
  ABOUT:           '/about',

  // Auth
  LOGIN:           '/login',
  REGISTER:        '/register',
  FORGOT_PASSWORD: '/forgot-password',
  RESET_PASSWORD:  '/reset-password',

  // Customer
  BOOKING:         '/booking/:courtId',
  MY_BOOKINGS:     '/my-bookings',
  PAYMENT:         '/payment/:bookingId',
  PAYMENT_RESULT:  '/payment/result',
  PROFILE:         '/profile',
  NOTIFICATIONS:   '/notifications',
  MEMBERSHIP:      '/membership',
  REVIEWS:         '/reviews',

  // Staff
  STAFF_DASHBOARD: '/staff/dashboard',
  STAFF_EQUIPMENT: '/staff/equipment',
  STAFF_BOOKINGS:  '/staff/bookings',
  STAFF_CUSTOMERS: '/staff/customers',
  STAFF_WALK_IN:   '/staff/walk-in',

  // Coach
  COACH_SCHEDULE:  '/coach/schedule',
  COACH_SESSIONS:  '/coach/sessions',

  // Admin
  ADMIN:           '/admin',
  ADMIN_COURTS:         '/admin/courts',
  ADMIN_COMPLEX_DETAIL: '/admin/courts/:complexId',
  ADMIN_BOOKINGS:  '/admin/bookings',
  ADMIN_USERS:     '/admin/users',
  ADMIN_PAYMENTS:  '/admin/payments',
  ADMIN_SERVICES:  '/admin/services',
  ADMIN_REPORTS:   '/admin/reports',
  ADMIN_PROMOTIONS:'/admin/promotions',
  ADMIN_MAINTENANCE:'/admin/maintenance',
  ADMIN_NOTIFICATIONS:'/admin/notifications',
  ADMIN_SETTINGS:  '/admin/settings',
  ADMIN_STAFF_SHIFTS: '/admin/staff-shifts',
} as const;
