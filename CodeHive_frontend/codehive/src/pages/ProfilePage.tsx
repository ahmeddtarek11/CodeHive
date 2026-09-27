import React, { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Layout } from '../components/layout/Layout';
import { PostCard } from '../components/post/PostCard';
import { Avatar } from '../components/user/Avatar';
import { useAuth } from '../contexts/AuthContext';
import { useProfile } from '../hooks/useProfile';
import { useFeed } from '../hooks/useFeed';
import { usersApi } from '../api/users';
import { chatApi } from '../api/chat';
import { Spinner } from '../components/ui/Spinner';
import { Calendar, Edit3, Share2, ArrowRight, MessageSquare, X } from 'lucide-react';
import toast from 'react-hot-toast';

export const ProfilePage: React.FC = () => {
  const { username } = useParams<{ username: string }>();
  const navigate = useNavigate();
  const { user, isAuthenticated, openAuthModal, refreshCurrentUser } = useAuth();
  const { profile, loading, toggleFollow, refetch } = useProfile(username);

  const isOwn = profile ? (profile.isOwnProfile || user?.username === profile.username) : false;

  const { posts: userPosts, loading: loadingPosts, toggleLike, toggleBookmark } = useFeed(
    'user',
    profile?.id
  );

  const [isEditing, setIsEditing] = useState(false);
  const [editDisplayName, setEditDisplayName] = useState('');
  const [editBio, setEditBio] = useState('');
  const [editAvatarUrl, setEditAvatarUrl] = useState('');
  const [updating, setUpdating] = useState(false);

  const openEditModal = () => {
    if (profile) {
      setEditDisplayName(profile.displayName || '');
      setEditBio(profile.bio || '');
      setEditAvatarUrl(profile.avatarUrl || '');
      setIsEditing(true);
    }
  };

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    setUpdating(true);
    try {
      await usersApi.updateProfile({
        displayName: editDisplayName,
        bio: editBio,
        avatarUrl: editAvatarUrl || undefined,
      });
      await refreshCurrentUser();
      toast.success('Profile updated!');
      setIsEditing(false);
      refetch();
    } catch (err) {
      console.error(err);
      toast.error('Failed to update profile.');
    } finally {
      setUpdating(false);
    }
  };

  const handleMessageUser = async () => {
    if (!isAuthenticated) {
      openAuthModal('login');
      return;
    }
    if (!profile) return;
    try {
      const conversationId = await chatApi.createConversation({ targetUserId: profile.id });
      navigate(`/messages?conversation=${encodeURIComponent(conversationId)}`);
    } catch (err) {
      console.error(err);
      toast.error('Could not start conversation.');
    }
  };

  const handleShare = () => {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(window.location.href);
      toast.success('Profile URL copied to clipboard!');
    }
  };

  if (loading) {
    return (
      <Layout>
        <div style={{ display: 'flex', justifyContent: 'center', padding: '100px 0' }}>
          <Spinner size={36} />
        </div>
      </Layout>
    );
  }

  if (!profile) {
    return (
      <Layout>
        <div style={{ padding: '80px 20px', textAlign: 'center' }}>
          <h2>User profile not found</h2>
          <Link to="/" className="btn btn-primary" style={{ marginTop: '16px' }}>
            Back to Home
          </Link>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      {/* Profile Hero Card */}
      <div className="profile-hero-card">
        <div className="profile-avatar-wrapper">
          <Avatar
            src={profile.avatarUrl}
            name={profile.displayName || profile.username}
            size="xl"
            className="border-4"
          />
          {isOwn && (
            <button
              className="profile-avatar-edit-badge"
              onClick={openEditModal}
              title="Edit profile"
              aria-label="Edit profile"
            >
              <Edit3 size={16} />
            </button>
          )}
        </div>

        <h1 className="profile-name">{profile.displayName || profile.username}</h1>
        <p className="profile-handle">@{profile.username}</p>

        {/* Joined Date */}
        <div className="profile-meta-row">
          {profile.joinedDate && (
            <div className="profile-meta-item">
              <Calendar size={16} />
              <span>Joined {new Date(profile.joinedDate).toLocaleDateString()}</span>
            </div>
          )}
        </div>

        {/* Followers / Following counts */}
        <div
          style={{
            display: 'flex',
            gap: '24px',
            marginBottom: '20px',
            fontSize: '14.5px',
          }}
        >
          <div>
            <strong style={{ color: 'var(--on-surface)' }}>{profile.followersCount}</strong>{' '}
            <span style={{ color: 'var(--secondary)' }}>Followers</span>
          </div>
          <div>
            <strong style={{ color: 'var(--on-surface)' }}>{profile.followingCount}</strong>{' '}
            <span style={{ color: 'var(--secondary)' }}>Following</span>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="profile-actions-row">
          {isOwn ? (
            <button
              onClick={openEditModal}
              className="btn btn-amber"
            >
              Edit Profile
            </button>
          ) : (
            <>
              <button
                onClick={toggleFollow}
                className={`btn ${profile.isFollowing ? 'btn-ghost' : 'btn-primary'}`}
              >
                {profile.isFollowing ? 'Unfollow' : 'Follow'}
              </button>

              <button
                onClick={handleMessageUser}
                className="btn btn-amber"
              >
                <MessageSquare size={16} /> Message
              </button>
            </>
          )}

          <button
            onClick={handleShare}
            className="btn btn-outline"
          >
            <Share2 size={16} /> Share
          </button>
        </div>

        {/* Bio */}
        {profile.bio && <div className="profile-bio-text">{profile.bio}</div>}
      </div>

      {/* Activity Header */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          marginTop: '16px',
          marginBottom: '16px',
        }}
      >
        <h2 style={{ fontFamily: 'var(--font-serif)', fontSize: '22px', fontWeight: 600 }}>
          Published Articles ({userPosts.length})
        </h2>

        <Link
          to="/discover"
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '4px',
            fontSize: '14px',
            color: 'var(--primary)',
            fontWeight: 600,
          }}
        >
          View All <ArrowRight size={16} />
        </Link>
      </div>

      {/* Activity Cards List */}
      {loadingPosts ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '40px 0' }}>
          <Spinner size={28} />
        </div>
      ) : userPosts.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '40px 20px', backgroundColor: 'var(--surface-container-lowest)', borderRadius: 'var(--radius)', border: '1px solid var(--outline-variant)' }}>
          <p style={{ color: 'var(--secondary)', fontSize: '14px' }}>
            No articles published yet by @{profile.username}.
          </p>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          {userPosts.map((post) => (
            <PostCard
              key={post.id}
              post={post}
              onLike={toggleLike}
              onBookmark={toggleBookmark}
            />
          ))}
        </div>
      )}

      {/* Edit Profile Modal */}
      {isEditing && (
        <div className="modal-backdrop" onClick={() => setIsEditing(false)}>
          <div className="auth-modal-card" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '440px' }}>
            <button className="modal-close-btn" onClick={() => setIsEditing(false)}>
              <X size={20} />
            </button>
            <h2 style={{ fontSize: '22px', fontWeight: 700, marginBottom: '20px', color: 'var(--on-surface)' }}>
              Edit Profile
            </h2>
            <form onSubmit={handleUpdateProfile}>
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
                  Display Name
                </label>
                <input
                  type="text"
                  className="input-field"
                  value={editDisplayName}
                  onChange={(e) => setEditDisplayName(e.target.value)}
                  placeholder="Your display name"
                />
              </div>
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
                  Avatar URL
                </label>
                <input
                  type="text"
                  className="input-field"
                  value={editAvatarUrl}
                  onChange={(e) => setEditAvatarUrl(e.target.value)}
                  placeholder="https://..."
                />
              </div>
              <div style={{ marginBottom: '20px' }}>
                <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
                  Bio
                </label>
                <textarea
                  className="input-field"
                  rows={4}
                  value={editBio}
                  onChange={(e) => setEditBio(e.target.value)}
                  placeholder="Tell the community about yourself..."
                />
              </div>
              <div style={{ display: 'flex', gap: '12px', justifyContent: 'flex-end' }}>
                <button type="button" className="btn btn-outline" onClick={() => setIsEditing(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-amber" disabled={updating}>
                  {updating ? 'Saving...' : 'Save Changes'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </Layout>
  );
};
