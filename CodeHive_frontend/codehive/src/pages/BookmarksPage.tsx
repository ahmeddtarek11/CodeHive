import React, { useState, useEffect } from 'react';
import { Layout } from '../components/layout/Layout';
import { PostCard } from '../components/post/PostCard';
import { EmptyState } from '../components/ui/EmptyState';
import { Spinner } from '../components/ui/Spinner';
import { PostSummary } from '../types';
import { usersApi } from '../api/users';
import { postsApi } from '../api/posts';
import { Bookmark } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';

export const BookmarksPage: React.FC = () => {
  const [bookmarks, setBookmarks] = useState<PostSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    async function loadBookmarks() {
      setLoading(true);
      try {
        const res = await usersApi.getBookmarks({ limit: 50 });
        setBookmarks(res.items.map((b) => ({ ...b, isBookmarked: true })));
      } catch (err) {
        console.error('Failed to load bookmarks:', err);
      } finally {
        setLoading(false);
      }
    }
    loadBookmarks();
  }, []);

  const handleToggleBookmark = async (id: string) => {
    setBookmarks((prev) => prev.filter((p) => p.id !== id));
    try {
      await postsApi.unbookmarkPost(id);
      toast('Removed from bookmarks');
    } catch (err) {
      console.error(err);
      toast.error('Failed to update bookmark');
    }
  };

  return (
    <Layout>
      <div style={{ marginBottom: '16px' }}>
        <h1 style={{ fontSize: '26px', fontWeight: 600 }}>Saved Bookmarks</h1>
        <p style={{ fontSize: '14px', color: 'var(--slate-text)', marginTop: '4px' }}>
          Articles and deep dives you've saved for offline reading or reference.
        </p>
      </div>

      {loading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '60px 0' }}>
          <Spinner size={32} />
        </div>
      ) : bookmarks.length === 0 ? (
        <EmptyState
          icon={<Bookmark size={28} />}
          title="No bookmarks yet"
          description="Click the bookmark icon on any article to save it to your personal library."
          actionText="Discover Articles"
          onAction={() => navigate('/discover')}
        />
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          {bookmarks.map((post) => (
            <PostCard
              key={post.id}
              post={post}
              onBookmark={handleToggleBookmark}
            />
          ))}
        </div>
      )}
    </Layout>
  );
};

