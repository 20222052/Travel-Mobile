import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../services/account_service.dart';
import '../state/session.dart';
import '../models/user.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _form = GlobalKey<FormState>();
  final _uCtrl = TextEditingController();
  final _pCtrl = TextEditingController();
  bool _obscure = true;
  bool _loading = false;

  @override
  void dispose() {
    _uCtrl.dispose();
    _pCtrl.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_form.currentState!.validate()) return;
    setState(() => _loading = true);
    try {
      final User user = await AccountService.login(_uCtrl.text.trim(), _pCtrl.text);
      Session.set(user);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Đăng nhập thành công')));
      context.pop(); // quay lại tab Account (đang mở)
    } catch (e) {
      if (!mounted) return;
      
      final errorMsg = e.toString();
      
      // Kiểm tra nếu lỗi là chưa xác thực OTP
      if (errorMsg.contains('requireOtpVerification') || 
          errorMsg.contains('chưa được xác thực') ||
          errorMsg.contains('chưa xác thực')) {
        
        // Hiển thị dialog yêu cầu xác thực OTP
        showDialog(
          context: context,
          builder: (context) => AlertDialog(
            icon: const Icon(Icons.warning_amber, color: Colors.orange, size: 60),
            title: const Text('Chưa xác thực tài khoản'),
            content: const Text(
              'Tài khoản của bạn chưa được xác thực.\n\n'
              'Vui lòng kiểm tra email và xác thực mã OTP để có thể đăng nhập.',
              textAlign: TextAlign.center,
            ),
            actionsAlignment: MainAxisAlignment.spaceEvenly,
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('Để sau'),
              ),
              FilledButton.icon(
                onPressed: () {
                  Navigator.of(context).pop();
                  // Hiển thị dialog nhập email để gửi lại OTP
                  _showResendOtpDialog();
                },
                icon: const Icon(Icons.email),
                label: const Text('Gửi lại OTP'),
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

  void _showResendOtpDialog() {
    final emailCtrl = TextEditingController();
    
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Nhập email của bạn'),
        content: TextField(
          controller: emailCtrl,
          decoration: const InputDecoration(
            labelText: 'Email',
            hintText: 'example@email.com',
            prefixIcon: Icon(Icons.email),
          ),
          keyboardType: TextInputType.emailAddress,
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Hủy'),
          ),
          FilledButton(
            onPressed: () async {
              final email = emailCtrl.text.trim();
              if (email.isEmpty) {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Vui lòng nhập email')),
                );
                return;
              }
              
              Navigator.of(context).pop();
              
              try {
                await AccountService.resendOtp(email);
                if (!mounted) return;
                
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('✅ Đã gửi lại OTP. Vui lòng kiểm tra email.'),
                    backgroundColor: Colors.green,
                  ),
                );
                
                // Chuyển đến màn xác thực OTP
                context.push('/verify-otp', extra: email);
              } catch (e) {
                if (!mounted) return;
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text('Gửi OTP thất bại: $e')),
                );
              }
            },
            child: const Text('Gửi'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Đăng nhập')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _form,
          child: Column(
            children: [
              TextFormField(
                controller: _uCtrl,
                decoration: const InputDecoration(labelText: 'Tên đăng nhập', prefixIcon: Icon(Icons.person_outline)),
                validator: (v) => (v == null || v.trim().isEmpty) ? 'Nhập tên đăng nhập' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _pCtrl,
                obscureText: _obscure,
                decoration: InputDecoration(
                  labelText: 'Mật khẩu',
                  prefixIcon: const Icon(Icons.lock_outline),
                  suffixIcon: IconButton(
                    icon: Icon(_obscure ? Icons.visibility : Icons.visibility_off),
                    onPressed: () => setState(() => _obscure = !_obscure),
                  ),
                ),
                validator: (v) => (v == null || v.isEmpty) ? 'Nhập mật khẩu' : null,
              ),
              const SizedBox(height: 20),
              SizedBox(
                width: double.infinity,
                child: FilledButton(
                  onPressed: _loading ? null : _submit,
                  child: _loading ? const CircularProgressIndicator() : const Text('Đăng nhập'),
                ),
              ),
              const SizedBox(height: 12),
              TextButton(
                onPressed: () => context.push('/register'),
                child: const Text('Chưa có tài khoản? Đăng ký'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
