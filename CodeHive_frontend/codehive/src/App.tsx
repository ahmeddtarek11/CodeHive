import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { NotificationProvider } from './contexts/NotificationContext';
import { Toaster } from 'react-hot-toast';
import { AuthModal } from './pages/AuthModal';

import { FeedPage } from './pages/FeedPage';
import { DiscoverPage } from './pages/DiscoverPage';
import { PostDetailPage } from './pages/PostDetailPage';
import { CreatePostPage } from './pages/CreatePostPage';
import { ProfilePage } from './pages/ProfilePage';
import { MessagesPage } from './pages/MessagesPage';
import { NotificationsPage } from './pages/NotificationsPage';
import { SearchPage } from './pages/SearchPage';
import { BookmarksPage } from './pages/BookmarksPage';
import { OAuthCallbackPage } from './pages/OAuthCallbackPage';

// Protected Route wrapper that respects: "Protected pages show the AuthModal overlay instead of redirecting"
const ProtectedRoute: React.FC<{ children: React.ReactElement }> = ({ children }) => {
  const { isAuthenticated, openAuthModal } = useAuth();

  React.useEffect(() => {
    if (!isAuthenticated) {
      openAuthModal('login');
    }
  }, [isAuthenticated, openAuthModal]);

  return children;
};

// Feed route: if not authenticated, redirect to /discover or show Feed with Auth prompt
const FeedRoute: React.FC = () => {
  const { isAuthenticated } = useAuth();
  if (!isAuthenticated) {
    return <Navigate to="/discover" replace />;
  }
  return <FeedPage />;
};

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <NotificationProvider>
          {/* Global Toast Notifications */}
          <Toaster
            position="bottom-right"
            toastOptions={{
              duration: 3000,
              style: {
                background: 'var(--surface-container-lowest)',
                color: 'var(--on-surface)',
                border: '1px solid var(--outline-variant)',
                borderRadius: 'var(--radius)',
                fontFamily: 'var(--font-sans)',
                fontSize: '14px',
                boxShadow: '0 4px 14px rgba(0,0,0,0.08)',
              },
              success: {
                iconTheme: {
                  primary: 'var(--primary-container)',
                  secondary: 'var(--on-primary)',
                },
              },
            }}
          />

          {/* Auth Modal Overlay */}
          <AuthModal />

          {/* App Routes */}
          <Routes>
            <Route path="/" element={<FeedRoute />} />
            <Route path="/discover" element={<DiscoverPage />} />
            <Route path="/posts/:id" element={<PostDetailPage />} />
            <Route
              path="/posts/new"
              element={
                <ProtectedRoute>
                  <CreatePostPage />
                </ProtectedRoute>
              }
            />
            <Route path="/profile/:username" element={<ProfilePage />} />
            <Route
              path="/messages"
              element={
                <ProtectedRoute>
                  <MessagesPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/notifications"
              element={
                <ProtectedRoute>
                  <NotificationsPage />
                </ProtectedRoute>
              }
            />
            <Route path="/search" element={<SearchPage />} />
            <Route
              path="/bookmarks"
              element={
                <ProtectedRoute>
                  <BookmarksPage />
                </ProtectedRoute>
              }
            />
            <Route path="/oauth-callback" element={<OAuthCallbackPage />} />
            {/* Fallback */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </NotificationProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}
