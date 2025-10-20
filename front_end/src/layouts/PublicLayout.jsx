import { Outlet } from "react-router-dom";
import Header from "@components/layout/Header";
import Footer from "@components/layout/Footer";

export default function PublicLayout() {
  return (
    <>
      <Header />
      <main className="container py-4"><Outlet /></main>
      <Footer />
    </>
  );
}
