import { Routes, Route, Navigate } from "react-router-dom";
import PublicLayout from "@layouts/PublicLayout";
import AdminLayout from "@layouts/AdminLayout";
import ProtectedRoute from "./ProtectedRoute";
import Home from "@pages/Home/Home";
import Login from "@pages/Auth/Login";
import Dashboard from "@pages/Admin/Dashboard/Dashboard";
import NotFound from "@pages/NotFound/NotFound";

export default function AppRouter() {
  const isAuthed = !!localStorage.getItem("accessToken");

  return (
    <Routes>
      {/* Public Routes */}
      <Route element={<PublicLayout />}>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
      </Route>

      {/* Admin Routes */}
      <Route path="/admin" element={<AdminLayout />}>
        <Route index element={<Navigate to="/admin/dashboard" replace />} />
        <Route path="dashboard" element={<Dashboard />} />
        {/* Add more admin routes here */}
      </Route>

      {/* 404 */}
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}
