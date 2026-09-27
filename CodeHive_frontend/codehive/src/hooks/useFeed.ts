import { useState, useEffect, useCallback } from 'react';
import { PostSummary } from '../types';
import { feedApi } from '../api/feed';
import { postsApi } from '../api/posts';

export function useFeed(
  type: 'following' | 'discover' | 'user' = 'following',
  authorId?: string
) {
  const [posts, setPosts] = useState<PostSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [nextCursor, setNextCursor] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(true);

  const fetchInitialPosts = useCallback(async () => {
    setLoading(true);
    try {
      let res;
      if (type === 'following') {
        res = await feedApi.getFeed({ limit: 10 });
      } else if (type === 'discover') {
        res = await feedApi.getDiscover({ limit: 10 });
      } else {
        res = await postsApi.getPosts({ authorId, limit: 10 });
      }

      setPosts(res.items);
      setNextCursor(res.nextCursor);
      setHasMore(!!res.nextCursor);
    } catch (err) {
      console.error('Error loading feed:', err);
      setPosts([]);
      setHasMore(false);
    } finally {
      setLoading(false);
    }
  }, [type, authorId]);

  useEffect(() => {
    fetchInitialPosts();
  }, [fetchInitialPosts]);

  const loadMore = async () => {
    if (!hasMore || loadingMore || !nextCursor) return;
    setLoadingMore(true);
    try {
      let res;
      if (type === 'following') {
        res = await feedApi.getFeed({ cursor: nextCursor, limit: 10 });
      } else if (type === 'discover') {
        res = await feedApi.getDiscover({ cursor: nextCursor, limit: 10 });
      } else {
        res = await postsApi.getPosts({ authorId, cursor: nextCursor, limit: 10 });
      }

      setPosts((prev) => [...prev, ...res.items]);
      setNextCursor(res.nextCursor);
      setHasMore(!!res.nextCursor);
    } catch (err) {
      console.error('Error loading more posts:', err);
    } finally {
      setLoadingMore(false);
    }
  };

  const toggleLike = async (postId: string) => {
    const target = posts.find((p) => p.id === postId);
    if (!target) return;
    const nextState = !target.isLiked;

    setPosts((prev) =>
      prev.map((p) => {
        if (p.id === postId) {
          return {
            ...p,
            isLiked: nextState,
            likesCount: nextState ? p.likesCount + 1 : Math.max(0, p.likesCount - 1),
          };
        }
        return p;
      })
    );

    try {
      if (nextState) {
        await postsApi.likePost(postId);
      } else {
        await postsApi.unlikePost(postId);
      }
    } catch {
      // Revert if API call fails
      setPosts((prev) =>
        prev.map((p) => (p.id === postId ? target : p))
      );
    }
  };

  const toggleBookmark = async (postId: string) => {
    const target = posts.find((p) => p.id === postId);
    if (!target) return;
    const nextState = !target.isBookmarked;

    setPosts((prev) =>
      prev.map((p) => {
        if (p.id === postId) {
          return { ...p, isBookmarked: nextState };
        }
        return p;
      })
    );

    try {
      if (nextState) {
        await postsApi.bookmarkPost(postId);
      } else {
        await postsApi.unbookmarkPost(postId);
      }
    } catch {
      setPosts((prev) =>
        prev.map((p) => (p.id === postId ? target : p))
      );
    }
  };

  return {
    posts,
    setPosts,
    loading,
    loadingMore,
    hasMore,
    loadMore,
    toggleLike,
    toggleBookmark,
    refetch: fetchInitialPosts,
  };
}

