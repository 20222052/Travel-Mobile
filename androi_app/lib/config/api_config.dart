// lib/config/api_config.dart
class ApiConfig {
  // 🔹 ĐỂ CHẠY TRÊN EMULATOR: dùng 10.0.2.2
  // 🔹 ĐỂ CHẠY TRÊN ĐIỆN THOẠI THẬT: thay bằng địa chỉ IP máy tính của bạn
  
  // Các IP khả dụng từ máy tính của bạn:
  // - 192.168.137.1
  // - 192.168.76.1
  // - 172.29.101.76
  // - 172.18.208.1
  
  // HƯỚNG DẪN:
  // 1. Kiểm tra điện thoại và máy tính cùng mạng WiFi
  // 2. Dùng ipconfig để tìm IP máy tính (thường là 192.168.x.x)
  // 3. Thay đổi baseUrl bên dưới
  // 4. Đảm bảo Firewall cho phép kết nối port 5014
  
  // CHẾ ĐỘ EMULATOR (Android Emulator) - Uncomment dòng này nếu dùng Emulator
  // static const String baseUrl = 'http://10.0.2.2:5014';
  
  // CHẾ ĐỘ ĐIỆN THOẠI THẬT - ✅ Đang dùng IP này
  // Điện thoại kết nối WiFi và test thành công từ trình duyệt
  static const String baseUrl = 'http://172.29.101.76:5014';  // WiFi chính
  
  // Nếu dùng Mobile Hotspot từ điện thoại:
  // static const String baseUrl = 'http://192.168.137.1:5014';
  
  // Các IP khác để thử:
  // static const String baseUrl = 'http://192.168.76.1:5014';
  
  static const String uploadsPath = '$baseUrl/Uploads';
  
  static bool get isEmulator => baseUrl.contains('10.0.2.2');
}
