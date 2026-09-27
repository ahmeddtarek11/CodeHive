import React from 'react';
import { Layout } from '../components/layout/Layout';
import { PostCard } from '../components/post/PostCard';
import { useFeed } from '../hooks/useFeed';
import { Spinner } from '../components/ui/Spinner';

export const DiscoverPage: React.FC = () => {
  const { posts, loading, loadingMore, hasMore, loadMore, toggleLike, toggleBookmark } = useFeed('discover');

  return (
    <Layout>
      <div style={{ marginBottom: '16px' }}>
        <h2 style={{ fontSize: '24px', fontWeight: 600, color: 'var(--on-surface)' }}>
          Discover Trending Engineering Deep Dives
        </h2>
        <p style={{ fontSize: '14px', color: 'var(--slate-text)', marginTop: '4px' }}>
          Explore curated articles, architectural breakdowns, and benchmark analyses from engineers across the hive.
        </p>
      </div>

      {loading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '60px 0' }}>
          <Spinner size={32} />
        </div>
      ) : posts.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '60px 20px', backgroundColor: 'var(--surface-container-lowest)', borderRadius: 'var(--radius)', border: '1px solid var(--outline-variant)' }}>
          <h3 style={{ fontSize: '18px', fontWeight: 600, color: 'var(--on-surface)', marginBottom: '8px' }}>
            No articles found
          </h3>
          <p style={{ fontSize: '14px', color: 'var(--secondary)' }}>
            Be the first to publish a deep dive on CodeHive!
          </p>
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

      {!loading && posts.length > 0 && (
        <div className="load-more-wrapper">
          {hasMore ? (
            <button
              type="button"
              className="btn btn-outline"
              onClick={loadMore}
              disabled={loadingMore}
            >
              {loadingMore ? <Spinner size={18} /> : 'Load more discover posts'}
            </button>
          ) : (
            <p style={{ fontSize: '13px', color: 'var(--secondary)' }}>
              All discovered posts loaded.
            </p>
          )}
        </div>
      )}
    </Layout>
  );
};

