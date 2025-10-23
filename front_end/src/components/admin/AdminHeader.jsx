import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAdmin } from "@/contexts/AdminContext";
import { adminLogout } from "@/services/adminAuthService";
import "./AdminHeader.css";

export default function AdminHeader() {
  const { toggleSidebar, user, notifications, messages, setUser } = useAdmin();
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await adminLogout();      // POST /api/ApiAccountAdmin/logout (withCredentials)
      setUser(null);
      navigate("/login");
    } catch (e) {
      console.error(e);
      alert("Đăng xuất thất bại!");
    }
  };

  return (
    <header className="admin-header">
      <Link to="/admin/dashboard" className="admin-logo">
        <span className="logo-mini">TA</span>
        <span className="logo-lg">Travel Admin</span>
      </Link>

      <nav className="admin-navbar">
        <button className="toggle-btn" onClick={toggleSidebar} title="Toggle sidebar">
          <i className="fa fa-bars" />
        </button>

        {/* đẩy cụm nút sang phải */}
        <div className="flex-spacer" />

        {/* CỤM NÚT BÊN PHẢI: bell, mail, user, logout */}
        <div className="navbar-right">
          {/* Notifications */}
          <div className="nav-item dropdown">
            <button className="icon-btn" title="Thông báo">
              <i className="fa fa-bell-o" />
              {notifications.length > 0 && <span className="badge">{notifications.length}</span>}
            </button>
            <div className="dropdown-menu">
              <p className="dropdown-header">
                {notifications.length ? `Bạn có ${notifications.length} thông báo` : "Không có thông báo"}
              </p>
              <ul className="dropdown-list">
                {notifications.slice(0, 5).map((n, i) => (
                  <li key={i}><i className={`fa ${n.icon} text-${n.type}`} /> {n.message}</li>
                ))}
              </ul>
              <Link to="/admin/notifications" className="dropdown-footer">Xem tất cả</Link>
            </div>
          </div>

          {/* Messages */}
          <div className="nav-item dropdown">
            <button className="icon-btn" title="Tin nhắn">
              <i className="fa fa-envelope-o" />
              {messages.length > 0 && <span className="badge success">{messages.length}</span>}
            </button>
            <div className="dropdown-menu">
              <p className="dropdown-header">
                {messages.length ? `Bạn có ${messages.length} tin nhắn` : "Không có tin nhắn"}
              </p>
              <ul className="dropdown-list">
                {messages.slice(0, 5).map((m, i) => (
                  <li key={i}>
                    <img src={m.avatar || user.avatar} className="avatar-sm" alt="" />
                    <div><strong>{m.sender}</strong><p>{m.message}</p><small>{m.time}</small></div>
                  </li>
                ))}
              </ul>
              <Link to="/admin/messages" className="dropdown-footer">Xem tất cả</Link>
            </div>
          </div>

          {/* User */}
          <div className="nav-item dropdown">
            <button className="user-btn" title={user.name}>
              <img src={user.avatar} className="avatar" alt="" />
              <span className="username">{user.name}</span>
              <i className="fa fa-caret-down" />
            </button>
            <div className="dropdown-menu user-dropdown">
              <div className="user-info">
                <img src={user.avatar} className="avatar-lg" alt="" />
                <h4>{user.name}</h4>
                <small>{user.email}</small>
              </div>
              <Link to="/admin/profile" className="dropdown-link">
                <i className="fa fa-user" /> Hồ sơ
              </Link>
            </div>
          </div>

          {/* Logout (tách nhưng vẫn trong cùng cụm phải) */}
          <button className="logout-btn" onClick={handleLogout} title="Đăng xuất" aria-label="Đăng xuất">
            <i className="fa fa-sign-out" />
          </button>
        </div>
      </nav>
    </header>
  );
}
