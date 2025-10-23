// src/routes/AppRouter.jsx
import { Routes, Route, Navigate } from "react-router-dom";

import PublicLayout from "@layouts/PublicLayout";
import AdminLayout from "@layouts/AdminLayout";
// import ProtectedRoute from "./ProtectedRoute"; // Nếu muốn bảo vệ route admin bằng token thì bật lại

import Home from "@pages/Home/Home";
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

        {/* Blog */}
        <Route path="blog" element={<BlogList />} />
        <Route path="blog/create" element={<BlogForm />} />
        <Route path="blog/edit/:id" element={<BlogForm />} />

        {/* Category */}
        <Route path="category" element={<CategoryList />} />
        <Route path="category/create" element={<CategoryForm />} />
        <Route path="category/edit/:id" element={<CategoryForm />} />
      </Route>

      {/* 404 */}
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}
