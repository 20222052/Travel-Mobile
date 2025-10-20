import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';

const Dashboard = () => {
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

  useEffect(() => {
    fetchDashboardStats();
  }, []);

  const fetchDashboardStats = async () => {
    try {
      // TODO: Replace with actual API call
      setStats({
        totalOrders: 150,
        newOrders: 23,
        totalUsers: 450,
        newUsers: 15,
        totalTours: 45,
        pendingOrders: 8,
        completedOrders: 120,
        cancelledOrders: 5,
        totalRevenue: 150000000,
        monthRevenue: 25000000
      });
    } catch (error) {
      console.error('Error fetching dashboard stats:', error);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', { 
      style: 'currency', 
      currency: 'VND' 
    }).format(amount);
  };

  return (
    <>
      <section className="content-header">
        <h1>
          Dashboard
          <small>Control panel</small>
        </h1>
        <ol className="breadcrumb">
          <li><Link to="/admin"><i className="fa fa-dashboard"></i> Home</Link></li>
          <li className="active">Dashboard</li>
        </ol>
      </section>

      <section className="content">
        <div className="row">
          <div className="col-lg-3 col-xs-6">
            <div className="small-box bg-aqua">
              <div className="inner">
                <h3>{stats.totalOrders}</h3>
                <p>Tổng đơn hàng</p>
              </div>
              <div className="icon">
                <i className="ion ion-bag"></i>
              </div>
              <Link to="/admin/order" className="small-box-footer">
                Xem chi tiết <i className="fa fa-arrow-circle-right"></i>
              </Link>
            </div>
          </div>

          <div className="col-lg-3 col-xs-6">
            <div className="small-box bg-green">
              <div className="inner">
                <h3>{stats.newOrders}</h3>
                <p>Đơn hàng mới (tháng này)</p>
              </div>
              <div className="icon">
                <i className="ion ion-stats-bars"></i>
              </div>
              <Link to="/admin/order" className="small-box-footer">
                Xem chi tiết <i className="fa fa-arrow-circle-right"></i>
              </Link>
            </div>
          </div>

          <div className="col-lg-3 col-xs-6">
            <div className="small-box bg-yellow">
              <div className="inner">
                <h3>{stats.totalUsers}</h3>
                <p>Tổng người dùng</p>
              </div>
              <div className="icon">
                <i className="ion ion-person-add"></i>
              </div>
              <Link to="/admin/user" className="small-box-footer">
                Xem chi tiết <i className="fa fa-arrow-circle-right"></i>
              </Link>
            </div>
          </div>

          <div className="col-lg-3 col-xs-6">
            <div className="small-box bg-red">
              <div className="inner">
                <h3>{stats.pendingOrders}</h3>
                <p>Đơn chờ xử lý</p>
              </div>
              <div className="icon">
                <i className="ion ion-pie-graph"></i>
              </div>
              <Link to="/admin/order" className="small-box-footer">
                Xem chi tiết <i className="fa fa-arrow-circle-right"></i>
              </Link>
            </div>
          </div>
        </div>
      </section>
    </>
  );
};

export default Dashboard;
