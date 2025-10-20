import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App.jsx";
import "@/index.scss";
import "@/assets/css/admin-simple.css";
import "bootstrap/dist/js/bootstrap.bundle.min.js"; // bật JS của Bootstrap

ReactDOM.createRoot(document.getElementById("root")).render(
  <BrowserRouter>
    <App />
  </BrowserRouter>
);
