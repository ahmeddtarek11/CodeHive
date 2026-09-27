import React, { createContext, useContext, useState, useEffect, useRef, useCallback } from 'react';
import type { Notification } from '../types';
import { initNotificationHubConnection } from '../api/chat';
import { notificationsApi } from '../api/notifications';
import { useAuth } from './AuthContext';
import toast from 'react-hot-toast';

interface NotificationContextType {
  notifications: Notification[];
  unreadCount: number;
  markAsRead: (id: string) => void;
  markAllAsRead: () => void;
  isNotificationsOpen: boolean;
  setIsNotificationsOpen: (open: boolean) => void;
  toggleNotifications: () => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAuth();
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);
  const hubRef = useRef<{ stop: () => void } | null>(null);

  // Load notifications from API when authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      setNotifications([]);
      return;
    }

    notificationsApi.getNotifications({ limit: 30 })
      .then((page) => setNotifications(page.items))
      .catch(() => {
        // Silently fail — user will still see the bell
      });
  }, [isAuthenticated]);

  // Real-time notification hub (only when authenticated)
  useEffect(() => {
    if (!isAuthenticated) return;

    const hub = initNotificationHubConnection((rawNotification: unknown) => {
      // The server sends a NotificationDto shape
      const n = rawNotification as {
        id: string;
        type: string;
        actorUsername: string;
        relatedPostId: string | null;
        isRead: boolean;
        createdAt: string;
      };

      const mapped: Notification = {
        id: n.id,
        type: n.type.toLowerCase() as Notification['type'],
        message: buildMessage(n.type),
        isRead: false,
        createdAt: n.createdAt,
        actor: { id: '', username: n.actorUsername, displayName: n.actorUsername, avatarUrl: null },
        targetPostId: n.relatedPostId ?? undefined,
      };

      setNotifications((prev) => [mapped, ...prev]);
      toast(`🔔 ${mapped.actor.displayName} ${mapped.message}`, { duration: 4000 });
    });

    hubRef.current = hub;
    return () => {
      hub.stop();
      hubRef.current = null;
    };
  }, [isAuthenticated]);

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const markAsRead = useCallback((id: string) => {
    setNotifications((prev) =>
      prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
    );
    notificationsApi.markAsRead(id).catch(() => {});
  }, []);

  const markAllAsRead = useCallback(async () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
    try {
      await notificationsApi.markAllAsRead();
      toast.success('All notifications marked as read');
    } catch {
      toast.error('Failed to mark notifications as read');
    }
  }, []);

  const toggleNotifications = () => setIsNotificationsOpen((prev) => !prev);

  return (
    <NotificationContext.Provider
      value={{
        notifications,
        unreadCount,
        markAsRead,
        markAllAsRead,
        isNotificationsOpen,
        setIsNotificationsOpen,
        toggleNotifications,
      }}
    >
      {children}
    </NotificationContext.Provider>
  );
};

export const useNotifications = (): NotificationContextType => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotifications must be used within a NotificationProvider');
  }
  return context;
};

function buildMessage(type: string): string {
  switch (type.toLowerCase()) {
    case 'like': return 'liked your post';
    case 'comment': return 'commented on your post';
    case 'follow': return 'started following you';
    case 'mention': return 'mentioned you in a post';
    default: return 'interacted with your content';
  }
}
