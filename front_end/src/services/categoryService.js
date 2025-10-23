import api from "./apiClient";

export const getCategories = async (params) => {
  const { data } = await api.get("/Api/ApiCategory", { params });
  return data;
};

export const getCategoryById = async (id) => {
  const { data } = await api.get(`/Api/ApiCategory/${id}`);
  return data;
};

export const createCategory = async (payload) => {
  // JSON body
  const { data } = await api.post("/Api/ApiCategory", payload);
  return data;
};

export const updateCategory = async (id, payload) => {
  await api.put(`/Api/ApiCategory/${id}`, payload);
};

export const deleteCategory = async (id) => {
  await api.delete(`/Api/ApiCategory/${id}`);
};
