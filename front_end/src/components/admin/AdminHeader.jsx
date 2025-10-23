// import React from 'react';
// import { Link } from 'react-router-dom';
// import { useAdmin } from '@/contexts/AdminContext';

// const AdminHeader = () => {
//   const { toggleSidebar, user, logout, notifications, messages } = useAdmin();

//   return (
//     <header className="main-header">
//       {/* Logo */}
//       <Link to="/admin/dashboard" className="logo">
//         <span className="logo-mini"><b>T</b>A</span>
//         <span className="logo-lg"><b>Travel</b> Admin</span>
//       </Link>

//       {/* Header Navbar */}
//       <nav className="navbar navbar-static-top">
//         {/* Sidebar toggle button */}
//         <a 
//           href="#" 
//           className="sidebar-toggle" 
//           onClick={(e) => {
//             e.preventDefault();
//             toggleSidebar();
//           }}
//           role="button"
//         >
//           <span className="sr-only">Toggle navigation</span>
//         </a>

//         <div className="navbar-custom-menu">
//           <ul className="nav navbar-nav">
//             {/* Messages Dropdown Menu */}
//             <li className="dropdown messages-menu">
//               <a href="#" className="dropdown-toggle" data-toggle="dropdown">
//                 <i className="fa fa-envelope-o"></i>
//                 {messages.length > 0 && (
//                   <span className="label label-success">{messages.length}</span>
//                 )}
//               </a>
//               <ul className="dropdown-menu">
//                 <li className="header">You have {messages.length} messages</li>
//                 <li>
//                   <ul className="menu">
//                     {messages.slice(0, 5).map((msg, index) => (
//                       <li key={index}>
//                         <a href="#">
//                           <div className="pull-left">
//                             <img src={msg.avatar || user.avatar} className="img-circle" alt="User" />
//                           </div>
//                           <h4>
//                             {msg.sender}
//                             <small><i className="fa fa-clock-o"></i> {msg.time}</small>
//                           </h4>
//                           <p>{msg.message}</p>
//                         </a>
//                       </li>
//                     ))}
//                   </ul>
//                 </li>
//                 <li className="footer"><Link to="/admin/messages">See All Messages</Link></li>
//               </ul>
//             </li>

//             {/* Notifications Dropdown Menu */}
//             <li className="dropdown notifications-menu">
//               <a href="#" className="dropdown-toggle" data-toggle="dropdown">
//                 <i className="fa fa-bell-o"></i>
//                 {notifications.length > 0 && (
//                   <span className="label label-warning">{notifications.length}</span>
//                 )}
//               </a>
//               <ul className="dropdown-menu">
//                 <li className="header">You have {notifications.length} notifications</li>
//                 <li>
//                   <ul className="menu">
//                     {notifications.slice(0, 5).map((notif, index) => (
//                       <li key={index}>
//                         <a href="#">
//                           <i className={`fa ${notif.icon} text-${notif.type}`}></i> {notif.message}
//                         </a>
//                       </li>
//                     ))}
//                   </ul>
//                 </li>
//                 <li className="footer"><Link to="/admin/notifications">View all</Link></li>
//               </ul>
//             </li>

//             {/* User Account Menu */}
//             <li className="dropdown user user-menu">
//               <a href="#" className="dropdown-toggle" data-toggle="dropdown">
//                 <img src={user.avatar} className="user-image" alt="User" />
//                 <span className="hidden-xs">{user.name}</span>
//               </a>
//               <ul className="dropdown-menu">
//                 <li className="user-header">
//                   <img src={user.avatar} className="img-circle" alt="User" />
//                   <p>
//                     {user.name}
//                     <small>{user.email}</small>
//                   </p>
//                 </li>
//                 <li className="user-footer">
//                   <div className="pull-left">
//                     <Link to="/admin/profile" className="btn btn-default btn-flat">
//                       Profile
//                     </Link>
//                   </div>
//                   <div className="pull-right">
//                     <button 
//                       onClick={logout} 
//                       className="btn btn-default btn-flat"
//                     >
//                       Sign out
//                     </button>
//                   </div>
//                 </li>
//               </ul>
//             </li>
//           </ul>
//         </div>
//       </nav>
//     </header>
//   );
// };

// export default AdminHeader;




import React from 'react';
import { Link } from 'react-router-dom';
import { useAdmin } from '@/contexts/AdminContext';

const AdminHeader = () => {
  const { toggleSidebar, user, logout, notifications, messages } = useAdmin();

  return (
    <header className="main-header">
      <Link to="/admin/dashboard" className="logo">
        <span className="logo-mini"><b>T</b>A</span>
        <span className="logo-lg"><b>Travel</b> Admin</span>
      </Link>

      <nav className="navbar navbar-static-top">
        <a
          href="#"
          className="sidebar-toggle"
          onClick={(e) => { e.preventDefault(); toggleSidebar(); }}
          role="button"
          title="Toggle sidebar"
        >
          <span className="sr-only">Toggle navigation</span>
        </a>

        <div className="navbar-custom-menu">
          <ul className="nav navbar-nav">
            {/* Messages */}
            <li className="dropdown messages-menu">
              <a href="#" className="dropdown-toggle" data-toggle="dropdown">
                <i className="fa fa-envelope-o"></i>
                {messages.length > 0 && <span className="label label-success">{messages.length}</span>}
              </a>
              <ul className="dropdown-menu">
                <li className="header">You have {messages.length} messages</li>
                <li>
                  <ul className="menu">
                    {messages.slice(0, 5).map((msg, i) => (
                      <li key={i}>
                        <a href="#">
                          <div className="pull-left">
                            <img src={msg.avatar || user.avatar} className="img-circle" alt="User" />
                          </div>
                          <h4>{msg.sender} <small><i className="fa fa-clock-o"></i> {msg.time}</small></h4>
                          <p>{msg.message}</p>
                        </a>
                      </li>
                    ))}
                  </ul>
                </li>
                <li className="footer"><Link to="/admin/messages">See All Messages</Link></li>
              </ul>
            </li>

            {/* Notifications */}
            <li className="dropdown notifications-menu">
              <a href="#" className="dropdown-toggle" data-toggle="dropdown">
                <i className="fa fa-bell-o"></i>
                {notifications.length > 0 && <span className="label label-warning">{notifications.length}</span>}
              </a>
              <ul className="dropdown-menu">
                <li className="header">You have {notifications.length} notifications</li>
                <li>
                  <ul className="menu">
                    {notifications.slice(0, 5).map((n, i) => (
                      <li key={i}><a href="#"><i className={`fa ${n.icon} text-${n.type}`}></i> {n.message}</a></li>
                    ))}
                  </ul>
                </li>
                <li className="footer"><Link to="/admin/notifications">View all</Link></li>
              </ul>
            </li>

            {/* User */}
            <li className="dropdown user user-menu">
              <a href="#" className="dropdown-toggle" data-toggle="dropdown">
                <img src={user.avatar} className="user-image" alt="User" />
                <span className="hidden-xs">{user.name}</span>
              </a>
              <ul className="dropdown-menu">
                <li className="user-header">
                  <img src={user.avatar} className="img-circle" alt="User" />
                  <p>{user.name}<small>{user.email}</small></p>
                </li>
                <li className="user-footer">
                  <div className="pull-left">
                    <Link to="/admin/profile" className="btn btn-default btn-flat">Profile</Link>
                  </div>
                  <div className="pull-right">
                    <button onClick={logout} className="btn btn-default btn-flat">Sign out</button>
                  </div>
                </li>
              </ul>
            </li>
          </ul>
        </div>
      </nav>
    </header>
  );
};

export default AdminHeader;
