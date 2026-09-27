import client from './client';
import type { PaginatedResult, PostSummary, UserSummary } from '../types';

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

interface BackendUserSummary {
  id: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
}

interface BackendCursorPage<T> {
  items: T[];
  nextCursor: string | null;
  hasMore: boolean;
}

function mapPostSummary(p: BackendPostSummary): PostSummary {
  return {
    id: p.id,
    title: p.title ?? '',
    excerpt: p.contentPreview,
    author: { id: '', username: p.authorUsername, displayName: p.authorUsername, avatarUrl: p.authorAvatar },
    tags: p.tags ?? [],
    likesCount: p.likeCount,
    commentsCount: p.commentCount,
    isLiked: false,
    isBookmarked: false,
    createdAt: p.createdAt,
  };
}

function mapUserSummary(u: BackendUserSummary): UserSummary {
  return { id: u.id, username: u.username, displayName: u.displayName, avatarUrl: u.avatarUrl };
}

export const searchApi = {
  // GET /api/v1/search/posts  ?q&cursor&limit
  searchPosts: async (q: string, params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<PostSummary>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendPostSummary> }>(
      '/api/v1/search/posts',
      { params: { q, cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    return {
      items: res.data.data.items.map(mapPostSummary),
      nextCursor: res.data.data.nextCursor,
    };
  },

  // GET /api/v1/search/users  ?q&cursor&limit
  searchUsers: async (q: string, params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<UserSummary>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendUserSummary> }>(
      '/api/v1/search/users',
      { params: { q, cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    return {
      items: res.data.data.items.map(mapUserSummary),
      nextCursor: res.data.data.nextCursor,
    };
  },

  // GET /api/v1/search/tags  ?q
  searchTags: async (q: string): Promise<string[]> => {
    const res = await client.get<{ message: string; data: string[] }>(
      '/api/v1/search/tags',
      { params: { q } }
    );
    return res.data.data;
  },
};
