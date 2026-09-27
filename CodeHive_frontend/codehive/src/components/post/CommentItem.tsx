import React, { useState } from 'react';
import { Comment } from '../../types';
import { Avatar } from '../user/Avatar';
import { ArrowUp, ArrowDown, MessageSquare, Share2, CheckCircle2 } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import toast from 'react-hot-toast';

interface CommentItemProps {
  comment: Comment;
  onReply?: (parentId: string, content: string) => void;
  isNested?: boolean;
}

export const CommentItem: React.FC<CommentItemProps> = ({
  comment,
  onReply,
  isNested = false,
}) => {
  const [likesCount, setLikesCount] = useState(comment.likesCount || 0);
  const [userVote, setUserVote] = useState<'up' | 'down' | null>(comment.userVote || null);
  const [isReplying, setIsReplying] = useState(false);
  const [replyText, setReplyText] = useState('');

  const formattedDate = (() => {
    try {
      return formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true });
    } catch {
      return 'just now';
    }
  })();

  const handleVote = (type: 'up' | 'down') => {
    if (userVote === type) {
      // Toggle off
      setUserVote(null);
      setLikesCount((prev) => (type === 'up' ? prev - 1 : prev + 1));
    } else {
      // Switch or new vote
      const delta = type === 'up' ? (userVote === 'down' ? 2 : 1) : userVote === 'up' ? -2 : -1;
      setUserVote(type);
      setLikesCount((prev) => prev + delta);
      if (type === 'up') toast.success('Upvoted comment');
    }
  };

  const handleSendReply = (e: React.FormEvent) => {
    e.preventDefault();
    if (!replyText.trim()) return;
    onReply?.(comment.id, replyText);
    setReplyText('');
    setIsReplying(false);
    toast.success('Reply submitted');
  };

  return (
    <div className="comment-thread-item" style={{ marginLeft: isNested ? '28px' : '0' }}>
      {!isNested && comment.replies && comment.replies.length > 0 && (
        <div className="comment-thread-line" />
      )}

      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', zIndex: 2 }}>
        <Avatar
          src={comment.author.avatarUrl}
          name={comment.author.displayName}
          size={isNested ? 'sm' : 'md'}
        />
      </div>

      <div className="comment-content-card">
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
          <span style={{ fontWeight: 600, fontSize: '14.5px', color: 'var(--on-surface)' }}>
            {comment.author.displayName}
          </span>
          {comment.author.isAuthor && (
            <span className="comment-author-badge">Author</span>
          )}
          <span style={{ fontSize: '12.5px', color: 'var(--secondary)' }}>
            {formattedDate}
          </span>
        </div>

        <div
          style={{
            fontFamily: 'var(--font-serif)',
            fontSize: '16.5px',
            lineHeight: '1.6',
            color: 'var(--on-surface)',
            marginBottom: '12px',
          }}
        >
          {comment.content}
        </div>

        {comment.codeSnippet && (
          <div className="code-block-container" style={{ margin: '12px 0' }}>
            <div className="code-block-header">
              <span>{comment.codeSnippet.language}</span>
            </div>
            <pre className="code-block-content">
              <code>{comment.codeSnippet.code}</code>
            </pre>
          </div>
        )}

        <div style={{ display: 'flex', alignItems: 'center', gap: '16px', marginTop: '8px' }}>
          {/* Vote pill */}
          <div
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              backgroundColor: 'var(--surface-container-low)',
              border: '1px solid var(--outline-variant)',
              borderRadius: 'var(--radius-full)',
              padding: '2px 8px',
              gap: '6px',
            }}
          >
            <button
              onClick={() => handleVote('up')}
              style={{
                background: 'none',
                border: 'none',
                color: userVote === 'up' ? 'var(--primary)' : 'var(--secondary)',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
              }}
              title="Upvote"
            >
              <ArrowUp size={16} strokeWidth={userVote === 'up' ? 3 : 2} />
            </button>
            <span style={{ fontSize: '13px', fontWeight: 600, minWidth: '18px', textAlign: 'center' }}>
              {likesCount}
            </span>
            <button
              onClick={() => handleVote('down')}
              style={{
                background: 'none',
                border: 'none',
                color: userVote === 'down' ? 'var(--error)' : 'var(--secondary)',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
              }}
              title="Downvote"
            >
              <ArrowDown size={16} strokeWidth={userVote === 'down' ? 3 : 2} />
            </button>
          </div>

          <button
            onClick={() => setIsReplying(!isReplying)}
            className="post-action-btn"
            style={{ padding: '4px 6px' }}
          >
            <MessageSquare size={16} />
            <span>Reply</span>
          </button>

          <button
            onClick={() => {
              navigator.clipboard?.writeText(window.location.href);
              toast.success('Link copied');
            }}
            className="post-action-btn"
            style={{ padding: '4px 6px' }}
          >
            <Share2 size={16} />
          </button>
        </div>

        {/* Inline reply editor */}
        {isReplying && (
          <form onSubmit={handleSendReply} style={{ marginTop: '14px' }}>
            <textarea
              className="textarea-field"
              style={{ minHeight: '70px', padding: '8px 12px', fontSize: '14px' }}
              placeholder={`Replying to ${comment.author.displayName}...`}
              value={replyText}
              onChange={(e) => setReplyText(e.target.value)}
              autoFocus
            />
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px', marginTop: '8px' }}>
              <button
                type="button"
                className="btn btn-outline btn-sm"
                onClick={() => setIsReplying(false)}
              >
                Cancel
              </button>
              <button type="submit" className="btn btn-amber btn-sm">
                Reply
              </button>
            </div>
          </form>
        )}

        {/* Render nested replies if any */}
        {comment.replies && comment.replies.length > 0 && (
          <div style={{ marginTop: '16px' }}>
            {comment.replies.map((reply) => (
              <CommentItem
                key={reply.id}
                comment={reply}
                isNested={true}
                onReply={onReply}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
