import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Navbar } from '../components/layout/Navbar';
import { Avatar } from '../components/user/Avatar';
import { CommentItem } from '../components/post/CommentItem';
import { usePost } from '../hooks/usePost';
import { useAuth } from '../contexts/AuthContext';
import { Spinner } from '../components/ui/Spinner';
import {
  ThumbsUp,
  MessageSquare,
  Bookmark,
  Share2,
  Clock,
  Calendar,
  Bold,
  Italic,
  Code,
  Link2,
  Quote,
  ArrowLeft,
} from 'lucide-react';
import toast from 'react-hot-toast';

export const PostDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { isAuthenticated, openAuthModal } = useAuth();
  const { post, comments, loading, toggleLike, toggleBookmark, addComment } = usePost(id);

  const [scrollProgress, setScrollProgress] = useState(0);
  const [newCommentText, setNewCommentText] = useState('');

  // Scroll Progress Tracking
  useEffect(() => {
    const handleScroll = () => {
      const totalHeight = document.documentElement.scrollHeight - window.innerHeight;
      if (totalHeight > 0) {
        const currentProgress = (window.scrollY / totalHeight) * 100;
        setScrollProgress(Math.min(100, Math.max(0, currentProgress)));
      }
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const handlePostComment = (e: React.FormEvent) => {
    e.preventDefault();
    if (!isAuthenticated) {
      openAuthModal('login');
      return;
    }
    if (!newCommentText.trim()) return;
    addComment(newCommentText);
    setNewCommentText('');
  };

  const scrollToDiscussion = () => {
    document.getElementById('discussion')?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleShare = () => {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(window.location.href);
      toast.success('Link copied to clipboard!');
    }
  };

  if (loading) {
    return (
      <>
        <Navbar />
        <div style={{ display: 'flex', justifyContent: 'center', padding: '120px 0' }}>
          <Spinner size={36} />
        </div>
      </>
    );
  }

  if (!post) {
    return (
      <>
        <Navbar />
        <div style={{ padding: '100px 20px', textAlign: 'center' }}>
          <h2>Post not found</h2>
          <Link to="/" className="btn btn-primary" style={{ marginTop: '16px' }}>
            Back to Feed
          </Link>
        </div>
      </>
    );
  }

  return (
    <>
      <Navbar />

      {/* 3px Amber Scroll Progress Indicator */}
      <div
        className="scroll-progress-bar"
        style={{ width: `${scrollProgress}%` }}
      />

      {/* Floating / Sticky Interaction Bar */}
      <div className="sticky-interaction-bar">
        <button
          className={`interaction-btn ${post.isLiked ? 'active' : ''}`}
          onClick={toggleLike}
          title="Like article"
          aria-label="Like article"
        >
          <ThumbsUp size={20} fill={post.isLiked ? 'currentColor' : 'none'} />
          <span className="interaction-btn-count">{post.likesCount}</span>
        </button>

        <button
          className="interaction-btn"
          onClick={scrollToDiscussion}
          title="Jump to discussion"
          aria-label="Jump to discussion"
        >
          <MessageSquare size={20} />
          <span className="interaction-btn-count">{comments.length}</span>
        </button>

        <button
          className={`interaction-btn ${post.isBookmarked ? 'active' : ''}`}
          onClick={toggleBookmark}
          title="Bookmark article"
          aria-label="Bookmark article"
        >
          <Bookmark size={20} fill={post.isBookmarked ? 'currentColor' : 'none'} />
        </button>

        <button
          className="interaction-btn"
          onClick={handleShare}
          title="Share article"
          aria-label="Share article"
        >
          <Share2 size={20} />
        </button>
      </div>

      <main style={{ maxWidth: '840px', margin: '0 auto', padding: '96px 24px 80px' }}>
        <div style={{ marginBottom: '16px' }}>
          <Link
            to="/"
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: '6px',
              fontSize: '14px',
              color: 'var(--secondary)',
              fontWeight: 500,
            }}
          >
            <ArrowLeft size={16} /> Back to Timeline
          </Link>
        </div>

        <article
          style={{
            backgroundColor: 'var(--surface-container-lowest)',
            border: '1px solid var(--outline-variant)',
            borderRadius: 'var(--radius)',
            padding: '40px',
          }}
        >
          {/* Header */}
          <header className="post-detail-header">
            <div className="tags-row" style={{ marginBottom: '16px' }}>
              {post.tags.map((t) => (
                <span key={t} className="tag-chip">
                  #{t}
                </span>
              ))}
            </div>

            <h1 className="post-detail-title">{post.title}</h1>

            {post.excerpt && <p className="post-detail-lead">{post.excerpt}</p>}

            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                flexWrap: 'wrap',
                gap: '16px',
              }}
            >
              <Link
                to={`/profile/${post.author.username}`}
                style={{ display: 'flex', alignItems: 'center', gap: '14px' }}
              >
                <Avatar
                  src={post.author.avatarUrl}
                  name={post.author.displayName || post.author.username}
                  size="md"
                />
                <div>
                  <div style={{ fontWeight: 700, fontSize: '15px' }}>
                    {post.author.displayName || post.author.username}
                  </div>
                  <div style={{ fontSize: '13px', color: 'var(--secondary)' }}>
                    @{post.author.username}
                  </div>
                </div>
              </Link>

              <div style={{ display: 'flex', alignItems: 'center', gap: '16px', color: 'var(--secondary)', fontSize: '13.5px' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                  <Calendar size={15} /> {new Date(post.createdAt).toLocaleDateString()}
                </div>
                {post.readTimeMinutes && (
                  <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                    <Clock size={15} /> {post.readTimeMinutes} min read
                  </div>
                )}
              </div>
            </div>
          </header>

          {/* Hero Image */}
          {post.coverImage && (
            <div className="post-hero-image-wrapper">
              <img
                src={post.coverImage}
                alt="Article hero image"
                className="post-hero-image"
              />
            </div>
          )}

          {/* Article Body HTML */}
          <div
            className="article-prose"
            dangerouslySetInnerHTML={{ __html: post.content }}
          />

          {/* Tags Footer */}
          <div
            style={{
              marginTop: '40px',
              paddingTop: '24px',
              borderTop: '1px solid var(--outline-variant)',
              display: 'flex',
              flexWrap: 'wrap',
              gap: '8px',
            }}
          >
            {post.tags.map((t) => (
              <Link
                key={t}
                to={`/search?q=${t}`}
                className="btn btn-outline btn-sm"
              >
                #{t}
              </Link>
            ))}
          </div>

          {/* Discussion Section */}
          <section id="discussion" className="discussion-section">
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                marginBottom: '24px',
              }}
            >
              <h2 style={{ fontSize: '26px', fontWeight: 600 }}>
                Discussion{' '}
                <span style={{ fontSize: '18px', color: 'var(--secondary)', fontWeight: 400 }}>
                  ({comments.length})
                </span>
              </h2>
            </div>

            {/* Comment Editor */}
            <form onSubmit={handlePostComment} className="comment-editor-box">
              <div className="comment-editor-toolbar">
                <button type="button" className="editor-tool-btn" title="Bold">
                  <Bold size={15} />
                </button>
                <button type="button" className="editor-tool-btn" title="Italic">
                  <Italic size={15} />
                </button>
                <button type="button" className="editor-tool-btn" title="Code snippet">
                  <Code size={15} />
                </button>
                <button type="button" className="editor-tool-btn" title="Insert Link">
                  <Link2 size={15} />
                </button>
                <button type="button" className="editor-tool-btn" title="Blockquote">
                  <Quote size={15} />
                </button>
              </div>

              <textarea
                className="comment-editor-textarea"
                placeholder={
                  isAuthenticated
                    ? 'Share your perspective or ask a question...'
                    : 'Sign in to join the conversation...'
                }
                value={newCommentText}
                onChange={(e) => setNewCommentText(e.target.value)}
                onClick={() => {
                  if (!isAuthenticated) openAuthModal('login');
                }}
              />

              <div className="comment-editor-footer">
                <span style={{ fontSize: '12px', color: 'var(--secondary)' }}>
                  Markdown syntax supported
                </span>
                <button
                  type="submit"
                  className="btn btn-amber"
                  disabled={!newCommentText.trim()}
                >
                  Post Comment
                </button>
              </div>
            </form>

            {/* Comments Thread List */}
            <div style={{ display: 'flex', flexDirection: 'column' }}>
              {comments.length === 0 ? (
                <p style={{ padding: '24px 0', color: 'var(--secondary)', textAlign: 'center' }}>
                  No comments yet. Be the first to start the discussion!
                </p>
              ) : (
                comments.map((comment) => (
                  <CommentItem
                    key={comment.id}
                    comment={comment}
                    onReply={(parentId, content) => {
                      addComment(`@${parentId} ${content}`);
                    }}
                  />
                ))
              )}
            </div>
          </section>
        </article>
      </main>
    </>
  );
};

