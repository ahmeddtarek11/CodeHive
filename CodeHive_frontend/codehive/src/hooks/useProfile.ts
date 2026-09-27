import { useState, useEffect, useCallback } from 'react';
import { UserProfile } from '../types';
import { usersApi } from '../api/users';
import { useAuth } from '../contexts/AuthContext';
import toast from 'react-hot-toast';

export function useProfile(username?: string) {
  const { user } = useAuth();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);

  const fetchProfile = useCallback(async () => {
    const targetUsername = username || user?.username;
    if (!targetUsername) {
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const data = await usersApi.getProfile(targetUsername, user?.id);
      setProfile(data);
    } catch (err) {
      console.error('Error fetching profile:', err);
      setProfile(null);
    } finally {
      setLoading(false);
    }
  }, [username, user?.username, user?.id]);

  useEffect(() => {
    fetchProfile();
  }, [fetchProfile]);

  const toggleFollow = async () => {
    if (!profile) return;
    const nextState = !profile.isFollowing;
    setProfile((prev) =>
      prev
        ? {
            ...prev,
            isFollowing: nextState,
            followersCount: nextState ? prev.followersCount + 1 : Math.max(0, prev.followersCount - 1),
          }
        : null
    );

    try {
      if (nextState) {
        await usersApi.followUser(profile.id);
        toast.success(`Following @${profile.username}`);
      } else {
        await usersApi.unfollowUser(profile.id);
        toast(`Unfollowed @${profile.username}`);
      }
    } catch {
      fetchProfile();
    }
  };

  return { profile, setProfile, loading, toggleFollow, refetch: fetchProfile };
}

