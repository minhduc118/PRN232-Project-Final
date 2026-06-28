import axiosInstance from './axiosInstance';
import type { User, LoginRequest, AuthResponse, MembershipTier, UpdateProfileRequest, ChangePasswordRequest } from '@/types/auth.types';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

// ─────────────────────────────────────────────────────────────────────────────
//  Register
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Registers a new customer account.
 * On success: backend sends a 6-digit OTP to the provided email.
 * Returns the email so the frontend can navigate to the OTP verification page.
 */
export async function register(payload: {
  fullName:        string;
  email:           string;
  phone?:          string;
  password:        string;
  confirmPassword: string;
}): Promise<{ email: string }> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 600));
    return { email: payload.email };
  }
  const response = await axiosInstance.post<{ data: { email: string } }>(
    '/auth/register',
    payload,
  );
  return response.data.data;
}

// ─────────────────────────────────────────────────────────────────────────────
//  Verify Email OTP
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Submits the 6-digit OTP received by email to activate the account.
 * Throws on invalid/expired OTP so the caller can show an error toast.
 */
export async function verifyEmail(payload: {
  email: string;
  otp:   string;
}): Promise<void> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    return; // always succeeds in mock mode
  }
  await axiosInstance.post('/auth/verify-email', payload);
}

// ─────────────────────────────────────────────────────────────────────────────
//  Login
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Authenticates with email + password.
 * Returns AccessToken, RefreshToken, and the User DTO.
 */
export async function login(credentials: LoginRequest): Promise<AuthResponse> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 500));
    const { default: users } = await import('@/mocks/users.json');
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const user = (users as any[]).find(
      (u) => u.email === credentials.email && u.password === credentials.password,
    );
    if (!user) throw new Error('Email hoặc mật khẩu không đúng.');
    const { password: _p, ...safeUser } = user;
    const fakeToken = btoa(JSON.stringify({ userId: safeUser.userId, role: safeUser.role }));
    localStorage.setItem('mock_current_user', JSON.stringify(safeUser));
    return {
      accessToken:  `mock.${fakeToken}.signature`,
      refreshToken: `refresh.${fakeToken}`,
      user: safeUser as User,
    };
  }

  const response = await axiosInstance.post<{ data: AuthResponse }>(
    '/auth/login',
    credentials,
  );
  return response.data.data;
}

// ─────────────────────────────────────────────────────────────────────────────
//  Get current user profile
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Fetches the current user's profile using the stored Access Token.
 * Used on page load to re-hydrate authentication state.
 * Returns null if the user is not authenticated.
 */
export async function getMe(): Promise<User | null> {
  if (USE_MOCK) {
    const token = localStorage.getItem('accessToken');
    if (!token) return null;
    try {
      const localUser = localStorage.getItem('mock_current_user');
      if (localUser) return JSON.parse(localUser);
      const payload = JSON.parse(atob(token.split('.')[1]));
      const { default: users } = await import('@/mocks/users.json');
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const user = (users as any[]).find((u) => u.userId === payload.userId);
      if (!user) return null;
      const { password: _p, ...safeUser } = user;
      return safeUser as User;
    } catch {
      return null;
    }
  }

  try {
    const response = await axiosInstance.get<{ data: User }>('/auth/me');
    return response.data.data;
  } catch {
    return null;
  }
}

// ─────────────────────────────────────────────────────────────────────────────
//  Logout
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Calls the backend logout endpoint to invalidate the Refresh Token in the DB.
 * The Access Token will expire naturally after its TTL (15 minutes).
 * Also clears tokens from localStorage.
 */
export async function logoutApi(): Promise<void> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 200));
    localStorage.removeItem('mock_current_user');
    return;
  }
  try {
    await axiosInstance.post('/auth/logout');
  } finally {
    // Always clear local storage regardless of server response
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('mock_current_user');
  }
}

// ─────────────────────────────────────────────────────────────────────────────
//  Update profile
// ─────────────────────────────────────────────────────────────────────────────
export async function updateProfile(payload: UpdateProfileRequest): Promise<User> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    const currentUser = await getMe();
    if (!currentUser) throw new Error('Chưa đăng nhập.');
    const updated = {
      ...currentUser,
      fullName: payload.fullName,
      phone: payload.phone,
      gender: payload.gender,
      avatarUrl: payload.avatarUrl,
      skillLevel: payload.skillLevel,
    };
    localStorage.setItem('mock_current_user', JSON.stringify(updated));
    return updated;
  }
  const response = await axiosInstance.put<{ data: User }>('/users/profile', payload);
  return response.data.data;
}

// ─────────────────────────────────────────────────────────────────────────────
//  Change password
// ─────────────────────────────────────────────────────────────────────────────
export async function changePassword(payload: ChangePasswordRequest): Promise<void> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    return;
  }
  await axiosInstance.post('/users/change-password', payload);
}

// ─────────────────────────────────────────────────────────────────────────────
//  Get membership tiers
// ─────────────────────────────────────────────────────────────────────────────
export async function getMembershipTiers(): Promise<MembershipTier[]> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 300));
    return [
      { tierId: 1, tierName: 'Bronze', minPoints: 0, discountPercent: 0 },
      { tierId: 2, tierName: 'Silver', minPoints: 500, discountPercent: 5 },
      { tierId: 3, tierName: 'Gold', minPoints: 2000, discountPercent: 10 },
      { tierId: 4, tierName: 'Platinum', minPoints: 5000, discountPercent: 15 }
    ];
  }
  const response = await axiosInstance.get<{ data: MembershipTier[] }>('/membershiptiers');
  return response.data.data;
}
