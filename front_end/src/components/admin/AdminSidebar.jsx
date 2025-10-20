import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAdmin } from '@/contexts/AdminContext';

const AdminSidebar = () => {
  const location = useLocation();
  const { user } = useAdmin();
  const [openMenus, setOpenMenus] = useState({});

  const isActive = (path) => {
    return location.pathname.startsWith(path) ? 'active' : '';
  };

  const isExactActive = (path) => {
    return location.pathname === path ? 'active' : '';
  };

  const toggleMenu = (menuName) => {
    setOpenMenus(prev => ({
      ...prev,
      [menuName]: !prev[menuName]
    }));
  };

  const menuItems = [
    {
      name: 'Dashboard',
      icon: 'fa-dashboard',
      path: '/admin/dashboard',
      exact: true
    },
    {
      name: 'Tour Management',
      icon: 'fa-plane',
      path: '/admin/tour',
      submenu: [
        { name: 'All Tours', path: '/admin/tour' },
        { name: 'Add New Tour', path: '/admin/tour/create' }
      ]
    },
    {
      name: 'Categories',
      icon: 'fa-list',
      path: '/admin/category',
      submenu: [
        { name: 'All Categories', path: '/admin/category' },
        { name: 'Add Category', path: '/admin/category/create' }
      ]
    },
    {
      name: 'Orders',
      icon: 'fa-shopping-cart',
      path: '/admin/order',
      submenu: [
        { name: 'All Orders', path: '/admin/order' },
        { name: 'Pending Orders', path: '/admin/order?status=pending' },
        { name: 'Completed Orders', path: '/admin/order?status=completed' }
      ]
    },
    {
      name: 'Users',
      icon: 'fa-users',
      path: '/admin/user',
      submenu: [
        { name: 'All Users', path: '/admin/user' },
        { name: 'Add User', path: '/admin/user/create' }
      ]
    },
    {
      name: 'Blog',
      icon: 'fa-file-text',
      path: '/admin/blog',
      submenu: [
        { name: 'All Posts', path: '/admin/blog' },
        { name: 'Add Post', path: '/admin/blog/create' }
      ]
    },
    {
      name: 'Contact Messages',
      icon: 'fa-envelope',
      path: '/admin/contact'
    },
    {
      name: 'History',
      icon: 'fa-history',
      path: '/admin/history'
    },
    {
      name: 'Cart Management',
      icon: 'fa-shopping-basket',
      path: '/admin/cart'
    }
  ];

  return (
    <aside className="main-sidebar">
      <section className="sidebar">
        {/* User Panel */}
        <div className="user-panel">
          <div className="pull-left image">
            <img src={user.avatar} className="img-circle" alt="User" />
          </div>
          <div className="pull-left info">
            <p>{user.name}</p>
            <a href="#"><i className="fa fa-circle text-success"></i> Online</a>
          </div>
        </div>

        {/* Search Form */}
        <form action="#" method="get" className="sidebar-form">
          <div className="input-group">
            <input 
              type="text" 
              name="q" 
              className="form-control" 
              placeholder="Search..."
            />
            <span className="input-group-btn">
              <button type="submit" className="btn btn-flat">
                <i className="fa fa-search"></i>
              </button>
            </span>
          </div>
        </form>

        {/* Sidebar Menu */}
        <ul className="sidebar-menu" data-widget="tree">
          <li className="header">MAIN NAVIGATION</li>
          
          {menuItems.map((item, index) => {
            if (item.submenu) {
              return (
                <li 
                  key={index} 
                  className={`treeview ${isActive(item.path)} ${openMenus[item.name] ? 'active menu-open' : ''}`}
                >
                  <a 
                    href="#" 
                    onClick={(e) => {
                      e.preventDefault();
                      toggleMenu(item.name);
                    }}
                  >
                    <i className={`fa ${item.icon}`}></i>
                    <span>{item.name}</span>
                    <span className="pull-right-container">
                      <i className="fa fa-angle-left pull-right"></i>
                    </span>
                  </a>
                  <ul className="treeview-menu">
                    {item.submenu.map((subitem, subindex) => (
                      <li key={subindex} className={isExactActive(subitem.path)}>
                        <Link to={subitem.path}>
                          <i className="fa fa-circle-o"></i> {subitem.name}
                        </Link>
                      </li>
                    ))}
                  </ul>
                </li>
              );
            } else {
              return (
                <li 
                  key={index} 
                  className={item.exact ? isExactActive(item.path) : isActive(item.path)}
                >
                  <Link to={item.path}>
                    <i className={`fa ${item.icon}`}></i>
                    <span>{item.name}</span>
                  </Link>
                </li>
              );
            }
          })}

          <li className="header">SETTINGS</li>
          <li>
            <Link to="/admin/settings">
              <i className="fa fa-cog"></i> <span>Settings</span>
            </Link>
          </li>
        </ul>
      </section>
    </aside>
  );
};

export default AdminSidebar;
