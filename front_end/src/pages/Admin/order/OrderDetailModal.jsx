import React from 'react';
import './OrderDetailModal.css';

const OrderDetailModal = ({ order, onClose, onStatusChange, onDelete }) => {
  const getStatusText = (status) => {
    const statusMap = {
      0: 'Chờ xác nhận',
      1: 'Đã xác nhận',
      2: 'Đang giao',
      3: 'Đã giao',
      4: 'Đã hủy'
    };
    return statusMap[status] || 'Không xác định';
  };

  const getStatusClass = (status) => {
    const statusClassMap = {
      0: 'status-pending',
      1: 'status-confirmed',
      2: 'status-shipping',
      3: 'status-delivered',
      4: 'status-cancelled'
    };
    return statusClassMap[status] || '';
  };

  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount || 0);
  };

  const handleStatusChange = (newStatus) => {
    if (window.confirm(`Bạn có chắc muốn chuyển trạng thái sang "${getStatusText(newStatus)}"?`)) {
      onStatusChange(order.id, newStatus);
      onClose();
    }
  };

  const handleDelete = () => {
    if (window.confirm('Bạn có chắc muốn xóa đơn hàng này?')) {
      onDelete(order.id);
      onClose();
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-container" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>Chi tiết đơn hàng #{order.id}</h3>
          <button className="close-btn" onClick={onClose}>
            <i className="fa fa-times"></i>
          </button>
        </div>

        <div className="modal-body">
          {/* Order Status */}
          <div className="order-status-section">
            <div className={`order-status-badge ${getStatusClass(order.status)}`}>
              {getStatusText(order.status)}
            </div>
          </div>

          {/* Customer Info */}
          <div className="info-section">
            <h4><i className="fa fa-user"></i> Thông tin khách hàng</h4>
            <div className="info-grid">
              <div className="info-item">
                <label>Tên khách hàng:</label>
                <span>{order.name}</span>
              </div>
              <div className="info-item">
                <label>Email:</label>
                <span>{order.email || 'N/A'}</span>
              </div>
              <div className="info-item">
                <label>Số điện thoại:</label>
                <span>{order.phone}</span>
              </div>
              <div className="info-item">
                <label>Địa chỉ:</label>
                <span>{order.address}</span>
              </div>
              <div className="info-item">
                <label>Giới tính:</label>
                <span>{order.gender || 'Nam'}</span>
              </div>
              <div className="info-item">
                <label>Ngày đặt:</label>
                <span>{formatDate(order.date)}</span>
              </div>
            </div>
          </div>

          {/* Tour Info */}
          <div className="info-section">
            <h4><i className="fa fa-plane"></i> Thông tin Tour</h4>
            <div className="info-grid">
              <div className="info-item">
                <label>Tên Tour:</label>
                <span>{order.tourName || 'N/A'}</span>
              </div>
              <div className="info-item">
                <label>Mô tả:</label>
                <span>{order.tourDescription || 'N/A'}</span>
              </div>
              <div className="info-item">
                <label>Giá tour:</label>
                <span className="price">{formatCurrency(order.tourPrice)}</span>
              </div>
            </div>
          </div>

          {/* Order Details */}
          {order.orderDetails && order.orderDetails.length > 0 && (
            <div className="info-section">
              <h4><i className="fa fa-list"></i> Chi tiết đơn hàng</h4>
              <table className="details-table">
                <thead>
                  <tr>
                    <th>Tour</th>
                    <th>Số lượng</th>
                    <th>Đơn giá</th>
                    <th>Thành tiền</th>
                  </tr>
                </thead>
                <tbody>
                  {order.orderDetails.map((detail, index) => (
                    <tr key={index}>
                      <td>{detail.tourName}</td>
                      <td>{detail.quantity}</td>
                      <td>{formatCurrency(detail.tourPrice)}</td>
                      <td>{formatCurrency(detail.quantity * detail.tourPrice)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Total Price */}
          <div className="total-section">
            <div className="total-row">
              <span className="total-label">Tổng tiền:</span>
              <span className="total-amount">{formatCurrency(order.totalPrice)}</span>
            </div>
          </div>

          {/* User Info */}
          {order.userName && (
            <div className="info-section">
              <h4><i className="fa fa-user-circle"></i> Thông tin tài khoản</h4>
              <div className="info-grid">
                <div className="info-item">
                  <label>Username:</label>
                  <span>{order.userName}</span>
                </div>
                <div className="info-item">
                  <label>Email tài khoản:</label>
                  <span>{order.userEmail}</span>
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="modal-footer">
          <div className="button-group">
            {/* Status Change Buttons */}
            {order.status === 0 && (
              <button
                className="btn btn-success"
                onClick={() => handleStatusChange(1)}
              >
                <i className="fa fa-check"></i> Xác nhận đơn hàng
              </button>
            )}
            {order.status === 1 && (
              <button
                className="btn btn-info"
                onClick={() => handleStatusChange(2)}
              >
                <i className="fa fa-truck"></i> Đang giao hàng
              </button>
            )}
            {order.status === 2 && (
              <button
                className="btn btn-primary"
                onClick={() => handleStatusChange(3)}
              >
                <i className="fa fa-check-circle"></i> Đã giao hàng
              </button>
            )}
            
            {/* Cancel Button */}
            {order.status !== 3 && order.status !== 4 && (
              <button
                className="btn btn-warning"
                onClick={() => handleStatusChange(4)}
              >
                <i className="fa fa-ban"></i> Hủy đơn hàng
              </button>
            )}

            {/* Delete Button */}
            <button
              className="btn btn-danger"
              onClick={handleDelete}
            >
              <i className="fa fa-trash"></i> Xóa đơn hàng
            </button>

            {/* Close Button */}
            <button
              className="btn btn-default"
              onClick={onClose}
            >
              <i className="fa fa-times"></i> Đóng
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default OrderDetailModal;
