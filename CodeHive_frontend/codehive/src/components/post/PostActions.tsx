import React from 'react';
import { ThumbsUp, MessageSquare, Bookmark, Share2 } from 'lucide-react';
import toast from 'react-hot-toast';

interface PostActionsProps {
  likesCount: number;
  commentsCount: number;
  isLiked?: boolean;
  isBookmarked?: boolean;
  onLike?: () => void;
  onCommentClick?: () => void;
  onBookmark?: () => void;
  showShare?: boolean;
  onShare?: () => void;
  postId?: string;
  postTitle?: string;
}

export const PostActions: React.FC<PostActionsProps> = ({
  likesCount,
  commentsCount,
  isLiked = false,
  isBookmarked = false,
  onLike,
  onCommentClick,
  onBookmark,
  showShare = false,
  onShare,
  postTitle = 'CodeHive Post',
}) => {
  const handleShare = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (onShare) {
      onShare();
    } else {
      if (navigator.clipboard) {
        navigator.clipboard.writeText(window.location.href);
        toast.success('Link copied to clipboard!');
      } else {
        toast.success(`Shared: ${postTitle}`);
      }
    }
  };

  return (
    <div className="feed-card-footer">
      <button
        type="button"
        className={`post-action-btn ${isLiked ? 'active active-like' : ''}`}
        onClick={(e) => {
          e.stopPropagation();
          onLike?.();
        }}
        title="Like post"
      >
        <ThumbsUp size={18} fill={isLiked ? 'currentColor' : 'none'} />
        <span>{likesCount}</span>
      </button>

      <button
        type="button"
        className="post-action-btn"
        onClick={(e) => {
          e.stopPropagation();
          onCommentClick?.();
        }}
        title="View discussion"
      >
        <MessageSquare size={18} />
        <span>{commentsCount}</span>
      </button>

      <button
        type="button"
        className={`post-action-btn right-align ${isBookmarked ? 'active active-bookmark' : ''}`}
        onClick={(e) => {
          e.stopPropagation();
          onBookmark?.();
        }}
        title={isBookmarked ? 'Remove bookmark' : 'Bookmark post'}
      >
        <Bookmark size={18} fill={isBookmarked ? 'currentColor' : 'none'} />
      </button>

      {showShare && (
        <button
          type="button"
          className="post-action-btn"
          onClick={handleShare}
          title="Share post"
        >
          <Share2 size={18} />
        </button>
      )}
    </div>
  );
};
