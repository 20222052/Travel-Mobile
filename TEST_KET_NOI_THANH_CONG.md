# 🎯 KIỂM TRA NHANH KẾT NỐI API

## ✅ Bạn đã làm đúng!

**Trình duyệt điện thoại truy cập được:** `http://172.29.101.76:5014/api/ApiTour`

→ Điều này chứng minh:
- ✅ Kết nối mạng OK
- ✅ API đang chạy
- ✅ Firewall đã mở (hoặc không chặn)
- ✅ IP chính xác

## 🔧 ĐÃ SỬA

Tôi đã thay đổi IP trong file `lib/config/api_config.dart` từ:
```dart
// CŨ (cho Emulator):
static const String baseUrl = 'http://10.0.2.2:5014';
```

Thành:
```dart
// MỚI (cho điện thoại thật):
static const String baseUrl = 'http://172.29.101.76:5014';
```

## 📱 APP ĐÃ CHẠY THÀNH CÔNG!

App đã được build và cài đặt trên điện thoại `23021RAAEG`.

## 🧪 CÁCH KIỂM TRA KẾT NỐI TRONG APP

### Cách 1: Dùng Debug Button (Đã thêm sẵn)

Trong app, bạn sẽ thấy một **nút tròn màu cam** ở góc dưới bên phải màn hình Home.

1. Nhấn vào nút tròn màu cam 🟠
2. Sẽ hiện dialog test kết nối
3. Xem kết quả:
   - ✅ **Connected** → API hoạt động tốt
   - ❌ **Failed** → Có lỗi, xem chi tiết và suggestion

### Cách 2: Thử đăng nhập / Load tour

1. Mở app
2. Thử đăng nhập hoặc xem danh sách tour
3. Nếu hiển thị dữ liệu → Thành công! 🎉
4. Nếu lỗi timeout → Xem log chi tiết

## 📊 CHECKLIST SAU KHI SỬA

- [x] IP đã đổi thành `172.29.101.76`
- [x] App đã rebuild và cài đặt lại
- [x] Trình duyệt test thành công
- [ ] **BẠN CẦN THỬ:** Mở app và test tính năng

## ⚠️ NẾU VẪN LỖI

### Lỗi: "No internet connection" hoặc "Network error"

**Khả năng:** App chưa rebuild hoàn toàn

**Giải pháp:**
```powershell
# Xóa cache và rebuild
cd androi_app
flutter clean
flutter pub get
flutter run -d 23021RAAEG
```

### Lỗi: Timeout sau 20 giây

**Khả năng:** App vẫn dùng code cũ (IP 10.0.2.2)

**Giải pháp:**
1. Tắt app hoàn toàn trên điện thoại (không chỉ thoát)
2. Mở lại từ launcher
3. Hoặc rebuild: `flutter run -d 23021RAAEG`

### Lỗi: Hình ảnh không hiển thị

**Nguyên nhân:** Đường dẫn uploads

**Đã sửa:** Tất cả file đã dùng `ApiConfig.uploadsPath`

---

## 🎉 THÀNH CÔNG!

Nếu app hiển thị dữ liệu tour, bạn đã kết nối thành công!

**Debug button màu cam** sẽ giúp bạn test kết nối bất cứ lúc nào.

---

## 💡 MẸO

### Khi đổi mạng WiFi

Nếu bạn kết nối mạng WiFi khác:
1. Chạy `ipconfig` trên máy tính để lấy IP mới
2. Sửa IP trong `lib/config/api_config.dart`
3. Rebuild app: `flutter run -d 23021RAAEG`

### Khi chuyển sang Emulator

Nếu muốn test trên Emulator:
1. Mở `lib/config/api_config.dart`
2. Đổi IP thành `10.0.2.2`:
   ```dart
   static const String baseUrl = 'http://10.0.2.2:5014';
   ```
3. Rebuild: `flutter run -d emulator-5554`

---

**Hãy thử mở app và cho tôi biết kết quả! 🚀**
