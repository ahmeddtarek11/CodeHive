import * as signalR from '@microsoft/signalr';
import client from './client';
import { API_BASE } from './client';
import { getToken } from '../utils/storage';
import type { Conversation, Message, PaginatedResult } from '../types';

// Backend ConversationDto: id, otherParticipant: {id, username, displayName, avatarUrl}, lastMessagePreview, lastMessageAt, unreadCount
// Backend MessageDto: id, senderId, senderUsername, content, isRead, sentAt
// Backend CursorPage<T>: { items, nextCursor, hasMore }
//
// SignalR events from server:
//   "NewMessage"  → { conversationId, message: MessageDto }
//   "UserTyping"  → { conversationId, senderUsername }

interface BackendConversation {
  id: string;
  otherParticipant: { id: string; username: string; displayName: string; avatarUrl: string | null };
  lastMessagePreview: string | null;
  lastMessageAt: string;
  unreadCount: number;
}

interface BackendMessage {
  id: string;
  senderId: string;
  senderUsername: string;
  content: string;
  isRead: boolean;
  sentAt: string;
}

interface BackendCursorPage<T> {
  items: T[];
  nextCursor: string | null;
  hasMore: boolean;
}

function mapConversation(c: BackendConversation, conversationId?: string): Conversation {
  return {
    id: conversationId ?? c.id,
    participant: {
      id: c.otherParticipant.id,
      username: c.otherParticipant.username,
      displayName: c.otherParticipant.displayName,
      avatarUrl: c.otherParticipant.avatarUrl,
    },
    lastMessage: c.lastMessagePreview ?? '',
    lastMessageAt: c.lastMessageAt,
    unreadCount: c.unreadCount,
  };
}

function mapMessage(m: BackendMessage, conversationId: string): Message {
  return {
    id: m.id,
    conversationId,
    content: m.content,
    senderId: m.senderId,
    createdAt: m.sentAt,
  };
}

export const chatApi = {
  // POST /api/v1/conversations  body: { targetUserId }
  // Backend returns the ID of the new or existing conversation.
  createConversation: async (data: { targetUserId: string }): Promise<string> => {
    const res = await client.post<{ message: string; data: { conversationId: string } }>(
      '/api/v1/conversations',
      { targetUserId: data.targetUserId }
    );
    return res.data.data.conversationId;
  },

  // GET /api/v1/conversations  ?cursor&limit
  getConversations: async (params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<Conversation>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendConversation> }>(
      '/api/v1/conversations',
      { params: { cursor: params?.cursor, limit: params?.limit ?? 20 } }
    );
    return {
      items: res.data.data.items.map((c) => mapConversation(c)),
      nextCursor: res.data.data.nextCursor,
    };
  },

  // GET /api/v1/conversations/:id/messages  ?cursor&limit
  getMessages: async (conversationId: string, params?: { cursor?: string | null; limit?: number }): Promise<PaginatedResult<Message>> => {
    const res = await client.get<{ message: string; data: BackendCursorPage<BackendMessage> }>(
      `/api/v1/conversations/${conversationId}/messages`,
      { params: { cursor: params?.cursor, limit: params?.limit ?? 30 } }
    );
    return {
      items: res.data.data.items.map((m) => mapMessage(m, conversationId)),
      nextCursor: res.data.data.nextCursor,
    };
  },

  // POST /api/v1/conversations/:id/messages  body: { content }
  sendMessage: async (conversationId: string, data: { content: string }): Promise<Message> => {
    const res = await client.post<{ message: string; data: BackendMessage }>(
      `/api/v1/conversations/${conversationId}/messages`,
      { content: data.content }
    );
    return mapMessage(res.data.data, conversationId);
  },

  // PATCH /api/v1/conversations/:id/read
  markConversationAsRead: async (conversationId: string): Promise<void> => {
    await client.patch(`/api/v1/conversations/${conversationId}/read`);
  },
};

// ── SignalR: /hubs/chat ───────────────────────────────────────────────────
export type ChatHubCallbacks = {
  onMessageReceived?: (conversationId: string, message: Message) => void;
  onUserTyping?: (conversationId: string, senderUsername: string) => void;
};

export function initChatHubConnection(callbacks: ChatHubCallbacks = {}) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE}/hubs/chat`, {
      accessTokenFactory: () => getToken() ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  connection.on('NewMessage', (payload: { conversationId: string; message: BackendMessage }) => {
    if (callbacks.onMessageReceived) {
      callbacks.onMessageReceived(
        payload.conversationId,
        mapMessage(payload.message, payload.conversationId)
      );
    }
  });

  connection.on('UserTyping', (payload: { conversationId: string; senderUsername: string }) => {
    if (callbacks.onUserTyping) {
      callbacks.onUserTyping(payload.conversationId, payload.senderUsername);
    }
  });

  connection.start().catch((err) => console.error('[SignalR Chat] Connection error:', err));

  return connection;
}

// ── SignalR: /hubs/notifications ──────────────────────────────────────────
export type NotificationHubCallbacks = {
  onNotificationReceived?: (notification: unknown) => void;
};

export function initNotificationHubConnection(callbacks: NotificationHubCallbacks | ((n: unknown) => void) = {}) {
  const onNotificationReceived = typeof callbacks === 'function'
    ? callbacks
    : callbacks.onNotificationReceived;

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE}/hubs/notifications`, {
      accessTokenFactory: () => getToken() ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  connection.on('NewNotification', (notification: unknown) => {
    if (onNotificationReceived) onNotificationReceived(notification);
  });

  connection.start().catch((err) => console.error('[SignalR Notifications] Connection error:', err));

  return connection;
}
