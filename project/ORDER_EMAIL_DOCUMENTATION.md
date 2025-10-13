# 📧 Order Email Notification System

## Tổng quan
Hệ thống tự động gửi email thông báo cho khách hàng khi:
1. **Tạo đơn hàng mới** - Gửi email xác nhận đơn hàng
2. **Thay đổi trạng thái đơn hàng** - Gửi email cập nhật trạng thái

---

## 🎯 Các trạng thái đơn hàng

| Status | Tên trạng thái | Màu Badge | Icon | Mô tả |
|--------|---------------|-----------|------|-------|
| 0 | Chờ xác nhận | 🟡 Vàng (#ffc107) | ⏰ | Đơn hàng mới tạo, chờ admin xác nhận |
| 1 | Đã xác nhận | 🔵 Xanh dương (#17a2b8) | ✅ | Admin đã xác nhận, chuẩn bị giao |
| 2 | Đang giao | 🔵 Xanh đậm (#007bff) | 🚚 | Đơn hàng đang được giao |
| 3 | Đã giao | 🟢 Xanh lá (#28a745) | 🎉 | Hoàn thành, đã giao thành công |
| 4 | Đã hủy | 🔴 Đỏ (#dc3545) | ❌ | Đơn hàng bị hủy |

---

## 📨 Email Templates

### 1. Email tạo đơn hàng mới
**Trigger:** Khi tạo đơn hàng (Status = 0)
**Subject:** `Xác nhận đơn hàng #{orderId} - {tourName}`

**Nội dung gồm:**
- Thông tin tour (tên, địa điểm, thời gian, giá)
- Thông tin đơn hàng (mã đơn, ngày đặt, thông tin khách hàng)
- Badge trạng thái "Chờ xác nhận"
- Lưu ý: Đơn hàng đang được xem xét

### 2. Email thay đổi trạng thái
**Trigger:** Khi admin cập nhật trạng thái đơn hàng
**Subject:** `Cập nhật trạng thái đơn hàng #{orderId}`

**Nội dung gồm:**
- Hiển thị thay đổi: `Trạng thái cũ ➡️ Trạng thái mới`
- Thông tin tour và đơn hàng
- Lưu ý theo từng trạng thái:
  - **Chờ xác nhận:** Đơn hàng đang chờ xác nhận
  - **Đã xác nhận:** Đơn hàng đã xác nhận, đang chuẩn bị
  - **Đang giao:** Chú ý điện thoại nhận thông tin
  - **Đã giao:** Chúc bạn có chuyến đi vui vẻ
  - **Đã hủy:** Liên hệ nếu có thắc mắc

---

## 🔧 Cấu hình Email

### File: `appsettings.json`
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "Travel Agency",
    "SenderEmail": "ngtung2004@gmail.com",
    "Password": "ugkf hgba zlak wvlq"
  }
}
```

**Note:** 
- Sử dụng Gmail App Password (không phải mật khẩu Gmail thông thường)
- Để tạo App Password: Google Account → Security → 2-Step Verification → App passwords

---

## 🏗️ Kiến trúc hệ thống

### Services
1. **IEmailService** - Service gửi email cơ bản (MailKit)
2. **IOrderEmailService** - Service chuyên biệt cho đơn hàng
   - `SendOrderCreatedEmailAsync()` - Gửi email tạo đơn
   - `SendOrderStatusChangedEmailAsync()` - Gửi email thay đổi trạng thái

### Registered in Program.cs
```csharp
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IOrderEmailService, OrderEmailService>();
```

---

## 📱 API Endpoints

### 1. Tạo đơn hàng (Mobile App)
```http
POST /api/ApiOrder
Content-Type: application/json

{
  "userId": 1,
  "tourId": 5,
  "name": "Nguyễn Văn A",
  "phone": "0912345678",
  "email": "customer@example.com",
  "address": "123 Main St, Hanoi",
  "gender": "Nam"
}
```

**Response:** 201 Created
**Email:** ✅ Tự động gửi email xác nhận đơn hàng

---

### 2. Lấy đơn hàng của user
```http
GET /api/ApiOrder/my-orders?userId=1
```

**Response:**
```json
[
  {
    "id": 1,
    "tourId": 5,
    "userId": 1,
    "name": "Nguyễn Văn A",
    "status": 0,
    "date": "2025-10-13T10:30:00",
    "tour": {
      "name": "Tour Hạ Long",
      "location": "Quảng Ninh",
      "price": 2000000
    }
  }
]
```

---

### 3. Hủy đơn hàng (Mobile App)
```http
PUT /api/ApiOrder/cancel/1?userId=1
```

**Response:** 200 OK
**Email:** ✅ Tự động gửi email thông báo hủy đơn

**Điều kiện:** Chỉ hủy được khi status = 0 (Chờ xác nhận) hoặc 1 (Đã xác nhận)

---

### 4. Lấy danh sách trạng thái
```http
GET /api/ApiOrder/status-list
```

**Response:**
```json
{
  "statuses": [
    { "value": 0, "text": "Chờ xác nhận", "color": "#ffc107" },
    { "value": 1, "text": "Đã xác nhận", "color": "#17a2b8" },
    { "value": 2, "text": "Đang giao", "color": "#007bff" },
    { "value": 3, "text": "Đã giao", "color": "#28a745" },
    { "value": 4, "text": "Đã hủy", "color": "#dc3545" }
  ]
}
```

---

## 🖥️ Admin Panel

### Quản lý đơn hàng
**URL:** `/Admin/Order`

**Tính năng:**
- ✅ Xem danh sách đơn hàng
- ✅ Lọc theo trạng thái
- ✅ Tìm kiếm theo tên khách hàng
- ✅ Cập nhật trạng thái (với email tự động)
- ✅ Xóa đơn hàng

**Buttons theo trạng thái:**

| Trạng thái hiện tại | Actions |
|-------------------|---------|
| **Chờ xác nhận** | ✅ Xác nhận / ❌ Hủy |
| **Đã xác nhận** | 🚚 Giao hàng / ❌ Hủy |
| **Đang giao** | ✅ Đã giao / ↶ Quay lại |
| **Đã giao** | ✅ Hoàn thành (read-only) |
| **Đã hủy** | ↶ Khôi phục |

**Email:** ✅ Mỗi lần click button cập nhật trạng thái → Gửi email tự động

---

## 🔍 Logging

Tất cả các hoạt động gửi email đều được log:
- ✅ **Success:** `Sent order creation email to {email} for Order #{id}`
- ❌ **Error:** `Failed to send order creation email for Order #{id}`

**Log Level:** Information (Success) / Error (Failure)

**Note:** Lỗi gửi email KHÔNG làm gián đoạn quá trình tạo/cập nhật đơn hàng

---

## ✅ Testing Checklist

### Admin Panel
- [ ] Tạo đơn hàng mới → Kiểm tra email
- [ ] Xác nhận đơn (0→1) → Kiểm tra email
- [ ] Giao hàng (1→2) → Kiểm tra email  
- [ ] Hoàn thành (2→3) → Kiểm tra email
- [ ] Hủy đơn (0/1→4) → Kiểm tra email
- [ ] Khôi phục (4→0) → Kiểm tra email

### Mobile API
- [ ] POST /api/ApiOrder → Kiểm tra email tạo đơn
- [ ] PUT /api/ApiOrder/cancel/{id} → Kiểm tra email hủy
- [ ] GET /api/ApiOrder/my-orders → Lấy được danh sách
- [ ] GET /api/ApiOrder/{id} → Lấy được chi tiết

### Email Content
- [ ] Subject line chính xác
- [ ] Thông tin đơn hàng đầy đủ
- [ ] Badge trạng thái hiển thị đúng màu
- [ ] Thông tin tour (nếu có) hiển thị đúng
- [ ] Lưu ý theo trạng thái phù hợp
- [ ] Footer thông tin liên hệ

---

## 🐛 Troubleshooting

### Email không gửi được
1. **Kiểm tra SMTP config** trong `appsettings.json`
2. **Gmail App Password:** Đảm bảo dùng App Password, không phải mật khẩu Gmail
3. **Firewall:** Kiểm tra port 587 có bị block không
4. **2FA:** Gmail phải bật 2-Step Verification để tạo App Password

### Email bị spam
1. Thêm sender email vào contact
2. Mark as "Not Spam" lần đầu
3. Cân nhắc dùng domain riêng thay vì Gmail

### User không có email
- System sẽ try-catch và log warning
- Đơn hàng vẫn được tạo/cập nhật bình thường
- Check log để biết email nào bị skip

---

## 📝 TODO / Future Improvements

- [ ] SMS notification (Twilio/AWS SNS)
- [ ] Push notification cho mobile app
- [ ] Email template customization trong admin panel
- [ ] Multi-language support
- [ ] Email tracking (opened, clicked)
- [ ] Scheduled reminder emails
- [ ] Admin notification khi có đơn mới
- [ ] Queue system cho bulk emails (RabbitMQ/Azure Service Bus)

---

## 👥 Contributors
- Development Team
- Last Updated: October 13, 2025
