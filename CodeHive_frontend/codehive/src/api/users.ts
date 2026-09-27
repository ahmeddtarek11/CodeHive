import client from './client';
import type { PaginatedResult, PostSummary, UserProfile, UserSummary } from '../types';

// Backend UserProfileDto: id, username, displayName, bio, avatarUrl, followersCount, followingCount, joinedAt
// Backend UserSummaryDto: id, username, displayName, avatarUrl
// Backend PostSummaryDto (bookmarks): same as posts list

interface BackendUserProfile {
  id: string;
  username: string;
  displayName: string;
  bio: string | null;
  avatarUrl: string | null;
  followersCount: number;
  followingCount: number;
  joinedAt: string;
}

interface BackendUserSummary {
  id: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
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

interface BackendCursorPage<T> {
  items: T[];
  nextCursor: string | null;
  hasMore: boolean;
}

function mapProfile(p: BackendUserProfile, currentUserId?: string): UserProfile {
  return {
    id: p.id,
    username: p.username,
    displayName: p.displayName,
    bio: p.bio,
    avatarUrl: p.avatarUrl,
    followersCount: p.followersCount,
    followingCount: p.followingCount,
    postsCount: 0,        // not in DTO — will be fetched via posts?authorId count if needed
    isFollowing: false,   // not in DTO — optimistic update pattern
    isOwnProfile: currentUserId ? p.id === currentUserId : false,
    joinedDate: p.joinedAt,
  };
}

function mapUserSummary(u: BackendUserSummary): UserSummary {
  return { id: u.id, username: u.username, displayName: u.displayName, avatarUrl: u.avatarUrl };
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

export const usersApi = {
  // GET /api/v1/users/me
  getCurrentProfile: async (): Promise<UserProfile> => {
    const res = await client.get<{ message: string; data: BackendUserProfile }>('/api/v1/users/me');
    return mapProfile(res.data.data, res.data.data.id);
  },

  // GET /api/v1/users/:username
  getProfile: async (username: string, currentUserId?: string): Promise<UserProfile> => {
    const res = await client.get<{ message: string; data: BackendUserProfile }>(`/api/v1/users/${username}`);
    return mapProfile(res.data.data, currentUserId);
  },

  // PUT /api/v1/users/me
  updateProfile: async (data: { displayName?: string; bio?: string; avatarUrl?: string }): Promise<void> => {
    await client.put('/api/v1/users/me', data);
  },

  // POST /api/v1/users/:id/follow
  followUser: async (id: string): Promise<void> => {
    await client.post(`/api/v1/users/${id}/follow`);
  },

  // DELETE /api/v1/users/:id/follow
  unfollowUser: async (id: string): Promise<void> => {
    await client.delete(`/api/v1/users/${id}/follow`);
  },

  // GET /api/v1/users/:id/followers
  getFollowers: async (id: string, params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<UserSummary>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendUserSummary> }>(
      `/api/v1/users/${id}/followers`,
      { params: { cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    return {
      items: res.data.data.items.map(mapUserSummary),
      nextCursor: res.data.data.nextCursor,
    };
  },

  // GET /api/v1/users/:id/following
  getFollowing: async (id: string, params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<UserSummary>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendUserSummary> }>(
      `/api/v1/users/${id}/following`,
      { params: { cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    return {
      items: res.data.data.items.map(mapUserSummary),
      nextCursor: res.data.data.nextCursor,
    };
  },

  // GET /api/v1/users/me/bookmarks
  getBookmarks: async (params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<PostSummary>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendPostSummary> }>(
      '/api/v1/users/me/bookmarks',
      { params: { cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    return {
      items: res.data.data.items.map(mapPostSummary),
      nextCursor: res.data.data.nextCursor,
    };
  },
};
