import { Routes, Route, Navigate } from "react-router-dom";

import PublicLayout from "@layouts/PublicLayout";
import ProtectedRoute from "./ProtectedRoute";

import Home from "@pages/Home/Home"; // nếu không dùng có thể bỏ import
import Login from "@pages/Auth/Login";
import Dashboard from "@pages/Admin/Dashboard/Dashboard";
import NotFound from "@pages/NotFound/NotFound";

// Blog
import BlogList from "@pages/Admin/Blog/BlogList";
import BlogForm from "@pages/Admin/Blog/BlogForm";

// Category
import CategoryList from "@pages/Admin/Category/CategoryList";
import CategoryForm from "@pages/Admin/Category/CategoryForm";

export default function AppRouter() {
  return (
    <Routes>
      <Route element={<PublicLayout />}>
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<Login />} />
      </Route>

      {/* Admin Routes (Protected) */}
      <Route path="/admin" element={<ProtectedRoute />}>
        <Route path="login" element={<Login />} />
        <Route index element={<Navigate to="/admin/dashboard" replace />} />
        <Route path="dashboard" element={<Dashboard />} />
        <Route path="blog" element={<BlogList />} />
        <Route path="blog/create" element={<BlogForm />} />
        <Route path="blog/edit/:id" element={<BlogForm />} />
        <Route path="category" element={<CategoryList />} />
        <Route path="category/create" element={<CategoryForm />} />
        <Route path="category/edit/:id" element={<CategoryForm />} />
      </Route>

      {/* 404 */}
      <Route path="*" element={<NotFound />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}
