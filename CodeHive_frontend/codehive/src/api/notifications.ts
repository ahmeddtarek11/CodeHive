import client from './client';
import type { Notification, PaginatedResult } from '../types';

// Backend NotificationDto (camelCase):
//   id, type (NotificationType enum as string), actorUsername, relatedPostId, isRead, createdAt
//
// NotificationType enum values from backend: Like, Comment, Follow, Mention (string enum)
// Backend GetUnreadCountQuery returns: { data: number } (raw count, not an object)

interface BackendNotification {
  id: string;
  type: string;  // 'Like' | 'Comment' | 'Follow' | 'Mention'
  actorUsername: string;
  relatedPostId: string | null;
  isRead: boolean;
  createdAt: string;
}

interface BackendCursorPage<T> {
  items: T[];
  nextCursor: string | null;
  hasMore: boolean;
}

function mapNotificationType(t: string): Notification['type'] {
  const lower = t.toLowerCase();
  if (lower === 'like') return 'like';
  if (lower === 'comment') return 'comment';
  if (lower === 'follow') return 'follow';
  if (lower === 'mention') return 'mention';
  return 'like';
}

function buildNotificationMessage(type: string): string {
  switch (type.toLowerCase()) {
    case 'like': return 'liked your post';
    case 'comment': return 'commented on your post';
    case 'follow': return 'started following you';
    case 'mention': return 'mentioned you in a post';
    default: return 'interacted with your content';
  }
}

function mapNotification(n: BackendNotification): Notification {
  return {
    id: n.id,
    type: mapNotificationType(n.type),
    message: buildNotificationMessage(n.type),
    isRead: n.isRead,
    createdAt: n.createdAt,
    actor: {
      id: '',
      username: n.actorUsername,
      displayName: n.actorUsername,
      avatarUrl: null,
    },
    targetPostId: n.relatedPostId ?? undefined,
  };
}

export const notificationsApi = {
  // GET /api/v1/notifications  ?cursor&limit
  getNotifications: async (params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<Notification>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendNotification> }>(
      '/api/v1/notifications',
      { params: { cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    return {
      items: res.data.data.items.map(mapNotification),
      nextCursor: res.data.data.nextCursor,
    };
  },

  // GET /api/v1/notifications/unread-count
  // Backend returns: ApiResponse<number> (data is directly the count)
  getUnreadCount: async (): Promise<number> => {
    const res = await client.get<{ message: string; data: number }>('/api/v1/notifications/unread-count');
    return res.data.data;
  },

  // PATCH /api/v1/notifications/:id/read
  markAsRead: async (id: string): Promise<void> => {
    await client.patch(`/api/v1/notifications/${id}/read`);
  },

  // PATCH /api/v1/notifications/read-all
  markAllAsRead: async (): Promise<void> => {
    await client.patch('/api/v1/notifications/read-all');
  },
};
