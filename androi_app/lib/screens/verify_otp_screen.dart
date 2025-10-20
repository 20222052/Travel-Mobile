import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
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
  final List<TextEditingController> _otpControllers = List.generate(
    6,
    (index) => TextEditingController(),
  );
  final List<FocusNode> _focusNodes = List.generate(6, (index) => FocusNode());

  bool _loading = false;
  bool _resending = false;
  int _remainingSeconds = 300; // 5 phút
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    _startTimer();
  }

  @override
  void dispose() {
    _timer?.cancel();
    for (var controller in _otpControllers) {
      controller.dispose();
    }
    for (var node in _focusNodes) {
      node.dispose();
    }
    super.dispose();
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

  String get _formattedTime {
    final minutes = _remainingSeconds ~/ 60;
    final seconds = _remainingSeconds % 60;
    return '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}';
  }

  String get _otpCode {
    return _otpControllers.map((c) => c.text).join();
  }

  Future<void> _verifyOtp() async {
    if (_otpCode.length != 6) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Vui lòng nhập đủ 6 số')),
      );
      return;
    }

    setState(() => _loading = true);
    try {
      await AccountService.verifyOtp(widget.email, _otpCode);

      if (!mounted) return;
      
      // Hiển thị dialog thành công
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (context) => AlertDialog(
          icon: const Icon(Icons.check_circle, color: Colors.green, size: 80),
          title: const Text('Xác thực thành công!'),
          content: const Text(
            'Tài khoản của bạn đã được kích hoạt.\n'
            'Bạn có thể đăng nhập ngay bây giờ.',
            textAlign: TextAlign.center,
          ),
          actionsAlignment: MainAxisAlignment.center,
          actions: [
            FilledButton.icon(
              onPressed: () {
                Navigator.of(context).pop();
                context.go('/login');
              },
              icon: const Icon(Icons.login),
              label: const Text('Đăng nhập ngay'),
            ),
          ],
        ),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('❌ $e'),
          backgroundColor: Colors.red,
        ),
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
      
      // Reset timer và OTP inputs
      setState(() {
        _remainingSeconds = 300;
        for (var controller in _otpControllers) {
          controller.clear();
        }
      });
      _timer?.cancel();
      _startTimer();
      
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('✅ Đã gửi lại mã OTP. Vui lòng kiểm tra email.'),
          backgroundColor: Colors.green,
        ),
      );
      
      // Focus vào ô đầu tiên
      _focusNodes[0].requestFocus();
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
              // Icon email
              Container(
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  color: Colors.blue.shade50,
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  Icons.email_outlined,
                  size: 80,
                  color: Theme.of(context).primaryColor,
                ),
              ),
              const SizedBox(height: 32),

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
                'Chúng tôi đã gửi mã OTP (6 chữ số) đến:',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: Colors.grey[600],
                    ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              
              Text(
                widget.email,
                style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: Colors.blue[700],
                    ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 32),

              // OTP Input boxes
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: List.generate(6, (index) {
                  return SizedBox(
                    width: 50,
                    child: TextFormField(
                      controller: _otpControllers[index],
                      focusNode: _focusNodes[index],
                      keyboardType: TextInputType.number,
                      textAlign: TextAlign.center,
                      maxLength: 1,
                      style: const TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                      ),
                      decoration: InputDecoration(
                        counterText: '',
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        filled: true,
                        fillColor: Colors.white,
                      ),
                      inputFormatters: [
                        FilteringTextInputFormatter.digitsOnly,
                      ],
                      onChanged: (value) {
                        if (value.isNotEmpty) {
                          if (index < 5) {
                            _focusNodes[index + 1].requestFocus();
                          } else {
                            _focusNodes[index].unfocus();
                          }
                        }
                      },
                      onTap: () {
                        _otpControllers[index].selection = TextSelection.fromPosition(
                          TextPosition(offset: _otpControllers[index].text.length),
                        );
                      },
                    ),
                  );
                }),
              ),
              const SizedBox(height: 24),

              // Timer
              if (_remainingSeconds > 0)
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      Icons.timer_outlined,
                      size: 20,
                      color: _remainingSeconds < 60 ? Colors.red : Colors.grey[600],
                    ),
                    const SizedBox(width: 8),
                    Text(
                      'Mã hết hạn sau: $_formattedTime',
                      style: TextStyle(
                        color: _remainingSeconds < 60 ? Colors.red : Colors.grey[600],
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                )
              else
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.red[50],
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(Icons.error_outline, color: Colors.red[700]),
                      const SizedBox(width: 8),
                      Text(
                        'Mã OTP đã hết hạn',
                        style: TextStyle(
                          color: Colors.red[700],
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                ),
              const SizedBox(height: 32),

              // Nút xác thực
              FilledButton(
                onPressed: (_loading || _remainingSeconds == 0) ? null : _verifyOtp,
                style: FilledButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                child: _loading
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                        ),
                      )
                    : const Text(
                        'Xác thực',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
              ),
              const SizedBox(height: 16),

              // Gửi lại OTP
              TextButton.icon(
                onPressed: _resending ? null : _resendOtp,
                icon: _resending
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.refresh),
                label: Text(_resending ? 'Đang gửi...' : 'Không nhận được OTP? Gửi lại'),
              ),
              const SizedBox(height: 8),

              // Nút quay lại đăng ký
              TextButton.icon(
                onPressed: () {
                  // Hiển thị dialog xác nhận
                  showDialog(
                    context: context,
                    builder: (context) => AlertDialog(
                      icon: const Icon(Icons.warning_amber, color: Colors.orange, size: 60),
                      title: const Text('Quay lại đăng ký?'),
                      content: const Text(
                        'Bạn có chắc muốn quay lại màn hình đăng ký?\n\n'
                        'Mã OTP hiện tại sẽ vẫn còn hiệu lực nếu bạn không thay đổi email.',
                        textAlign: TextAlign.center,
                      ),
                      actionsAlignment: MainAxisAlignment.spaceEvenly,
                      actions: [
                        TextButton(
                          onPressed: () => Navigator.of(context).pop(),
                          child: const Text('Hủy'),
                        ),
                        FilledButton.icon(
                          onPressed: () {
                            Navigator.of(context).pop();
                            context.go('/register');
                          },
                          icon: const Icon(Icons.arrow_back),
                          label: const Text('Quay lại'),
                          style: FilledButton.styleFrom(
                            backgroundColor: Colors.orange,
                          ),
                        ),
                      ],
                    ),
                  );
                },
                icon: const Icon(Icons.edit_outlined),
                label: const Text('Thông tin sai? Quay lại đăng ký'),
                style: TextButton.styleFrom(
                  foregroundColor: Colors.orange[700],
                ),
              ),
              const SizedBox(height: 24),

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
                        Icon(Icons.info_outline, color: Colors.blue[700], size: 20),
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
                      '• Mỗi mã chỉ sử dụng được 1 lần\n'
                      '• Có thể gửi lại OTP nếu không nhận được',
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
