# 🔧 KHẮC PHỤC LỖI TIMEOUTEXCEPTION

## ❌ Lỗi gặp phải:
```
TimeoutException after 0:00:20.000000: Future not completed
```

Lỗi này có nghĩa: **App không thể kết nối đến API trong vòng 20 giây**

---

## ✅ GIẢI PHÁP THEO THỨ TỰ ƯU TIÊN

### 🎯 **Bước 1: XÁC ĐỊNH BẠN ĐANG DÙNG THIẾT BỊ GÌ**

Chạy lệnh để xem danh sách thiết bị:
```powershell
cd androi_app
flutter devices
```

**Kết quả:**
- Nếu thấy `emulator-5554` → Bạn đang dùng **EMULATOR**
- Nếu thấy `23021RAAEG` (hoặc số khác) → Bạn đang dùng **ĐIỆN THOẠI THẬT**

---

### 🎯 **Bước 2: CẤU HÌNH IP ĐÚNG**

Mở file: `lib/config/api_config.dart`

#### ✅ **Nếu dùng EMULATOR:**
```dart
static const String baseUrl = 'http://10.0.2.2:5014';
```

#### ✅ **Nếu dùng ĐIỆN THOẠI THẬT:**

Trước tiên, lấy IP máy tính:
```powershell
ipconfig | Select-String "IPv4|Wireless"
```

Kết quả trên máy bạn:
```
Wireless LAN adapter Wi-Fi:
   IPv4 Address. . . . . . . . . . . : 172.29.101.76  ← Dùng IP này!
```

Trong `lib/config/api_config.dart`:
```dart
// Uncomment dòng này và thay IP
static const String baseUrl = 'http://172.29.101.76:5014';
```

**Lưu ý:** Điện thoại và máy tính PHẢI cùng mạng WiFi!

---

### 🎯 **Bước 3: KIỂM TRA API ĐANG CHẠY**

Kiểm tra port 5014:
```powershell
netstat -ano | Select-String ":5014"
```

**Nếu không thấy gì** → API chưa chạy! Chạy API:
```powershell
cd D:\BACHKHOAAPTECH\DOAN_KI4\app\project\project
dotnet run --launch-profile http
```

Đợi đến khi thấy:
```
Now listening on: http://0.0.0.0:5014
```

---

### 🎯 **Bước 4: MỞ FIREWALL**

**Cách 1: Giao diện (Dễ nhất)**

1. Nhấn `Windows + R` → gõ `wf.msc` → Enter
2. Click **"Inbound Rules"** → **"New Rule..."**
3. Chọn **"Port"** → Next
4. Chọn **"TCP"**, nhập `5014` → Next
5. Chọn **"Allow the connection"** → Next
6. Chọn tất cả (Domain, Private, Public) → Next
7. Đặt tên: **"ASP.NET Core API"** → Finish

**Cách 2: Tắt tạm Firewall để test (Không khuyến nghị)**

Settings → Windows Security → Firewall & network protection → Turn off

---

### 🎯 **Bước 5: TEST KẾT NỐI**

#### Test 1: Từ máy tính
```powershell
curl http://localhost:5014/api/ApiTour
```

Nếu thấy dữ liệu JSON → API OK ✅

#### Test 2: Từ điện thoại

**A. Dùng trình duyệt điện thoại:**

Mở Chrome trên điện thoại, truy cập:
```
http://172.29.101.76:5014/api/ApiTour
```
(Thay `172.29.101.76` bằng IP máy tính của bạn)

Nếu thấy dữ liệu JSON → Kết nối OK ✅

**B. Dùng Network Test Screen trong app:**

1. Chạy app Flutter
2. Thêm nút test vào màn hình chính hoặc chạy:
```dart
Navigator.push(
  context,
  MaterialPageRoute(builder: (_) => NetworkTestScreen()),
);
```

---

### 🎯 **Bước 6: REBUILD VÀ CHẠY LẠI APP**

