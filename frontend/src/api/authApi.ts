import type { User, LoginRequest, AuthResponse } from '@/types/auth.types';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

/**
 * Login with email + password.
 * In mock mode: validates against users.json.
 */
export async function login(credentials: LoginRequest): Promise<AuthResponse> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 500));
    const { default: users } = await import('@/mocks/users.json');
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const user = (users as any[]).find(
      (u) => u.email === credentials.email && u.password === credentials.password
    );
    if (!user) throw new Error('Email hoặc mật khẩu không đúng.');
    const { password: _p, ...safeUser } = user;
    const fakeToken = btoa(JSON.stringify({ userId: safeUser.userId, role: safeUser.role }));
    return {
      accessToken: `mock.${fakeToken}.signature`,
      refreshToken: `refresh.${fakeToken}`,
      user: safeUser as User,
    };
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.post<{ data: AuthResponse }>('/auth/login', credentials);
  return response.data.data;
}

/**
 * Register a new account.
 */
export async function register(payload: {
  fullName: string;
  email: string;
  phone: string;
  password: string;
}): Promise<void> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 600));
    return;
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  await axiosInstance.post('/auth/register', payload);
}

/**
 * Get current user profile from stored token.
 */
export async function getMe(): Promise<User | null> {
  if (USE_MOCK) {
    const token = localStorage.getItem('accessToken');
    if (!token) return null;
    try {
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
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: User }>('/auth/me');
  return response.data.data;
}
