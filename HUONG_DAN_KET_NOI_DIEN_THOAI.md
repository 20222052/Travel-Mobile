# 📱 HƯỚNG DẪN KẾT NỐI ĐIỆN THOẠI ĐẾN API TRÊN MÁY TÍNH

## 🎯 Tổng quan
Để điện thoại thật có thể kết nối đến ASP.NET Core API trên máy tính, bạn cần:
1. ✅ Cùng mạng WiFi
2. ✅ Biết địa chỉ IP máy tính
3. ✅ Mở Firewall cho port 5014
4. ✅ Cấu hình đúng IP trong Flutter app

---

## 📋 BƯỚC 1: Kiểm tra địa chỉ IP máy tính

Mở **PowerShell** và chạy:
```powershell
ipconfig | Select-String "IPv4"
```

Kết quả trên máy bạn:
```
IPv4 Address. . . . . . . . . . . : 192.168.137.1
IPv4 Address. . . . . . . . . . . : 192.168.76.1
IPv4 Address. . . . . . . . . . . : 172.29.101.76
IPv4 Address. . . . . . . . . . . : 172.18.208.1
```

**Chọn IP nào?**
- Nếu điện thoại và máy tính cùng kết nối WiFi nhà/công ty: thường là `192.168.x.x`
- Nếu dùng Mobile Hotspot từ điện thoại: thường là `192.168.137.1`
- Nếu dùng VMware/Docker: thường là `172.x.x.x`

**💡 Cách kiểm tra:** 
1. Vào Settings trên điện thoại → WiFi → Xem thông tin IP điện thoại
2. Nếu điện thoại có IP dạng `192.168.137.x` → Máy tính dùng `192.168.137.1`
3. Nếu điện thoại có IP dạng `192.168.76.x` → Máy tính dùng `192.168.76.1`

---

## 📋 BƯỚC 2: Đảm bảo cùng mạng WiFi

✅ Kết nối cả máy tính và điện thoại vào **CÙNG 1 mạng WiFi**

**Lưu ý:**
- Nếu máy tính dùng dây LAN, hãy bật WiFi và kết nối cùng mạng với điện thoại
- Hoặc bật Mobile Hotspot từ điện thoại và kết nối máy tính vào

---

## 📋 BƯỚC 3: Mở Windows Firewall cho port 5014

### Cách 1: Dùng giao diện (Khuyến nghị)

1. Nhấn `Windows + R` → gõ `wf.msc` → Enter
2. Click **"Inbound Rules"** → **"New Rule..."**
3. Chọn **"Port"** → Next
4. Chọn **"TCP"**, nhập `5014` → Next
5. Chọn **"Allow the connection"** → Next
6. Chọn tất cả các profile (Domain, Private, Public) → Next
7. Đặt tên: **"ASP.NET Core API Port 5014"** → Finish

### Cách 2: Dùng PowerShell (Cần quyền Administrator)

Mở PowerShell **với quyền Admin** và chạy:
```powershell
New-NetFirewallRule -DisplayName "ASP.NET Core API" -Direction Inbound -LocalPort 5014 -Protocol TCP -Action Allow
```

### Kiểm tra Firewall đã mở chưa:
```powershell
Get-NetFirewallRule -DisplayName "ASP.NET Core API*"
```

---

## 📋 BƯỚC 4: Khởi động ASP.NET Core API

Di chuyển đến thư mục project:
```powershell
cd D:\BACHKHOAAPTECH\DOAN_KI4\app\project\project
```

Chạy API:
```powershell
dotnet run --launch-profile http
```

**Chờ đến khi thấy:**
```
Now listening on: http://0.0.0.0:5014
```

✅ API đã sẵn sàng nhận kết nối từ mọi địa chỉ IP!

---

## 📋 BƯỚC 5: Cấu hình địa chỉ IP trong Flutter App

Mở file `lib/config/api_config.dart` và sửa:

```dart
// CHẾ ĐỘ EMULATOR (Android Emulator)
// static const String baseUrl = 'http://10.0.2.2:5014';

// CHẾ ĐỘ ĐIỆN THOẠI THẬT - Chọn IP phù hợp:
static const String baseUrl = 'http://192.168.137.1:5014';  // Thử cái này trước
// static const String baseUrl = 'http://192.168.76.1:5014';
// static const String baseUrl = 'http://172.29.101.76:5014';
```

