import React from 'react';
import { useNavigate } from 'react-router-dom';
import { PostSummary } from '../../types';
import { Avatar } from '../user/Avatar';
import { PostActions } from './PostActions';
import { formatDistanceToNow } from 'date-fns';
import { Copy, Bookmark } from 'lucide-react';
import toast from 'react-hot-toast';

interface PostCardProps {
  post: PostSummary & { codeSnippet?: { lang: string; code: string } };
  onLike?: (id: string) => void;
  onBookmark?: (id: string) => void;
}

export const PostCard: React.FC<PostCardProps> = ({ post, onLike, onBookmark }) => {
  const navigate = useNavigate();

  const formattedDate = (() => {
    try {
      return formatDistanceToNow(new Date(post.createdAt), { addSuffix: true });
    } catch {
      return 'recently';
    }
  })();

  const handleCardClick = () => {
    navigate(`/posts/${post.id}`);
  };

  const handleCopyCode = (e: React.MouseEvent, code: string) => {
    e.stopPropagation();
    navigator.clipboard.writeText(code);
    toast.success('Code copied to clipboard!');
  };

  return (
    <article className="feed-card" onClick={handleCardClick} style={{ cursor: 'pointer' }}>
      <div className="feed-card-header">
        <div
          className="feed-card-author-info"
          onClick={(e) => {
            e.stopPropagation();
            navigate(`/profile/${post.author.username}`);
          }}
          style={{ cursor: 'pointer' }}
        >
          <Avatar src={post.author.avatarUrl} name={post.author.displayName} size="sm" />
          <div>
            <div className="author-name">{post.author.displayName}</div>
            <div className="author-meta">@{post.author.username} • {formattedDate}</div>
          </div>
        </div>

        <button
          className={`post-action-btn ${post.isBookmarked ? 'active active-bookmark' : ''}`}
          onClick={(e) => {
            e.stopPropagation();
            onBookmark?.(post.id);
          }}
          title="Bookmark"
        >
          <Bookmark size={18} fill={post.isBookmarked ? 'currentColor' : 'none'} />
        </button>
      </div>

      <h2 className="feed-card-title">{post.title}</h2>

      <p className="feed-card-excerpt">{post.excerpt}</p>

      {/* Optional code snippet preview matching screenshots */}
      {post.codeSnippet && (
        <div
          className="code-block-container"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="code-block-header">
            <span>{post.codeSnippet.lang}</span>
            <button
              className="code-copy-btn"
              onClick={(e) => handleCopyCode(e, post.codeSnippet!.code)}
              title="Copy code"
            >
              <Copy size={14} /> Copy
            </button>
          </div>
          <pre className="code-block-content">
            <code>{post.codeSnippet.code}</code>
          </pre>
        </div>
      )}

      {post.tags && post.tags.length > 0 && (
        <div className="tags-row" onClick={(e) => e.stopPropagation()}>
          {post.tags.map((tag) => (
            <span
              key={tag}
              className="tag-chip"
              onClick={() => navigate(`/search?q=${encodeURIComponent(tag)}`)}
              style={{ cursor: 'pointer' }}
            >
              #{tag}
            </span>
          ))}
        </div>
      )}

      <PostActions
        likesCount={post.likesCount}
        commentsCount={post.commentsCount}
        isLiked={post.isLiked}
        isBookmarked={post.isBookmarked}
        onLike={() => onLike?.(post.id)}
        onCommentClick={handleCardClick}
        onBookmark={() => onBookmark?.(post.id)}
        postId={post.id}
        postTitle={post.title}
      />
    </article>
  );
};
