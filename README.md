# 🌍 Travel Mobile

Ứng dụng du lịch full-stack bao gồm **Backend API (.NET 8)**, **Frontend Web (React + Vite)** và **Mobile App (Flutter)**.

---

## 📋 Mục lục

- [Tổng quan](#tổng-quan)
- [Kiến trúc hệ thống](#kiến-trúc-hệ-thống)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt & Chạy dự án](#cài-đặt--chạy-dự-án)
  - [1. Backend (ASP.NET Core)](#1-backend-aspnet-core)
  - [2. Frontend Web (React)](#2-frontend-web-react)
  - [3. Mobile App (Flutter)](#3-mobile-app-flutter)
- [Cấu trúc thư mục](#cấu-trúc-thư-mục)
- [Tính năng](#tính-năng)
- [API Endpoints](#api-endpoints)

---

## Tổng quan

**Travel Mobile** là một nền tảng đặt tour du lịch trực tuyến, cho phép người dùng tìm kiếm, xem chi tiết và đặt các tour du lịch. Hệ thống gồm 3 thành phần chính:

- **Backend API**: Xử lý nghiệp vụ, quản lý dữ liệu, xác thực người dùng và gửi email OTP.
- **Frontend Web**: Giao diện quản trị (Admin) để quản lý tour, danh mục, người dùng, đơn hàng và blog.
- **Mobile App**: Ứng dụng di động cho người dùng cuối để duyệt, đặt tour và quản lý đơn hàng.

---

## Kiến trúc hệ thống

```
┌─────────────────┐     HTTP/REST     ┌──────────────────────┐
│   Flutter App   │ ◄───────────────► │  ASP.NET Core API    │
│   (Android/iOS) │                   │  (localhost:5014)    │
└─────────────────┘                   └──────────┬───────────┘
                                                 │
┌─────────────────┐     HTTP/REST               │
│  React Web App  │ ◄───────────────►            │
│ (localhost:5173)│                   ┌──────────▼───────────┐
└─────────────────┘                   │   SQL Server (MSSQL) │
                                      │   Database: Travel   │
                                      └──────────────────────┘
```

---

## Công nghệ sử dụng

| Thành phần    | Công nghệ                                         |
|---------------|---------------------------------------------------|
| Backend       | ASP.NET Core 8, Entity Framework Core 9, MailKit  |
| Database      | Microsoft SQL Server                              |
| Frontend Web  | React 19, Vite 7, Bootstrap 5, React Router DOM   |
| Mobile App    | Flutter 3, Dart, go_router, http, image_picker    |

---

## Yêu cầu hệ thống

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB hoặc Express)
- [Node.js 18+](https://nodejs.org/) và npm
- [Flutter SDK 3.x+](https://flutter.dev/docs/get-started/install)
- Android Studio / VS Code (cho phát triển mobile)

---

## Cài đặt & Chạy dự án

### 1. Backend (ASP.NET Core)

#### Thiết lập Database

**Cách 1:** Dùng file SQL có sẵn:
```
Mở SQL Server Management Studio (SSMS), kết nối tới server của bạn.
Chạy file project/DB.sql để tạo schema.
Chạy file project/SampleData.sql để thêm dữ liệu mẫu.
```

**Cách 2:** Dùng Entity Framework Migrations (Visual Studio):
```
# Mở Package Manager Console trong Visual Studio
Update-Database

# Nếu lệnh trên lỗi, chạy:
Add-Migration InitialCreate
Update-Database
```

#### Cấu hình kết nối

Mở file `project/project/appsettings.json` và chỉnh chuỗi kết nối phù hợp với SQL Server của bạn:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\MSSQLSERVER01;Database=Travel;User Id=sa;Password=<password>;MultipleActiveResultSets=True;TrustServerCertificate=True"
}
```

#### Khởi động Backend

```bash
# Cách 1: Dùng file batch (Windows)
START_BACKEND.bat

# Cách 2: Dùng CLI
cd project/project
dotnet run
```

API sẽ chạy tại:
- HTTP: `http://localhost:5014`
- HTTPS: `https://localhost:7078`

---

### 2. Frontend Web (React)

```bash
cd front_end

# Cài đặt dependencies
npm install

# Cấu hình API URL (tạo hoặc chỉnh file .env)
# VITE_API_URL=http://localhost:5014

# Khởi động development server
npm run dev
```

Ứng dụng web sẽ chạy tại: `http://localhost:5173`

Để build production:
```bash
npm run build
```

---

### 3. Mobile App (Flutter)

#### Cấu hình API

Mở file `androi_app/lib/config/api_config.dart` và chọn chế độ phù hợp:

```dart
// Dùng Android Emulator
static const String baseUrl = 'http://10.0.2.2:5014';

// Dùng điện thoại thật (thay IP bằng IP máy tính của bạn)
// static const String baseUrl = 'http://192.168.x.x:5014';
```

> **Lưu ý:** Khi dùng điện thoại thật, máy tính và điện thoại phải cùng mạng WiFi. Dùng `ipconfig` (Windows) hoặc `ifconfig` (macOS/Linux) để tìm IP máy tính. Đảm bảo Firewall cho phép kết nối port 5014.

#### Khởi động ứng dụng

```bash
cd androi_app

# Cài đặt dependencies
flutter pub get

# Chạy ứng dụng (kết nối thiết bị hoặc mở emulator trước)
flutter run
```

---

## Cấu trúc thư mục

```
Travel-Mobile/
├── project/                    # Backend ASP.NET Core
│   ├── project/
│   │   ├── Controllers/        # MVC & API Controllers
│   │   │   └── Api/            # REST API endpoints
│   │   ├── Models/             # Entity models (Tour, Order, User, ...)
│   │   ├── Services/           # Business logic services
│   │   ├── Data/               # DbContext, seeding
│   │   ├── Migrations/         # EF Core migrations
│   │   ├── Areas/              # Admin area (MVC)
│   │   ├── appsettings.json    # Cấu hình kết nối DB, email
│   │   └── Program.cs          # Entry point
│   ├── DB.sql                  # Script tạo database
│   └── SampleData.sql          # Dữ liệu mẫu
│
├── front_end/                  # Admin Web (React + Vite)
│   ├── src/
│   │   ├── pages/              # Các trang (Admin, Auth, Home)
│   │   ├── components/         # Reusable components
│   │   ├── services/           # API call services
│   │   ├── routes/             # React Router configuration
│   │   └── contexts/           # React Context (auth, ...)
│   ├── .env                    # Biến môi trường (API URL)
│   └── package.json
│
├── androi_app/                 # Mobile App (Flutter)
│   ├── lib/
│   │   ├── screens/            # Các màn hình (home, login, tour, ...)
│   │   ├── models/             # Data models
│   │   ├── services/           # HTTP services
│   │   ├── widgets/            # Reusable widgets
│   │   ├── config/             # API configuration
│   │   └── main.dart           # Entry point
│   └── pubspec.yaml
│
├── Travel_API_Postman_Collection.json  # Postman collection để test API
└── START_BACKEND.bat           # Script khởi động backend (Windows)
```

---

## Tính năng

### 👤 Người dùng (Mobile App)
- Đăng ký, đăng nhập, xác thực OTP qua email
- Xem danh sách và chi tiết tour du lịch
- Tìm kiếm tour theo danh mục
- Thêm tour vào giỏ hàng và đặt tour
- Xem lịch sử đơn hàng
- Quản lý thông tin cá nhân và ảnh đại diện
- Đọc blog du lịch

### 🛠️ Quản trị viên (Frontend Web)
- Dashboard thống kê doanh thu và đơn hàng
- Quản lý tour (thêm, sửa, xóa, upload ảnh)
- Quản lý danh mục tour
- Quản lý người dùng
- Quản lý đơn hàng
- Quản lý blog
- Xem và xử lý liên hệ từ khách hàng

---

## API Endpoints

File `Travel_API_Postman_Collection.json` ở thư mục gốc chứa toàn bộ các API endpoint. Import vào [Postman](https://www.postman.com/) để test.

Một số endpoint chính:

| Phương thức | Endpoint                  | Mô tả                    |
|-------------|---------------------------|--------------------------|
| POST        | `/api/account/register`   | Đăng ký tài khoản        |
| POST        | `/api/account/login`      | Đăng nhập                |
| POST        | `/api/account/verify-otp` | Xác thực OTP             |
| GET         | `/api/tour`               | Lấy danh sách tour       |
| GET         | `/api/tour/{id}`          | Lấy chi tiết tour        |
| GET         | `/api/category`           | Lấy danh sách danh mục   |
| GET         | `/api/cart`               | Lấy giỏ hàng             |
| POST        | `/api/cart/add`           | Thêm vào giỏ hàng        |
| POST        | `/api/order`              | Tạo đơn hàng             |
| GET         | `/api/order/history`      | Lịch sử đơn hàng         |
| GET         | `/api/blog`               | Lấy danh sách blog       |
