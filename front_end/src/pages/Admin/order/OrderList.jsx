import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import ContentHeader from '@/components/admin/ContentHeader';
import { LoadingSpinner } from '@/components/admin/AdminComponents';
import { usePageTitle } from '@/hooks/useAdminHooks';
import orderService from '@/services/orderService';
import OrderDetailModal from './OrderDetailModal';
import './OrderList.css';

const OrderList = () => {
  usePageTitle('Orders');
  
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchString, setSearchString] = useState('');
  const [sortOrder, setSortOrder] = useState('date_desc');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize] = useState(10);
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [showModal, setShowModal] = useState(false);

  const breadcrumbs = [
    { label: 'Home', link: '/admin', icon: 'fa-dashboard' },
    { label: 'Orders' }
  ];

  useEffect(() => {
    fetchOrders();
  }, [currentPage, sortOrder]);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const response = await orderService.getAllOrders({
        searchString,
        sortOrder,
        page: currentPage,
        pageSize
      });
      
      setOrders(response.orders || []);
      setTotalPages(response.totalPages || 1);
      setLoading(false);
    } catch (error) {
      console.error('Error fetching orders:', error);
      setLoading(false);
    }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    setCurrentPage(1);
    fetchOrders();
  };

  const handleReset = () => {
    setSearchString('');
    setSortOrder('date_desc');
    setCurrentPage(1);
    setTimeout(() => fetchOrders(), 100);
  };

  const handleViewDetails = async (orderId) => {
    try {
      const orderDetail = await orderService.getOrderById(orderId);
      setSelectedOrder(orderDetail);
      setShowModal(true);
    } catch (error) {
      console.error('Error fetching order details:', error);
      alert('Không thể tải chi tiết đơn hàng');
    }
  };

  const handleStatusChange = async (orderId, newStatus) => {
    if (!window.confirm('Bạn có chắc muốn thay đổi trạng thái đơn hàng này?')) {
      return;
    }

    try {
      await orderService.updateOrderStatus(orderId, newStatus);
      alert('Cập nhật trạng thái thành công! Email đã được gửi đến khách hàng.');
      fetchOrders();
    } catch (error) {
      console.error('Error updating order status:', error);
      alert('Không thể cập nhật trạng thái đơn hàng');
    }
  };

  const handleDelete = async (orderId) => {
    if (!window.confirm('Bạn có chắc muốn xóa đơn hàng này?')) {
      return;
    }

    try {
      await orderService.deleteOrder(orderId);
      alert('Xóa đơn hàng thành công!');
      fetchOrders();
    } catch (error) {
      console.error('Error deleting order:', error);
      alert('Không thể xóa đơn hàng');
    }
  };

  const getStatusBadge = (status) => {
    const statusMap = {
      0: { text: 'Chờ xác nhận', class: 'status-pending' },
      1: { text: 'Đã xác nhận', class: 'status-confirmed' },
      2: { text: 'Đang giao', class: 'status-shipping' },
      3: { text: 'Đã giao', class: 'status-delivered' },
      4: { text: 'Đã hủy', class: 'status-cancelled' }
    };
    const statusInfo = statusMap[status] || { text: 'Không xác định', class: '' };
    return <span className={`status-badge ${statusInfo.class}`}>{statusInfo.text}</span>;
  };

  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
  };

  if (loading && orders.length === 0) {
    return (
      <>
        <ContentHeader title="Orders" subtitle="Control panel" breadcrumbs={breadcrumbs} />
        <section className="content">
          <LoadingSpinner size="lg" text="Loading orders..." />
        </section>
      </>
    );
  }

  return (
    <>
      <ContentHeader title="Orders" subtitle="Control panel" breadcrumbs={breadcrumbs} />

      <section className="content">
        <div className="box box-primary">
          {/* Search and Filter */}
          <div className="box-header with-border">
            <div className="row">
              <div className="col-md-12">
                <form onSubmit={handleSearch} className="form-inline search-form">
                  <div className="form-group" style={{ marginRight: '10px' }}>
                    <input
                      type="text"
                      className="form-control"
                      placeholder="Tìm theo tên, email, phone..."
                      value={searchString}
                      onChange={(e) => setSearchString(e.target.value)}
                      style={{ width: '250px' }}
                    />
                  </div>
                  
                  <div className="form-group" style={{ marginRight: '10px' }}>
                    <select
                      className="form-control"
                      value={sortOrder}
                      onChange={(e) => setSortOrder(e.target.value)}
                    >
                      <option value="date_desc">Ngày giảm dần</option>
                      <option value="date_asc">Ngày tăng dần</option>
                      <option value="name_asc">Tên A-Z</option>
                      <option value="name_desc">Tên Z-A</option>
                    </select>
                  </div>

                  <button type="submit" className="btn btn-primary">
                    <i className="fa fa-search"></i> Tìm kiếm
                  </button>
                  
                  <button type="button" className="btn btn-default" onClick={handleReset} style={{ marginLeft: '5px' }}>
                    <i className="fa fa-refresh"></i> Reset
                  </button>
                </form>
              </div>
            </div>

            <div className="row" style={{ marginTop: '15px' }}>
              <div className="col-md-12">
                <Link to="/admin/order/create" className="btn btn-success">
                  <i className="fa fa-plus"></i> Thêm mới
                </Link>
              </div>
            </div>
          </div>

          {/* Orders Table */}
          <div className="box-body">
            <div className="table-header">
              <h4>DANH SÁCH ĐẶT HÀNG</h4>
            </div>
            
            <div className="table-responsive">
              <table className="table table-bordered table-hover orders-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Tour</th>
                    <th>User</th>
                    <th>Tên #</th>
                    <th>Địa chỉ</th>
                    <th>Giới tính</th>
                    <th>Trạng thái #</th>
                    <th>Ngày đặt #</th>
                    <th>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {orders.length === 0 ? (
                    <tr>
                      <td colSpan="9" className="text-center">Không có dữ liệu</td>
                    </tr>
                  ) : (
                    orders.map((order) => (
                      <tr key={order.id}>
                        <td>{order.id}</td>
                        <td>{order.tourName || '-'}</td>
                        <td>{order.userEmail || '-'}</td>
                        <td>
                          <a 
                            href="#" 
                            onClick={(e) => { e.preventDefault(); handleViewDetails(order.id); }}
                            className="order-link"
                          >
                            {order.name}
                          </a>
                        </td>
                        <td>{order.address}</td>
                        <td>{order.gender || 'Nam'}</td>
                        <td>{getStatusBadge(order.status)}</td>
                        <td>{formatDate(order.date)}</td>
                        <td className="action-buttons">
                          <button
                            className="btn btn-warning btn-sm"
                            onClick={() => handleViewDetails(order.id)}
                            title="Chi tiết"
                          >
                            <i className="fa fa-edit"></i>
                          </button>
                          
                          {order.status === 3 ? (
                            <button className="btn btn-success btn-sm" disabled title="Hoàn thành">
                              <i className="fa fa-check"></i> Hoàn thành
                            </button>
                          ) : (
                            <button
                              className="btn btn-info btn-sm"
                              onClick={() => handleStatusChange(order.id, order.status + 1)}
                              title="Xác nhận"
                            >
                              <i className="fa fa-check-circle"></i> Xác nhận
                            </button>
                          )}
                          
                          <button
                            className="btn btn-danger btn-sm"
                            onClick={() => handleDelete(order.id)}
                            title="Xóa"
                          >
                            <i className="fa fa-trash"></i> Hủy
                          </button>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="box-footer clearfix">
                <ul className="pagination pagination-sm no-margin pull-right">
                  <li className={currentPage === 1 ? 'disabled' : ''}>
                    <a href="#" onClick={(e) => { e.preventDefault(); if (currentPage > 1) setCurrentPage(currentPage - 1); }}>
                      Previous
                    </a>
                  </li>
                  
                  {[...Array(totalPages)].map((_, index) => (
                    <li key={index + 1} className={currentPage === index + 1 ? 'active' : ''}>
                      <a href="#" onClick={(e) => { e.preventDefault(); setCurrentPage(index + 1); }}>
                        {index + 1}
                      </a>
                    </li>
                  ))}
                  
                  <li className={currentPage === totalPages ? 'disabled' : ''}>
                    <a href="#" onClick={(e) => { e.preventDefault(); if (currentPage < totalPages) setCurrentPage(currentPage + 1); }}>
                      Next
                    </a>
                  </li>
                </ul>
              </div>
            )}
          </div>
        </div>
      </section>

      {/* Order Detail Modal */}
      {showModal && selectedOrder && (
        <OrderDetailModal
          order={selectedOrder}
          onClose={() => {
            setShowModal(false);
            setSelectedOrder(null);
          }}
          onStatusChange={handleStatusChange}
          onDelete={handleDelete}
        />
      )}
    </>
  );
};

export default OrderList;
