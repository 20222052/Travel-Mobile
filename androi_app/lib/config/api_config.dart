// lib/config/api_config.dart
class ApiConfig {
  // HƯỚNG DẪN:
  // 1. Kiểm tra điện thoại và máy tính cùng mạng WiFi
  // 2. Dùng ipconfig để tìm IP máy tính (thường là 192.168.x.x)
  // 3. Thay đổi baseUrl bên dưới
  // 4. Đảm bảo Firewall cho phép kết nối port 5014
  
  // CHẾ ĐỘ EMULATOR (Android Emulator) - Uncomment dòng này nếu dùng Emulator
  // static const String baseUrl = 'http://10.0.2.2:5014';
  
  // CHẾ ĐỘ ĐIỆN THOẠI THẬT - ✅ Đang dùng IP này
  // IP mới: 192.168.100.117 (Wi-Fi) - Cập nhật ngày 17/10/2025
  static const String baseUrl ='http://10.142.16.38:5014';


  
  static const String uploadsPath = '$baseUrl/Uploads';
  
  static bool get isEmulator => baseUrl.contains('10.0.2.2');
}
