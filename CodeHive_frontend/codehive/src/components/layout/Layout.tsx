import React from 'react';
import { Navbar } from './Navbar';
import { Sidebar } from './Sidebar';

interface LayoutProps {
  children: React.ReactNode;
  showLeftSidebar?: boolean;
  showRightSidebar?: boolean;
  layoutMode?: 'three-column' | 'single-column' | 'wide';
}

export const Layout: React.FC<LayoutProps> = ({
  children,
  showLeftSidebar = true,
  showRightSidebar = true,
  layoutMode = 'three-column',
}) => {
  if (layoutMode === 'single-column') {
    return (
      <>
        <Navbar />
        <main className="layout-single-column">{children}</main>
      </>
    );
  }

  if (layoutMode === 'wide') {
    return (
      <>
        <Navbar />
        <main className="layout-wide-center">{children}</main>
      </>
    );
  }

  return (
    <>
      <Navbar />
      <div className="layout-container">
        {showLeftSidebar ? <Sidebar variant="left" /> : <div />}
        <main className="center-column">{children}</main>
        {showRightSidebar ? <Sidebar variant="right" /> : <div />}
      </div>
    </>
  );
};
