import React, { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { getBlogs, deleteBlog } from "@/services/blogService";
import ContentHeader from "@/components/admin/ContentHeader";
import { Box, LoadingSpinner } from "@/components/admin/AdminComponents";

const PAGE_SIZE_DEFAULT = 10;

export default function BlogList() {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [rows, setRows] = useState([]);
  const [page, setPage] = useState(Number(searchParams.get("page")) || 1);
  const [pageSize, setPageSize] = useState(Number(searchParams.get("pageSize")) || PAGE_SIZE_DEFAULT);
  const [totalPages, setTotalPages] = useState(1);
  const [title, setTitle] = useState(searchParams.get("title") || "");
  const [categoryId, setCategoryId] = useState(searchParams.get("categoryId") || "");
  const [sort, setSort] = useState(searchParams.get("sort") || "desc");

  const breadcrumbs = useMemo(() => [
    { label: "Home", link: "/admin", icon: "fa-dashboard" },
    { label: "Blog" }
  ], []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const data = await getBlogs({
        page,
        pageSize,
        title: title?.trim() || undefined,
        categoryId: categoryId ? Number(categoryId) : undefined,
        sort
      });
      setRows(data.items || []);
      setTotalPages(data.totalPages || 1);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
    // sync URL
    const p = new URLSearchParams();
    p.set("page", String(page));
    p.set("pageSize", String(pageSize));
    if (title) p.set("title", title);
    if (categoryId) p.set("categoryId", categoryId);
    if (sort) p.set("sort", sort);
    setSearchParams(p);
  }, [page, pageSize, title, categoryId, sort]);

  const onDelete = async (id) => {
    if (!window.confirm("Xóa bài blog này?")) return;
    try {
      await deleteBlog(id);
      // nếu xóa dòng cuối của trang cuối, có thể lùi 1 trang
      if (rows.length === 1 && page > 1) setPage(page - 1);
      else fetchData();
    } catch (e) {
      alert("Xóa thất bại!");
    }
  };

  return (
    <>
      <ContentHeader title="Blog" subtitle="Quản trị bài viết" breadcrumbs={breadcrumbs} />
      <section className="content">
        <Box title="Danh sách" type="primary">
          {/* Filter */}
          <div className="row" style={{ marginBottom: 12 }}>
            <div className="col-sm-3">
              <input
                className="form-control"
                placeholder="Tìm theo tiêu đề..."
                value={title}
                onChange={(e) => { setPage(1); setTitle(e.target.value); }}
              />
            </div>
            <div className="col-sm-3">
              <input
                className="form-control"
                placeholder="CategoryId (optional)"
                value={categoryId}
                onChange={(e) => { setPage(1); setCategoryId(e.target.value); }}
              />
            </div>
            <div className="col-sm-2">
              <select className="form-control" value={sort} onChange={(e) => setSort(e.target.value)}>
                <option value="desc">Mới nhất</option>
                <option value="asc">Cũ trước</option>
              </select>
            </div>
            <div className="col-sm-2">
              <select
                className="form-control"
                value={pageSize}
                onChange={(e) => { setPage(1); setPageSize(Number(e.target.value)); }}
              >
                {[5,10,20,50].map(s => <option key={s} value={s}>{s}/trang</option>)}
              </select>
            </div>
            <div className="col-sm-2 text-right">
              <Link to="/admin/blog/create" className="btn btn-primary">
                <i className="fa fa-plus" /> Thêm bài viết
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
                    <th>Tiêu đề</th>
                    <th style={{width:100}}>Ảnh</th>
                    <th>CategoryId</th>
                    <th>View</th>
                    <th>PostedDate</th>
                    <th style={{width:160}}>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.length === 0 && (
                    <tr><td colSpan={7} className="text-center">Không có dữ liệu</td></tr>
                  )}
                  {rows.map(r => (
                    <tr key={r.id}>
                      <td>{r.id}</td>
                      <td>{r.title}</td>
                      <td>
                        {r.image ? (
                          <img src={import.meta.env.VITE_API_URL + r.image} alt="" style={{ width: 90, height: 60, objectFit: "cover" }} />
                        ) : "—"}
                      </td>
                      <td>{r.categoryId ?? "—"}</td>
                      <td>{r.view ?? 0}</td>
                      <td>{r.postedDate ? new Date(r.postedDate).toLocaleString() : "—"}</td>
                      <td>
                        <div className="btn-group btn-group-sm">
                          <Link to={`/admin/blog/edit/${r.id}`} className="btn btn-warning">
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
                  <ul className="pagination">
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
