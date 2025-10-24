import adminApiClient from "./adminApiClient";

/**
 * Dashboard Service - Gọi các API thống kê Dashboard
 */
const dashboardService = {
  /**
   * Lấy thống kê tổng quan của Dashboard
   * GET /api/ApiDashboardAdmin/statistics
   */
  getStatistics: async () => {
    try {
      const response = await adminApiClient.get("/api/ApiDashboardAdmin/statistics");
      return response.data;
    } catch (error) {
      console.error("Error fetching dashboard statistics:", error);
      throw error;
    }
  },

  /**
   * Lấy thống kê đơn hàng theo tháng
   * GET /api/ApiDashboardAdmin/orders-per-month?months=6
   */
  getOrdersPerMonth: async (months = 6) => {
    try {
      const response = await adminApiClient.get("/api/ApiDashboardAdmin/orders-per-month", {
        params: { months }
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching orders per month:", error);
      throw error;
    }
  },

  /**
   * Lấy thống kê đơn hàng theo trạng thái
   * GET /api/ApiDashboardAdmin/orders-by-status
   */
  getOrdersByStatus: async () => {
    try {
      const response = await adminApiClient.get("/api/ApiDashboardAdmin/orders-by-status");
      return response.data;
    } catch (error) {
      console.error("Error fetching orders by status:", error);
      throw error;
    }
  },

  /**
   * Lấy thống kê doanh thu
   * GET /api/ApiDashboardAdmin/revenue-statistics
   */
  getRevenueStatistics: async () => {
    try {
      const response = await adminApiClient.get("/api/ApiDashboardAdmin/revenue-statistics");
      return response.data;
    } catch (error) {
      console.error("Error fetching revenue statistics:", error);
      throw error;
    }
  },

  /**
   * Lấy doanh thu theo tháng
   * GET /api/ApiDashboardAdmin/revenue-per-month?months=12
   */
  getRevenuePerMonth: async (months = 12) => {
    try {
      const response = await adminApiClient.get("/api/ApiDashboardAdmin/revenue-per-month", {
        params: { months }
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching revenue per month:", error);
      throw error;
    }
  },

  /**
   * Lấy danh sách tour bán chạy nhất
   * GET /api/ApiDashboardAdmin/top-tours?limit=10
   */
  getTopTours: async (limit = 10) => {
    try {
      const response = await adminApiClient.get("/api/ApiDashboardAdmin/top-tours", {
        params: { limit }
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching top tours:", error);
      throw error;
    }
  },

  /**
   * Lấy danh sách đơn hàng gần đây
   * GET /api/ApiDashboardAdmin/recent-orders?limit=10
   */
  getRecentOrders: async (limit = 10) => {
    try {
      const response = await adminApiClient.get("/api/ApiDashboardAdmin/recent-orders", {
        params: { limit }
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching recent orders:", error);
      throw error;
    }
  },

  /**
   * Lấy danh sách người dùng mới
   * GET /api/ApiDashboardAdmin/recent-users?limit=10
   */
  getRecentUsers: async (limit = 10) => {
    try {
      const response = await adminApiClient.get("/api/ApiDashboardAdmin/recent-users", {
        params: { limit }
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching recent users:", error);
      throw error;
    }
  },

  /**
   * Lấy tất cả dữ liệu Dashboard (All in One)
   * GET /api/ApiDashboardAdmin/overview
   */
  getOverview: async () => {
    try {
      const response = await adminApiClient.get("/api/ApiDashboardAdmin/overview");
      return response.data;
    } catch (error) {
      console.error("Error fetching dashboard overview:", error);
      throw error;
    }
  }
};

export default dashboardService;
