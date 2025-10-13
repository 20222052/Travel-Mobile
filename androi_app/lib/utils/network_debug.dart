// lib/utils/network_debug.dart
import 'dart:async';
import 'dart:io';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';

class NetworkDebug {
  /// Test kết nối đến API server
  static Future<Map<String, dynamic>> testConnection() async {
    final result = <String, dynamic>{
      'baseUrl': ApiConfig.baseUrl,
      'isEmulator': ApiConfig.isEmulator,
      'timestamp': DateTime.now().toIso8601String(),
    };

    try {
      print('🔍 Testing connection to: ${ApiConfig.baseUrl}');
      
      // Test 1: Ping server
      final pingStart = DateTime.now();
      final response = await http
          .get(Uri.parse('${ApiConfig.baseUrl}/api/ApiTour'))
          .timeout(const Duration(seconds: 10));
      final pingEnd = DateTime.now();
      
      result['success'] = true;
      result['statusCode'] = response.statusCode;
      result['responseTime'] = '${pingEnd.difference(pingStart).inMilliseconds}ms';
      result['contentLength'] = response.body.length;
      
      print('✅ Connection successful!');
      print('   Status: ${response.statusCode}');
      print('   Response time: ${result['responseTime']}');
      print('   Content length: ${result['contentLength']} bytes');
      
    } on SocketException catch (e) {
      result['success'] = false;
      result['error'] = 'SocketException: ${e.message}';
      result['suggestion'] = _getSuggestionForSocketError(e);
      
      print('❌ Connection failed: SocketException');
      print('   Error: ${e.message}');
      print('   💡 ${result['suggestion']}');
      
    } on TimeoutException catch (e) {
      result['success'] = false;
      result['error'] = 'TimeoutException: ${e.message}';
      result['suggestion'] = 'API không phản hồi. Kiểm tra:\n'
          '  1. API đang chạy? (dotnet run)\n'
          '  2. Firewall đã mở port 5014?\n'
          '  3. IP trong api_config.dart đúng chưa?';
      
      print('❌ Connection timeout after 10 seconds');
      print('   💡 ${result['suggestion']}');
      
    } catch (e) {
      result['success'] = false;
      result['error'] = e.toString();
      result['suggestion'] = 'Lỗi không xác định. Kiểm tra log chi tiết.';
      
      print('❌ Unexpected error: $e');
    }

    return result;
  }

  static String _getSuggestionForSocketError(SocketException e) {
    final msg = e.message.toLowerCase();
    
    if (msg.contains('connection refused') || msg.contains('econnrefused')) {
      return 'API không chạy hoặc sai port. Kiểm tra:\n'
          '  1. Chạy: cd project/project && dotnet run\n'
          '  2. Port phải là 5014';
    }
    
    if (msg.contains('network is unreachable') || msg.contains('no route to host')) {
      return 'Không thể kết nối đến máy chủ. Kiểm tra:\n'
          '  1. Máy tính và điện thoại cùng WiFi?\n'
          '  2. IP trong api_config.dart đúng chưa?\n'
          '  3. Nếu dùng Emulator: dùng 10.0.2.2\n'
          '  4. Nếu dùng điện thoại thật: dùng IP WiFi của máy tính';
    }
    
    if (msg.contains('failed host lookup') || msg.contains('host')) {
      return 'Không thể phân giải hostname. IP có thể sai.';
    }
    
    return 'Lỗi kết nối. Kiểm tra:\n'
        '  1. IP trong api_config.dart\n'
        '  2. Firewall của Windows\n'
        '  3. API đang chạy';
  }

  /// In ra thông tin cấu hình hiện tại
  static void printConfig() {
    print('📱 ===== NETWORK CONFIG =====');
    print('   Base URL: ${ApiConfig.baseUrl}');
    print('   Uploads: ${ApiConfig.uploadsPath}');
    print('   Is Emulator: ${ApiConfig.isEmulator}');
    print('   Platform: ${Platform.operatingSystem}');
    print('=============================');
  }
}
