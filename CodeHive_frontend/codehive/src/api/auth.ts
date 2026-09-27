import client from './client';
import { API_BASE } from './client';
import type { AuthResponse } from '../types';

// Backend returns: ApiResponse<AuthTokensDto>
// AuthTokensDto: { AccessToken, RefreshToken, AccessTokenExpiry }
// Register returns: ApiResponse<{ userId: Guid }>
// The JWT sub claim contains the userId, so after login we decode it.

function parseJwtPayload(token: string): Record<string, unknown> {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64));
  } catch {
    return {};
  }
}

export const authApi = {
  // POST /api/v1/auth/register  body: { username, email, password, displayName }
  register: async (data: { username: string; email: string; password: string }): Promise<{ userId: string }> => {
    const res = await client.post<{ message: string; data: { userId: string } }>(
      '/api/v1/auth/register',
      { ...data, displayName: data.username }
    );
    return res.data.data;
  },

  // POST /api/v1/auth/login  body: { email, password }
  // Backend returns AuthTokensDto (AccessToken, RefreshToken, AccessTokenExpiry)
  // We decode the JWT to get userId + username + avatarUrl
  login: async (data: { email: string; password: string }): Promise<AuthResponse> => {
    const res = await client.post<{ message: string; data: { accessToken: string; refreshToken: string; accessTokenExpiry: string } }>(
      '/api/v1/auth/login',
      data
    );
    const { accessToken, refreshToken } = res.data.data;
    const payload = parseJwtPayload(accessToken);

    // Standard JWT claims used by the backend: sub = userId, unique_name = username
    const userId = (payload['sub'] as string) ?? '';
    const username = (payload['unique_name'] as string) ?? (payload['name'] as string) ?? '';
    const avatarUrl = (payload['avatarUrl'] as string) ?? null;

    return { accessToken, refreshToken, userId, username, avatarUrl };
  },

  // POST /api/v1/auth/refresh  body: { accessToken, refreshToken }
  refresh: async (tokens: { accessToken: string; refreshToken: string }): Promise<{ accessToken: string; refreshToken: string }> => {
    const res = await client.post<{ message: string; data: { accessToken: string; refreshToken: string } }>(
      '/api/v1/auth/refresh',
      tokens
    );
    return res.data.data;
  },

  // OAuth: redirect to backend which handles the flow
  getGoogleLoginUrl: (): string => `${API_BASE}/api/v1/auth/login/google`,
  getGithubLoginUrl: (): string => `${API_BASE}/api/v1/auth/login/github`,
};
