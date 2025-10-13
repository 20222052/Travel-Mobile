# API Documentation - Đăng ký với OTP

## 📋 Tổng quan luồng đăng ký với OTP

### Luồng hoàn chỉnh:
1. **User đăng ký** → API tạo tài khoản (chưa active) và gửi OTP qua email
2. **User nhập OTP** → API xác thực OTP và active tài khoản
3. **User đăng nhập** → Chỉ cho phép đăng nhập nếu đã xác thực OTP

---

## 🔐 API 1: Đăng ký tài khoản (Register)

### Endpoint:
```
POST /api/ApiAccount/register
```

### Request Body:
```json
{
    "name": "Nguyễn Văn A",
    "gender": "Nam",
    "address": "123 Đường ABC, TP.HCM",
    "phone": "0901234567",
    "email": "nguyenvana@email.com",
    "username": "nguyenvana",
    "password": "Password123@",
    "dateOfBirth": "1995-01-15T00:00:00.000Z"
}
```

### Response - Thành công (200 OK):
```json
{
    "message": "Đăng ký thành công! Vui lòng kiểm tra email để lấy mã OTP xác thực tài khoản.",
    "email": "nguyenvana@email.com",
    "otpSent": true
}
```

### Response - Trùng username/email (409 Conflict):
```json
{
    "message": "Username hoặc Email đã tồn tại."
}
```

### Lưu ý:
- Sau khi đăng ký thành công, user sẽ nhận email chứa mã OTP (6 chữ số)
- OTP có hiệu lực trong **5 phút**
- Tài khoản chưa được active, không thể đăng nhập cho đến khi xác thực OTP

---

## ✅ API 2: Xác thực OTP

### Endpoint:
```
POST /api/ApiAccount/verify-otp
```

### Request Body:
```json
{
    "email": "nguyenvana@email.com",
    "code": "123456"
}
```

### Response - Thành công (200 OK):
```json
{
    "message": "Xác thực OTP thành công! Tài khoản đã được kích hoạt.",
    "verified": true
}
```

### Response - OTP không hợp lệ (400 Bad Request):
```json
{
    "message": "Mã OTP không hợp lệ!"
}
```

### Response - OTP hết hạn (400 Bad Request):
```json
{
    "message": "Mã OTP đã hết hạn!"
}
```

### Response - Email không tồn tại (404 Not Found):
```json
{
    "message": "Không tìm thấy tài khoản với email này."
}
```

### Response - Đã xác thực trước đó (400 Bad Request):
```json
{
    "message": "Tài khoản đã được xác thực trước đó."
}
```

---

## 🔄 API 3: Gửi lại OTP

### Endpoint:
```
POST /api/ApiAccount/resend-otp
```

### Request Body:
```json
{
    "email": "nguyenvana@email.com"
}
```

### Response - Thành công (200 OK):
```json
{
    "message": "Đã gửi lại mã OTP qua email."
}
```

### Response - Email không tồn tại (404 Not Found):
```json
{
    "message": "Không tìm thấy tài khoản với email này."
}
```

### Response - Đã xác thực (400 Bad Request):
```json
{
    "message": "Tài khoản đã được xác thực."
}
```

### Lưu ý:
- OTP cũ vẫn có hiệu lực cho đến khi hết hạn
- OTP mới sẽ được tạo và gửi qua email
- Chỉ OTP mới nhất và chưa sử dụng mới được chấp nhận

---

## 🔑 API 4: Đăng nhập (Login) - CẬP NHẬT

### Endpoint:
```
POST /api/ApiAccount/login
```

### Request Body:
```json
{
    "username": "nguyenvana",
    "password": "Password123@"
}
```

### Response - Thành công (200 OK):
```json
{
    "message": "Đăng nhập thành công.",
    "user": {
        "id": 1,
        "username": "nguyenvana",
        "name": "Nguyễn Văn A",
        "role": "USER",
        "image": "avatar.jpg"
    }
}
```

### Response - Chưa xác thực OTP (401 Unauthorized):
```json
{
    "message": "Tài khoản chưa được xác thực. Vui lòng kiểm tra email và xác thực OTP.",
    "email": "nguyenvana@email.com",
    "requireOtpVerification": true
}
```

### Response - Sai username/password (401 Unauthorized):
```json
{
    "message": "Tài khoản hoặc mật khẩu không chính xác."
}
```

---

## 📧 Email Template

Khi user đăng ký, họ sẽ nhận email với nội dung:

```html
<h3>Mã OTP xác thực</h3>
<p>Mã OTP của bạn là: <b>123456</b></p>
<p>Hết hạn sau 5 phút.</p>
```

---

## 🔄 Luồng xử lý chi tiết

