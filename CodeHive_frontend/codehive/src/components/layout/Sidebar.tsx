import React, { useState, useEffect } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { Home, Bookmark, Tag, TrendingUp, Compass, Sparkles } from 'lucide-react';
import { feedApi } from '../../api/feed';
import { PostSummary } from '../../types';
import { Spinner } from '../ui/Spinner';

interface SidebarProps {
  variant?: 'left' | 'right';
}

export const Sidebar: React.FC<SidebarProps> = ({ variant = 'left' }) => {
  const navigate = useNavigate();
  const [trending, setTrending] = useState<PostSummary[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (variant === 'right') {
      setLoading(true);
      feedApi
        .getTrending()
        .then((res) => setTrending(res))
        .catch((err) => console.error('Failed to load trending posts:', err))
        .finally(() => setLoading(false));
    }
  }, [variant]);

  if (variant === 'right') {
    return (
      <aside className="sidebar-sticky hidden-on-tablet">
        <div className="sidebar-box">
          <div className="sidebar-title">
            <TrendingUp size={18} color="var(--primary)" />
            <span>Trending Posts</span>
          </div>

          {loading ? (
            <div style={{ display: 'flex', justifyContent: 'center', padding: '20px 0' }}>
              <Spinner size={20} />
            </div>
          ) : trending.length === 0 ? (
            <p style={{ fontSize: '13px', color: 'var(--secondary)', margin: '12px 0' }}>
              No trending articles right now.
            </p>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
              {trending.map((item) => (
                <div
                  key={item.id}
                  className="trending-item"
                  onClick={() => navigate(`/posts/${item.id}`)}
                >
                  <h4 className="trending-title">{item.title}</h4>
                  <p className="trending-snippet">{item.excerpt}</p>
                </div>
              ))}
            </div>
          )}

          <button
            onClick={() => navigate('/discover')}
            style={{
              marginTop: '16px',
              background: 'none',
              border: 'none',
              color: 'var(--primary)',
              fontSize: '13px',
              fontWeight: 600,
              cursor: 'pointer',
              padding: 0,
              display: 'inline-flex',
              alignItems: 'center',
              gap: '4px',
            }}
          >
            View all trending →
          </button>
        </div>
      </aside>
    );
  }

  return (
    <aside className="sidebar-sticky hidden-on-tablet">
      <div className="sidebar-box">
        <div className="sidebar-title">Quick Links</div>
        <ul className="quick-links-list">
          <li className="quick-link-item">
            <NavLink
              to="/"
              end
              className={({ isActive }) => (isActive ? 'active' : '')}
            >
              <Home size={18} />
              <span>Feed</span>
            </NavLink>
          </li>
          <li className="quick-link-item">
            <NavLink
              to="/discover"
              className={({ isActive }) => (isActive ? 'active' : '')}
            >
              <Compass size={18} />
              <span>Discover</span>
            </NavLink>
          </li>
          <li className="quick-link-item">
            <NavLink
              to="/bookmarks"
              className={({ isActive }) => (isActive ? 'active' : '')}
            >
              <Bookmark size={18} />
              <span>Bookmarks</span>
            </NavLink>
          </li>
          <li className="quick-link-item">
            <NavLink
              to="/search?q=react"
              className={({ isActive }) => (isActive ? 'active' : '')}
            >
              <Tag size={18} />
              <span>Tags</span>
            </NavLink>
          </li>
        </ul>
      </div>

      <div
        className="sidebar-box"
        style={{
          background: 'linear-gradient(135deg, var(--muted-honey), var(--surface-container-lowest))',
          borderColor: 'var(--amber-highlight)',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
          <Sparkles size={16} color="var(--primary)" />
          <span style={{ fontSize: '13px', fontWeight: 700, color: 'var(--primary)' }}>
            CodeHive Pro
          </span>
        </div>
        <p style={{ fontSize: '12.5px', color: 'var(--on-surface-variant)', lineHeight: 1.45, marginBottom: '12px' }}>
          Publish canonical technical deep-dives with syntax highlighting, custom domains, and peer reviews.
        </p>
        <button
          onClick={() => navigate('/posts/new')}
          className="btn btn-amber btn-sm"
          style={{ width: '100%' }}
        >
          Draft an Article
        </button>
      </div>
    </aside>
  );
};