**Lưu ý:** Thay `192.168.137.1` bằng địa chỉ IP thật của máy tính bạn!

---

## 📋 BƯỚC 6: Build và chạy Flutter App trên điện thoại

1. Kết nối điện thoại với máy tính qua USB
2. Bật **USB Debugging** trên điện thoại (Developer Options)
3. Chạy lệnh:

```powershell
cd D:\BACHKHOAAPTECH\DOAN_KI4\app\androi_app
flutter run
```

Hoặc nhấn **F5** trong VS Code để chạy.

---

## 🧪 KIỂM TRA KẾT NỐI

### Test 1: Ping máy tính từ điện thoại

1. Tải app **"Ping & Net"** hoặc **"Network Utilities"** trên điện thoại
2. Ping địa chỉ IP máy tính (vd: `192.168.137.1`)
3. Nếu **ping thành công** → Mạng OK ✅

### Test 2: Truy cập API từ trình duyệt điện thoại

Mở trình duyệt trên điện thoại, truy cập:
```
http://192.168.137.1:5014/api/ApiTour
```

Nếu thấy dữ liệu JSON → API hoạt động ✅

### Test 3: Chạy app Flutter

Mở app → Đăng nhập → Xem danh sách tour

Nếu hiển thị dữ liệu → Thành công! 🎉

---

## ⚠️ XỬ LÝ SỰ CỐ

### Lỗi: "Connection refused" hoặc "Network error"

**Nguyên nhân:** Không kết nối được API

**Giải pháp:**
1. ✅ Kiểm tra API đang chạy (`dotnet run`)
2. ✅ Kiểm tra Firewall đã mở port 5014
3. ✅ Kiểm tra cùng mạng WiFi
4. ✅ Thử các IP khác trong file `api_config.dart`

### Lỗi: "SocketException: OS Error: Connection timed out"

**Nguyên nhân:** Firewall chặn hoặc IP sai

**Giải pháp:**
1. Tắt Firewall tạm thời để test (Settings → Windows Security → Firewall)
2. Thử IP khác
3. Dùng Mobile Hotspot từ điện thoại

### Lỗi: "Failed to load image" (hình ảnh không hiển thị)

**Nguyên nhân:** Đường dẫn uploads sai

**Giải pháp:**
- Tất cả file đã được cập nhật dùng `ApiConfig.uploadsPath`
- Kiểm tra file `api_config.dart` đã đúng IP chưa

---

## 🎯 CHECKLIST NHANH

Trước khi chạy app trên điện thoại, kiểm tra:

- [ ] Máy tính và điện thoại cùng mạng WiFi
- [ ] Đã lấy địa chỉ IP máy tính (`ipconfig`)
- [ ] Đã mở Firewall cho port 5014
- [ ] Đã sửa IP trong `lib/config/api_config.dart`
- [ ] Đã chạy API (`dotnet run --launch-profile http`)
- [ ] Đã test API từ trình duyệt điện thoại
- [ ] Đã build lại Flutter app (`flutter run`)

---

## 💡 MẸO HAY

### Chuyển đổi giữa Emulator và điện thoại thật

Trong file `lib/config/api_config.dart`, chỉ cần comment/uncomment:

```dart
// Dùng Emulator:
// static const String baseUrl = 'http://10.0.2.2:5014';

// Dùng điện thoại thật:
static const String baseUrl = 'http://192.168.137.1:5014';
```

### Dùng Mobile Hotspot (Đơn giản nhất!)

1. Bật Mobile Hotspot trên điện thoại
2. Kết nối máy tính vào hotspot đó
3. Máy tính sẽ nhận IP `192.168.137.1` (hoặc tương tự)
4. Điện thoại sẽ là `192.168.137.x`
5. Dùng IP `192.168.137.1` trong Flutter app

### Kiểm tra log để debug

Xem log của app:
```powershell
flutter logs
```

Xem log của API:
- Xem trong terminal đang chạy `dotnet run`
- Tìm dòng `HTTP GET /api/ApiTour` để biết có request từ app không

---

## 📞 HỖ TRỢ

Nếu vẫn gặp vấn đề:
1. Chụp màn hình log của API (terminal)
2. Chụp màn hình log của Flutter app
3. Chụp màn hình kết quả `ipconfig`
4. Gửi cho tôi để debug tiếp!

---

**Chúc bạn thành công! 🚀**
