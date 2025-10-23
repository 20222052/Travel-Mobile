import api from "./apiClient";

// List + filter + phân trang
export const getBlogs = async ({ page = 1, pageSize = 10, categoryId, title, sort = "desc" } = {}) => {
  const params = { page, pageSize, sort };
  if (categoryId != null) params.categoryId = categoryId;
  if (title) params.title = title;
  const { data } = await api.get("/api/ApiBlog", { params });
  return data; // { items, totalPages, totalCount, page, pageSize }
};

// Lấy detail
export const getBlogById = async (id) => {
  const { data } = await api.get(`/api/ApiBlog/${id}`);
  return data;
};

// Tạo (multipart/form-data)
export const createBlog = async (payload) => {
  // payload: { title, content, categoryId, view?, postedDate?, imageFile? }
  const form = new FormData();
  if (payload.title) form.append("Title", payload.title);
  if (payload.content) form.append("Content", payload.content);
  if (payload.categoryId != null) form.append("CategoryId", String(payload.categoryId));
  if (payload.view != null) form.append("View", String(payload.view));
  if (payload.postedDate) form.append("PostedDate", payload.postedDate); // ISO yyyy-MM-ddTHH:mm:ss
  if (payload.imageFile) form.append("ImageFile", payload.imageFile);

  const { data } = await api.post("/api/ApiBlog", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
};

// Sửa (multipart/form-data)
export const updateBlog = async (id, payload) => {
  const form = new FormData();
  if (payload.title) form.append("Title", payload.title);
  if (payload.content) form.append("Content", payload.content);
  if (payload.categoryId != null) form.append("CategoryId", String(payload.categoryId));
  if (payload.view != null) form.append("View", String(payload.view));
  if (payload.postedDate) form.append("PostedDate", payload.postedDate);
  if (payload.imageFile) form.append("ImageFile", payload.imageFile); // optional replace

  await api.put(`/api/ApiBlog/${id}`, form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
};

// Xóa
export const deleteBlog = async (id) => {
  await api.delete(`/api/ApiBlog/${id}`);
};
