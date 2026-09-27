import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../components/layout/Layout';
import { Avatar } from '../components/user/Avatar';
import { useNotifications } from '../contexts/NotificationContext';
import { CheckCircle2, Heart, MessageSquare, UserPlus, AtSign, Award } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';

export const NotificationsPage: React.FC = () => {
  const { notifications, markAsRead, markAllAsRead } = useNotifications();
  const [filter, setFilter] = useState<'all' | 'unread'>('all');
  const navigate = useNavigate();

  const filteredNotifications = notifications.filter((n) => {
    if (filter === 'unread') return !n.isRead;
    return true;
  });

  const getIcon = (type: string) => {
    switch (type) {
      case 'like':
        return <Heart size={16} fill="var(--error)" color="var(--error)" />;
      case 'comment':
        return <MessageSquare size={16} color="var(--tertiary)" />;
      case 'follow':
        return <UserPlus size={16} color="var(--primary)" />;
      case 'mention':
        return <AtSign size={16} color="var(--primary-container)" />;
      case 'badge':
        return <Award size={16} color="var(--amber-highlight)" />;
      default:
        return <MessageSquare size={16} />;
    }
  };

  return (
    <Layout>
      <div
        style={{
          backgroundColor: 'var(--surface-container-lowest)',
          border: '1px solid var(--outline-variant)',
          borderRadius: 'var(--radius)',
          overflow: 'hidden',
        }}
      >
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            padding: '20px 24px',
            borderBottom: '1px solid var(--outline-variant)',
          }}
        >
          <div>
            <h1 style={{ fontSize: '24px', fontWeight: 600 }}>Notifications</h1>
            <p style={{ fontSize: '13.5px', color: 'var(--slate-text)', marginTop: '2px' }}>
              Stay updated with your articles, peers, and hive milestones.
            </p>
          </div>

          <button
            onClick={markAllAsRead}
            className="btn btn-outline btn-sm"
          >
            <CheckCircle2 size={16} /> Mark all as read
          </button>
        </div>

        {/* Filter Tabs */}
        <div
          style={{
            display: 'flex',
            gap: '8px',
            padding: '12px 24px',
            borderBottom: '1px solid var(--surface-container-high)',
            backgroundColor: 'var(--surface-container-low)',
          }}
        >
          <button
            className={`btn btn-sm ${filter === 'all' ? 'btn-amber' : 'btn-outline'}`}
            onClick={() => setFilter('all')}
          >
            All
          </button>
          <button
            className={`btn btn-sm ${filter === 'unread' ? 'btn-amber' : 'btn-outline'}`}
            onClick={() => setFilter('unread')}
          >
            Unread
          </button>
        </div>

        {/* Notifications List */}
        <div>
          {filteredNotifications.length === 0 ? (
            <div style={{ padding: '60px 20px', textAlign: 'center', color: 'var(--secondary)' }}>
              No notifications to display.
            </div>
          ) : (
            filteredNotifications.map((notif) => (
              <div
                key={notif.id}
                className={`notification-item ${!notif.isRead ? 'unread' : ''}`}
                style={{ padding: '20px 24px' }}
                onClick={() => {
                  markAsRead(notif.id);
                  if (notif.targetPostId) {
                    navigate(`/posts/${notif.targetPostId}`);
                  } else {
                    navigate(`/profile/${notif.actor.username}`);
                  }
                }}
              >
                <div style={{ position: 'relative' }}>
                  <Avatar
                    src={notif.actor.avatarUrl}
                    name={notif.actor.displayName}
                    size="md"
                  />
                  <div
                    style={{
                      position: 'absolute',
                      bottom: '-4px',
                      right: '-4px',
                      backgroundColor: 'var(--surface-container-lowest)',
                      border: '1px solid var(--outline-variant)',
                      borderRadius: '50%',
                      padding: '3px',
                      display: 'flex',
                    }}
                  >
                    {getIcon(notif.type)}
                  </div>
                </div>

                <div style={{ flex: 1, minWidth: 0 }}>
                  <p style={{ fontSize: '15px', color: 'var(--on-surface)', lineHeight: 1.45 }}>
                    <strong>{notif.actor.displayName}</strong> {notif.message}{' '}
                    {notif.targetPostTitle && (
                      <span style={{ fontWeight: 600 }}>"{notif.targetPostTitle}"</span>
                    )}
                  </p>

                  {notif.quoteSnippet && (
                    <div
                      style={{
                        fontSize: '13.5px',
                        fontStyle: 'italic',
                        color: 'var(--slate-text)',
                        borderLeft: '3px solid var(--outline-variant)',
                        paddingLeft: '10px',
                        marginTop: '6px',
                        lineHeight: 1.5,
                      }}
                    >
                      "{notif.quoteSnippet}"
                    </div>
                  )}

                  <span style={{ fontSize: '12px', color: 'var(--primary)', marginTop: '6px', display: 'block' }}>
                    {formatDistanceToNow(new Date(notif.createdAt), { addSuffix: true })}
                  </span>
                </div>

                {!notif.isRead && (
                  <div
                    style={{
                      width: '10px',
                      height: '10px',
                      backgroundColor: 'var(--primary-container)',
                      borderRadius: '50%',
                      alignSelf: 'center',
                    }}
                  />
                )}
              </div>
            ))
          )}
        </div>
      </div>
    </Layout>
  );
};
