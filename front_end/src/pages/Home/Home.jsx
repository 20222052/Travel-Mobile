import { Link } from "react-router-dom";

export default function Home() {
  return (
    <div className="container">
      <div className="row align-items-center min-vh-50 py-5">
        <div className="col-md-6">
          <h1 className="display-3 fw-bold">Buy & Sell Phones</h1>
          <p className="lead text-muted mb-4">
            The best marketplace for buying and selling used phones online.
          </p>
          <div className="d-flex gap-3">
            <Link to="/products" className="btn btn-primary btn-lg">
              Browse Products
            </Link>
            <Link to="/login" className="btn btn-outline-secondary btn-lg">
              Sign In
            </Link>
          </div>
        </div>
        <div className="col-md-6">
          {/* ✅ Dùng ảnh từ public folder */}
          <img
            src="/images/hero.jpg"
            alt="Buy and sell phones"
            className="img-fluid rounded shadow"
            onError={(e) => {
              // Fallback nếu ảnh không tồn tại
              e.target.style.display = "none";
            }}
          />
        </div>
      </div>
    </div>
  );
}
