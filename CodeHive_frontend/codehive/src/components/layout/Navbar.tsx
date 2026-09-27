import React, { useState, useRef, useEffect } from 'react';
import { Link, NavLink, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useNotifications } from '../../contexts/NotificationContext';
import { Avatar } from '../user/Avatar';
import { Bell, MessageSquare, Search, CheckCircle, User, LogOut, Bookmark, PenSquare } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';

export const Navbar: React.FC = () => {
  const { user, isAuthenticated, logout, openAuthModal } = useAuth();
  const {
    notifications,
    unreadCount,
    markAsRead,
    markAllAsRead,
    isNotificationsOpen,
    setIsNotificationsOpen,
    toggleNotifications,
  } = useNotifications();

  const [searchQuery, setSearchQuery] = useState('');
  const [isProfileMenuOpen, setIsProfileMenuOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const dropdownRef = useRef<HTMLDivElement>(null);
  const profileMenuRef = useRef<HTMLDivElement>(null);

  // Close dropdowns on outside click
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setIsNotificationsOpen(false);
      }
      if (profileMenuRef.current && !profileMenuRef.current.contains(e.target as Node)) {
        setIsProfileMenuOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [setIsNotificationsOpen]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
    }
  };

  return (
    <nav className="navbar">
      <div className="navbar-inner">
        {/* Brand & Tabs */}
        <div className="navbar-brand-group">
          <Link to="/" className="navbar-logo-link">
            <img
              src="/logo.png"
              alt="CodeHive"
              className="navbar-logo-img"
              onError={(e) => {
                // If logo.png fails or is blank, fallback to logo.svg
                (e.currentTarget as HTMLImageElement).src = '/logo.svg';
              }}
            />
          </Link>

          <div className="navbar-nav-tabs">
            <NavLink
              to="/"
              end
              className={({ isActive }) =>
                `nav-tab-item ${isActive && location.pathname === '/' ? 'active' : ''}`
              }
            >
              Following
            </NavLink>
            <NavLink
              to="/discover"
              className={({ isActive }) => `nav-tab-item ${isActive ? 'active' : ''}`}
            >
              Discover
            </NavLink>
          </div>
        </div>

        {/* Center / Search */}
        <form onSubmit={handleSearchSubmit} className="navbar-search-wrapper">
          <span className="navbar-search-icon">
            <Search size={17} />
          </span>
          <input
            type="text"
            className="navbar-search-input"
            placeholder="Search articles, tags, authors..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </form>

        {/* Actions & User */}
        <div className="navbar-actions">
          {isAuthenticated && (
            <Link
              to="/posts/new"
              className="btn btn-amber btn-sm"
              style={{ display: 'flex', alignItems: 'center', gap: '6px' }}
            >
              <PenSquare size={15} />
              <span>Write</span>
            </Link>
          )}

          {/* Notifications Trigger */}
          <div style={{ position: 'relative' }} ref={dropdownRef}>
            <button
              type="button"
              className={`nav-icon-btn ${isNotificationsOpen ? 'active' : ''}`}
              onClick={toggleNotifications}
              title="Notifications"
              aria-label="Notifications"
            >
              <Bell size={20} />
              {unreadCount > 0 && <span className="badge-unread-dot" />}
            </button>

            {/* Notifications Dropdown Panel (matching screenshot) */}
            {isNotificationsOpen && (
              <div className="notifications-dropdown">
                <div className="notifications-header">
                  <h3 style={{ fontSize: '18px', fontWeight: 600 }}>Notifications</h3>
                  <button
                    onClick={markAllAsRead}
                    style={{
                      background: 'none',
                      border: 'none',
                      color: 'var(--secondary)',
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      gap: '4px',
                      fontSize: '13px',
                    }}
                    title="Mark all as read"
                  >
                    <CheckCircle size={16} /> Mark all read
                  </button>
                </div>

                <div className="notification-list-scroll">
                  {notifications.map((notif) => (
                    <div
                      key={notif.id}
                      className={`notification-item ${!notif.isRead ? 'unread' : ''}`}
                      onClick={() => {
                        markAsRead(notif.id);
                        if (notif.targetPostId) {
                          navigate(`/posts/${notif.targetPostId}`);
                          setIsNotificationsOpen(false);
                        } else {
                          navigate(`/profile/${notif.actor.username}`);
                          setIsNotificationsOpen(false);
                        }
                      }}
                    >
                      <Avatar
                        src={notif.actor.avatarUrl}
                        name={notif.actor.displayName}
                        size="sm"
                      />
                      <div style={{ flex: 1, minWidth: 0 }}>
                        <p style={{ fontSize: '13.5px', lineHeight: 1.4 }}>
                          <strong>{notif.actor.displayName}</strong> {notif.message}
                          {notif.targetPostTitle && (
                            <strong> "{notif.targetPostTitle}"</strong>
                          )}
                        </p>
                        {notif.quoteSnippet && (
                          <div
                            style={{
                              fontSize: '12.5px',
                              fontStyle: 'italic',
                              color: 'var(--secondary)',
                              borderLeft: '2px solid var(--outline-variant)',
                              paddingLeft: '8px',
                              marginTop: '4px',
                            }}
                          >
                            "{notif.quoteSnippet}"
                          </div>
                        )}
                        <span style={{ fontSize: '11.5px', color: 'var(--primary)', marginTop: '4px', display: 'block' }}>
                          {formatDistanceToNow(new Date(notif.createdAt), { addSuffix: true })}
                        </span>
                      </div>
                      {!notif.isRead && (
                        <div
                          style={{
                            width: '8px',
                            height: '8px',
                            backgroundColor: 'var(--primary-container)',
                            borderRadius: '50%',
                            flexShrink: 0,
                            marginTop: '6px',
                          }}
                        />
                      )}
                    </div>
                  ))}
                </div>

                <div
                  style={{
                    padding: '10px',
                    textAlign: 'center',
                    borderTop: '1px solid var(--surface-container-high)',
                    backgroundColor: 'var(--surface-container-lowest)',
                  }}
                >
                  <Link
                    to="/notifications"
                    onClick={() => setIsNotificationsOpen(false)}
                    style={{ fontSize: '13px', color: 'var(--primary)', fontWeight: 600 }}
                  >
                    View all notifications →
                  </Link>
                </div>
              </div>
            )}
          </div>

          {/* Messages Link */}
          <Link
            to="/messages"
            className={`nav-icon-btn ${location.pathname === '/messages' ? 'active' : ''}`}
            title="Messages"
            aria-label="Messages"
          >
            <MessageSquare size={20} />
          </Link>

          {/* User Menu / Auth Button */}
          {isAuthenticated && user ? (
            <div style={{ position: 'relative' }} ref={profileMenuRef}>
              <button
                type="button"
                onClick={() => setIsProfileMenuOpen(!isProfileMenuOpen)}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  padding: 0,
                  display: 'flex',
                }}
                aria-label="User profile menu"
              >
                <Avatar src={user.avatarUrl} name={user.displayName} size="sm" />
              </button>

              {isProfileMenuOpen && (
                <div
                  style={{
                    position: 'absolute',
                    top: '48px',
                    right: 0,
                    width: '210px',
                    backgroundColor: 'var(--surface-container-lowest)',
                    border: '1px solid var(--outline-variant)',
                    borderRadius: 'var(--radius)',
                    boxShadow: '0 6px 18px rgba(0,0,0,0.1)',
                    zIndex: 60,
                    padding: '6px 0',
                  }}
                >
                  <div style={{ padding: '10px 16px', borderBottom: '1px solid var(--surface-container-high)' }}>
                    <div style={{ fontWeight: 600, fontSize: '14px' }}>{user.displayName}</div>
                    <div style={{ fontSize: '12px', color: 'var(--secondary)' }}>@{user.username}</div>
                  </div>
                  <Link
                    to={`/profile/${user.username}`}
                    onClick={() => setIsProfileMenuOpen(false)}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '10px',
                      padding: '10px 16px',
                      fontSize: '13.5px',
                    }}
                    className="hover-bg-low"
                  >
                    <User size={16} /> Profile
                  </Link>
                  <Link
                    to="/bookmarks"
                    onClick={() => setIsProfileMenuOpen(false)}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '10px',
                      padding: '10px 16px',
                      fontSize: '13.5px',
                    }}
                  >
                    <Bookmark size={16} /> Bookmarks
                  </Link>
                  <button
                    onClick={() => {
                      setIsProfileMenuOpen(false);
                      logout();
                    }}
                    style={{
                      width: '100%',
                      display: 'flex',
                      alignItems: 'center',
                      gap: '10px',
                      padding: '10px 16px',
                      fontSize: '13.5px',
                      color: 'var(--error)',
                      background: 'none',
                      border: 'none',
                      borderTop: '1px solid var(--surface-container-high)',
                      cursor: 'pointer',
                      textAlign: 'left',
                    }}
                  >
                    <LogOut size={16} /> Log Out
                  </button>
                </div>
              )}
            </div>
          ) : (
            <button
              onClick={() => openAuthModal('login')}
              className="btn btn-primary btn-sm"
            >
              Sign In
            </button>
          )}
        </div>
      </div>
    </nav>
  );
};
