import React, { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { getUsers, deleteUser } from "@/services/userService";
import ContentHeader from "@/components/admin/ContentHeader";
import { Box, LoadingSpinner } from "@/components/admin/AdminComponents";

const PAGE_SIZE_DEFAULT = 10;

export default function UserList() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [loading, setLoading] = useState(true);
  const [rows, setRows] = useState([]);
  const [page, setPage] = useState(Number(searchParams.get("page")) || 1);
  const [pageSize, setPageSize] = useState(Number(searchParams.get("pageSize")) || PAGE_SIZE_DEFAULT);
  const [totalPages, setTotalPages] = useState(1);
  const [keyword, setKeyword] = useState(searchParams.get("keyword") || "");
  const [role, setRole] = useState(searchParams.get("role") || "");

  const breadcrumbs = useMemo(() => [
    { label: "Home", link: "/admin", icon: "fa-dashboard" },
    { label: "Users" }
  ], []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const data = await getUsers({
        page, pageSize,
        keyword: keyword?.trim() || undefined,
        role: role || undefined
      });
      setRows(data.items || []);
      setTotalPages(data.totalPages || 1);
    } catch (e) {
      console.error(e);
    } finally { setLoading(false); }
  };

  useEffect(() => {
    fetchData();
    const p = new URLSearchParams();
    p.set("page", String(page));
    p.set("pageSize", String(pageSize));
    if (keyword) p.set("keyword", keyword);
    if (role) p.set("role", role);
    setSearchParams(p);
  }, [page, pageSize, keyword, role]);

  const onDelete = async (id) => {
    if (!window.confirm("Xóa người dùng này?")) return;
    try {
      await deleteUser(id);
      if (rows.length === 1 && page > 1) setPage(page - 1);
      else fetchData();
    } catch { alert("Xóa thất bại!"); }
  };

  return (
    <>
      <ContentHeader title="Users" subtitle="Quản trị người dùng" breadcrumbs={breadcrumbs} />
      <section className="content">
        <Box title="Danh sách" type="primary">
          {/* Filter */}
          <div className="row" style={{ marginBottom: 12, alignItems:'center', rowGap:8 }}>
            <div className="col-sm-4">
              <div className="input-group">
                <span className="input-group-addon"><i className="fa fa-search"/></span>
                <input
                  className="form-control"
                  placeholder="Tìm theo username, email, tên…"
                  value={keyword}
                  onChange={(e) => { setPage(1); setKeyword(e.target.value); }}
                />
              </div>
            </div>
            <div className="col-sm-3">
              <select className="form-control" value={role} onChange={(e) => { setPage(1); setRole(e.target.value); }}>
                <option value="">-- Tất cả vai trò --</option>
                <option value="USER">USER</option>
                <option value="ADMIN">ADMIN</option>
                <option value="EDITOR">EDITOR</option>
              </select>
            </div>
            <div className="col-sm-3">
              <select
                className="form-control"
                value={pageSize}
                onChange={(e) => { setPage(1); setPageSize(Number(e.target.value)); }}
              >
                {[5,10,20,50].map(s => <option key={s} value={s}>{s}/trang</option>)}
              </select>
            </div>
            <div className="col-sm-2 text-right">
              <Link to="/admin/users/create" className="btn btn-primary">
                <i className="fa fa-plus" /> Thêm người dùng
              </Link>
            </div>
          </div>

          {/* Table */}
          {loading ? (
            <LoadingSpinner text="Đang tải..." />
          ) : (
            <div className="table-responsive">
              <table className="table table-bordered table-striped">
                <thead>
                  <tr>
                    <th style={{width:70}}>ID</th>
                    <th style={{width:80}}>Avatar</th>
                    <th>Username</th>
                    <th>Tên</th>
                    <th>Email</th>
                    <th>Phone</th>
                    <th>Role</th>
                    <th>Ngày tạo</th>
                    <th style={{width:160}}>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.length === 0 && (
                    <tr><td colSpan={9} className="text-center">Không có dữ liệu</td></tr>
                  )}
                  {rows.map(r => (
                    <tr key={r.id}>
                      <td>{r.id}</td>
                      <td>
                        {r.image ? (
                          <div className="thumb">
                            <img src={import.meta.env.VITE_API_URL + r.image} alt="" />
                          </div>
                        ) : "—"}
                      </td>
                      <td>{r.username}</td>
                      <td>{r.name ?? "—"}</td>
                      <td>{r.email ?? "—"}</td>
                      <td>{r.phone ?? "—"}</td>
                      <td>{r.role ?? "—"}</td>
                      <td>{r.createdDate ? new Date(r.createdDate).toLocaleString() : "—"}</td>
                      <td>
                        <div className="btn-group btn-group-sm">
                          <Link to={`/admin/users/edit/${r.id}`} className="btn btn-warning">
                            <i className="fa fa-pencil" />
                          </Link>
                          <button className="btn btn-danger" onClick={() => onDelete(r.id)}>
                            <i className="fa fa-trash" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>

              {/* Pagination */}
              <div className="row">
                <div className="col-sm-12 text-center">
                  <ul className="pagination" style={{ display:'inline-flex', gap:6 }}>
                    <li className={`paginate_button previous ${page <= 1 ? "disabled" : ""}`}>
                      <button className="btn btn-default" onClick={() => setPage(1)} disabled={page <= 1}>Đầu</button>
                    </li>
                    <li className={`paginate_button ${page <= 1 ? "disabled" : ""}`}>
                      <button className="btn btn-default" onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page <= 1}>«</button>
                    </li>
                    <li className="paginate_button active"><span className="btn btn-primary">{page} / {totalPages}</span></li>
                    <li className={`paginate_button ${page >= totalPages ? "disabled" : ""}`}>
                      <button className="btn btn-default" onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page >= totalPages}>»</button>
                    </li>
                    <li className={`paginate_button next ${page >= totalPages ? "disabled" : ""}`}>
                      <button className="btn btn-default" onClick={() => setPage(totalPages)} disabled={page >= totalPages}>Cuối</button>
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          )}
        </Box>
      </section>
    </>
  );
}
