import React from 'react';
import { Bookmark, FileText } from 'lucide-react';

interface EmptyStateProps {
  icon?: React.ReactNode;
  title: string;
  description: string;
  actionText?: string;
  onAction?: () => void;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  icon,
  title,
  description,
  actionText,
  onAction,
}) => {
  return (
    <div className="empty-state-box">
      <div className="empty-state-icon">
        {icon || <FileText size={28} />}
      </div>
      <h3 className="empty-state-title">{title}</h3>
      <p className="empty-state-desc">{description}</p>
      {actionText && onAction && (
        <button className="btn btn-ghost btn-sm" onClick={onAction}>
          {actionText}
        </button>
      )}
    </div>
  );
};
