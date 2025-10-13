// lib/services/app_client.dart
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:http/io_client.dart';
import '../config/api_config.dart';

class ApiClient {
  ApiClient._internal() {
    client = _createClient();
  }
  static final ApiClient instance = ApiClient._internal();

  // Sử dụng ApiConfig để dễ dàng thay đổi giữa Emulator và điện thoại thật
  static String get base => ApiConfig.baseUrl;

  /// DEV: chấp nhận chứng chỉ tự ký (không cần thiết khi dùng HTTP)
  static const bool allowBadCert = true;

  late http.Client client;

  String? cookieHeader; // ví dụ: "user.auth=abcdef..."

  http.Client _createClient() {
    if (!allowBadCert) return http.Client();

    final httpClient = HttpClient()
      ..badCertificateCallback = (X509Certificate cert, String host, int port) => true;
    return IOClient(httpClient);
  }

  /// Lưu cookie từ header Set-Cookie của response /login
  void saveCookieFromResponseHeaders(Map<String, String> headers) {
    final setCookie = headers['set-cookie'];
    if (setCookie == null || setCookie.isEmpty) return;
    try {
      // Parse 1 cookie từ Set-Cookie
      final c = Cookie.fromSetCookieValue(setCookie);
      cookieHeader = '${c.name}=${c.value}';
    } catch (_) {
      // Fallback: lấy phần trước dấu ';'
      cookieHeader = setCookie.split(';').first.trim();
    }
  }

  /// Build absolute Uri từ path (vd: /api/ApiTour) + query
  Uri uri(String path, [Map<String, String>? query]) {
    final u = Uri.parse('$base$path');
    return (query == null || query.isEmpty) ? u : u.replace(queryParameters: query);
  }

  /// Header JSON mặc định
  Map<String, String> get jsonHeaders => const {
    'Accept': 'application/json',
    'Content-Type': 'application/json; charset=utf-8',
  };

  /// Header JSON + Cookie (cho các GET/POST/PUT/DELETE thường)
  Map<String, String> authJsonHeaders() {
    final h = Map<String, String>.from(jsonHeaders);
    if (cookieHeader != null) h['cookie'] = cookieHeader!;
    return h;
  }

  static String resolveUploads(String? relativeOrAbsolute) {
    if (relativeOrAbsolute == null || relativeOrAbsolute.isEmpty) return '';
    final v = relativeOrAbsolute;
    if (v.startsWith('http://') || v.startsWith('https://')) return v;
    if (v.startsWith('/')) return '$base$v';
    return '$base/$v';
  }
}
