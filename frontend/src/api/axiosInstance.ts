import axios, { type AxiosRequestConfig } from 'axios';

/**
 * Axios instance with base URL, timeout and Authorization header injection.
 * Includes an automatic token-refresh interceptor:
 *   - On 401: attempts to silently refresh the Access Token using the Refresh Token.
 *   - On refresh success: retries the original failed request with the new token.
 *   - On refresh failure: clears auth state and redirects to /login.
 */
const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL as string,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' },
});

// ── Request Interceptor — attach JWT Access Token ────────────────────────────
axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error: unknown) => Promise.reject(error),
);

// ── Response Interceptor — handle 401 with silent token rotation ─────────────
let isRefreshing = false; // prevents multiple concurrent refresh calls
let failedQueue: Array<{
  resolve: (value: unknown) => void;
  reject:  (reason?: unknown) => void;
}> = [];

/** Flush the queue of requests that were waiting for the token refresh. */
const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else       resolve(token);
  });
  failedQueue = [];
};

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (!axios.isAxiosError(error)) return Promise.reject(error);

    const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };

    // Only attempt refresh on 401 and only once per request
    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }

    // Do NOT try to refresh if the failing request IS the refresh-token endpoint
    // (that would create an infinite loop)
    if (originalRequest.url?.includes('/auth/refresh-token')) {
      clearAuthAndRedirect();
      return Promise.reject(error);
    }

    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      clearAuthAndRedirect();
      return Promise.reject(error);
    }

    // If a refresh is already in progress, queue this request to retry later
    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((newToken) => {
        originalRequest.headers = {
          ...originalRequest.headers,
          Authorization: `Bearer ${newToken}`,
        };
        return axiosInstance(originalRequest);
      });
    }

    // Mark as refreshing — prevents parallel refresh calls
    isRefreshing = true;
    originalRequest._retry = true;

    try {
      const accessToken = localStorage.getItem('accessToken') ?? '';

      // Call refresh-token endpoint with the old (expired) access token + refresh token
      const { data } = await axios.post<{
        data: { accessToken: string; refreshToken: string };
      }>(
        `${import.meta.env.VITE_API_BASE_URL}/auth/refresh-token`,
        { accessToken, refreshToken },
      );

      const newAccessToken  = data.data.accessToken;
      const newRefreshToken = data.data.refreshToken;

      // Persist new tokens
      localStorage.setItem('accessToken',  newAccessToken);
      localStorage.setItem('refreshToken', newRefreshToken);

      // Update default headers for all future requests
      axiosInstance.defaults.headers.common['Authorization'] =
        `Bearer ${newAccessToken}`;

      // Flush queued requests using the new token
      processQueue(null, newAccessToken);

      // Retry the original request
      originalRequest.headers = {
        ...originalRequest.headers,
        Authorization: `Bearer ${newAccessToken}`,
      };
      return axiosInstance(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError, null);
      clearAuthAndRedirect();
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);

/** Wipes auth data from localStorage and redirects to the login page. */
function clearAuthAndRedirect() {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  // Use replace to prevent back-navigation to a protected page
  window.location.replace('/login');
}

export default axiosInstance;
