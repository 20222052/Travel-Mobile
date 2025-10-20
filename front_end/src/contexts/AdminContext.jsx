import React, { createContext, useContext, useState, useEffect } from 'react';

const AdminContext = createContext();

export const useAdmin = () => {
  const context = useContext(AdminContext);
  if (!context) {
    throw new Error('useAdmin must be used within AdminProvider');
  }
  return context;
};

export const AdminProvider = ({ children }) => {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(() => {
    const saved = localStorage.getItem('adminSidebarCollapsed');
    return saved ? JSON.parse(saved) : false;
  });

  const [user, setUser] = useState(() => {
    const savedUser = localStorage.getItem('adminUser');
    return savedUser ? JSON.parse(savedUser) : {
      name: 'Admin User',
      email: 'admin@travel.com',
      avatar: '/img/user-avatar.png',
      role: 'ADMIN'
    };
  });

  const [notifications, setNotifications] = useState([]);
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    localStorage.setItem('adminSidebarCollapsed', JSON.stringify(sidebarCollapsed));
  }, [sidebarCollapsed]);

  const toggleSidebar = () => {
    setSidebarCollapsed(prev => !prev);
  };

  const logout = () => {
    localStorage.removeItem('adminToken');
    localStorage.removeItem('adminUser');
    window.location.href = '/admin/login';
  };

  const value = {
    sidebarCollapsed,
    setSidebarCollapsed,
    toggleSidebar,
    user,
    setUser,
    notifications,
    setNotifications,
    messages,
    setMessages,
    logout
  };

  return (
    <AdminContext.Provider value={value}>
      {children}
    </AdminContext.Provider>
  );
};
