import React, { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { getTours, deleteTour } from "@/services/tourService";
import { getCategoriesLite } from "@/services/categoryService";
import ContentHeader from "@/components/admin/ContentHeader";
import { Box, LoadingSpinner } from "@/components/admin/AdminComponents";

const PAGE_SIZE_DEFAULT = 6;

export default function TourList() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [loading, setLoading] = useState(true);
  const [rows, setRows] = useState([]);
  const [page, setPage] = useState(Number(searchParams.get("page")) || 1);
  const [pageSize, setPageSize] = useState(Number(searchParams.get("pageSize")) || PAGE_SIZE_DEFAULT);
  const [totalPages, setTotalPages] = useState(1);

  // filters
  const [name, setName] = useState(searchParams.get("name") || "");
  const [categoryId, setCategoryId] = useState(searchParams.get("categoryId") || "");
  const [sort, setSort] = useState(searchParams.get("sort") || "desc");
  const [categories, setCategories] = useState([]);

  const breadcrumbs = useMemo(() => [
    { label: "Home", link: "/admin", icon: "fa-dashboard" },
    { label: "Tours" }
  ], []);

  // load categories for filter
  useEffect(() => {
    (async () => {
      try {
        const cats = await getCategoriesLite();
        setCategories(Array.isArray(cats) ? cats : []);
      } catch (e) {
        console.error(e);
        setCategories([]);
      }
    })();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const data = await getTours({
        page, pageSize,
        name: name?.trim() || undefined,
        categoryId: categoryId ? Number(categoryId) : undefined,
        sort
      });
      setRows(Array.isArray(data.items) ? data.items : []);
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
    if (name) p.set("name", name);
    if (categoryId) p.set("categoryId", categoryId);
    if (sort) p.set("sort", sort);
    setSearchParams(p);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, pageSize, name, categoryId, sort]);

  const onDelete = async (id) => {
    if (!window.confirm("Xóa tour này?")) return;
    try {
      await deleteTour(id);
      if (rows.length === 1 && page > 1) setPage(page - 1);
      else fetchData();
    } catch (err) {
      const msg = err?.response?.data?.message || "Xóa thất bại!";
      alert(msg);
    }
  };

  const fmtPrice = (v) => (v == null ? "—" : Number(v).toLocaleString());

  return (
    <>
      <ContentHeader title="Tours" subtitle="Quản trị tour" breadcrumbs={breadcrumbs} />
      <section className="content">
        <Box title="Danh sách" type="primary">
          {/* Filters */}
          <div className="row" style={{ marginBottom: 12, alignItems:'center', rowGap:8 }}>
            <div className="col-sm-3">
              <div className="input-group">
                <span className="input-group-addon"><i className="fa fa-search"/></span>
                <input
                  className="form-control"
                  placeholder="Tìm theo tên…"
                  value={name}
                  onChange={(e) => { setPage(1); setName(e.target.value); }}
                />
              </div>
            </div>
            <div className="col-sm-3">
              <select
                className="form-control"
                value={categoryId}
                onChange={(e) => { setPage(1); setCategoryId(e.target.value); }}
              >
                <option value="">-- Tất cả Category --</option>
                {categories.map(c => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
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
                {[6,12,24,48].map(s => <option key={s} value={s}>{s}/trang</option>)}
              </select>
            </div>
            <div className="col-sm-2 text-right">
              <Link to="/admin/tours/create" className="btn btn-primary">
                <i className="fa fa-plus" /> Thêm tour
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
                    <th style={{width:110}}>Ảnh</th>
                    <th>Tên tour</th>
                    <th>Category</th>
                    <th>Giá</th>
                    <th>Lượt xem</th>
                    <th>Ngày tạo</th>
                    <th style={{width:160}}>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.length === 0 && (
                    <tr><td colSpan={8} className="text-center">Không có dữ liệu</td></tr>
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
                      <td>{r.name}</td>
                      <td>{r.categoryName ?? (r.categoryId ?? "—")}</td>
                      <td>{fmtPrice(r.price)}</td>
                      <td>{r.view ?? 0}</td>
                      <td>{r.createdDate ? new Date(r.createdDate).toLocaleString() : "—"}</td>
                      <td>
                        <div className="btn-group btn-group-sm">
                          <Link to={`/admin/tours/edit/${r.id}`} className="btn btn-warning">
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
