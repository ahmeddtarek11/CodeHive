import client from './client';
import type { Comment, PaginatedResult, PostDetail, PostSummary } from '../types';

// Backend PostDetailDto fields (camelCase after serialization):
//   id, author: { id, username, displayName, avatarUrl }, title, content, language,
//   tags, likeCount, commentCount, isLikedByCurrentUser, isBookmarkedByCurrentUser, createdAt, updatedAt
//
// Backend CommentDto: id, author: { id, username, displayName, avatarUrl }, content, createdAt, replies: []
//
// Backend PostSummaryDto (for list): id, authorUsername, authorAvatar, title, contentPreview, tags, likeCount, commentCount, createdAt

interface BackendPostAuthor {
  id: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
}

interface BackendPostDetail {
  id: string;
  author: BackendPostAuthor;
  title: string | null;
  content: string;
  language: string | null;
  tags: string[];
  likeCount: number;
  commentCount: number;
  isLikedByCurrentUser: boolean;
  isBookmarkedByCurrentUser: boolean;
  createdAt: string;
  updatedAt: string;
}

interface BackendPostSummary {
  id: string;
  authorUsername: string;
  authorAvatar: string | null;
  title: string | null;
  contentPreview: string;
  tags: string[];
  likeCount: number;
  commentCount: number;
  createdAt: string;
}

interface BackendComment {
  id: string;
  author: BackendPostAuthor;
  content: string;
  createdAt: string;
  replies: BackendComment[];
}

interface BackendCursorPage<T> {
  items: T[];
  nextCursor: string | null;
  hasMore: boolean;
}

function mapPostDetail(p: BackendPostDetail): PostDetail {
  return {
    id: p.id,
    title: p.title ?? '',
    excerpt: p.content.slice(0, 200),
    content: p.content,
    author: {
      id: p.author.id,
      username: p.author.username,
      displayName: p.author.displayName,
      avatarUrl: p.author.avatarUrl,
    },
    tags: p.tags ?? [],
    likesCount: p.likeCount,
    commentsCount: p.commentCount,
    isLiked: p.isLikedByCurrentUser,
    isBookmarked: p.isBookmarkedByCurrentUser,
    createdAt: p.createdAt,
  };
}

function mapPostSummary(p: BackendPostSummary): PostSummary {
  return {
    id: p.id,
    title: p.title ?? '',
    excerpt: p.contentPreview,
    author: {
      id: '',
      username: p.authorUsername,
      displayName: p.authorUsername,
      avatarUrl: p.authorAvatar,
    },
    tags: p.tags ?? [],
    likesCount: p.likeCount,
    commentsCount: p.commentCount,
    isLiked: false,
    isBookmarked: false,
    createdAt: p.createdAt,
  };
}

function mapComment(c: BackendComment): Comment {
  return {
    id: c.id,
    content: c.content,
    author: {
      id: c.author.id,
      username: c.author.username,
      displayName: c.author.displayName,
      avatarUrl: c.author.avatarUrl,
    },
    createdAt: c.createdAt,
    replies: c.replies?.map(mapComment) ?? [],
  };
}

export const postsApi = {
  // GET /api/v1/posts  ?authorId&cursor&limit
  getPosts: async (params?: { authorId?: string; cursor?: string | null; limit?: number }): Promise<PaginatedResult<PostSummary>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendPostSummary> }>(
      '/api/v1/posts',
      { params: { authorId: params?.authorId, cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    const page = res.data.data;
    return {
      items: page.items.map(mapPostSummary),
      nextCursor: page.nextCursor,
    };
  },

  // GET /api/v1/posts/:id
  getPostById: async (id: string): Promise<PostDetail> => {
    const res = await client.get<{ message: string; data: BackendPostDetail }>(`/api/v1/posts/${id}`);
    return mapPostDetail(res.data.data);
  },

  // POST /api/v1/posts  body: { type, title?, content, language?, tags }
  createPost: async (data: {
    type: 'Short' | 'Blog' | 'Snippet';
    title?: string;
    content: string;
    language?: string;
    tags: string[];
  }): Promise<{ postId: string }> => {
    const res = await client.post<{ message: string; data: { postId: string } }>('/api/v1/posts', data);
    return res.data.data;
  },

  // PUT /api/v1/posts/:id
  updatePost: async (id: string, data: { title: string; content: string; tags: string[] }): Promise<void> => {
    await client.put(`/api/v1/posts/${id}`, data);
  },

  // DELETE /api/v1/posts/:id
  deletePost: async (id: string): Promise<void> => {
    await client.delete(`/api/v1/posts/${id}`);
  },

  // POST /api/v1/posts/:id/like
  likePost: async (id: string): Promise<void> => {
    await client.post(`/api/v1/posts/${id}/like`);
  },

  // DELETE /api/v1/posts/:id/like
  unlikePost: async (id: string): Promise<void> => {
    await client.delete(`/api/v1/posts/${id}/like`);
  },

  // POST /api/v1/posts/:id/bookmark
  bookmarkPost: async (id: string): Promise<void> => {
    await client.post(`/api/v1/posts/${id}/bookmark`);
  },

  // DELETE /api/v1/posts/:id/bookmark
  unbookmarkPost: async (id: string): Promise<void> => {
    await client.delete(`/api/v1/posts/${id}/bookmark`);
  },

  // GET /api/v1/posts/:id/comments  ?cursor&limit
  getComments: async (postId: string, params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<Comment>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendComment> }>(
      `/api/v1/posts/${postId}/comments`,
      { params: { cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    const page = res.data.data;
    return {
      items: page.items.map(mapComment),
      nextCursor: page.nextCursor,
    };
  },

  // POST /api/v1/posts/:id/comments  body: { content }
  createComment: async (postId: string, data: { content: string }): Promise<{ commentId: string }> => {
    const res = await client.post<{ message: string; data: { commentId: string } }>(
      `/api/v1/posts/${postId}/comments`,
      data
    );
    return res.data.data;
  },

  // DELETE /api/v1/posts/:id/comments/:commentId
  deleteComment: async (postId: string, commentId: string): Promise<void> => {
    await client.delete(`/api/v1/posts/${postId}/comments/${commentId}`);
  },
};
