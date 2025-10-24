import api from "./apiClient"; // baseURL: https://localhost:7023 (hoặc /api) + withCredentials

// List + filter + phân trang
export const getUsers = async ({ page = 1, pageSize = 10, keyword, role } = {}) => {
  const params = { page, pageSize };
  if (keyword) params.keyword = keyword;
  if (role) params.role = role;
  const { data } = await api.get("/api/ApiUser", { params });
  return data; // { items, totalPages, totalCount, page, pageSize }
};

// Detail
export const getUserById = async (id) => {
  const { data } = await api.get(`/api/ApiUser/${id}`);
  return data;
};

// Create (multipart/form-data)
export const createUser = async (payload) => {
  const form = new FormData();
  form.append("Username", payload.username);
  form.append("Password", payload.password);
  if (payload.name) form.append("Name", payload.name);
  if (payload.email) form.append("Email", payload.email);
  if (payload.phone) form.append("Phone", payload.phone);
  if (payload.role) form.append("Role", payload.role);
  if (payload.gender) form.append("Gender", payload.gender);
  if (payload.address) form.append("Address", payload.address);
  if (payload.dateOfBirth) form.append("DateOfBirth", payload.dateOfBirth); // YYYY-MM-DD
  if (payload.otpVerified != null) form.append("OtpVerified", String(payload.otpVerified));
  if (payload.imageFile) form.append("ImageFile", payload.imageFile);

  const { data } = await api.post("/api/ApiUser", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
};

// Update (multipart/form-data)
export const updateUser = async (id, payload) => {
  const form = new FormData();
  if (payload.name) form.append("Name", payload.name);
  if (payload.email) form.append("Email", payload.email);
  if (payload.phone) form.append("Phone", payload.phone);
  if (payload.role) form.append("Role", payload.role);
  if (payload.gender) form.append("Gender", payload.gender);
  if (payload.address) form.append("Address", payload.address);
  if (payload.dateOfBirth) form.append("DateOfBirth", payload.dateOfBirth);
  if (payload.otpVerified != null) form.append("OtpVerified", String(payload.otpVerified));
  if (payload.newPassword) form.append("NewPassword", payload.newPassword);
  if (payload.imageFile) form.append("ImageFile", payload.imageFile);

  await api.put(`/api/ApiUser/${id}`, form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
};

// Delete
export const deleteUser = async (id) => {
  await api.delete(`/api/ApiUser/${id}`);
};
