import React, { useState, useRef, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Navbar } from '../components/layout/Navbar';
import { Sidebar } from '../components/layout/Sidebar';
import { Avatar } from '../components/user/Avatar';
import { Spinner } from '../components/ui/Spinner';
import { useAuth } from '../contexts/AuthContext';
import { Conversation, Message } from '../types';
import { initChatHubConnection, chatApi } from '../api/chat';
import {
  Search,
  Send,
  PlusCircle,
  Paperclip,
  Smile,
  Info,
} from 'lucide-react';
import toast from 'react-hot-toast';

export const MessagesPage: React.FC = () => {
  const { user, isAuthenticated, openAuthModal } = useAuth();
  const [searchParams] = useSearchParams();
  const requestedConversationId = searchParams.get('conversation');
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activeConvId, setActiveConvId] = useState<string | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [loadingConversations, setLoadingConversations] = useState(true);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [inputText, setInputText] = useState('');
  const [searchFilter, setSearchFilter] = useState('');

  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  // 1. Load Conversations
  const loadConversations = useCallback(async () => {
    if (!isAuthenticated) {
      setLoadingConversations(false);
      return;
    }
    setLoadingConversations(true);
    try {
      const res = await chatApi.getConversations({ limit: 50 });
      setConversations(res.items);
      setActiveConvId((currentConversationId) => {
        if (requestedConversationId && res.items.some((conversation) => conversation.id === requestedConversationId)) {
          return requestedConversationId;
        }

        if (currentConversationId && res.items.some((conversation) => conversation.id === currentConversationId)) {
          return currentConversationId;
        }

        return res.items[0]?.id ?? null;
      });
    } catch (err) {
      console.error('Error fetching conversations:', err);
    } finally {
      setLoadingConversations(false);
    }
  }, [isAuthenticated, requestedConversationId]);

  useEffect(() => {
    loadConversations();
  }, [loadConversations]);

  // 2. Load Messages when Active Conversation changes
  const loadMessages = useCallback(async (convId: string) => {
    setLoadingMessages(true);
    try {
      const res = await chatApi.getMessages(convId, { limit: 50 });
      // API returns newest first or oldest first — reverse if needed
      setMessages(res.items.reverse());
      await chatApi.markConversationAsRead(convId);
      setConversations((prev) =>
        prev.map((c) => (c.id === convId ? { ...c, unreadCount: 0 } : c))
      );
    } catch (err) {
      console.error('Error fetching messages:', err);
    } finally {
      setLoadingMessages(false);
      setTimeout(scrollToBottom, 100);
    }
  }, []);

  useEffect(() => {
    if (activeConvId) {
      loadMessages(activeConvId);
    }
  }, [activeConvId, loadMessages]);

  // 3. Connect to SignalR Chat Hub
  useEffect(() => {
    if (!isAuthenticated) return;

    const hub = initChatHubConnection({
      onMessageReceived: (convId, msg) => {
        if (convId === activeConvId) {
          setMessages((prev) => [...prev, msg]);
          setTimeout(scrollToBottom, 100);
        }
        setConversations((prev) =>
          prev.map((c) => {
            if (c.id === convId) {
              return {
                ...c,
                lastMessage: msg.content,
                lastMessageAt: new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
                unreadCount: convId === activeConvId ? 0 : c.unreadCount + 1,
              };
            }
            return c;
          })
        );
      },
    });

    return () => {
      hub.stop();
    };
  }, [isAuthenticated, activeConvId]);

  const activeConversation = conversations.find((c) => c.id === activeConvId);

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isAuthenticated) {
      openAuthModal('login');
      return;
    }
    if (!inputText.trim() || !activeConvId) return;

    const content = inputText;
    setInputText('');

    try {
      const sentMsg = await chatApi.sendMessage(activeConvId, { content });
      setMessages((prev) => [...prev, sentMsg]);
      setConversations((prev) =>
        prev.map((c) =>
          c.id === activeConvId
            ? {
                ...c,
                lastMessage: content,
                lastMessageAt: 'Just now',
              }
            : c
        )
      );
      setTimeout(scrollToBottom, 100);
    } catch (err) {
      console.error('Error sending message:', err);
      toast.error('Failed to send message');
    }
  };

  const filteredConversations = conversations.filter((c) =>
    (c.participant.displayName || c.participant.username)
      .toLowerCase()
      .includes(searchFilter.toLowerCase())
  );

  return (
    <>
      <Navbar />

      <div
        className="layout-container"
        style={{
          gridTemplateColumns: '240px 1fr',
          paddingBottom: '24px',
        }}
      >
        <Sidebar variant="left" />

        {/* Two-Pane Messaging Container */}
        <section className="messages-container">
          {/* Left Pane: Conversation List */}
          <aside className="conversations-sidebar">
            <div className="conversations-header">
              <h2 style={{ fontSize: '20px', fontWeight: 600, marginBottom: '12px' }}>
                Messages
              </h2>
              <div style={{ position: 'relative' }}>
                <span
                  style={{
                    position: 'absolute',
                    left: '10px',
                    top: '50%',
                    transform: 'translateY(-50%)',
                    color: 'var(--secondary)',
                    display: 'flex',
                  }}
                >
                  <Search size={16} />
                </span>
                <input
                  type="text"
                  placeholder="Search conversations..."
                  className="input-field"
                  style={{ paddingLeft: '34px', paddingBottom: '7px', paddingTop: '7px', fontSize: '13.5px' }}
                  value={searchFilter}
                  onChange={(e) => setSearchFilter(e.target.value)}
                />
              </div>
            </div>

            <div className="conversations-list">
              {loadingConversations ? (
                <div style={{ display: 'flex', justifyContent: 'center', padding: '40px 0' }}>
                  <Spinner size={24} />
                </div>
              ) : filteredConversations.length === 0 ? (
                <div style={{ padding: '30px 16px', textAlign: 'center', color: 'var(--secondary)', fontSize: '13.5px' }}>
                  No conversations yet. Visit a developer's profile to send them a message!
                </div>
              ) : (
                filteredConversations.map((conv) => {
                  const isActive = conv.id === activeConvId;
                  return (
                    <div
                      key={conv.id}
                      className={`conversation-item ${isActive ? 'active' : ''}`}
                      onClick={() => setActiveConvId(conv.id)}
                    >
                      <div style={{ position: 'relative' }}>
                        <Avatar
                          src={conv.participant.avatarUrl}
                          name={conv.participant.displayName || conv.participant.username}
                          size="md"
                        />
                      </div>

                      <div style={{ flex: 1, minWidth: 0 }}>
                        <div className="conversation-meta-header">
                          <span className="conversation-participant-name">
                            {conv.participant.displayName || conv.participant.username}
                          </span>
                          <span className="conversation-time">
                            {conv.lastMessageAt ? new Date(conv.lastMessageAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : ''}
                          </span>
                        </div>
                        <p className="conversation-preview">{conv.lastMessage}</p>
                      </div>
                      {conv.unreadCount > 0 && (
                        <span className="unread-badge">{conv.unreadCount}</span>
                      )}
                    </div>
                  );
                })
              )}
            </div>
          </aside>

          {/* Right Pane: Active Thread */}
          <div className="chat-thread-container">
            {activeConversation ? (
              <>
                <div className="chat-header">
                  <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                    <Avatar
                      src={activeConversation.participant.avatarUrl}
                      name={activeConversation.participant.displayName || activeConversation.participant.username}
                      size="sm"
                    />
                    <div>
                      <h3 style={{ fontSize: '15px', fontWeight: 600 }}>
                        {activeConversation.participant.displayName || activeConversation.participant.username}
                      </h3>
                      <span style={{ fontSize: '12px', color: 'var(--secondary)' }}>
                        @{activeConversation.participant.username}
                      </span>
                    </div>
                  </div>

                  <button
                    className="nav-icon-btn"
                    onClick={() => toast(`Chatting with @${activeConversation.participant.username}`)}
                    title="Conversation info"
                  >
                    <Info size={18} />
                  </button>
                </div>

                {/* Messages stream */}
                <div className="chat-messages-area">
                  {loadingMessages ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '40px 0' }}>
                      <Spinner size={28} />
                    </div>
                  ) : messages.length === 0 ? (
                    <div style={{ padding: '40px 0', textAlign: 'center', color: 'var(--secondary)', fontSize: '14px' }}>
                      Say hello to start the conversation!
                    </div>
                  ) : (
                    messages.map((msg) => {
                      const isSent = msg.senderId === user?.id;
                      return (
                        <div
                          key={msg.id}
                          className={`message-bubble-wrapper ${isSent ? 'sent' : 'received'}`}
                        >
                          {!isSent && (
                            <Avatar
                              src={activeConversation.participant.avatarUrl}
                              name={activeConversation.participant.displayName || activeConversation.participant.username}
                              size="xs"
                            />
                          )}

                          <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                            <div
                              style={{
                                display: 'flex',
                                alignItems: 'baseline',
                                gap: '6px',
                                fontSize: '11.5px',
                                color: 'var(--secondary)',
                                justifyContent: isSent ? 'flex-end' : 'flex-start',
                              }}
                            >
                              <span>{isSent ? 'You' : activeConversation.participant.displayName || activeConversation.participant.username}</span>
                              <span>{new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                            </div>

                            <div
                              className={`message-bubble ${
                                isSent ? 'sent-bubble' : 'received-bubble'
                              }`}
                            >
                              {msg.content}
                            </div>
                          </div>
                        </div>
                      );
                    })
                  )}
                  <div ref={messagesEndRef} />
                </div>

                {/* Chat input box */}
                <form onSubmit={handleSendMessage} className="chat-input-area">
                  <div className="chat-input-box">
                    <button
                      type="button"
                      className="nav-icon-btn"
                      onClick={() => toast('Attachment feature ready')}
                      title="Add file"
                    >
                      <PlusCircle size={18} />
                    </button>
                    <button
                      type="button"
                      className="nav-icon-btn"
                      onClick={() => toast('Attach file')}
                      title="Attach file"
                    >
                      <Paperclip size={18} />
                    </button>

                    <textarea
                      className="chat-textarea"
                      placeholder="Type your message..."
                      rows={1}
                      value={inputText}
                      onChange={(e) => setInputText(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' && !e.shiftKey) {
                          e.preventDefault();
                          handleSendMessage(e);
                        }
                      }}
                    />

                    <button
                      type="button"
                      className="nav-icon-btn"
                      onClick={() => toast('Emoji picker')}
                      title="Emoji"
                    >
                      <Smile size={18} />
                    </button>
                    <button
                      type="submit"
                      className="btn btn-amber btn-sm"
                      style={{ padding: '7px 12px' }}
                      title="Send message"
                      disabled={!inputText.trim()}
                    >
                      <Send size={15} />
                    </button>
                  </div>
                </form>
              </>
            ) : (
              <div style={{ padding: '60px', textAlign: 'center', color: 'var(--secondary)' }}>
                {loadingConversations ? <Spinner size={28} /> : 'Select a conversation to start chatting.'}
              </div>
            )}
          </div>
        </section>
      </div>
    </>
  );
};
