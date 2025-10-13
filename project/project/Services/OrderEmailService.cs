using project.Models;

namespace project.Services
{
    public interface IOrderEmailService
    {
        Task SendOrderCreatedEmailAsync(Order order, User user, Tour? tour, List<OrderDetail>? orderDetails = null);
        Task SendOrderStatusChangedEmailAsync(Order order, User user, Tour? tour, byte oldStatus);
    }

    public class OrderEmailService : IOrderEmailService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderEmailService> _logger;

        public OrderEmailService(IEmailService emailService, ILogger<OrderEmailService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendOrderCreatedEmailAsync(Order order, User user, Tour? tour, List<OrderDetail>? orderDetails = null)
        {
            try
            {
                var subject = $"Xác nhận đơn hàng #{order.Id} - {tour?.Name ?? "Tour"}";
                var body = GenerateOrderCreatedEmailBody(order, user, tour, orderDetails);
                
                await _emailService.SendEmailAsync(user.Email ?? order.Email ?? "", subject, body);
                _logger.LogInformation($"Sent order creation email to {user.Email} for Order #{order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send order creation email for Order #{order.Id}");
                // Don't throw - email failure shouldn't break order creation
            }
        }

        public async Task SendOrderStatusChangedEmailAsync(Order order, User user, Tour? tour, byte oldStatus)
        {
            try
            {
                var subject = $"Cập nhật trạng thái đơn hàng #{order.Id}";
                var body = GenerateOrderStatusChangedEmailBody(order, user, tour, oldStatus);
                
                await _emailService.SendEmailAsync(user.Email ?? order.Email ?? "", subject, body);
                _logger.LogInformation($"Sent status change email to {user.Email} for Order #{order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send status change email for Order #{order.Id}");
                // Don't throw - email failure shouldn't break status update
            }
        }

        private string GenerateOrderCreatedEmailBody(Order order, User user, Tour? tour, List<OrderDetail>? orderDetails = null)
        {
            // Tạo HTML cho danh sách tour
            string tourListHtml = "";
            decimal totalAmount = 0;
            
            if (orderDetails != null && orderDetails.Any())
            {
                tourListHtml = "<div style='margin: 20px 0;'><h3 style='color: #667eea;'>🎫 Danh Sách Tour Đã Đặt</h3>";
                
                foreach (var detail in orderDetails)
                {
                    var detailTour = detail.Tour;
                    if (detailTour != null)
                    {
                        var itemTotal = (decimal)(detailTour.Price ?? 0) * (detail.Quantity ?? 1);
                        totalAmount += itemTotal;
                        
                        tourListHtml += $@"
                        <div class='tour-info'>
                            <h4 style='margin: 0 0 10px 0;'>📍 {detailTour.Name}</h4>
                            <p style='margin: 5px 0;'><strong>Địa điểm:</strong> {detailTour.Location}</p>
                            <p style='margin: 5px 0;'><strong>Thời gian:</strong> {detailTour.Duration}</p>
                            <p style='margin: 5px 0;'><strong>Giá/người:</strong> {detailTour.Price:N0} VNĐ</p>
                            <p style='margin: 5px 0;'><strong>Số lượng:</strong> {detail.Quantity ?? 1} người</p>
                            <p style='margin: 5px 0; font-size: 16px;'><strong>Thành tiền:</strong> <span style='color: #f5576c;'>{itemTotal:N0} VNĐ</span></p>
                        </div>";
                    }
                }
                
                tourListHtml += $@"
                <div style='background: #fff3cd; padding: 15px; border-radius: 8px; margin-top: 15px; text-align: right;'>
                    <h3 style='margin: 0; color: #856404;'>💰 Tổng Tiền: <span style='color: #d9534f;'>{totalAmount:N0} VNĐ</span></h3>
                </div>
                </div>";
            }
            else if (tour != null)
            {
                // Fallback: chỉ có 1 tour chính
                tourListHtml = $@"
                <div class='tour-info'>
                    <h3>📍 {tour.Name}</h3>
                    <p><strong>Địa điểm:</strong> {tour.Location}</p>
                    <p><strong>Thời gian:</strong> {tour.Duration}</p>
                    <p><strong>Giá:</strong> {tour.Price:N0} VNĐ</p>
                </div>";
            }
            
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #ddd; }}
        .order-info {{ background: white; padding: 20px; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .info-row {{ display: flex; padding: 10px 0; border-bottom: 1px solid #eee; }}
        .info-label {{ font-weight: bold; width: 150px; color: #555; }}
        .info-value {{ color: #333; }}
        .status-badge {{ display: inline-block; padding: 8px 16px; background: #ffc107; color: white; border-radius: 20px; font-weight: bold; }}
        .footer {{ background: #333; color: white; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; font-size: 12px; }}
        .tour-info {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Đặt Tour Thành Công!</h1>
            <p>Cảm ơn bạn đã đặt tour với chúng tôi</p>
        </div>
        
        <div class='content'>
            <p>Xin chào <strong>{user.Name ?? order.Name}</strong>,</p>
            <p>Chúng tôi đã nhận được đơn đặt tour của bạn. Dưới đây là thông tin chi tiết:</p>
            
            {tourListHtml}
            
            <div class='order-info'>
                <h3>📋 Thông Tin Đơn Hàng</h3>
                <div class='info-row'>
                    <div class='info-label'>Mã đơn hàng:</div>
                    <div class='info-value'>#{order.Id}</div>
                </div>
                <div class='info-row'>
                    <div class='info-label'>Ngày đặt:</div>
                    <div class='info-value'>{order.Date?.ToString("dd/MM/yyyy HH:mm")}</div>
                </div>
                <div class='info-row'>
                    <div class='info-label'>Tên khách hàng:</div>
                    <div class='info-value'>{order.Name}</div>
                </div>
                <div class='info-row'>
                    <div class='info-label'>Số điện thoại:</div>
                    <div class='info-value'>{order.Phone}</div>
                </div>
                <div class='info-row'>
                    <div class='info-label'>Địa chỉ:</div>
                    <div class='info-value'>{order.Address}</div>
                </div>
                <div class='info-row'>
                    <div class='info-label'>Trạng thái:</div>
                    <div class='info-value'><span class='status-badge'>Chờ xác nhận</span></div>
                </div>
            </div>
            
            <div style='margin-top: 20px; padding: 15px; background: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px;'>
                <p style='margin: 0;'><strong>⏰ Lưu ý:</strong> Đơn hàng của bạn đang được xem xét. Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất để xác nhận!</p>
            </div>
        </div>
        
        <div class='footer'>
            <p>Travel Agency - Đồng hành cùng bạn trên mọi hành trình</p>
            <p>📧 Email: ngtung2004@gmail.com | 📱 Hotline: 1900-xxxx</p>
            <p style='margin-top: 10px; font-size: 11px;'>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateOrderStatusChangedEmailBody(Order order, User user, Tour? tour, byte oldStatus)
        {
            var (statusText, statusColor, statusIcon) = GetStatusInfo(order.Status ?? 0);
            var (oldStatusText, _, _) = GetStatusInfo(oldStatus);
            
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #ddd; }}
        .status-change {{ background: white; padding: 20px; margin: 20px 0; border-radius: 8px; text-align: center; }}
        .status-badge {{ display: inline-block; padding: 12px 24px; background: {statusColor}; color: white; border-radius: 25px; font-weight: bold; font-size: 16px; margin: 10px; }}
        .arrow {{ font-size: 24px; color: #666; margin: 0 10px; }}
        .order-info {{ background: white; padding: 20px; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .info-row {{ padding: 8px 0; border-bottom: 1px solid #eee; }}
        .footer {{ background: #333; color: white; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; font-size: 12px; }}
        .note-box {{ margin-top: 20px; padding: 15px; border-left: 4px solid {statusColor}; border-radius: 4px; background: #f8f9fa; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{statusIcon} Cập Nhật Trạng Thái Đơn Hàng</h1>
            <p>Đơn hàng #{order.Id} đã được cập nhật</p>
        </div>
        
        <div class='content'>
            <p>Xin chào <strong>{user.Name ?? order.Name}</strong>,</p>
            <p>Trạng thái đơn hàng của bạn đã được cập nhật:</p>
            
            <div class='status-change'>
                <h3>Thay Đổi Trạng Thái</h3>
                <div>
                    <span class='status-badge' style='background: #6c757d;'>{oldStatusText}</span>
                    <span class='arrow'>➡️</span>
                    <span class='status-badge'>{statusText}</span>
                </div>
            </div>
            
            {(tour != null ? $@"
            <div class='order-info'>
                <h3>📍 Thông Tin Tour</h3>
                <div class='info-row'><strong>Tên tour:</strong> {tour.Name}</div>
                <div class='info-row'><strong>Địa điểm:</strong> {tour.Location}</div>
                <div class='info-row'><strong>Thời gian:</strong> {tour.Duration}</div>
            </div>" : "")}
            
            <div class='order-info'>
                <h3>📋 Thông Tin Đơn Hàng</h3>
                <div class='info-row'><strong>Mã đơn:</strong> #{order.Id}</div>
                <div class='info-row'><strong>Ngày đặt:</strong> {order.Date?.ToString("dd/MM/yyyy HH:mm")}</div>
                <div class='info-row'><strong>Khách hàng:</strong> {order.Name}</div>
                <div class='info-row'><strong>Số điện thoại:</strong> {order.Phone}</div>
            </div>
            
            {GetStatusNote(order.Status ?? 0)}
        </div>
        
        <div class='footer'>
            <p>Travel Agency - Đồng hành cùng bạn trên mọi hành trình</p>
            <p>📧 Email: ngtung2004@gmail.com | 📱 Hotline: 1900-xxxx</p>
            <p style='margin-top: 10px; font-size: 11px;'>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }

        private (string text, string color, string icon) GetStatusInfo(byte status)
        {
            return status switch
            {
                0 => ("Chờ xác nhận", "#ffc107", "⏰"),
                1 => ("Đã xác nhận", "#17a2b8", "✅"),
                2 => ("Đang giao", "#007bff", "🚚"),
                3 => ("Đã giao", "#28a745", "🎉"),
                4 => ("Đã hủy", "#dc3545", "❌"),
                _ => ("Không xác định", "#6c757d", "❓")
            };
        }

        private string GetStatusNote(byte status)
        {
            var note = status switch
            {
                0 => "⏰ Đơn hàng của bạn đang chờ được xác nhận. Chúng tôi sẽ liên hệ với bạn sớm nhất!",
                1 => "✅ Đơn hàng đã được xác nhận! Chúng tôi đang chuẩn bị để giao tour cho bạn.",
                2 => "🚚 Tour đang được giao! Vui lòng chú ý điện thoại để nhận thông tin từ nhân viên.",
                3 => "🎉 Tour đã được giao thành công! Chúc bạn có một chuyến đi vui vẻ!",
                4 => "❌ Đơn hàng đã bị hủy. Nếu có thắc mắc, vui lòng liên hệ với chúng tôi.",
                _ => ""
            };

            if (string.IsNullOrEmpty(note)) return "";

            var color = status switch
            {
                0 => "#ffc107",
                1 => "#17a2b8",
                2 => "#007bff",
                3 => "#28a745",
                4 => "#dc3545",
                _ => "#6c757d"
            };

            return $@"<div class='note-box' style='border-left-color: {color};'>
                <p style='margin: 0;'><strong>Lưu ý:</strong> {note}</p>
            </div>";
        }
    }
}
