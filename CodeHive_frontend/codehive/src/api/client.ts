import axios, { AxiosError } from 'axios';
import { getToken, getRefreshToken, setToken, setRefreshToken, clearAuth } from '../utils/storage';

export const API_BASE = import.meta.env.VITE_API_BASE ?? '';

const client = axios.create({ baseURL: API_BASE });

// ── Request interceptor: attach Bearer token ──────────────────────────────
client.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ── Response interceptor: refresh token on 401 ───────────────────────────
let isRefreshing = false;
let failedQueue: Array<{ resolve: (v: string) => void; reject: (e: unknown) => void }> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) prom.reject(error);
    else prom.resolve(token!);
  });
  failedQueue = [];
};

client.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as (typeof error.config & { _retry?: boolean });

    if (error.response?.status === 401 && !originalRequest?._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers!.Authorization = `Bearer ${token}`;
          return client(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const accessToken = getToken();
      const refreshToken = getRefreshToken();

      if (!accessToken || !refreshToken) {
        clearAuth();
        window.dispatchEvent(new Event('auth:logout'));
        return Promise.reject(error);
      }

      try {
        const res = await axios.post<{ message: string; data: { accessToken: string; refreshToken: string } }>(
          `${API_BASE}/api/v1/auth/refresh`,
          { accessToken, refreshToken }
        );
        const { accessToken: newAccess, refreshToken: newRefresh } = res.data.data;
        setToken(newAccess);
        setRefreshToken(newRefresh);
        processQueue(null, newAccess);
        originalRequest.headers!.Authorization = `Bearer ${newAccess}`;
        return client(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        clearAuth();
        window.dispatchEvent(new Event('auth:logout'));
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default client;
