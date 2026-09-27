import { useState, useEffect, useCallback } from 'react';
import { Comment, PostDetail } from '../types';
import { postsApi } from '../api/posts';
import toast from 'react-hot-toast';

export function usePost(postId?: string) {
  const [post, setPost] = useState<PostDetail | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchPostAndComments = useCallback(async () => {
    if (!postId) {
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const [postRes, commentsRes] = await Promise.all([
        postsApi.getPostById(postId),
        postsApi.getComments(postId),
      ]);
      setPost(postRes);
      setComments(commentsRes.items);
    } catch (err) {
      console.error('Error fetching post detail:', err);
      setPost(null);
    } finally {
      setLoading(false);
    }
  }, [postId]);

  useEffect(() => {
    fetchPostAndComments();
  }, [fetchPostAndComments]);

  const toggleLike = async () => {
    if (!post) return;
    const nextState = !post.isLiked;
    setPost({
      ...post,
      isLiked: nextState,
      likesCount: nextState ? post.likesCount + 1 : Math.max(0, post.likesCount - 1),
    });

    try {
      if (nextState) {
        await postsApi.likePost(post.id);
        toast.success('Liked post!');
      } else {
        await postsApi.unlikePost(post.id);
      }
    } catch {
      fetchPostAndComments();
    }
  };

  const toggleBookmark = async () => {
    if (!post) return;
    const nextState = !post.isBookmarked;
    setPost({
      ...post,
      isBookmarked: nextState,
    });

    try {
      if (nextState) {
        await postsApi.bookmarkPost(post.id);
        toast.success('Saved to bookmarks!');
      } else {
        await postsApi.unbookmarkPost(post.id);
        toast('Removed from bookmarks');
      }
    } catch {
      fetchPostAndComments();
    }
  };

  const addComment = async (content: string) => {
    if (!content.trim() || !post) return;
    try {
      await postsApi.createComment(post.id, { content });
      toast.success('Comment posted!');
      // Refresh comments
      const commentsRes = await postsApi.getComments(post.id);
      setComments(commentsRes.items);
      setPost((prev) => (prev ? { ...prev, commentsCount: prev.commentsCount + 1 } : prev));
    } catch (err) {
      console.error('Error adding comment:', err);
      toast.error('Failed to post comment');
    }
  };

  return { post, comments, loading, toggleLike, toggleBookmark, addComment, refetch: fetchPostAndComments };
}

