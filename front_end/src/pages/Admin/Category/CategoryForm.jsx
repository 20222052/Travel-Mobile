import React, { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import ContentHeader from "@/components/admin/ContentHeader";
import { Box } from "@/components/admin/AdminComponents";
import { createCategory, getCategoryById, updateCategory } from "@/services/categoryService";

export default function CategoryForm() {
  const { id } = useParams();
  const isEdit = !!id;
  const navigate = useNavigate();

  const [type, setType] = useState("");

  useEffect(() => {
    if (!isEdit) return;
    (async () => {
      const data = await getCategoryById(id);
      setType(data?.type || "");
    })();
  }, [id, isEdit]);

  const onSubmit = async (e) => {
    e.preventDefault();
    try {
      const payload = { type: type?.trim() || null };
      if (!payload.type) {
        alert("Vui lòng nhập Type");
        return;
      }
      if (isEdit) await updateCategory(id, payload);
      else await createCategory(payload);
      navigate("/admin/category");
    } catch (err) {
      console.error(err);
      alert("Lưu thất bại!");
    }
  };

  const breadcrumbs = [
    { label: "Home", link: "/admin", icon: "fa-dashboard" },
    { label: "Categories", link: "/admin/category" },
    { label: isEdit ? "Sửa" : "Tạo mới" }
  ];

  return (
    <>
      <ContentHeader title={isEdit ? "Sửa Category" : "Tạo Category"} breadcrumbs={breadcrumbs} />
      <section className="content">
        <Box title="Thông tin Category" type="primary">
          <form onSubmit={onSubmit}>
            <div className="row">
              <div className="col-md-6">
                <div className="form-group">
                  <label>Tên (Type)</label>
                  <input
                    className="form-control"
                    value={type}
                    onChange={(e) => setType(e.target.value)}
                    placeholder="VD: Tin tức, Kinh nghiệm, ..."
                    required
                  />
                </div>
              </div>
            </div>

            <div className="text-right">
              <Link to="/admin/category" className="btn btn-default" style={{ marginRight: 8 }}>
                Hủy
              </Link>
              <button className="btn btn-primary" type="submit">
                {isEdit ? "Cập nhật" : "Tạo mới"}
              </button>
            </div>
          </form>
        </Box>
      </section>
    </>
  );
}
