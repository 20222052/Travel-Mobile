import adminApiClient from "./adminApiClient";

/**
 * Order Service - Gọi các API quản lý đơn hàng
 */
const orderService = {
  /**
   * Lấy danh sách tất cả đơn hàng (Admin - có phân trang)
   * GET /api/ApiOrder/all?searchString=&sortOrder=&page=1&pageSize=10
   */
  getAllOrders: async (params = {}) => {
    try {
      const { searchString = "", sortOrder = "date_desc", page = 1, pageSize = 10 } = params;
      const response = await adminApiClient.get("/api/ApiOrder/all", {
        params: { searchString, sortOrder, page, pageSize }
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching all orders:", error);
      throw error;
    }
  },

  /**
   * Lấy chi tiết đơn hàng theo ID
   * GET /api/ApiOrder/detail/{id}
   */
  getOrderById: async (id) => {
    try {
      const response = await adminApiClient.get(`/api/ApiOrder/detail/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching order ${id}:`, error);
      throw error;
    }
  },

  /**
   * Lấy danh sách đơn hàng của user
   * GET /api/ApiOrder?id=1&search=&sort=desc
   */
  getOrdersByUserId: async (userId, search = "", sort = "desc") => {
    try {
      const response = await adminApiClient.get("/api/ApiOrder", {
        params: { id: userId, search, sort }
      });
      return response.data;
    } catch (error) {
      console.error(`Error fetching orders for user ${userId}:`, error);
      throw error;
    }
  },

  /**
   * Lấy chi tiết các item trong đơn hàng
   * GET /api/ApiOrder/orderDetail/{id}
   */
  getOrderDetails: async (orderId) => {
    try {
      const response = await adminApiClient.get(`/api/ApiOrder/orderDetail/${orderId}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching order details for ${orderId}:`, error);
      throw error;
    }
  },

  /**
   * Tạo đơn hàng mới (Admin)
   * POST /api/ApiOrder/create
   */
  createOrder: async (orderData) => {
    try {
      const response = await adminApiClient.post("/api/ApiOrder/create", orderData);
      return response.data;
    } catch (error) {
      console.error("Error creating order:", error);
      throw error;
    }
  },

  /**
   * Cập nhật đơn hàng (Admin)
   * PUT /api/ApiOrder/update/{id}
   */
  updateOrder: async (id, orderData) => {
    try {
      const response = await adminApiClient.put(`/api/ApiOrder/update/${id}`, orderData);
      return response.data;
    } catch (error) {
      console.error(`Error updating order ${id}:`, error);
      throw error;
    }
  },

  /**
   * Cập nhật trạng thái đơn hàng (gửi email)
   * PUT /api/ApiOrder/update-status/{id}
   */
  updateOrderStatus: async (id, status) => {
    try {
      const response = await adminApiClient.put(`/api/ApiOrder/update-status/${id}`, {
        status: status
      });
      return response.data;
    } catch (error) {
      console.error(`Error updating order status ${id}:`, error);
      throw error;
    }
  },

  /**
   * Xóa đơn hàng (Admin)
   * DELETE /api/ApiOrder/delete/{id}
   */
  deleteOrder: async (id) => {
    try {
      const response = await adminApiClient.delete(`/api/ApiOrder/delete/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error deleting order ${id}:`, error);
      throw error;
    }
  },

  /**
   * Lấy thống kê đơn hàng
   * GET /api/ApiOrder/statistics
   */
  getStatistics: async () => {
    try {
      const response = await adminApiClient.get("/api/ApiOrder/statistics");
      return response.data;
    } catch (error) {
      console.error("Error fetching order statistics:", error);
      throw error;
    }
  }
};

export default orderService;