### Kịch bản 1: Đăng ký thành công với OTP
```
1. User gửi thông tin đăng ký → POST /api/ApiAccount/register
2. Server tạo tài khoản với OtpVerified = false
3. Server tạo OTP (6 số ngẫu nhiên) và lưu vào database
4. Server gửi OTP qua email
5. Response: "Đăng ký thành công! Vui lòng kiểm tra email..."

6. User nhận email, lấy mã OTP
7. User gửi email + OTP → POST /api/ApiAccount/verify-otp
8. Server kiểm tra OTP (hợp lệ, chưa dùng, chưa hết hạn)
9. Server cập nhật OtpVerified = true
10. Response: "Xác thực OTP thành công!"

11. User có thể đăng nhập → POST /api/ApiAccount/login
12. Server kiểm tra OtpVerified = true → Cho phép đăng nhập
```

### Kịch bản 2: OTP hết hạn, cần gửi lại
```
1. User nhập OTP sau 5 phút
2. Server response: "Mã OTP đã hết hạn!"
3. User yêu cầu gửi lại → POST /api/ApiAccount/resend-otp
4. Server tạo OTP mới và gửi email
5. User nhập OTP mới → Xác thực thành công
```

### Kịch bản 3: User cố đăng nhập trước khi xác thực OTP
```
1. User đăng ký nhưng không xác thực OTP
2. User cố đăng nhập → POST /api/ApiAccount/login
3. Server kiểm tra OtpVerified = false
4. Response: "Tài khoản chưa được xác thực..."
5. User quay lại xác thực OTP
```

---

## 🧪 Test với Postman

### Test Case 1: Đăng ký → Xác thực OTP → Đăng nhập

#### Bước 1: Đăng ký
```http
POST http://localhost:5014/api/ApiAccount/register
Content-Type: application/json

{
    "name": "Test User",
    "gender": "Nam",
    "email": "test@email.com",
    "username": "testuser",
    "password": "Test123@",
    "dateOfBirth": "1995-01-15"
}
```
→ Kiểm tra email nhận OTP

#### Bước 2: Xác thực OTP
```http
POST http://localhost:5014/api/ApiAccount/verify-otp
Content-Type: application/json

{
    "email": "test@email.com",
    "code": "123456"
}
```

#### Bước 3: Đăng nhập
```http
POST http://localhost:5014/api/ApiAccount/login
Content-Type: application/json

{
    "username": "testuser",
    "password": "Test123@"
}
```

### Test Case 2: Gửi lại OTP
```http
POST http://localhost:5014/api/ApiAccount/resend-otp
Content-Type: application/json

{
    "email": "test@email.com"
}
```

### Test Case 3: Đăng nhập trước khi xác thực (Phải bị từ chối)
```http
POST http://localhost:5014/api/ApiAccount/login
Content-Type: application/json

{
    "username": "testuser",
    "password": "Test123@"
}
```
→ Response: "Tài khoản chưa được xác thực..."

---

## 🗄️ Database Schema

### Bảng User
```sql
CREATE TABLE [User] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    Username NVARCHAR(50) UNIQUE,
    Password NVARCHAR(MAX),
    Gender NVARCHAR(10),
    Phone NVARCHAR(20),
    Address NVARCHAR(200),
    DateOfBirth DATETIME,
    Role NVARCHAR(20),
    OtpVerified BIT DEFAULT 0,  -- ✅ Trạng thái xác thực OTP
    CreatedDate DATETIME,
    Image NVARCHAR(MAX)
)
```

### Bảng OtpCode
```sql
CREATE TABLE OtpCode (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(100),
    Code NVARCHAR(10),
    ExpireAt DATETIME,
    Used BIT DEFAULT 0
)
```

---

## ⚙️ Cấu hình Email (appsettings.json)

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "OTP System",
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### Lấy App Password từ Gmail:
1. Vào Google Account → Security
2. Bật 2-Step Verification
3. Tạo App Password cho "Mail"
4. Copy password vào `appsettings.json`

---

## 🔒 Security Considerations

### ✅ Điểm mạnh:
- OTP hết hạn sau 5 phút
- OTP chỉ sử dụng được 1 lần
- Tài khoản không thể đăng nhập trước khi xác thực
- Password được hash bằng PasswordHasher

### ⚠️ Khuyến nghị thêm:
1. **Rate limiting**: Giới hạn số lần gửi OTP (VD: 3 lần/15 phút)
2. **Brute force protection**: Giới hạn số lần nhập sai OTP
3. **OTP cleanup**: Xóa OTP cũ sau khi hết hạn
4. **Log audit**: Ghi log các hoạt động OTP

---

## 📱 Tích hợp vào Flutter App

Xem file riêng: `FLUTTER_OTP_INTEGRATION.md`

---

**Tác giả:** GitHub Copilot  
**Ngày cập nhật:** 13/10/2025  
**Version:** 1.0
