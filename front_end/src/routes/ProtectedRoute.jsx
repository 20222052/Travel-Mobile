import { Navigate, Outlet } from "react-router-dom";
import AdminLayout from "@layouts/AdminLayout";

export default function ProtectedRoute({ isAuthed }) {
  if (!isAuthed) return <Navigate to="/login" replace />;
  return <AdminLayout><Outlet /></AdminLayout>;
}
