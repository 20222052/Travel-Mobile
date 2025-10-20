import api from "./apiClient";
export const getProducts = (params) => api.get("/products", { params });
