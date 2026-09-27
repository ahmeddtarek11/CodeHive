import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Layout } from '../components/layout/Layout';
import { PostCard } from '../components/post/PostCard';
import { UserCard } from '../components/user/UserCard';
import { Spinner } from '../components/ui/Spinner';
import { PostSummary, UserSummary } from '../types';
import { searchApi } from '../api/search';
import { postsApi } from '../api/posts';
import { Search } from 'lucide-react';

export const SearchPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const query = searchParams.get('q') || '';
  const [searchInput, setSearchInput] = useState(query);
  const [activeTab, setActiveTab] = useState<'posts' | 'users' | 'tags'>('posts');

  const [posts, setPosts] = useState<PostSummary[]>([]);
  const [users, setUsers] = useState<UserSummary[]>([]);
  const [tags, setTags] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setSearchInput(query);
  }, [query]);

  useEffect(() => {
    async function performSearch() {
      if (!query.trim()) {
        setPosts([]);
        setUsers([]);
        setTags([]);
        return;
      }
      setLoading(true);
      try {
        const [postsRes, usersRes, tagsRes] = await Promise.all([
          searchApi.searchPosts(query, { limit: 20 }),
          searchApi.searchUsers(query, { limit: 20 }),
          searchApi.searchTags(query),
        ]);
        setPosts(postsRes.items);
        setUsers(usersRes.items);
        setTags(tagsRes);
      } catch (err) {
        console.error('Error executing search:', err);
      } finally {
        setLoading(false);
      }
    }
    performSearch();
  }, [query]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchInput.trim()) {
      setSearchParams({ q: searchInput });
    }
  };

  const handleLike = async (postId: string) => {
    const target = posts.find((p) => p.id === postId);
    if (!target) return;
    const nextState = !target.isLiked;

    setPosts((prev) =>
      prev.map((p) =>
        p.id === postId
          ? { ...p, isLiked: nextState, likesCount: nextState ? p.likesCount + 1 : Math.max(0, p.likesCount - 1) }
          : p
      )
    );

    try {
      if (nextState) await postsApi.likePost(postId);
      else await postsApi.unlikePost(postId);
    } catch {
      setPosts((prev) => prev.map((p) => (p.id === postId ? target : p)));
    }
  };

  const handleBookmark = async (postId: string) => {
    const target = posts.find((p) => p.id === postId);
    if (!target) return;
    const nextState = !target.isBookmarked;

    setPosts((prev) =>
      prev.map((p) => (p.id === postId ? { ...p, isBookmarked: nextState } : p))
    );

    try {
      if (nextState) await postsApi.bookmarkPost(postId);
      else await postsApi.unbookmarkPost(postId);
    } catch {
      setPosts((prev) => prev.map((p) => (p.id === postId ? target : p)));
    }
  };

  return (
    <Layout>
      <div style={{ marginBottom: '24px' }}>
        <h1 style={{ fontSize: '28px', fontWeight: 600, marginBottom: '16px' }}>
          Search CodeHive
        </h1>

        <form onSubmit={handleSearchSubmit} style={{ display: 'flex', gap: '12px' }}>
          <div style={{ position: 'relative', flex: 1 }}>
            <span
              style={{
                position: 'absolute',
                left: '12px',
                top: '50%',
                transform: 'translateY(-50%)',
                color: 'var(--secondary)',
                display: 'flex',
              }}
            >
              <Search size={18} />
            </span>
            <input
              type="text"
              className="input-field"
              style={{ paddingLeft: '38px', fontSize: '16px' }}
              placeholder="Search posts, topics, authors..."
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
            />
          </div>
          <button type="submit" className="btn btn-amber">
            Search
          </button>
        </form>
      </div>

      {/* Tabs */}
      <div
        style={{
          display: 'flex',
          gap: '8px',
          borderBottom: '1px solid var(--outline-variant)',
          paddingBottom: '10px',
          marginBottom: '24px',
        }}
      >
        <button
          className={`btn btn-sm ${activeTab === 'posts' ? 'btn-amber' : 'btn-outline'}`}
          onClick={() => setActiveTab('posts')}
        >
          Posts ({posts.length})
        </button>
        <button
          className={`btn btn-sm ${activeTab === 'users' ? 'btn-amber' : 'btn-outline'}`}
          onClick={() => setActiveTab('users')}
        >
          Users ({users.length})
        </button>
        <button
          className={`btn btn-sm ${activeTab === 'tags' ? 'btn-amber' : 'btn-outline'}`}
          onClick={() => setActiveTab('tags')}
        >
          Tags ({tags.length})
        </button>
      </div>

      {loading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '60px 0' }}>
          <Spinner size={32} />
        </div>
      ) : (
        <>
          {/* Results content */}
          {activeTab === 'posts' && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              {posts.length === 0 ? (
                <div style={{ padding: '60px 20px', textAlign: 'center', color: 'var(--secondary)' }}>
                  {query ? `No articles found matching "${query}".` : 'Enter a search term to find articles.'}
                </div>
              ) : (
                posts.map((post) => (
                  <PostCard
                    key={post.id}
                    post={post}
                    onLike={handleLike}
                    onBookmark={handleBookmark}
                  />
                ))
              )}
            </div>
          )}

          {activeTab === 'users' && (
            <div>
              {users.length === 0 ? (
                <div style={{ padding: '60px 20px', textAlign: 'center', color: 'var(--secondary)' }}>
                  {query ? `No developers found matching "${query}".` : 'Enter a search term to find developers.'}
                </div>
              ) : (
                users.map((u) => <UserCard key={u.id} user={u} />)
              )}
            </div>
          )}

          {activeTab === 'tags' && (
            <div
              style={{
                display: 'flex',
                flexWrap: 'wrap',
                gap: '12px',
                backgroundColor: 'var(--surface-container-lowest)',
                border: '1px solid var(--outline-variant)',
                borderRadius: 'var(--radius)',
                padding: '24px',
              }}
            >
              {tags.length === 0 ? (
                <div style={{ width: '100%', textAlign: 'center', color: 'var(--secondary)' }}>
                  {query ? `No tags matching "${query}".` : 'Enter a topic to search tags.'}
                </div>
              ) : (
                tags.map((tag) => (
                  <button
                    key={tag}
                    className="btn btn-outline btn-sm"
                    onClick={() => {
                      setSearchParams({ q: tag });
                      setActiveTab('posts');
                    }}
                  >
                    #{tag}
                  </button>
                ))
              )}
            </div>
          )}
        </>
      )}
    </Layout>
  );
};

