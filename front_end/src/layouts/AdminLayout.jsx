// import React, { useEffect } from 'react';
// import { Outlet } from 'react-router-dom';
// import { AdminProvider, useAdmin } from '@/contexts/AdminContext';
// import AdminHeader from '@/components/admin/AdminHeader';
// import AdminSidebar from '@/components/admin/AdminSidebar';
// import AdminFooter from '@/components/admin/AdminFooter';

// // Inner component that uses the context
// const AdminLayoutContent = () => {
//   const { sidebarCollapsed } = useAdmin();

//   useEffect(() => {
//     // Load AdminLTE scripts if needed
//     const loadAdminLTE = () => {
//       // Add AdminLTE initialization here if needed
//       console.log('AdminLTE initialized');
//     };

//     loadAdminLTE();
//   }, []);

//   // Bỏ kiểm tra authentication - chỉ hiển thị giao diện
//   // const isAuthenticated = () => {
//   //   const token = localStorage.getItem('adminToken');
//   //   return !!token;
//   // };

//   // if (!isAuthenticated()) {
//   //   return <Navigate to="/admin/login" replace />;
//   // }

//   return (
//     <div className={`hold-transition skin-blue sidebar-mini ${sidebarCollapsed ? 'sidebar-collapse' : ''}`}>
//       <div className="wrapper">
//         <AdminHeader />
//         <AdminSidebar />
        
//         {/* Content Wrapper */}
//         <div className="content-wrapper">
//           <Outlet />
//         </div>

//         <AdminFooter />
//       </div>
//     </div>
//   );
// };

// // Main component with Provider
// const AdminLayout = () => {
//   return (
//     <AdminProvider>
//       <AdminLayoutContent />
//     </AdminProvider>
//   );
// };

// export default AdminLayout;



import React, { useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { AdminProvider, useAdmin } from '@/contexts/AdminContext';
import AdminHeader from '@/components/admin/AdminHeader';
import AdminSidebar from '@/components/admin/AdminSidebar';
import AdminFooter from '@/components/admin/AdminFooter';

const AdminLayoutContent = () => {
  const { sidebarCollapsed } = useAdmin();

  useEffect(() => {
    // AdminLTE init (giữ nguyên)
    const loadAdminLTE = () => {
      // có thể gắn các widget nếu cần
      // console.log('AdminLTE initialized');
    };
    loadAdminLTE();
  }, []);

  return (
    <div className={`hold-transition skin-blue sidebar-mini ${sidebarCollapsed ? 'sidebar-collapse' : ''}`}>
      <div className="wrapper">
        <AdminHeader />
        <AdminSidebar />
        {/* Content Wrapper */}
        <div className="content-wrapper">
          <Outlet />
        </div>
        <AdminFooter />
      </div>
    </div>
  );
};

const AdminLayout = () => (
  <AdminProvider>
    <AdminLayoutContent />
  </AdminProvider>
);

export default AdminLayout;
