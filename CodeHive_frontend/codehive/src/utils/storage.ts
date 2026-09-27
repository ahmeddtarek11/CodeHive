const TOKEN_KEY = 'ch_access_token';
const REFRESH_KEY = 'ch_refresh_token';
const USER_KEY = 'ch_user';

export const getToken = (): string | null => localStorage.getItem(TOKEN_KEY);
export const setToken = (t: string): void => localStorage.setItem(TOKEN_KEY, t);
export const getRefreshToken = (): string | null => localStorage.getItem(REFRESH_KEY);
export const setRefreshToken = (t: string): void => localStorage.setItem(REFRESH_KEY, t);

export const clearAuth = (): void => {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_KEY);
  localStorage.removeItem(USER_KEY);
};

export const getUser = <T = unknown>(): T | null => {
  const u = localStorage.getItem(USER_KEY);
  if (!u) return null;
  try {
    return JSON.parse(u) as T;
  } catch {
    return null;
  }
};

export const setUser = (u: object): void => {
  localStorage.setItem(USER_KEY, JSON.stringify(u));
};
