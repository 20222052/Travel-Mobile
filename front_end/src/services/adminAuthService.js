import api from "./apiClient";
export const adminLogin   = (data) => api.post("/api/ApiAccountAdmin/login", data);
export const adminUserInfo= () => api.get("/api/ApiAccountAdmin/userinfo");
export const adminLogout  = () => api.post("/api/ApiAccountAdmin/logout");
