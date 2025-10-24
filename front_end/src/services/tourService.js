// src/services/tourService.js
import api from "./apiClient";

/**
 * Lấy danh sách tour (có lọc & phân trang)
 * API: GET /api/ApiTourAdmin?page=&pageSize=&name=&categoryId=&sort=
 * Trả về: { items, totalPages, totalCount, page, pageSize }
 */
export const getTours = async ({ page = 1, pageSize = 6, categoryId, name, sort = "desc" } = {}) => {
  const params = { page, pageSize, sort };
  if (name) params.name = name.trim();
  if (categoryId != null && categoryId !== "") params.categoryId = Number(categoryId);

  const { data } = await api.get("/api/ApiTourAdmin", { params });
  return data;
};

/**
 * Lấy chi tiết 1 tour
 * API: GET /api/ApiTourAdmin/:id
 */
export const getTourById = async (id) => {
  const { data } = await api.get(`/api/ApiTourAdmin/${id}`);
  return data; // { id, name, image, location, duration, price, people, view, description, createdDate, categoryId, categoryName }
};

/**
 * Tạo tour (multipart/form-data để upload ảnh)
 * API: POST /api/ApiTourAdmin
 * payload: { name, location, duration, price, people, view, description, categoryId, createdDate('YYYY-MM-DDTHH:mm:ss'), imageFile }
 */
export const createTour = async (payload) => {
  const form = new FormData();
  if (payload.name)         form.append("Name", payload.name);
  if (payload.location)     form.append("Location", payload.location);
  if (payload.duration)     form.append("Duration", payload.duration);
  if (payload.price != null)   form.append("Price", String(payload.price));
  if (payload.people != null)  form.append("People", String(payload.people));
  if (payload.view != null)    form.append("View", String(payload.view));
  if (payload.description)  form.append("Description", payload.description);
  if (payload.categoryId != null) form.append("CategoryId", String(payload.categoryId));
  if (payload.createdDate)  form.append("CreatedDate", payload.createdDate); // ví dụ: '2025-10-25T09:30:00'
  if (payload.imageFile)    form.append("ImageFile", payload.imageFile);

  const { data } = await api.post("/api/ApiTourAdmin", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data; // { id } hoặc object tour tuỳ bạn trả về
};

/**
 * Cập nhật tour (multipart/form-data)
 * API: PUT /api/ApiTourAdmin/:id
 * payload: giống createTour, các field không gửi sẽ giữ nguyên
 */
export const updateTour = async (id, payload) => {
  const form = new FormData();
  if (payload.name)         form.append("Name", payload.name);
  if (payload.location)     form.append("Location", payload.location);
  if (payload.duration)     form.append("Duration", payload.duration);
  if (payload.price != null)   form.append("Price", String(payload.price));
  if (payload.people != null)  form.append("People", String(payload.people));
  if (payload.view != null)    form.append("View", String(payload.view));
  if (payload.description)  form.append("Description", payload.description);
  if (payload.categoryId != null) form.append("CategoryId", String(payload.categoryId));
  if (payload.createdDate)  form.append("CreatedDate", payload.createdDate);
  if (payload.imageFile)    form.append("ImageFile", payload.imageFile); // optional: thay ảnh

  await api.put(`/api/ApiTourAdmin/${id}`, form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
};

/**
 * Xoá tour
 * API: DELETE /api/ApiTourAdmin/:id
 * Lưu ý: nếu bị ràng buộc OrderDetail, API sẽ trả 409 Conflict với { message }
 */
export const deleteTour = async (id) => {
  await api.delete(`/api/ApiTourAdmin/${id}`);
};
