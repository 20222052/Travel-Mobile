import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:go_router/go_router.dart';

import '../services/account_service.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _form = GlobalKey<FormState>();

  final _name = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _address = TextEditingController();
  final _username = TextEditingController();
  final _password = TextEditingController();
  String _gender = 'Nam';
  DateTime? _dob;

  bool _loading = false;
  bool _obscurePassword = true;

  @override
  void dispose() {
    _name.dispose();
    _email.dispose();
    _phone.dispose();
    _address.dispose();
    _username.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _pickDob() async {
    final now = DateTime.now();
    final init = _dob ?? DateTime(now.year - 18, now.month, now.day);
    final d = await showDatePicker(
      context: context,
      initialDate: init,
      firstDate: DateTime(1900),
      lastDate: now,
    );
    if (d != null) setState(() => _dob = d);
  }

  Future<void> _submit() async {
    if (!_form.currentState!.validate()) return;
    if (_dob == null) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Vui lòng chọn ngày sinh')));
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
      
      // Hiển thị dialog thành công và chuyển đến màn xác thực OTP
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (context) => AlertDialog(
          icon: const Icon(Icons.mark_email_read, color: Colors.green, size: 80),
          title: const Text('Đăng ký thành công!'),
          content: Text(
            'Chúng tôi đã gửi mã OTP đến email:\n${_email.text.trim()}\n\n'
            'Vui lòng kiểm tra email và nhập mã OTP để kích hoạt tài khoản.',
            textAlign: TextAlign.center,
          ),
          actionsAlignment: MainAxisAlignment.center,
          actions: [
            FilledButton.icon(
              onPressed: () {
                Navigator.of(context).pop();
                // Chuyển đến màn xác thực OTP với email
                context.go('/verify-otp', extra: _email.text.trim());
              },
              icon: const Icon(Icons.verified_user),
              label: const Text('Xác thực ngay'),
            ),
          ],
        ),
      );
    } catch (e) {
      if (!mounted) return;
      // Sẽ thấy message rõ ràng: 409 trùng username/email, 400 thiếu trường, v.v.
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Đăng ký thất bại: $e')));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final df = DateFormat('dd/MM/yyyy');
    return Scaffold(
      appBar: AppBar(title: const Text('Đăng ký')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _form,
          child: Column(
            children: [
              TextFormField(
                controller: _name,
                decoration: const InputDecoration(
                  labelText: 'Họ tên *',
                  prefixIcon: Icon(Icons.badge_outlined),
                  hintText: 'Nguyễn Văn A',
                  border: OutlineInputBorder(),
                ),
                textInputAction: TextInputAction.next,
                validator: (v) {
                  if (v == null || v.trim().isEmpty) {
                    return 'Vui lòng nhập họ tên';
                  }
                  if (v.trim().length < 2) {
                    return 'Họ tên phải có ít nhất 2 ký tự';
                  }
                  // Kiểm tra có chứa số không
                  if (RegExp(r'[0-9]').hasMatch(v)) {
                    return 'Họ tên không được chứa số';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 12),

              Row(
                children: [
                  const Text('Giới tính: ', style: TextStyle(fontSize: 16)),
                  const SizedBox(width: 12),
                  Expanded(
                    child: ChoiceChip(
                      label: const Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.male, size: 18),
                          SizedBox(width: 4),
                          Text('Nam'),
                        ],
                      ),
                      selected: _gender == 'Nam',
                      onSelected: (_) => setState(() => _gender = 'Nam'),
                    ),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: ChoiceChip(
                      label: const Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.female, size: 18),
                          SizedBox(width: 4),
                          Text('Nữ'),
                        ],
                      ),
                      selected: _gender == 'Nữ',
                      onSelected: (_) => setState(() => _gender = 'Nữ'),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),

              TextFormField(
                controller: _email,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                  labelText: 'Email *',
                  prefixIcon: Icon(Icons.email_outlined),
                  hintText: 'example@email.com',
                  border: OutlineInputBorder(),
                ),
                textInputAction: TextInputAction.next,
                validator: (v) {
                  if (v == null || v.trim().isEmpty) {
                    return 'Vui lòng nhập email';
                  }
                  // Validate email format chuẩn hơn
                  final emailRegex = RegExp(
                    r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$'
                  );
                  if (!emailRegex.hasMatch(v.trim())) {
                    return 'Email không đúng định dạng (VD: user@gmail.com)';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _phone,
                keyboardType: TextInputType.phone,
                decoration: const InputDecoration(
                  labelText: 'Số điện thoại',
                  prefixIcon: Icon(Icons.phone_outlined),
                  hintText: '0901234567',
                  border: OutlineInputBorder(),
                  helperText: 'Không bắt buộc',
                ),
                textInputAction: TextInputAction.next,
                validator: (v) {
                  // Nếu có nhập thì phải đúng format
                  if (v != null && v.trim().isNotEmpty) {
                    // Loại bỏ khoảng trắng và dấu gạch ngang
                    final cleaned = v.trim().replaceAll(RegExp(r'[\s-]'), '');
                    
                    // Validate phone format (VN: 10-11 số bắt đầu bằng 0)
                    final phoneRegex = RegExp(r'^0[0-9]{9,10}$');
                    if (!phoneRegex.hasMatch(cleaned)) {
                      return 'SĐT không hợp lệ. Phải có 10-11 số, bắt đầu bằng 0';
                    }
                  }
                  return null;
                },
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _address,
                decoration: const InputDecoration(
                  labelText: 'Địa chỉ',
                  prefixIcon: Icon(Icons.home_outlined),
                  hintText: '123 Đường ABC, Quận XYZ',
                  border: OutlineInputBorder(),
                  helperText: 'Không bắt buộc',
                ),
                textInputAction: TextInputAction.next,
                maxLines: 2,
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _username,
                decoration: const InputDecoration(
                  labelText: 'Tên đăng nhập *',
                  prefixIcon: Icon(Icons.person_outline),
                  hintText: 'username',
                  border: OutlineInputBorder(),
                ),
                textInputAction: TextInputAction.next,
                validator: (v) {
                  if (v == null || v.trim().isEmpty) {
                    return 'Vui lòng nhập tên đăng nhập';
                  }
                  if (v.trim().length < 3) {
                    return 'Tên đăng nhập phải có ít nhất 3 ký tự';
                  }
                  if (v.trim().length > 20) {
                    return 'Tên đăng nhập không được quá 20 ký tự';
                  }
                  // Chỉ cho phép chữ cái, số và dấu gạch dưới
                  if (!RegExp(r'^[a-zA-Z0-9_]+$').hasMatch(v.trim())) {
                    return 'Chỉ được dùng chữ, số và dấu gạch dưới (_)';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _password,
                obscureText: _obscurePassword,
                decoration: InputDecoration(
                  labelText: 'Mật khẩu *',
                  prefixIcon: const Icon(Icons.lock_outline),
                  hintText: 'Tối thiểu 6 ký tự',
                  border: const OutlineInputBorder(),
                  suffixIcon: IconButton(
                    icon: Icon(_obscurePassword ? Icons.visibility_off : Icons.visibility),
                    onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                  ),
                ),
                textInputAction: TextInputAction.done,
                validator: (v) {
                  if (v == null || v.isEmpty) {
                    return 'Vui lòng nhập mật khẩu';
                  }
                  if (v.length < 6) {
                    return 'Mật khẩu phải có ít nhất 6 ký tự';
                  }
                  if (v.length > 50) {
                    return 'Mật khẩu không được quá 50 ký tự';
                  }
                  // Kiểm tra có chữ và số
                  if (!RegExp(r'[a-zA-Z]').hasMatch(v)) {
                    return 'Mật khẩu phải có ít nhất 1 chữ cái';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),

              Container(
                width: double.infinity,
                decoration: BoxDecoration(
                  border: Border.all(
                    color: _dob == null ? Colors.red.shade300 : Colors.grey.shade400,
                    width: _dob == null ? 2 : 1,
                  ),
                  borderRadius: BorderRadius.circular(4),
                ),
                child: OutlinedButton.icon(
                  onPressed: _pickDob,
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    side: BorderSide.none,
                  ),
                  icon: Icon(
                    Icons.calendar_today_outlined,
                    color: _dob == null ? Colors.red : null,
                  ),
                  label: Text(
                    _dob == null ? 'Chọn ngày sinh *' : df.format(_dob!),
                    style: TextStyle(
                      fontSize: 16,
                      color: _dob == null ? Colors.red : null,
                    ),
                  ),
                ),
              ),
              if (_dob == null)
                const Padding(
                  padding: EdgeInsets.only(left: 12, top: 4),
                  child: Text(
                    'Vui lòng chọn ngày sinh',
                    style: TextStyle(color: Colors.red, fontSize: 12),
                  ),
                ),
              const SizedBox(height: 24),

              SizedBox(
                width: double.infinity,
                height: 50,
                child: FilledButton.icon(
                  onPressed: _loading ? null : _submit,
                  style: FilledButton.styleFrom(
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                  ),
                  icon: _loading
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                      : const Icon(Icons.app_registration, size: 24),
                  label: Text(
                    _loading ? 'Đang xử lý...' : 'Đăng ký tài khoản',
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                ),
              ),
              const SizedBox(height: 16),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text('Đã có tài khoản? '),
                  TextButton(
                    onPressed: () => context.pop(),
                    child: const Text('Đăng nhập ngay', style: TextStyle(fontWeight: FontWeight.bold)),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
