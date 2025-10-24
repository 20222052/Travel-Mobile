import api from "./apiClient";

export const getCategories = async (params) => {
  const { data } = await api.get("/api/ApiCategory", { params });
  return data;
};

export const getCategoryById = async (id) => {
  const { data } = await api.get(`/api/ApiCategory/${id}`);
  return data;
};

export const createCategory = async (payload) => {
  // JSON body
  const { data } = await api.post("/api/ApiCategory", payload);
  return data;
};

export const updateCategory = async (id, payload) => {
  await api.put(`/api/ApiCategory/${id}`, payload);
};

export const deleteCategory = async (id) => {
  await api.delete(`/api/ApiCategory/${id}`);
};

export const getCategoriesLite = async (q) => {
  const params = q ? { q } : undefined;
  const { data } = await api.get("/api/ApiCategory/lookup", { params });
  return Array.isArray(data) ? data : [];
};