import { useEffect, useState } from "react";
import { Navigate, Outlet } from "react-router-dom";
import AdminLayout from "@layouts/AdminLayout";
import { adminUserInfo } from "@/services/adminAuthService";

export default function ProtectedRoute() {
  const [loading, setLoading] = useState(true);
  const [isAuthed, setIsAuthed] = useState(false);

  useEffect(() => {
    let mounted = true;
    // Gọi API để kiểm tra cookie đăng nhập
    adminUserInfo()
      .then(() => mounted && setIsAuthed(true))
      .catch(() => mounted && setIsAuthed(false))
      .finally(() => mounted && setLoading(false));
    return () => { mounted = false; };
  }, []);

  if (loading) return <div style={{ padding: 24 }}>Đang kiểm tra đăng nhập...</div>;

  if (!isAuthed) return <Navigate to="/login" replace />;

  return (
    <AdminLayout>
      <Outlet />
    </AdminLayout>
  );
}
