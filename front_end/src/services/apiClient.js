import axios from "axios";

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true, // để cookie AdminCookie tự gửi khi gọi ApiAccountAdmin
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");

  // Nếu không phải API admin thì mới gắn Bearer
  if (token && !config.url?.includes("ApiAccountAdmin"))
    config.headers.Authorization = `Bearer ${token}`;

  return config;
});

export default apiClient;
