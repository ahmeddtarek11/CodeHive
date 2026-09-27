import client from './client';
import type { PaginatedResult, PostSummary } from '../types';

// Backend response wrapper: ApiResponse<T> = { message: string, data: T }
// Pagination: CursorPage<T> = { items: T[], nextCursor: string|null, hasMore: bool }
//
// Backend PostSummaryDto fields (PascalCase serialized as camelCase by ASP.NET):
//   id, authorUsername, authorAvatar, title, contentPreview, tags, likeCount, commentCount, createdAt
//
// We map them to the frontend PostSummary type.

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
    author: {
      id: '',
      username: p.authorUsername,
      displayName: p.authorUsername,
      avatarUrl: p.authorAvatar,
    },
    tags: p.tags ?? [],
    likesCount: p.likeCount,
    commentsCount: p.commentCount,
    isLiked: false,       // not returned by list endpoints
    isBookmarked: false,  // not returned by list endpoints
    createdAt: p.createdAt,
  };
}

export const feedApi = {
  // GET /api/v1/feed  (requires auth)
  getFeed: async (params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<PostSummary>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendPostSummary> }>(
      '/api/v1/feed',
      { params: { cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    const page = res.data.data;
    return {
      items: page.items.map(mapPostSummary),
      nextCursor: page.nextCursor,
    };
  },

  // GET /api/v1/feed/discover  (public)
  getDiscover: async (params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<PostSummary>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendPostSummary> }>(
      '/api/v1/feed/discover',
      { params: { cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    const page = res.data.data;
    return {
      items: page.items.map(mapPostSummary),
      nextCursor: page.nextCursor,
    };
  },

  // GET /api/v1/feed/trending  (public)
  getTrending: async (): Promise<PostSummary[]> => {
    const res = await client.get<{ message: string; data: BackendPostSummary[] }>('/api/v1/feed/trending');
    return res.data.data.map(mapPostSummary);
  },
};
