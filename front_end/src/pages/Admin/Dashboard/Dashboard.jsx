import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import ContentHeader from '@/components/admin/ContentHeader';
import { Box, SmallBox, InfoBox, LoadingSpinner } from '@/components/admin/AdminComponents';
import { usePageTitle } from '@/hooks/useAdminHooks';
import dashboardService from '@/services/dashboardService';

const AdminDashboard = () => {
  usePageTitle('Dashboard');
  
  const [stats, setStats] = useState({
    totalOrders: 0,
    newOrders: 0,
    totalUsers: 0,
    newUsers: 0,
    totalTours: 0,
    pendingOrders: 0,
    completedOrders: 0,
    cancelledOrders: 0,
    totalRevenue: 0,
    monthRevenue: 0
  });
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetchDashboardStats();
  }, []);

  const fetchDashboardStats = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Gọi API lấy thống kê chính
      const statisticsData = await dashboardService.getStatistics();
      
      // Gọi API lấy thống kê doanh thu
      const revenueData = await dashboardService.getRevenueStatistics();
      
      // Map dữ liệu từ API vào state
      setStats({
        totalOrders: statisticsData.totalOrders || 0,
        newOrders: statisticsData.newOrders || 0,
        totalUsers: statisticsData.totalUsers || 0,
        newUsers: statisticsData.newUsers || 0,
        totalTours: statisticsData.totalTours || 0,
        pendingOrders: statisticsData.pendingOrders || 0,
        completedOrders: statisticsData.completedOrders || 0,
        cancelledOrders: statisticsData.cancelledOrders || 0,
        totalRevenue: revenueData.totalRevenue || 0,
        monthRevenue: revenueData.monthlyRevenue || 0
      });
      
      setLoading(false);
    } catch (error) {
      console.error('Error fetching dashboard stats:', error);
      setError('Không thể tải dữ liệu dashboard. Vui lòng thử lại sau.');
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', { 
      style: 'currency', 
      currency: 'VND' 
    }).format(amount);
  };

  const breadcrumbs = [
    { label: 'Home', link: '/admin', icon: 'fa-dashboard' },
    { label: 'Dashboard' }
  ];

  if (loading) {
    return (
      <>
        <ContentHeader 
          title="Dashboard" 
          subtitle="Control panel" 
          breadcrumbs={breadcrumbs}
        />
        <section className="content">
          <LoadingSpinner size="lg" text="Loading dashboard data..." />
        </section>
      </>
    );
  }

  if (error) {
    return (
      <>
        <ContentHeader 
          title="Dashboard" 
          subtitle="Control panel" 
          breadcrumbs={breadcrumbs}
        />
        <section className="content">
          <div className="alert alert-danger">
            <h4><i className="icon fa fa-ban"></i> Lỗi!</h4>
            {error}
            <button 
              className="btn btn-default btn-sm" 
              onClick={fetchDashboardStats}
              style={{ marginLeft: '10px' }}
            >
              <i className="fa fa-refresh"></i> Thử lại
            </button>
          </div>
        </section>
      </>
    );
  }

  return (
    <>
      <ContentHeader 
        title="Dashboard" 
        subtitle="Control panel" 
        breadcrumbs={breadcrumbs}
      />

      <section className="content">
        {/* Small boxes (Stat box) */}
        <div className="row">
          <div className="col-lg-3 col-xs-6">
            <SmallBox
              title="Tổng đơn hàng"
              value={stats.totalOrders}
              icon="ion-bag"
              color="aqua"
              link="/admin/order"
              linkText="Xem chi tiết"
            />
          </div>

          <div className="col-lg-3 col-xs-6">
            <SmallBox
              title="Đơn hàng mới (tháng này)"
              value={stats.newOrders}
              icon="ion-stats-bars"
              color="green"
              link="/admin/order"
              linkText="Xem chi tiết"
            />
          </div>

          <div className="col-lg-3 col-xs-6">
            <SmallBox
              title="Tổng người dùng"
              value={stats.totalUsers}
              icon="ion-person-add"
              color="yellow"
              link="/admin/dashboard"
              linkText="Xem chi tiết"
            />
          </div>

          <div className="col-lg-3 col-xs-6">
            <SmallBox
              title="Đơn chờ xử lý"
              value={stats.pendingOrders}
              icon="ion-pie-graph"
              color="red"
              link="/admin/order"
              linkText="Xem chi tiết"
            />
          </div>
        </div>

        {/* Thống kê chi tiết */}
        <div className="row">
          <div className="col-md-12">
            <Box title="Thống kê chi tiết" type="primary">
              <div className="row">
                <div className="col-md-3 col-sm-6 col-xs-12">
                  <InfoBox
                    title="Hoàn thành (tháng này)"
                    value={stats.completedOrders}
                    icon="fa-check-circle"
                    color="blue"
                  />
                </div>

                <div className="col-md-3 col-sm-6 col-xs-12">
                  <InfoBox
                    title="Đã hủy (tháng này)"
                    value={stats.cancelledOrders}
                    icon="fa-times-circle"
                    color="red"
                  />
                </div>

                <div className="col-md-3 col-sm-6 col-xs-12">
                  <InfoBox
                    title="Người dùng mới"
                    value={stats.newUsers}
                    icon="fa-users"
                    color="aqua"
                  />
                </div>

                <div className="col-md-3 col-sm-6 col-xs-12">
                  <InfoBox
                    title="Tổng số Tour"
                    value={stats.totalTours}
                    icon="fa-plane"
                    color="green"
                  />
                </div>
              </div>
            </Box>
          </div>
        </div>

        {/* Revenue Stats */}
        <div className="row">
          <div className="col-md-6">
            <Box title="Doanh thu" type="success">
              <div className="row">
                <div className="col-xs-6">
                  <div className="text-center">
                    <h4>Tổng doanh thu</h4>
                    <h3 className="text-success">
                      <strong>{formatCurrency(stats.totalRevenue)}</strong>
                    </h3>
                  </div>
                </div>
                <div className="col-xs-6">
                  <div className="text-center">
                    <h4>Doanh thu tháng này</h4>
                    <h3 className="text-primary">
                      <strong>{formatCurrency(stats.monthRevenue)}</strong>
                    </h3>
                  </div>
                </div>
              </div>
            </Box>
          </div>

          <div className="col-md-6">
            <Box title="Quick Actions" type="info">
              <div className="row">
                <div className="col-xs-6 mb-2">
                  <Link to="/admin/blog/create" className="btn btn-primary btn-block">
                    <i className="fa fa-plus"></i> Thêm Blog
                  </Link>
                </div>
                <div className="col-xs-6 mb-2">
                  <Link to="/admin/order" className="btn btn-success btn-block">
                    <i className="fa fa-list"></i> Xem Đơn hàng
                  </Link>
                </div>
              </div>
              <div className="row" style={{ marginTop: '10px' }}>
                <div className="col-xs-6">
                  <Link to="/admin/category" className="btn btn-warning btn-block">
                    <i className="fa fa-tags"></i> Quản lý Category
                  </Link>
                </div>
                <div className="col-xs-6">
                  <Link to="/admin/category/create" className="btn btn-info btn-block">
                    <i className="fa fa-plus"></i> Thêm Category
                  </Link>
                </div>
              </div>
            </Box>
          </div>
        </div>
      </section>
    </>
  );
};

export default AdminDashboard;
