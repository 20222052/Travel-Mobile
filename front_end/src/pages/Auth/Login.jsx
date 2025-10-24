import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { adminLogin, adminUserInfo } from "@/services/adminAuthService";
import "./login.css";

export default function Login() {
  const [form, setForm] = useState({ username: "", password: "" });
  const [touched, setTouched] = useState({ username: false, password: false });
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname || "/admin";

  useEffect(() => {
    adminUserInfo()
      .then(() => navigate("/admin", { replace: true }))
      .catch(() => {});
  }, [navigate]);

  const onChange = (e) => {
    setForm((s) => ({ ...s, [e.target.name]: e.target.value }));
  };

  const onBlur = (e) => {
    setTouched((t) => ({ ...t, [e.target.name]: true }));
  };

  const validate = () => {
    const errs = {};
    if (!form.username || form.username.trim().length < 3 || form.username.length > 50) {
      errs.username = "Vui lòng nhập tên đăng nhập (3-50 ký tự)";
    }
    if (!form.password || form.password.length < 5) {
      errs.password = "Vui lòng nhập mật khẩu (tối thiểu 6 ký tự)";
    }
    return errs;
  };

  const errors = validate();
  const isInvalid = (name) => touched[name] && errors[name];

  const handleSubmit = async (e) => {
    e.preventDefault();
    setTouched({ username: true, password: true });
    setError("");
    if (Object.keys(errors).length) return;

    try {
      await adminLogin({ username: form.username.trim(), password: form.password });
      navigate(from, { replace: true }); // về trang định vào (mặc định /admin)
    } catch (err) {
      setError(err?.response?.data?.message || "Đăng nhập thất bại");
    }
  };

  return (
    <div className="login-body">
      <div className="container mt-5">
        <div className="row justify-content-center">
          <div className="box_login col-md-12" style={{marginBottom:0,width:600}}>
            <div className="card shadow-sm">
              <div className="card-header bg-primary text-white text-center">
                <h4 className="mb-0">ĐĂNG NHẬP</h4>
              </div>
              <div className="card-body">
                <form onSubmit={handleSubmit} noValidate>
                  <div className="form-group mb-3">
                    <label className="required-field">Tên đăng nhập</label>
                    <div className="input-group-with-icon">
                      <i className="fa fa-user input-icon" />
                      <input
                        name="username"
                        value={form.username}
                        onChange={onChange}
                        onBlur={onBlur}
                        placeholder="Nhập tên đăng nhập"
                        className={`form-control ${isInvalid("username") ? "is-invalid" : ""}`}
                        minLength={3}
                        maxLength={50}
                        required
                        autoFocus
                      />
                      {isInvalid("username") && (
                        <div className="invalid-feedback d-block">{errors.username}</div>
                      )}
                    </div>
                  </div>

                  <div className="form-group mb-2">
                    <label className="required-field">Mật khẩu</label>
                    <div className="input-group-with-icon">
                      <i className="fa fa-lock input-icon" />
                      <input
                        name="password"
                        type="password"
                        value={form.password}
                        onChange={onChange}
                        onBlur={onBlur}
                        placeholder="Nhập mật khẩu"
                        className={`form-control ${isInvalid("password") ? "is-invalid" : ""}`}
                        minLength={6}
                        required
                      />
                      {isInvalid("password") && (
                        <div className="invalid-feedback d-block">{errors.password}</div>
                      )}
                    </div>
                  </div>

                  <button type="submit" className="btn btn-primary btn-submit">
                    <i className="fa fa-sign-in" /> Đăng nhập
                  </button>

                  {error && <div className="text-danger mt-3">{error}</div>}
                </form>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
