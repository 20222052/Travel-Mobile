// import React, { useEffect, useState } from "react";
// import { Link, useNavigate, useParams } from "react-router-dom";
// import ContentHeader from "@/components/admin/ContentHeader";
// import { Box } from "@/components/admin/AdminComponents";
// import { createBlog, getBlogById, updateBlog } from "@/services/blogService";

// export default function BlogForm() {
//   const { id } = useParams(); // có id => edit, không id => create
//   const isEdit = !!id;
//   const navigate = useNavigate();

//   const [form, setForm] = useState({
//     title: "",
//     content: "",
//     categoryId: "",
//     view: 1,
//     postedDate: "", // ISO string yyyy-MM-ddTHH:mm:ss
//     imageFile: null,
//     imagePreview: null
//   });

//   useEffect(() => {
//     if (isEdit) {
//       (async () => {
//         const data = await getBlogById(id);
//         setForm(f => ({
//           ...f,
//           title: data?.title || "",
//           content: data?.content || "",
//           categoryId: data?.categoryId ?? "",
//           view: data?.view ?? 1,
//           postedDate: data?.postedDate ? new Date(data.postedDate).toISOString().slice(0,19) : "",
//           imagePreview: data?.image ? (import.meta.env.VITE_API_URL + data.image) : null
//         }));
//       })();
//     }
//   }, [id, isEdit]);

//   const onChange = (e) => {
//     const { name, value, files } = e.target;
//     if (name === "imageFile") {
//       const file = files?.[0];
//       setForm(prev => ({
//         ...prev,
//         imageFile: file || null,
//         imagePreview: file ? URL.createObjectURL(file) : prev.imagePreview
//       }));
//       return;
//     }
//     setForm(prev => ({ ...prev, [name]: value }));
//   };

//   const onSubmit = async (e) => {
//     e.preventDefault();
//     try {
//       const payload = {
//         title: form.title,
//         content: form.content,
//         categoryId: form.categoryId ? Number(form.categoryId) : null,
//         view: form.view ? Number(form.view) : null,
//         postedDate: form.postedDate || null,
//         imageFile: form.imageFile || null
//       };

//       if (isEdit) await updateBlog(id, payload);
//       else await createBlog(payload);

//       navigate("/admin/blog");
//     } catch (err) {
//       console.error(err);
//       alert("Lưu thất bại!");
//     }
//   };

//   const breadcrumbs = [
//     { label: "Home", link: "/admin", icon: "fa-dashboard" },
//     { label: "Blog", link: "/admin/blog" },
//     { label: isEdit ? "Sửa" : "Tạo mới" }
//   ];

//   return (
//     <>
//       <ContentHeader title={isEdit ? "Sửa bài viết" : "Tạo bài viết"} breadcrumbs={breadcrumbs} />
//       <section className="content">
//         <Box title="Thông tin bài viết" type="primary">
//           <form onSubmit={onSubmit}>
//             <div className="row">
//               <div className="col-md-8">
//                 <div className="form-group">
//                   <label>Tiêu đề</label>
//                   <input className="form-control" name="title" value={form.title} onChange={onChange} required />
//                 </div>

//                 <div className="form-group">
//                   <label>Nội dung</label>
//                   <textarea className="form-control" rows="8" name="content" value={form.content} onChange={onChange} />
//                 </div>
//               </div>

//               <div className="col-md-4">
//                 <div className="form-group">
//                   <label>CategoryId</label>
//                   <input className="form-control" name="categoryId" value={form.categoryId} onChange={onChange} placeholder="VD: 1" />
//                 </div>

//                 <div className="form-group">
//                   <label>View</label>
//                   <input className="form-control" name="view" type="number" value={form.view} onChange={onChange} />
//                 </div>

//                 <div className="form-group">
//                   <label>PostedDate (optional)</label>
//                   <input
//                     className="form-control"
//                     name="postedDate"
//                     type="datetime-local"
//                     value={form.postedDate}
//                     onChange={onChange}
//                   />
//                 </div>

//                 <div className="form-group">
//                   <label>Ảnh (ImageFile)</label>
//                   <input className="form-control" name="imageFile" type="file" accept="image/*" onChange={onChange} />
//                   <div style={{ marginTop: 8 }}>
//                     {form.imagePreview ? (
//                       <img src={form.imagePreview} alt="preview" style={{ maxWidth: "100%", height: 120, objectFit: "cover" }} />
//                     ) : <span className="text-muted">Chưa có ảnh</span>}
//                   </div>
//                 </div>
//               </div>
//             </div>

