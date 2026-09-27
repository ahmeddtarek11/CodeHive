import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import type { AuthResponse, UserSummary } from '../types';
import { clearAuth, getRefreshToken, getToken, getUser, setRefreshToken, setToken, setUser } from '../utils/storage';
import { authApi } from '../api/auth';
import { usersApi } from '../api/users';
import toast from 'react-hot-toast';

interface AuthContextType {
  user: UserSummary | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string) => Promise<void>;
  logout: () => void;
  isAuthModalOpen: boolean;
  openAuthModal: (initialTab?: 'login' | 'register') => void;
  closeAuthModal: () => void;
  authModalTab: 'login' | 'register';
  setAuthModalTab: (tab: 'login' | 'register') => void;
  handleOAuthLogin: (provider: 'google' | 'github') => void;
  setAuthSession: (authData: AuthResponse) => void;
  refreshCurrentUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [token, setTokenState] = useState<string | null>(() => getToken());
  const [user, setUserState] = useState<UserSummary | null>(() => getUser<UserSummary>());
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
  const [authModalTab, setAuthModalTab] = useState<'login' | 'register'>('login');

  // Listen for auth:logout events fired by the Axios 401 interceptor
  useEffect(() => {
    const handleForcedLogout = () => {
      clearAuth();
      setTokenState(null);
      setUserState(null);
      toast.error('Your session has expired. Please log in again.');
    };
    window.addEventListener('auth:logout', handleForcedLogout);
    return () => window.removeEventListener('auth:logout', handleForcedLogout);
  }, []);

  const openAuthModal = (tab: 'login' | 'register' = 'login') => {
    setAuthModalTab(tab);
    setIsAuthModalOpen(true);
  };

  const closeAuthModal = () => setIsAuthModalOpen(false);

  const refreshCurrentUser = useCallback(async () => {
    try {
      const profile = await usersApi.getCurrentProfile();
      const currentUser: UserSummary = {
        id: profile.id,
        username: profile.username,
        displayName: profile.displayName,
        avatarUrl: profile.avatarUrl,
      };
      setUser(currentUser);
      setUserState(currentUser);
    } catch (error) {
      // Keep the session usable if the optional profile refresh is temporarily unavailable.
      console.warn('Unable to refresh the signed-in user profile.', error);
    }
  }, []);

  useEffect(() => {
    if (token) {
      void refreshCurrentUser();
    }
  }, [token, refreshCurrentUser]);

  const setAuthSession = (authData: AuthResponse) => {
    setToken(authData.accessToken);
    setRefreshToken(authData.refreshToken);
    setTokenState(authData.accessToken);
    const userObj: UserSummary = {
      id: authData.userId,
      username: authData.username,
      displayName: authData.username,
      avatarUrl: authData.avatarUrl,
    };
    setUser(userObj);
    setUserState(userObj);
    closeAuthModal();
    void refreshCurrentUser();
  };

  const login = async (email: string, password: string) => {
    const res = await authApi.login({ email, password });
    setAuthSession(res);
    toast.success(`Welcome back, ${res.username}!`);
  };

  const register = async (username: string, email: string, password: string) => {
    await authApi.register({ username, email, password });
    // Auto-login after registration
    const loginRes = await authApi.login({ email, password });
    setAuthSession(loginRes);
    toast.success(`Welcome to CodeHive, ${username}!`);
  };

  const logout = () => {
    clearAuth();
    setTokenState(null);
    setUserState(null);
    toast.success('Logged out successfully');
  };

  // OAuth: open the backend OAuth redirect URL directly in the same tab
  const handleOAuthLogin = (provider: 'google' | 'github') => {
    const url = provider === 'google'
      ? authApi.getGoogleLoginUrl()
      : authApi.getGithubLoginUrl();
    window.location.href = url;
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!user && !!token,
        login,
        register,
        logout,
        isAuthModalOpen,
        openAuthModal,
        closeAuthModal,
        authModalTab,
        setAuthModalTab,
        handleOAuthLogin,
        setAuthSession,
        refreshCurrentUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
