import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User } from '@/types/auth.types';

interface AuthState {
  accessToken:     string | null;
  refreshToken:    string | null;
  user:            User | null;
  isAuthenticated: boolean;

  /** Store tokens and user after successful login */
  setAuth: (accessToken: string, refreshToken: string, user: User) => void;

  /** Clear all auth state on logout (call logoutWithApi instead when possible) */
  logout: () => void;

  /** Update user profile in store without re-login */
  setUser: (user: User) => void;
}

/**
 * Auth store — persisted to localStorage via zustand/middleware.
 * Manages authentication state globally across the app.
 *
 * Token lifecycle:
 *  1. setAuth()  — called after login; writes tokens to store + localStorage.
 *  2. logout()   — clears store and localStorage; called after logoutApi() resolves.
 *  3. axiosInstance interceptor handles silent token refresh on 401.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken:     null,
      refreshToken:    null,
      user:            null,
      isAuthenticated: false,

      setAuth: (accessToken, refreshToken, user) => {
        // Also keep localStorage in sync for axiosInstance interceptor
        localStorage.setItem('accessToken',  accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        set({ accessToken, refreshToken, user, isAuthenticated: true });
      },

      logout: () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        set({ accessToken: null, refreshToken: null, user: null, isAuthenticated: false });
      },

      setUser: (user) => set({ user }),
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        accessToken:     state.accessToken,
        refreshToken:    state.refreshToken,
        user:            state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
);
