export interface PaginatedResult<T> {
  items: T[];
  nextCursor: string | null;
}

export interface UserSummary {
  id: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
}

export interface UserProfile extends UserSummary {
  bio: string | null;
  followersCount: number;
  followingCount: number;
  postsCount: number;
  isFollowing: boolean;
  isOwnProfile: boolean;
  location?: string;
  joinedDate?: string;
  expertise?: { name: string; icon?: string; badgeText?: string }[];
}

export interface PostSummary {
  id: string;
  title: string;
  excerpt: string;
  author: UserSummary;
  tags: string[];
  likesCount: number;
  commentsCount: number;
  isLiked: boolean;
  isBookmarked: boolean;
  createdAt: string; // ISO
  coverImage?: string;
  readTimeMinutes?: number;
}

export interface PostDetail extends PostSummary {
  content: string; // HTML
}

export interface Comment {
  id: string;
  content: string;
  author: UserSummary & { isAuthor?: boolean; title?: string };
  createdAt: string;
  likesCount?: number;
  userVote?: 'up' | 'down' | null;
  codeSnippet?: { language: string; code: string };
  replies?: Comment[];
}

export interface Notification {
  id: string;
  type: 'like' | 'comment' | 'follow' | 'mention' | 'badge';
  message: string;
  isRead: boolean;
  createdAt: string;
  actor: UserSummary;
  targetPostId?: string;
  targetPostTitle?: string;
  quoteSnippet?: string;
}

export interface Conversation {
  id: string;
  participant: UserSummary & { isOnline?: boolean };
  lastMessage: string;
  lastMessageAt: string;
  unreadCount: number;
}

export interface Message {
  id: string;
  conversationId: string;
  content: string;
  senderId: string;
  createdAt: string;
  codeBlock?: { filename: string; code: string };
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;
  username: string;
  avatarUrl: string | null;
}

export interface ApiResponse<T> {
  message: string;
  data: T;
}
