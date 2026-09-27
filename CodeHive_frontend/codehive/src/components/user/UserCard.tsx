import React, { useState } from 'react';
import { UserSummary } from '../../types';
import { Avatar } from './Avatar';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';

interface UserCardProps {
  user: UserSummary & { bio?: string; followersCount?: number };
}

export const UserCard: React.FC<UserCardProps> = ({ user }) => {
  const [isFollowing, setIsFollowing] = useState(false);

  const toggleFollow = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsFollowing((prev) => {
      const next = !prev;
      if (next) toast.success(`Followed @${user.username}`);
      else toast(`Unfollowed @${user.username}`);
      return next;
    });
  };

  return (
    <div
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '16px',
        backgroundColor: 'var(--surface-container-lowest)',
        border: '1px solid var(--outline-variant)',
        borderRadius: 'var(--radius)',
        marginBottom: '12px',
        gap: '16px',
      }}
    >
      <Link
        to={`/profile/${user.username}`}
        style={{ display: 'flex', alignItems: 'center', gap: '14px', flex: 1, minWidth: 0 }}
      >
        <Avatar src={user.avatarUrl} name={user.displayName} size="md" />
        <div style={{ minWidth: 0 }}>
          <div style={{ fontWeight: 600, fontSize: '15px', color: 'var(--on-surface)' }}>
            {user.displayName}
          </div>
          <div style={{ fontSize: '13px', color: 'var(--secondary)' }}>
            @{user.username}
          </div>
          {user.bio && (
            <div
              style={{
                fontSize: '13px',
                color: 'var(--slate-text)',
                marginTop: '4px',
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }}
            >
              {user.bio}
            </div>
          )}
        </div>
      </Link>
      <button
        onClick={toggleFollow}
        className={`btn ${isFollowing ? 'btn-ghost' : 'btn-primary'} btn-sm`}
        style={{ flexShrink: 0 }}
      >
        {isFollowing ? 'Following' : 'Follow'}
      </button>
    </div>
  );
};
