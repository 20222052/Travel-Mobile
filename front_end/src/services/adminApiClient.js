import axios from "axios";

const adminApiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,                 
  headers: { "Content-Type": "application/json" },
});

export default adminApiClient;
