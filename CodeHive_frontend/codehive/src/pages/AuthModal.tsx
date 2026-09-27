import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { X, Mail, Lock, User, Github } from 'lucide-react';

export const AuthModal: React.FC = () => {
  const {
    isAuthModalOpen,
    closeAuthModal,
    authModalTab,
    setAuthModalTab,
    login,
    register,
    handleOAuthLogin,
  } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [username, setUsername] = useState('');
  const [loading, setLoading] = useState(false);

  if (!isAuthModalOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (authModalTab === 'login') {
        await login(email, password);
      } else {
        await register(username || email.split('@')[0], email, password);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={closeAuthModal}>
      <div className="auth-modal-card" onClick={(e) => e.stopPropagation()}>
        <button
          className="modal-close-btn"
          onClick={closeAuthModal}
          aria-label="Close modal"
        >
          <X size={20} />
        </button>

        <div style={{ textAlign: 'center', marginBottom: '28px' }}>
          <h1
            style={{
              fontFamily: 'var(--font-serif)',
              fontSize: '32px',
              fontWeight: 700,
              color: 'var(--primary)',
              marginBottom: '8px',
            }}
          >
            Join CodeHive
          </h1>
          <p
            style={{
              fontFamily: 'var(--font-serif)',
              fontSize: '16px',
              color: 'var(--on-surface-variant)',
              maxWidth: '320px',
              margin: '0 auto',
            }}
          >
            Join the Hive to read the full post, share your thoughts, and connect with the community.
          </p>
        </div>

        {/* Toggle Switch */}
        <div className="auth-toggle-slider">
          <button
            type="button"
            className={`auth-toggle-tab ${authModalTab === 'login' ? 'active' : ''}`}
            onClick={() => setAuthModalTab('login')}
          >
            Login
          </button>
          <button
            type="button"
            className={`auth-toggle-tab ${authModalTab === 'register' ? 'active' : ''}`}
            onClick={() => setAuthModalTab('register')}
          >
            Register
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit}>
          {authModalTab === 'register' && (
            <div style={{ position: 'relative', marginBottom: '16px' }}>
              <span
                style={{
                  position: 'absolute',
                  left: '14px',
                  top: '50%',
                  transform: 'translateY(-50%)',
                  color: 'var(--secondary)',
                  display: 'flex',
                }}
              >
                <User size={18} />
              </span>
              <input
                type="text"
                className="input-field"
                style={{ paddingLeft: '44px' }}
                placeholder="Choose a username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />
            </div>
          )}

          <div style={{ position: 'relative', marginBottom: '16px' }}>
            <span
              style={{
                position: 'absolute',
                left: '14px',
                top: '50%',
                transform: 'translateY(-50%)',
                color: 'var(--secondary)',
                display: 'flex',
              }}
            >
              <Mail size={18} />
            </span>
            <input
              type="email"
              className="input-field"
              style={{ paddingLeft: '44px' }}
              placeholder="Email address"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div style={{ position: 'relative', marginBottom: '20px' }}>
            <span
              style={{
                position: 'absolute',
                left: '14px',
                top: '50%',
                transform: 'translateY(-50%)',
                color: 'var(--secondary)',
                display: 'flex',
              }}
            >
              <Lock size={18} />
            </span>
            <input
              type="password"
              className="input-field"
              style={{ paddingLeft: '44px' }}
              placeholder="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button
            type="submit"
            className="btn btn-amber btn-lg btn-full"
            disabled={loading}
          >
            {loading ? 'Processing...' : 'Continue'}
          </button>
        </form>

        {/* Divider */}
        <div className="divider-with-text">
          <div className="divider-line" />
          <span className="divider-label">Or continue with</span>
          <div className="divider-line" />
        </div>

        {/* Social Buttons */}
        <div className="social-login-grid">
          <button
            type="button"
            className="social-btn"
            onClick={() => handleOAuthLogin('github')}
          >
            <Github size={18} />
            <span>GitHub</span>
          </button>
          <button
            type="button"
            className="social-btn"
            onClick={() => handleOAuthLogin('google')}
          >
            <svg width="18" height="18" viewBox="0 0 24 24">
              <path
                fill="#4285F4"
                d="M23.745 12.27c0-.7-.06-1.4-.19-2.07H12v4.51h6.6c-.29 1.52-1.14 2.8-2.4 3.66v3.04h3.88c2.28-2.09 3.66-5.18 3.66-9.14z"
              />
              <path
                fill="#34A853"
                d="M12 24c3.24 0 5.95-1.08 7.93-2.91l-3.88-3.04c-1.08.72-2.45 1.16-4.05 1.16-3.12 0-5.77-2.1-6.72-4.93H1.24v3.13C3.26 21.36 7.33 24 12 24z"
              />
              <path
                fill="#FBBC05"
                d="M5.28 14.28c-.25-.72-.38-1.49-.38-2.28s.13-1.56.38-2.28V6.59H1.24C.45 8.16 0 9.99 0 12s.45 3.84 1.24 5.41l4.04-3.13z"
              />
              <path
                fill="#EA4335"
                d="M12 4.75c1.77 0 3.35.61 4.6 1.8l3.42-3.42C17.95 1.19 15.24 0 12 0 7.33 0 3.26 2.64 1.24 6.59l4.04 3.13c.95-2.83 3.6-4.97 6.72-4.97z"
              />
            </svg>
            <span>Google</span>
          </button>
        </div>

        <div style={{ marginTop: '24px', textAlign: 'center' }}>
          <p style={{ fontSize: '12px', color: 'var(--secondary)' }}>
            By continuing, you agree to our{' '}
            <a href="#" style={{ color: 'var(--primary)', textDecoration: 'underline' }}>
              Terms of Service
            </a>{' '}
            and{' '}
            <a href="#" style={{ color: 'var(--primary)', textDecoration: 'underline' }}>
              Privacy Policy
            </a>
            .
          </p>
        </div>
      </div>
    </div>
  );
};
