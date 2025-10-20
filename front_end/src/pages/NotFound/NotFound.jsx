import { Link } from "react-router-dom";

export default function NotFound() {
  return (
    <div className="container">
      <div className="row justify-content-center align-items-center min-vh-100">
        <div className="col-md-6 text-center">
          <h1 className="display-1 fw-bold text-primary">404</h1>
          <h2 className="mb-4">Page Not Found</h2>
          <p className="lead text-muted mb-4">
            Sorry, the page you are looking for does not exist.
          </p>
          <Link to="/" className="btn btn-primary btn-lg">
            Go Back Home
          </Link>
        </div>
      </div>
    </div>
  );
}