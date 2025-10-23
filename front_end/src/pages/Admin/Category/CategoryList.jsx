import React, { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import ContentHeader from "@/components/admin/ContentHeader";
import { Box, LoadingSpinner } from "@/components/admin/AdminComponents";
import { getCategories, deleteCategory } from "@/services/categoryService";

const PAGE_SIZE_DEFAULT = 10;

export default function CategoryList() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [loading, setLoading] = useState(true);
  const [rows, setRows] = useState([]);
  const [page, setPage] = useState(Number(searchParams.get("page")) || 1);
  const [pageSize, setPageSize] = useState(Number(searchParams.get("pageSize")) || PAGE_SIZE_DEFAULT);
  const [totalPages, setTotalPages] = useState(1);
  const [keyword, setKeyword] = useState(searchParams.get("keyword") || "");
  const [sort, setSort] = useState(searchParams.get("sort") || "desc");

  const breadcrumbs = useMemo(() => [
    { label: "Home", link: "/admin", icon: "fa-dashboard" },
    { label: "Categories" }
  ], []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const data = await getCategories({
        page, pageSize,
        keyword: keyword?.trim() || undefined,
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
    const p = new URLSearchParams();
    p.set("page", String(page));
    p.set("pageSize", String(pageSize));
    if (keyword) p.set("keyword", keyword);
    if (sort) p.set("sort", sort);
    setSearchParams(p);
  }, [page, pageSize, keyword, sort]);

  const onDelete = async (id) => {
    if (!window.confirm("Xoá category này?")) return;
    try {
      await deleteCategory(id);
      if (rows.length === 1 && page > 1) setPage(page - 1);
      else fetchData();
    } catch (e) {
      alert("Xoá thất bại!");
    }
  };

  return (
    <>
      <ContentHeader title="Categories" subtitle="Quản trị danh mục" breadcrumbs={breadcrumbs} />
      <section className="content">
        <Box title="Danh sách" type="primary">
          {/* Filter */}
          <div className="row" style={{ marginBottom: 12 }}>
            <div className="col-sm-4">
              <input
                className="form-control"
                placeholder="Tìm theo tên (Type)..."
                value={keyword}
                onChange={(e) => { setPage(1); setKeyword(e.target.value); }}
              />
            </div>
            <div className="col-sm-3">
              <select className="form-control" value={sort} onChange={(e) => setSort(e.target.value)}>
                <option value="desc">Mới nhất</option>
                <option value="asc">Cũ trước</option>
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
              <Link to="/admin/category/create" className="btn btn-primary">
                <i className="fa fa-plus" /> Thêm Category
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
                    <th style={{width:80}}>ID</th>
                    <th>Tên (Type)</th>
                    <th style={{width:160}}>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.length === 0 && (
                    <tr><td colSpan={3} className="text-center">Không có dữ liệu</td></tr>
                  )}
                  {rows.map(r => (
                    <tr key={r.id}>
                      <td>{r.id}</td>
                      <td>{r.type}</td>
                      <td>
                        <div className="btn-group btn-group-sm">
                          <Link to={`/admin/category/edit/${r.id}`} className="btn btn-warning">
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
