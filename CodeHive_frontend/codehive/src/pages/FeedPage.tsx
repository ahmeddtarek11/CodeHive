import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../components/layout/Layout';
import { PostCard } from '../components/post/PostCard';
import { Avatar } from '../components/user/Avatar';
import { useAuth } from '../contexts/AuthContext';
import { useFeed } from '../hooks/useFeed';
import { Spinner } from '../components/ui/Spinner';

export const FeedPage: React.FC = () => {
  const { user, isAuthenticated, openAuthModal } = useAuth();
  const navigate = useNavigate();
  const { posts, loading, loadingMore, hasMore, loadMore, toggleLike, toggleBookmark } = useFeed('following');

  const handleQuickCreateClick = () => {
    if (!isAuthenticated) {
      openAuthModal('login');
    } else {
      navigate('/posts/new');
    }
  };

  return (
    <Layout>
      {/* Quick Share Bar matching prototype */}
      <div className="create-post-quick-bar">
        <Avatar src={user?.avatarUrl} name={user?.displayName || 'User'} size="sm" />
        <input
          type="text"
          className="create-post-quick-input"
          placeholder="Share your latest build or thought..."
          onClick={handleQuickCreateClick}
          readOnly
        />
        <button
          type="button"
          className="btn btn-primary"
          onClick={handleQuickCreateClick}
        >
          Post
        </button>
      </div>

      {/* Feed list */}
      {loading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '60px 0' }}>
          <Spinner size={32} />
        </div>
      ) : posts.length === 0 ? (
        <div className="empty-state-box" style={{ textAlign: 'center', padding: '60px 20px', backgroundColor: 'var(--surface-container-lowest)', borderRadius: 'var(--radius)', border: '1px solid var(--outline-variant)' }}>
          <h3 style={{ fontSize: '18px', fontWeight: 600, color: 'var(--on-surface)', marginBottom: '8px' }}>
            Your feed is empty
          </h3>
          <p style={{ fontSize: '14px', color: 'var(--secondary)', marginBottom: '20px' }}>
            Follow other developers or check out the Discover page to see trending articles.
          </p>
          <button
            type="button"
            className="btn btn-amber"
            onClick={() => navigate('/discover')}
          >
            Explore Discover
          </button>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          {posts.map((post) => (
            <PostCard
              key={post.id}
              post={post}
              onLike={toggleLike}
              onBookmark={toggleBookmark}
            />
          ))}
        </div>
      )}

      {/* Infinite scroll load more */}
      {!loading && posts.length > 0 && (
        <div className="load-more-wrapper">
          {hasMore ? (
            <button
              type="button"
              className="btn btn-outline"
              onClick={loadMore}
              disabled={loadingMore}
            >
              {loadingMore ? <Spinner size={18} /> : 'Load more posts'}
            </button>
          ) : (
            <p style={{ fontSize: '13px', color: 'var(--secondary)' }}>
              You have caught up with all following updates.
            </p>
          )}
        </div>
      )}
    </Layout>
  );
};

