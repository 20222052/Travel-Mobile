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
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Đăng ký thành công. Mời đăng nhập.')));
      context.go('/login'); // quay về màn đăng nhập
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
                decoration: const InputDecoration(labelText: 'Họ tên', prefixIcon: Icon(Icons.badge_outlined)),
                validator: (v) => (v == null || v.trim().isEmpty) ? 'Nhập họ tên' : null,
              ),
              const SizedBox(height: 12),

              Row(
                children: [
                  const Text('Giới tính:'),
                  const SizedBox(width: 12),
                  ChoiceChip(
                    label: const Text('Nam'),
                    selected: _gender == 'Nam',
                    onSelected: (_) => setState(() => _gender = 'Nam'),
                  ),
                  const SizedBox(width: 8),
                  ChoiceChip(
                    label: const Text('Nữ'),
                    selected: _gender == 'Nữ',
                    onSelected: (_) => setState(() => _gender = 'Nữ'),
                  ),
                ],
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _email,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(labelText: 'Email', prefixIcon: Icon(Icons.email_outlined)),
                validator: (v) {
                  if (v == null || v.trim().isEmpty) return 'Nhập email';
                  // Validate email format
                  final emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
                  if (!emailRegex.hasMatch(v.trim())) return 'Email không hợp lệ';
                  return null;
                },
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _phone,
                keyboardType: TextInputType.phone,
                decoration: const InputDecoration(labelText: 'Số điện thoại (không bắt buộc)', prefixIcon: Icon(Icons.phone_outlined)),
                validator: (v) {
                  // Nếu có nhập thì phải đúng format
                  if (v != null && v.trim().isNotEmpty) {
                    // Validate phone format (VN: 10 số bắt đầu bằng 0)
                    final phoneRegex = RegExp(r'^0[0-9]{9}$');
                    if (!phoneRegex.hasMatch(v.trim())) {
                      return 'Số điện thoại không hợp lệ (VD: 0901234567)';
                    }
                  }
                  return null;
                },
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _address,
                decoration: const InputDecoration(labelText: 'Địa chỉ', prefixIcon: Icon(Icons.home_outlined)),
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _username,
                decoration: const InputDecoration(labelText: 'Username', prefixIcon: Icon(Icons.person_outline)),
                validator: (v) => (v == null || v.trim().isEmpty) ? 'Nhập username' : null,
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _password,
                obscureText: true,
                decoration: const InputDecoration(labelText: 'Mật khẩu', prefixIcon: Icon(Icons.lock_outline)),
                validator: (v) => (v == null || v.isEmpty) ? 'Nhập mật khẩu' : null,
              ),
              const SizedBox(height: 12),

              SizedBox(
                width: double.infinity,
                child: OutlinedButton.icon(
                  onPressed: _pickDob,
                  icon: const Icon(Icons.calendar_today_outlined),
                  label: Text(_dob == null ? 'Chọn ngày sinh' : df.format(_dob!)),
                ),
              ),
              const SizedBox(height: 16),

              SizedBox(
                width: double.infinity,
                child: FilledButton.icon(
                  onPressed: _loading ? null : _submit,
                  icon: _loading
                      ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                      : const Icon(Icons.check),
                  label: const Text('Đăng ký'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
