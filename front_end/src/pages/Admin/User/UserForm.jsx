import React, { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import ContentHeader from "@/components/admin/ContentHeader";
import { Box } from "@/components/admin/AdminComponents";
import { createUser, getUserById, updateUser } from "@/services/userService";

export default function UserForm() {
  const { id } = useParams();
  const isEdit = !!id;
  const navigate = useNavigate();

  const [form, setForm] = useState({
    username: "",
    password: "",       
    newPassword: "",    
    name: "",
    email: "",
    phone: "",
    role: "USER",
    gender: "",
    address: "",
    dateOfBirth: "",
    otpVerified: false,
    imageFile: null,
    imagePreview: null
  });

  useEffect(() => {
    if (isEdit) {
      (async () => {
        const u = await getUserById(id);
        setForm(f => ({
          ...f,
          username: u.username || "",
          name: u.name || "",
          email: u.email || "",
          phone: u.phone || "",
          role: u.role || "USER",
          gender: u.gender || "",
          address: u.address || "",
          dateOfBirth: u.dateOfBirth ? u.dateOfBirth.substring(0, 10) : "",
          otpVerified: !!u.otpVerified,
          imagePreview: u.image ? (import.meta.env.VITE_API_URL + u.image) : null
        }));
      })();
    }
  }, [id, isEdit]);

  const onChange = (e) => {
    const { name, value, files, type, checked } = e.target;
    if (name === "imageFile") {
      const file = files?.[0];
      setForm(prev => ({
        ...prev,
        imageFile: file || null,
        imagePreview: file ? URL.createObjectURL(file) : prev.imagePreview
      }));
      return;
    }
    setForm(prev => ({ ...prev, [name]: type === "checkbox" ? checked : value }));
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    try {
      const payload = {
        username: form.username,
        password: form.password,
        newPassword: form.newPassword,
        name: form.name,
        email: form.email,
        phone: form.phone,
        role: form.role,
        gender: form.gender,
        address: form.address,
        dateOfBirth: form.dateOfBirth || null,
        otpVerified: form.otpVerified,
        imageFile: form.imageFile || null
      };
      if (isEdit) {
        await updateUser(id, payload);
      } else {
        if (!payload.password) {
          alert("Vui lòng nhập mật khẩu cho người dùng mới.");
          return;
        }
        await createUser(payload);
      }
      navigate("/admin/users");
    } catch (err) {
      console.error(err);
      alert("Lưu thất bại!");
    }
  };

  const breadcrumbs = [
    { label: "Home", link: "/admin", icon: "fa-dashboard" },
    { label: "Users", link: "/admin/users" },
    { label: isEdit ? "Sửa" : "Tạo mới" }
  ];

  return (
    <>
      <ContentHeader title={isEdit ? "Sửa người dùng" : "Tạo người dùng"} breadcrumbs={breadcrumbs} />
      <section className="content">
        <Box title="Thông tin người dùng" type="primary">
          <form onSubmit={onSubmit}>
            <div className="row">
              <div className="col-md-8">
                <div className="form-group">
                  <label>Username</label>
                  <input className="form-control" name="username" value={form.username} onChange={onChange} required disabled={isEdit} />
                </div>

                {!isEdit && (
                  <div className="form-group">
                    <label>Password</label>
                    <input className="form-control" type="password" name="password" value={form.password} onChange={onChange} required />
                  </div>
                )}

                {isEdit && (
                  <div className="form-group">
                    <label>Đổi mật khẩu (tuỳ chọn)</label>
                    <input className="form-control" type="password" name="newPassword" value={form.newPassword} onChange={onChange} placeholder="Để trống nếu không đổi" />
                  </div>
                )}

                <div className="form-group">
                  <label>Họ tên</label>
                  <input className="form-control" name="name" value={form.name} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>Email</label>
                  <input className="form-control" type="email" name="email" value={form.email} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>Điện thoại</label>
                  <input className="form-control" name="phone" value={form.phone} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>Địa chỉ</label>
                  <input className="form-control" name="address" value={form.address} onChange={onChange} />
                </div>
              </div>

              <div className="col-md-4">
                <div className="form-group">
                  <label>Vai trò</label>
                  <select className="form-control" name="role" value={form.role} onChange={onChange}>
                    <option value="USER">USER</option>
                    <option value="EDITOR">EDITOR</option>
                    <option value="ADMIN">ADMIN</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Giới tính</label>
                  <input className="form-control" name="gender" value={form.gender} onChange={onChange} placeholder="Nam/Nữ/Khác..." />
                </div>

                <div className="form-group">
                  <label>Ngày sinh</label>
                  <input className="form-control" type="date" name="dateOfBirth" value={form.dateOfBirth} onChange={onChange} />
                </div>

                <div className="form-group">
                  <label>Xác minh OTP</label><br/>
                  <label style={{fontWeight: "normal"}}>
                    <input type="checkbox" name="otpVerified" checked={form.otpVerified} onChange={onChange} /> Đã xác minh
                  </label>
                </div>

                <div className="form-group">
                  <label>Ảnh đại diện</label>
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
              <Link to="/admin/users" className="btn btn-default" style={{ marginRight: 8 }}>
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
