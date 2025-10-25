import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;

import 'api_client.dart';
import '../models/user.dart';

class EditProfilePayload {
  String? username;
  String? role;

  String? name, gender, address, phone, email;
  DateTime? dateOfBirth;
  File? imageFile;

  EditProfilePayload({
    this.username,
    this.role,
    this.name,
    this.gender,
    this.address,
    this.phone,
    this.email,
    this.dateOfBirth,
    this.imageFile,
  });

  Map<String, String> toFieldsWithoutDob() => {
    if (username != null) 'Username': username!,
    if (role != null) 'Role': role!,
    if (name != null) 'Name': name!,
    if (gender != null) 'Gender': gender!,
    if (address != null) 'Address': address!,
    if (phone != null) 'Phone': phone!,
    if (email != null) 'Email': email!,
  };
}

class RegisterPayload {
  final String name;
  final String gender;
  final String address;
  final String phone;
  final String email;
  final String username;
  final String password;
  final DateTime dateOfBirth;

  RegisterPayload({
    required this.name,
    required this.gender,
    required this.address,
    required this.phone,
    required this.email,
    required this.username,
    required this.password,
    required this.dateOfBirth,
  });

  Map<String, dynamic> toJson() {
    // Format DateTime về UTC để tránh vấn đề múi giờ
    final formattedDate = DateTime.utc(
      dateOfBirth.year,
      dateOfBirth.month,
      dateOfBirth.day,
    ).toIso8601String();

    return {
      'name': name,
      'gender': gender,
      'address': address.isEmpty ? null : address,  // null nếu rỗng
      'phone': phone.isEmpty ? null : phone,        // null nếu rỗng
      'email': email,
      'username': username,
      'password': password,
      'dateOfBirth': formattedDate,
    };
  }
}

class AccountService {
  static final _api = ApiClient.instance;
  static const _timeout = Duration(seconds: 20);

  static Exception _ex(http.Response res) {
    final body = utf8.decode(res.bodyBytes);
    try {
      final m = jsonDecode(body);
      final msg = (m is Map && m['message'] != null) ? m['message'].toString() : body;
      return Exception('HTTP ${res.statusCode}: $msg');
    } catch (_) {
      return Exception('HTTP ${res.statusCode}: $body');
    }
  }

  static Future<User> login(String username, String password) async {
    print('=== LOGIN REQUEST ===');
    print('URL: ${_api.uri('/api/ApiAccount/login')}');
    print('Username: $username');
    
    final res = await _api.client
        .post(
      _api.uri('/api/ApiAccount/login'),
      headers: _api.jsonHeaders,
      body: jsonEncode({'username': username, 'password': password}),
    )
        .timeout(_timeout);

    print('Response Status: ${res.statusCode}');
    print('Response Headers: ${res.headers}');
    print('Response Body: ${utf8.decode(res.bodyBytes)}');

    // LƯU COOKIE cho các request kế tiếp
    _api.saveCookieFromResponseHeaders(res.headers);

    if (res.statusCode != 200) {
      throw _ex(res);
    }

    // user tối thiểu từ /login (server của bạn đã trả 'user': {...})
    final loginJson = jsonDecode(utf8.decode(res.bodyBytes)) as Map<String, dynamic>;
    final minimal = (loginJson['user'] is Map<String, dynamic>)
        ? User.fromJson(loginJson['user'] as Map<String, dynamic>)
        : User.fromJson(loginJson);

    // thử lấy hồ sơ đầy đủ
    try {
      final full = await userInfo();
      return full;
    } catch (_) {
      return minimal;
    }
  }

  /// Lấy hồ sơ đầy đủ sau login (cần cookie) – gửi kèm 'cookie' header nếu có
  static Future<User> userInfo() async {
    final res = await _api.client
        .get(_api.uri('/api/ApiAccount/userinfo'), headers: _api.authJsonHeaders())
        .timeout(_timeout);

    if (res.statusCode != 200) {
      throw _ex(res);
    }
    final map = jsonDecode(utf8.decode(res.bodyBytes)) as Map<String, dynamic>;
    return User.fromJson(map);
  }

