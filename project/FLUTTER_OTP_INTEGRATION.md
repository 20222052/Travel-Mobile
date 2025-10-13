# Tích hợp OTP vào Flutter App

## 📋 Tổng quan

Hướng dẫn tích hợp luồng đăng ký với OTP vào Flutter app hiện tại.

---

## 🔧 Các bước cần làm

### Bước 1: Cập nhật Models

#### Tạo file `lib/models/otp_request.dart`:
```dart
class OtpRequest {
  final String email;

  OtpRequest({required this.email});

  Map<String, dynamic> toJson() => {
    'email': email,
  };
}
```

#### Tạo file `lib/models/verify_otp_request.dart`:
```dart
class VerifyOtpRequest {
  final String email;
  final String code;

  VerifyOtpRequest({
    required this.email,
    required this.code,
  });

  Map<String, dynamic> toJson() => {
    'email': email,
    'code': code,
  };
}
```

---

### Bước 2: Cập nhật Account Service

Thêm các method mới vào `lib/services/account_service.dart`:

```dart
/// Xác thực OTP
static Future<void> verifyOtp(String email, String code) async {
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

  print('=== VERIFY OTP RESPONSE ===');
  print('Status: ${res.statusCode}');
  print('Body: ${utf8.decode(res.bodyBytes)}');

  if (res.statusCode != 200) {
    throw _ex(res);
  }
}

/// Gửi lại OTP
static Future<void> resendOtp(String email) async {
  final res = await _api.client
      .post(
    _api.uri('/api/ApiAccount/resend-otp'),
    headers: _api.jsonHeaders,
    body: jsonEncode({
      'email': email,
    }),
  )
      .timeout(_timeout);

  print('=== RESEND OTP RESPONSE ===');
  print('Status: ${res.statusCode}');
  print('Body: ${utf8.decode(res.bodyBytes)}');

  if (res.statusCode != 200) {
    throw _ex(res);
  }
}
```

---

### Bước 3: Tạo OTP Verification Screen

Tạo file `lib/screens/verify_otp_screen.dart`:

```dart
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../services/account_service.dart';

class VerifyOtpScreen extends StatefulWidget {
  final String email;

  const VerifyOtpScreen({super.key, required this.email});

  @override
  State<VerifyOtpScreen> createState() => _VerifyOtpScreenState();
}

class _VerifyOtpScreenState extends State<VerifyOtpScreen> {
  final _formKey = GlobalKey<FormState>();
  final _otpController = TextEditingController();
  bool _loading = false;
  bool _resending = false;

  @override
  void dispose() {
    _otpController.dispose();
    super.dispose();
  }

  Future<void> _verifyOtp() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _loading = true);
    try {
      await AccountService.verifyOtp(widget.email, _otpController.text.trim());

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('✅ Xác thực thành công! Bạn có thể đăng nhập.'),
          backgroundColor: Colors.green,
        ),
      );
      
      // Chuyển về màn đăng nhập
      context.go('/login');
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('❌ Xác thực thất bại: $e')),
      );
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _resendOtp() async {
    setState(() => _resending = true);
    try {
      await AccountService.resendOtp(widget.email);

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('📧 Đã gửi lại mã OTP. Vui lòng kiểm tra email.'),
          backgroundColor: Colors.blue,
        ),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('❌ Gửi lại OTP thất bại: $e')),
      );
    } finally {
      if (mounted) setState(() => _resending = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Xác thực OTP'),
        centerTitle: true,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Icon
              Icon(
                Icons.email_outlined,
                size: 80,
                color: Theme.of(context).primaryColor,
              ),
              const SizedBox(height: 24),

              // Tiêu đề
              Text(
                'Kiểm tra email của bạn',
                style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 12),

              // Mô tả
              Text(
                'Chúng tôi đã gửi mã OTP (6 chữ số) đến:\n${widget.email}',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: Colors.grey[600],
                    ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 32),

              // Input OTP
              TextFormField(
                controller: _otpController,
                keyboardType: TextInputType.number,
                maxLength: 6,
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 24,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 8,
                ),
                decoration: const InputDecoration(
                  labelText: 'Mã OTP',
                  hintText: '000000',
                  prefixIcon: Icon(Icons.pin_outlined),
                  counterText: '',
                ),
                validator: (v) {
                  if (v == null || v.trim().isEmpty) return 'Nhập mã OTP';
                  if (v.trim().length != 6) return 'OTP phải có 6 chữ số';
                  if (!RegExp(r'^\d{6}$').hasMatch(v.trim())) {
                    return 'OTP chỉ chứa chữ số';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 24),

              // Nút xác thực
              FilledButton(
                onPressed: _loading ? null : _verifyOtp,
                style: FilledButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                ),
                child: _loading
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Xác thực', style: TextStyle(fontSize: 16)),
              ),
              const SizedBox(height: 16),

              // Gửi lại OTP
              TextButton(
                onPressed: _resending ? null : _resendOtp,
                child: _resending
                    ? const Text('Đang gửi...')
                    : const Text('Không nhận được OTP? Gửi lại'),
              ),

              const SizedBox(height: 16),

              // Hướng dẫn
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.blue[50],
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: Colors.blue[200]!),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Icon(Icons.info_outline, 
                             color: Colors.blue[700], 
                             size: 20),
                        const SizedBox(width: 8),
                        Text(
                          'Lưu ý:',
                          style: TextStyle(
                            fontWeight: FontWeight.bold,
                            color: Colors.blue[700],
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Text(
                      '• Mã OTP có hiệu lực trong 5 phút\n'
                      '• Kiểm tra cả thư mục Spam/Junk\n'
                      '• Mỗi mã chỉ sử dụng được 1 lần',
                      style: TextStyle(
                        fontSize: 13,
                        color: Colors.grey[700],
                        height: 1.5,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
```