//             <div className="text-right">
//               <Link to="/admin/blog" className="btn btn-default" style={{ marginRight: 8 }}>
//                 Hủy
//               </Link>
//               <button className="btn btn-primary" type="submit">
//                 {isEdit ? "Cập nhật" : "Tạo mới"}
//               </button>
//             </div>
//           </form>
//         </Box>
//       </section>
//     </>
//   );
// }




import React, { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import ContentHeader from "@/components/admin/ContentHeader";
import { Box } from "@/components/admin/AdminComponents";
import { createBlog, getBlogById, updateBlog } from "@/services/blogService";

const toLocalInputValue = (d) => {
  if (!d) return "";
  const pad = (n) => String(n).padStart(2, "0");
  const local = new Date(d.getTime() - d.getTimezoneOffset() * 60000);
  return `${local.getFullYear()}-${pad(local.getMonth() + 1)}-${pad(local.getDate())}T${pad(local.getHours())}:${pad(local.getMinutes())}`;
};

export default function BlogForm() {
  const { id } = useParams();
  const isEdit = !!id;
  const navigate = useNavigate();

  const [form, setForm] = useState({
    title: "",
    content: "",
    categoryId: "",
    view: 1,
    postedDate: "",
    imageFile: null,
    imagePreview: null
  });

  useEffect(() => {
    if (isEdit) {
      (async () => {
        const data = await getBlogById(id);
        const dt = data?.postedDate ? new Date(data.postedDate) : null;
        setForm(f => ({
          ...f,
          title: data?.title || "",
          content: data?.content || "",
          categoryId: data?.categoryId ?? "",
          view: data?.view ?? 1,
          postedDate: dt ? toLocalInputValue(dt) : "",
          imagePreview: data?.image ? (import.meta.env.VITE_API_URL + data.image) : null
        }));
      })();
    }
  }, [id, isEdit]);

  const onChange = (e) => {
    const { name, value, files } = e.target;
    if (name === "imageFile") {
      const file = files?.[0];
      setForm(prev => ({
        ...prev,
        imageFile: file || null,
        imagePreview: file ? URL.createObjectURL(file) : prev.imagePreview
      }));
      return;
    }
    setForm(prev => ({ ...prev, [name]: value }));
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    try {
      const postedForServer = form.postedDate ? `${form.postedDate}:00` : null; // 'YYYY-MM-DDTHH:MM:00'
      const payload = {
        title: form.title,
        content: form.content,
        categoryId: form.categoryId ? Number(form.categoryId) : null,
        view: form.view ? Number(form.view) : null,
        postedDate: postedForServer,
        imageFile: form.imageFile || null
      };
      if (isEdit) await updateBlog(id, payload);
      else await createBlog(payload);
      navigate("/admin/blog");
    } catch (err) {
      console.error(err);
      alert("Lưu thất bại!");
    }
  };

  const breadcrumbs = [
    { label: "Home", link: "/admin", icon: "fa-dashboard" },
    { label: "Blog", link: "/admin/blog" },
    { label: isEdit ? "Sửa" : "Tạo mới" }
  ];

  return (
    <>
      <ContentHeader title={isEdit ? "Sửa bài viết" : "Tạo bài viết"} breadcrumbs={breadcrumbs} />
      <section className="content">
        <Box title="Thông tin bài viết" type="primary">
          <form onSubmit={onSubmit}>
            <div className="row">
              <div className="col-md-8">
                <div className="form-group">
                  <label>Tiêu đề</label>
                  <input className="form-control" name="title" value={form.title} onChange={onChange} required />
                </div>

                <div className="form-group">
                  <label>Nội dung</label>
                  <textarea className="form-control" rows="10" name="content" value={form.content} onChange={onChange} />
                </div>
              </div>

              <div className="col-md-4">
                <div className="form-group">
                  <label>CategoryId</label>
                  <input className="form-control" name="categoryId" value={form.categoryId} onChange={onChange} placeholder="VD: 1" />
                </div>

                <div className="form-group">
                  <label>View</label>
                  <input className="form-control" name="view" type="number" value={form.view} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>PostedDate (optional)</label>
                  <input
                    className="form-control"
                    name="postedDate"
                    type="datetime-local"
                    step="60"
                    value={form.postedDate}
                    onChange={onChange}
                  />
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
              <Link to="/admin/blog" className="btn btn-default" style={{ marginRight: 8 }}>
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

