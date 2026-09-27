import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Navbar } from '../components/layout/Navbar';
import { useAuth } from '../contexts/AuthContext';
import { postsApi } from '../api/posts';
import { X, Eye, PenLine, BookOpen, Braces, MessageSquareText } from 'lucide-react';
import toast from 'react-hot-toast';

type PostType = 'Short' | 'Blog' | 'Snippet';

const postTypes: Array<{
  value: PostType;
  label: string;
  description: string;
  icon: typeof BookOpen;
}> = [
  {
    value: 'Blog',
    label: 'Article',
    description: 'A complete technical write-up',
    icon: BookOpen,
  },
  {
    value: 'Short',
    label: 'Short post',
    description: 'A quick idea, update, or question',
    icon: MessageSquareText,
  },
  {
    value: 'Snippet',
    label: 'Code snippet',
    description: 'Share a focused solution in code',
    icon: Braces,
  },
];

export const CreatePostPage: React.FC = () => {
  const { isAuthenticated, openAuthModal } = useAuth();
  const navigate = useNavigate();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [postType, setPostType] = useState<PostType>('Blog');
  const [language, setLanguage] = useState('');
  const [tagInput, setTagInput] = useState('');
  const [tags, setTags] = useState<string[]>(['react', 'architecture']);
  const [isPreview, setIsPreview] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleAddTag = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' || e.key === ',') {
      e.preventDefault();
      const clean = tagInput.trim().toLowerCase().replace(/^[#,\s]+|[,\s]+$/g, '');
      if (clean && !tags.includes(clean)) {
        setTags([...tags, clean]);
        setTagInput('');
      }
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    setTags(tags.filter((t) => t !== tagToRemove));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isAuthenticated) {
      openAuthModal('login');
      return;
    }
    if (!content.trim()) {
      toast.error('Please enter your post content');
      return;
    }
    if (postType === 'Blog' && !title.trim()) {
      toast.error('Please enter a title for your article');
      return;
    }
    if (postType === 'Snippet' && !language.trim()) {
      toast.error('Please choose the snippet language');
      return;
    }

    setIsSubmitting(true);
    try {
      const newPost = await postsApi.createPost({
        type: postType,
        title: title.trim() || undefined,
        content: content.trim(),
        language: language.trim() || undefined,
        tags,
      });
      toast.success('Post published to CodeHive!');
      navigate(`/posts/${newPost.postId}`);
    } catch {
      toast.error('Failed to publish post');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <>
      <Navbar />

      <main style={{ maxWidth: '820px', margin: '0 auto', padding: '96px 24px 80px' }}>
        <div
          style={{
            backgroundColor: 'var(--surface-container-lowest)',
            border: '1px solid var(--outline-variant)',
            borderRadius: 'var(--radius)',
            padding: '40px',
          }}
        >
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              marginBottom: '24px',
              borderBottom: '1px solid var(--surface-container-high)',
              paddingBottom: '16px',
            }}
          >
            <h1 style={{ fontSize: '24px', fontWeight: 600 }}>Create New Post</h1>

            <div style={{ display: 'flex', gap: '10px' }}>
              <button
                type="button"
                className={`btn btn-sm ${!isPreview ? 'btn-amber' : 'btn-outline'}`}
                onClick={() => setIsPreview(false)}
              >
                <PenLine size={15} /> Edit
              </button>
              <button
                type="button"
                className={`btn btn-sm ${isPreview ? 'btn-amber' : 'btn-outline'}`}
                onClick={() => setIsPreview(true)}
              >
                <Eye size={15} /> Preview
              </button>
            </div>
          </div>

          <form onSubmit={handleSubmit}>
            {/* Post type */}
            <div style={{ marginBottom: '24px' }}>
              <label className="form-label" style={{ marginBottom: '8px', display: 'block' }}>
                What are you publishing?
              </label>
              <div className="post-type-grid" role="radiogroup" aria-label="Post type">
                {postTypes.map((option) => {
                  const Icon = option.icon;
                  const isSelected = postType === option.value;

                  return (
                    <button
                      key={option.value}
                      type="button"
                      className={`post-type-option ${isSelected ? 'selected' : ''}`}
                      role="radio"
                      aria-checked={isSelected}
                      onClick={() => setPostType(option.value)}
                    >
                      <span className="post-type-option-icon"><Icon size={18} strokeWidth={1.8} /></span>
                      <span>
                        <strong>{option.label}</strong>
                        <small>{option.description}</small>
                      </span>
                    </button>
                  );
                })}
              </div>
            </div>

            {/* Title Input */}
            <div style={{ marginBottom: '24px' }}>
              <label className="form-label" style={{ marginBottom: '4px', display: 'block' }}>
                Title {postType === 'Blog' ? '(required)' : '(optional)'}
              </label>
              <input
                type="text"
                className="input-field-large"
                placeholder={postType === 'Blog' ? 'Article title...' : 'Add a concise title...'}
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                required={postType === 'Blog'}
                autoFocus
              />
            </div>

            {postType === 'Snippet' && (
              <div style={{ marginBottom: '24px' }}>
                <label className="form-label" htmlFor="snippet-language" style={{ marginBottom: '8px', display: 'block' }}>
                  Language (required)
                </label>
                <input
                  id="snippet-language"
                  type="text"
                  className="input-field"
                  placeholder="e.g. csharp, typescript, python"
                  value={language}
                  onChange={(e) => setLanguage(e.target.value)}
                  required
                />
              </div>
            )}

            {/* Tags Input */}
            <div style={{ marginBottom: '24px' }}>
              <label className="form-label" style={{ marginBottom: '8px', display: 'block' }}>
                Tags (press Enter or comma to add):
              </label>
              <div
                style={{
                  display: 'flex',
                  flexWrap: 'wrap',
                  alignItems: 'center',
                  gap: '8px',
                  padding: '8px 12px',
                  backgroundColor: 'var(--surface-container-low)',
                  border: '1px solid var(--outline-variant)',
                  borderRadius: 'var(--radius)',
                }}
              >
                {tags.map((tag) => (
                  <span
                    key={tag}
                    className="tag-chip"
                    style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}
                  >
                    #{tag}
                    <button
                      type="button"
                      onClick={() => handleRemoveTag(tag)}
                      style={{
                        background: 'none',
                        border: 'none',
                        color: 'inherit',
                        cursor: 'pointer',
                        padding: 0,
                        display: 'flex',
                      }}
                    >
                      <X size={12} />
                    </button>
                  </span>
                ))}
                <input
                  type="text"
                  placeholder={tags.length === 0 ? 'e.g. react, nextjs, devops' : 'Add tag...'}
                  value={tagInput}
                  onChange={(e) => setTagInput(e.target.value)}
                  onKeyDown={handleAddTag}
                  style={{
                    border: 'none',
                    background: 'transparent',
                    outline: 'none',
                    fontSize: '14px',
                    flex: 1,
                    minWidth: '120px',
                  }}
                />
              </div>
            </div>

            {/* Editor vs Preview */}
            {!isPreview ? (
              <div style={{ marginBottom: '28px' }}>
                <label className="form-label" style={{ marginBottom: '8px', display: 'block' }}>
                  {postType === 'Snippet' ? 'Code or snippet content:' : 'Body content (Markdown / Rich Text placeholder):'}
                </label>
                {/* Note: In production replace with a full rich editor (TipTap, Slate, Lexical) */}
                <textarea
                  className="textarea-field"
                  style={{ minHeight: '340px', fontFamily: 'var(--font-mono)', fontSize: '15px' }}
                  placeholder={postType === 'Snippet'
                    ? 'Paste or write your code snippet here...'
                    : postType === 'Short'
                      ? 'Share a quick thought, update, or question...'
                      : 'Write your technical article here in Markdown or plain text...'}
                  value={content}
                  onChange={(e) => setContent(e.target.value)}
                  required
                />
              </div>
            ) : (
              <div
                style={{
                  minHeight: '340px',
                  padding: '24px',
                  backgroundColor: 'var(--surface-container-low)',
                  border: '1px solid var(--outline-variant)',
                  borderRadius: 'var(--radius)',
                  marginBottom: '28px',
                }}
              >
                <h1 style={{ fontFamily: 'var(--font-serif)', fontSize: '32px', marginBottom: '16px' }}>
                  {title || 'Untitled Post'}
                </h1>
                <div style={{ whiteSpace: 'pre-wrap', lineHeight: 1.7, fontFamily: 'var(--font-serif)' }}>
                  {content || 'No content yet. Switch back to Edit mode to write.'}
                </div>
              </div>
            )}

            {/* Action Bar */}
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '14px' }}>
              <button
                type="button"
                className="btn btn-outline"
                onClick={() => navigate(-1)}
              >
                Discard
              </button>
              <button
                type="submit"
                className="btn btn-amber btn-lg"
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Publishing...' : `Publish ${postTypes.find((option) => option.value === postType)?.label}`}
              </button>
            </div>
          </form>
        </div>
      </main>
    </>
  );
};