---

### Bước 4: Cập nhật Register Screen

Sửa file `lib/screens/register_screen.dart`:

```dart
Future<void> _submit() async {
  if (!_form.currentState!.validate()) return;
  if (_dob == null) {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Vui lòng chọn ngày sinh'))
    );
    return;
  }

  setState(() => _loading = true);
  try {
    final payload = RegisterPayload(
      name: _name.text.trim(),
      gender: _gender,
      address: _address.text.trim().isEmpty ? '' : _address.text.trim(),
      phone: _phone.text.trim().isEmpty ? '' : _phone.text.trim(),
      email: _email.text.trim(),
      username: _username.text.trim(),
      password: _password.text,
      dateOfBirth: _dob!,
    );

    await AccountService.register(payload);

    if (!mounted) return;
    
    // Hiển thị dialog thành công
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        icon: const Icon(Icons.check_circle, color: Colors.green, size: 60),
        title: const Text('Đăng ký thành công!'),
        content: Text(
          'Chúng tôi đã gửi mã OTP đến email:\n${_email.text.trim()}\n\n'
          'Vui lòng kiểm tra email và nhập mã OTP để kích hoạt tài khoản.'
        ),
        actions: [
          FilledButton(
            onPressed: () {
              Navigator.of(context).pop();
              // Chuyển đến màn xác thực OTP
              context.go('/verify-otp', extra: _email.text.trim());
            },
            child: const Text('Xác thực ngay'),
          ),
        ],
      ),
    );
  } catch (e) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Đăng ký thất bại: $e'))
    );
  } finally {
    if (mounted) setState(() => _loading = false);
  }
}
```

---

### Bước 5: Cập nhật Router

Thêm route cho OTP verification screen:

```dart
GoRoute(
  path: '/verify-otp',
  builder: (context, state) {
    final email = state.extra as String?;
    if (email == null) {
      return const Scaffold(
        body: Center(child: Text('Email không hợp lệ')),
      );
    }
    return VerifyOtpScreen(email: email);
  },
),
```

---

### Bước 6: Xử lý Login khi chưa xác thực OTP

Cập nhật login screen để hiển thị thông báo khi user chưa xác thực OTP:

```dart
Future<void> _login() async {
  if (!_formKey.currentState!.validate()) return;

  setState(() => _loading = true);
  try {
    final user = await AccountService.login(
      _username.text.trim(),
      _password.text,
    );

    if (!mounted) return;
    
    // Đăng nhập thành công
    context.go('/home');
  } catch (e) {
    if (!mounted) return;
    
    final errorMsg = e.toString();
    
    // Kiểm tra nếu lỗi là chưa xác thực OTP
    if (errorMsg.contains('requireOtpVerification') || 
        errorMsg.contains('chưa được xác thực')) {
      
      // Lấy email từ error message hoặc hỏi user nhập
      showDialog(
        context: context,
        builder: (context) => AlertDialog(
          icon: const Icon(Icons.warning, color: Colors.orange, size: 60),
          title: const Text('Chưa xác thực tài khoản'),
          content: const Text(
            'Tài khoản của bạn chưa được xác thực.\n'
            'Vui lòng kiểm tra email và xác thực OTP để đăng nhập.'
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Để sau'),
            ),
            FilledButton(
              onPressed: () {
                Navigator.of(context).pop();
                // TODO: Lấy email từ backend hoặc yêu cầu user nhập
                // context.go('/verify-otp', extra: email);
              },
              child: const Text('Xác thực ngay'),
            ),
          ],
        ),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Đăng nhập thất bại: $e')),
      );
    }
  } finally {
    if (mounted) setState(() => _loading = false);
  }
}
```

