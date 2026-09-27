import React, { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { Spinner } from '../components/ui/Spinner';
import toast from 'react-hot-toast';

function parseJwtPayload(token: string): Record<string, unknown> {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64));
  } catch {
    return {};
  }
}

export const OAuthCallbackPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { setAuthSession } = useAuth();

  useEffect(() => {
    const accessToken = searchParams.get('access_token');
    const refreshToken = searchParams.get('refresh_token');

    if (!accessToken) {
      toast.error('Authentication failed. Please try again.');
      navigate('/');
      return;
    }

    // Decode JWT to get user info (same as login flow)
    const payload = parseJwtPayload(accessToken);
    const userId = (payload['sub'] as string) ?? '';
    const username =
      (payload['unique_name'] as string) ??
      (payload['name'] as string) ??
      'user';
    const avatarUrl = (payload['avatarUrl'] as string) ?? null;

    setAuthSession({
      accessToken,
      refreshToken: refreshToken ?? '',
      userId,
      username,
      avatarUrl,
    });

    toast.success(`Welcome, ${username}!`);
    navigate('/');
  }, [searchParams, navigate, setAuthSession]);

  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        gap: '16px',
      }}
    >
      <Spinner size={36} />
      <p style={{ fontSize: '15px', color: 'var(--secondary)' }}>
        Completing authentication with CodeHive...
      </p>
    </div>
  );
};