Sau khi đã:
- ✅ Sửa IP trong `api_config.dart`
- ✅ API đang chạy
- ✅ Mở Firewall

Rebuild app:
```powershell
cd androi_app

# Nếu dùng Emulator:
flutter run -d emulator-5554

# Nếu dùng điện thoại thật:
flutter run -d 23021RAAEG
```

Hoặc trong VS Code: Nhấn **Shift + F5** (Stop) → **F5** (Run lại)

---

## 🐛 **XỬ LÝ SỰ CỐ NÂNG CAO**

### ❌ Lỗi: "SocketException: Connection refused"

**Nguyên nhân:** API không chạy hoặc sai port

**Giải pháp:**
1. Kiểm tra API: `netstat -ano | Select-String ":5014"`
2. Chạy API: `dotnet run --launch-profile http`

---

### ❌ Lỗi: "SocketException: Network is unreachable"

**Nguyên nhân:** Không cùng mạng WiFi hoặc IP sai

**Giải pháp:**
1. Kiểm tra điện thoại Settings → WiFi → Tên mạng
2. Kiểm tra máy tính Settings → Network → WiFi → Tên mạng
3. Đảm bảo CÙNG TÊN MẠNG
4. Thử các IP khác trong `ipconfig`

---

### ❌ Lỗi: "TimeoutException" (vẫn timeout sau khi làm đủ)

**Giải pháp cuối cùng: Dùng Mobile Hotspot**

1. **Trên điện thoại:**
   - Settings → Network → Hotspot → Turn ON
   - Đặt tên & mật khẩu (ví dụ: `MyPhone`)

2. **Trên máy tính:**
   - Settings → WiFi → Kết nối vào `MyPhone`
   - Chạy: `ipconfig | Select-String "IPv4"`
   - Sẽ thấy IP dạng: `192.168.137.1` hoặc `192.168.43.1`

3. **Trong app:**
   ```dart
   static const String baseUrl = 'http://192.168.137.1:5014';
   ```

4. Rebuild app: `flutter run`

✅ Cách này **CHẮC CHẮN THÀNH CÔNG** vì điện thoại và máy tính trực tiếp kết nối với nhau!

---

## 📊 **CHECKLIST NHANH**

Trước khi chạy app, kiểm tra:

```
[ ] API đang chạy (dotnet run)
[ ] Port 5014 đang listen (netstat -ano | Select-String ":5014")
[ ] Firewall đã mở port 5014 (wf.msc)
[ ] Cùng mạng WiFi (hoặc dùng Mobile Hotspot)
[ ] IP trong api_config.dart đúng (ipconfig)
[ ] Đã rebuild app (flutter run)
[ ] Test từ trình duyệt điện thoại (http://IP:5014/api/ApiTour)
```

---

## 💡 **MẸO DEBUG**

### Xem log chi tiết:

**Terminal 1 (API):**
```powershell
cd project/project
dotnet run --launch-profile http
```
Xem log: Mỗi request từ app sẽ hiện dòng `HTTP GET /api/...`

**Terminal 2 (Flutter):**
```powershell
cd androi_app
flutter logs
```
Xem log: Mọi lỗi network sẽ hiện ở đây

---

## 🎯 **CÔNG THỨC THÀNH CÔNG 100%**

```
EMULATOR:
  → IP = 10.0.2.2

ĐIỆN THOẠI THẬT + CÙNG WIFI:
  → IP = WiFi IP máy tính (172.29.101.76)

ĐIỆN THOẠI THẬT + MOBILE HOTSPOT:
  → IP = 192.168.137.1 hoặc 192.168.43.1
```

**Nếu vẫn lỗi sau khi làm đủ → Gửi cho tôi:**
1. Screenshot `ipconfig`
2. Screenshot `netstat -ano | Select-String ":5014"`
3. Screenshot lỗi trong app
4. Tên mạng WiFi điện thoại đang kết nối

---

**Chúc bạn thành công! 🚀**