---

## 🎨 UI/UX Improvements

### Countdown Timer cho OTP
Thêm countdown timer để hiển thị thời gian còn lại:

```dart
class _VerifyOtpScreenState extends State<VerifyOtpScreen> {
  int _remainingSeconds = 300; // 5 phút
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    _startTimer();
  }

  void _startTimer() {
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_remainingSeconds > 0) {
        setState(() => _remainingSeconds--);
      } else {
        timer.cancel();
      }
    });
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  String get _formattedTime {
    final minutes = _remainingSeconds ~/ 60;
    final seconds = _remainingSeconds % 60;
    return '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}';
  }

  // Trong build():
  if (_remainingSeconds > 0)
    Text(
      'Mã hết hạn sau: $_formattedTime',
      style: TextStyle(
        color: _remainingSeconds < 60 ? Colors.red : Colors.grey[600],
        fontWeight: FontWeight.bold,
      ),
    ),
}
```

### Pin Code Input (Optional)
Sử dụng package `pin_code_fields` để có UI đẹp hơn:

```yaml
dependencies:
  pin_code_fields: ^8.0.1
```

```dart
import 'package:pin_code_fields/pin_code_fields.dart';

PinCodeTextField(
  length: 6,
  appContext: context,
  controller: _otpController,
  keyboardType: TextInputType.number,
  pinTheme: PinTheme(
    shape: PinCodeFieldShape.box,
    borderRadius: BorderRadius.circular(8),
    fieldHeight: 50,
    fieldWidth: 45,
    activeFillColor: Colors.white,
    selectedFillColor: Colors.blue[50],
    inactiveFillColor: Colors.grey[100],
  ),
  onChanged: (value) {},
),
```

---

## 🧪 Testing

### Test Case 1: Đăng ký → Xác thực OTP → Đăng nhập
1. Mở app, chọn Đăng ký
2. Điền thông tin đầy đủ
3. Nhấn Đăng ký
4. Kiểm tra email, lấy mã OTP
5. Nhập OTP vào màn xác thực
6. Nhấn Xác thực
7. Quay về đăng nhập
8. Đăng nhập thành công

### Test Case 2: OTP hết hạn
1. Đăng ký tài khoản
2. Đợi > 5 phút
3. Nhập OTP
4. Thấy lỗi "OTP đã hết hạn"
5. Nhấn "Gửi lại OTP"
6. Nhập OTP mới → Thành công

### Test Case 3: Đăng nhập trước khi xác thực
1. Đăng ký tài khoản
2. Không xác thực OTP
3. Cố đăng nhập
4. Thấy thông báo "Chưa xác thực tài khoản"
5. Nhấn "Xác thực ngay"
6. Xác thực OTP → Đăng nhập được

---

## 📚 Dependencies cần thêm

```yaml
dependencies:
  # Existing
  http: ^1.1.0
  go_router: ^12.0.0
  
  # Optional for better OTP UI
  pin_code_fields: ^8.0.1
```

---

## 🔧 Troubleshooting

### Không nhận được email
1. Kiểm tra email có đúng không
2. Kiểm tra thư mục Spam/Junk
3. Kiểm tra EmailSettings trong appsettings.json
4. Kiểm tra Gmail App Password còn hiệu lực

### Lỗi "OTP không hợp lệ"
1. Kiểm tra email nhập đúng
2. Kiểm tra OTP chưa hết hạn (< 5 phút)
3. Kiểm tra OTP chưa được sử dụng
4. Thử gửi lại OTP mới

### App không chuyển màn hình
1. Kiểm tra GoRouter đã cấu hình route `/verify-otp`
2. Kiểm tra `extra` parameter có được truyền đúng
3. Kiểm tra context.go() có được gọi sau khi mounted

---

**Tác giả:** GitHub Copilot  
**Ngày cập nhật:** 13/10/2025  
**Version:** 1.0
