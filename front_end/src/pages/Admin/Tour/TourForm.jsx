import React, { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import ContentHeader from "@/components/admin/ContentHeader";
import { Box } from "@/components/admin/AdminComponents";
import { createTour, getTourById, updateTour } from "@/services/tourService";
import { getCategoriesLite } from "@/services/categoryService";

const toLocalInput = (d) => {
  if (!d) return "";
  const date = new Date(d);
  const pad = (n) => String(n).padStart(2, "0");
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  return `${local.getFullYear()}-${pad(local.getMonth() + 1)}-${pad(local.getDate())}T${pad(local.getHours())}:${pad(local.getMinutes())}`;
};

export default function TourForm() {
  const { id } = useParams();
  const isEdit = !!id;
  const navigate = useNavigate();

  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({
    name: "",
    location: "",
    duration: "",
    price: "",
    people: "",
    view: 0,
    description: "",
    createdDate: "",
    categoryId: "",
    imageFile: null,
    imagePreview: null
  });

  // load categories
  useEffect(() => {
    (async () => {
      try {
        const cats = await getCategoriesLite();
        setCategories(Array.isArray(cats) ? cats : []);
      } catch (e) { console.error(e); setCategories([]); }
    })();
  }, []);

  // load detail (edit)
  useEffect(() => {
    if (isEdit) {
      (async () => {
        const t = await getTourById(id);
        setForm((f) => ({
          ...f,
          name: t?.name || "",
          location: t?.location || "",
          duration: t?.duration || "",
          price: t?.price ?? "",
          people: t?.people ?? "",
          view: t?.view ?? 0,
          description: t?.description || "",
          createdDate: t?.createdDate ? toLocalInput(t.createdDate) : "",
          categoryId: t?.categoryId ?? "",
          imagePreview: t?.image ? (import.meta.env.VITE_API_URL + t.image) : null
        }));
      })();
    }
  }, [id, isEdit]);

  const onChange = (e) => {
    const { name, value, files } = e.target;
    if (name === "imageFile") {
      const file = files?.[0];
      setForm((prev) => ({
        ...prev,
        imageFile: file || null,
        imagePreview: file ? URL.createObjectURL(file) : prev.imagePreview
      }));
      return;
    }
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    try {
      const payload = {
        name: form.name,
        location: form.location,
        duration: form.duration,
        price: form.price ? Number(form.price) : null,
        people: form.people ? Number(form.people) : null,
        view: form.view ? Number(form.view) : 0,
        description: form.description,
        categoryId: form.categoryId ? Number(form.categoryId) : null,
        createdDate: form.createdDate ? `${form.createdDate}:00` : null, // 'YYYY-MM-DDTHH:mm:00'
        imageFile: form.imageFile || null
      };

      if (isEdit) await updateTour(id, payload);
      else await createTour(payload);

      navigate("/admin/tours");
    } catch (err) {
      console.error(err);
      alert("Lưu thất bại!");
    }
  };

  const breadcrumbs = [
    { label: "Home", link: "/admin", icon: "fa-dashboard" },
    { label: "Tours", link: "/admin/tours" },
    { label: isEdit ? "Sửa" : "Tạo mới" }
  ];

  return (
    <>
      <ContentHeader title={isEdit ? "Sửa tour" : "Tạo tour"} breadcrumbs={breadcrumbs} />
      <section className="content">
        <Box title="Thông tin tour" type="primary">
          <form onSubmit={onSubmit}>
            <div className="row">
              <div className="col-md-8">
                <div className="form-group">
                  <label>Tên tour</label>
                  <input className="form-control" name="name" value={form.name} onChange={onChange} required />
                </div>

                <div className="form-group">
                  <label>Vị trí (Location)</label>
                  <input className="form-control" name="location" value={form.location} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>Thời lượng (Duration)</label>
                  <input className="form-control" name="duration" value={form.duration} onChange={onChange} placeholder="Ví dụ: 3 ngày 2 đêm" />
                </div>

                <div className="form-group">
                  <label>Giá (Price)</label>
                  <input className="form-control" type="number" name="price" value={form.price} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>Số người (People)</label>
                  <input className="form-control" type="number" name="people" value={form.people} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>Mô tả (Description)</label>
                  <textarea className="form-control" rows={6} name="description" value={form.description} onChange={onChange} />
                </div>
              </div>

              <div className="col-md-4">
                {/* Category dropdown */}
                <div className="form-group">
                  <label>Category</label>
                  <select
                    className="form-control"
                    name="categoryId"
                    value={form.categoryId}
                    onChange={onChange}
                  >
                    <option value="">-- Chọn category --</option>
                    {categories.map(c => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label>Lượt xem (View)</label>
                  <input className="form-control" type="number" name="view" value={form.view} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>Ngày tạo (CreatedDate)</label>
                  <input className="form-control" type="datetime-local" name="createdDate" value={form.createdDate} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>Ảnh (ImageFile)</label>
                  <input className="form-control" name="imageFile" type="file" accept="image/*" onChange={onChange} />
                  <div style={{ marginTop: 10 }}>
                    {form.imagePreview ? (
                      <div style={{ border:'1px dashed var(--border)', borderRadius:12, padding:8, display:'inline-block', background:'var(--surface-2)' }}>
                        <img src={form.imagePreview} alt="preview" style={{ width: 220, height: 140, borderRadius: 8, objectFit: "cover" }} />
                      </div>
                    ) : <span className="text-muted">Chưa có ảnh</span>}
                  </div>
                </div>
              </div>
            </div>

            <div className="text-right">
              <Link to="/admin/tours" className="btn btn-default" style={{ marginRight: 8 }}>
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
