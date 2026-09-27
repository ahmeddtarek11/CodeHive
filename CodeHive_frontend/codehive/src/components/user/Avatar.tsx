import React, { useState } from 'react';

interface AvatarProps {
  src?: string | null;
  alt?: string;
  name?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  className?: string;
}

export const Avatar: React.FC<AvatarProps> = ({
  src,
  alt = 'Avatar',
  name = 'User',
  size = 'md',
  className = '',
}) => {
  const [imgError, setImgError] = useState(false);

  const getInitials = (text: string) => {
    const parts = text.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return text.slice(0, 2).toUpperCase() || 'U';
  };

  const sizeClass = `avatar-${size}`;

  if (src && !imgError) {
    return (
      <div className={`avatar-container ${sizeClass} ${className}`.trim()}>
        <img
          src={src}
          alt={alt}
          onError={() => setImgError(true)}
          loading="lazy"
        />
      </div>
    );
  }

  // Generate pleasant background color based on name
  const colors = [
    { bg: '#FEF3C7', color: '#855300' },
    { bg: '#D6E0F3', color: '#00658B' },
    { bg: '#FFDDB8', color: '#653E00' },
    { bg: '#E1E3E4', color: '#555F6F' },
  ];
  const charCode = name.charCodeAt(0) || 0;
  const chosen = colors[charCode % colors.length];

  return (
    <div
      className={`avatar-container ${sizeClass} ${className}`.trim()}
      style={{ backgroundColor: chosen.bg, color: chosen.color }}
    >
      <span>{getInitials(name)}</span>
    </div>
  );
};