  /// Đăng ký: JSON thuần
  static Future<void> register(RegisterPayload p) async {
    final payload = p.toJson();
    print('=== REGISTER REQUEST ===');
    print('URL: ${_api.uri('/api/ApiAccount/register')}');
    print('Payload: ${jsonEncode(payload)}');
    
    final res = await _api.client
        .post(
      _api.uri('/api/ApiAccount/register'),
      headers: _api.jsonHeaders,
      body: jsonEncode(payload),
    )
        .timeout(_timeout);

    print('Response Status: ${res.statusCode}');
    print('Response Body: ${utf8.decode(res.bodyBytes)}');

    if (res.statusCode != 200) {
      throw _ex(res);
    }
  }

  /// Cập nhật hồ sơ (multipart) — thêm cookie vào header của MultipartRequest
  static Future<User> editProfile(EditProfilePayload p) async {
    final req = http.MultipartRequest('PUT', _api.uri('/api/ApiAccount/editProfile'));

    // fields
    req.fields.addAll(p.toFieldsWithoutDob());
    if (p.dateOfBirth != null) {
      final y = p.dateOfBirth!.year.toString().padLeft(4, '0');
      final m = p.dateOfBirth!.month.toString().padLeft(2, '0');
      final d = p.dateOfBirth!.day.toString().padLeft(2, '0');
      req.fields['DateOfBirth'] = '$y-$m-$d';
    }
    if (p.imageFile != null) {
      req.files.add(await http.MultipartFile.fromPath('ImageFile', p.imageFile!.path));
    }

    // >>> THÊM COOKIE nếu có (KHÔNG set Content-Type thủ công)
    if (_api.cookieHeader != null) {
      req.headers['cookie'] = _api.cookieHeader!;
    }

    final streamed = await _api.client.send(req).timeout(_timeout);
    final res = await http.Response.fromStream(streamed);

    if (res.statusCode != 200) {
      throw _ex(res);
    }
    final map = jsonDecode(utf8.decode(res.bodyBytes)) as Map<String, dynamic>;
    return User.fromJson(map['user'] as Map<String, dynamic>);
  }

  static Future<void> logout() async {
    final res = await _api.client
        .post(_api.uri('/api/ApiAccount/logout'), headers: _api.authJsonHeaders())
        .timeout(_timeout);

    if (res.statusCode != 200) {
      throw _ex(res);
    }
  }

  /// Xác thực OTP
  static Future<void> verifyOtp(String email, String code) async {
    print('=== VERIFY OTP REQUEST ===');
    print('URL: ${_api.uri('/api/ApiAccount/verify-otp')}');
    print('Email: $email, Code: $code');

    final res = await _api.client
        .post(
      _api.uri('/api/ApiAccount/verify-otp'),
      headers: _api.jsonHeaders,
      body: jsonEncode({
        'email': email,
        'code': code,
      }),
    )
        .timeout(_timeout);

    print('Response Status: ${res.statusCode}');
    print('Response Body: ${utf8.decode(res.bodyBytes)}');

    if (res.statusCode != 200) {
      throw _ex(res);
    }
  }

  /// Gửi lại OTP
  static Future<void> resendOtp(String email) async {
    print('=== RESEND OTP REQUEST ===');
    print('URL: ${_api.uri('/api/ApiAccount/resend-otp')}');
    print('Email: $email');

    final res = await _api.client
        .post(
      _api.uri('/api/ApiAccount/resend-otp'),
      headers: _api.jsonHeaders,
      body: jsonEncode({
        'email': email,
      }),
    )
        .timeout(_timeout);

    print('Response Status: ${res.statusCode}');
    print('Response Body: ${utf8.decode(res.bodyBytes)}');

    if (res.statusCode != 200) {
      throw _ex(res);
    }
  }
}
