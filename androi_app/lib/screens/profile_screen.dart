import 'dart:io';

import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';

import '../config/api_config.dart';
import '../state/session.dart';
import '../models/user.dart';
import '../services/account_service.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _form = GlobalKey<FormState>();

  // controllers
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _address = TextEditingController();
  final ValueNotifier<String> _gender = ValueNotifier<String>('Nam');
  DateTime? _dob;

  // avatar
  final _picker = ImagePicker();
  XFile? _picked;

  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _prefillFromSession();
    _refreshFromServer(); // tải lại từ /userinfo để đồng bộ
  }

  void _prefillFromSession() {
    final me = Session.current.value;
    if (me == null) return;
    _name.text = me.name ?? '';
    _email.text = me.email ?? '';
    _phone.text = me.phone ?? '';
    _address.text = me.address ?? '';
    _gender.value = (me.gender ?? 'Nam');
    _dob = me.dateOfBirth;
  }

  Future<void> _refreshFromServer() async {
    try {
      final u = await AccountService.userInfo();
      Session.set(u);
      if (!mounted) return;
      setState(() {
        _name.text = u.name ?? '';
        _email.text = u.email ?? '';
        _phone.text = u.phone ?? '';
        _address.text = u.address ?? '';
        _gender.value = (u.gender ?? 'Nam');
        _dob = u.dateOfBirth;
      });
    } catch (_) {
      // im lặng: nếu 401 thì coi như chưa login
    }
  }

  @override
  void dispose() {
    _name.dispose();
    _email.dispose();
    _phone.dispose();
    _address.dispose();
    _gender.dispose();
    super.dispose();
  }

  String? _avatarUrl(String? fileName) {
    if (fileName == null || fileName.isEmpty) return null;
    return '${ApiConfig.uploadsPath}/$fileName';
  }

  Future<void> _pickImage() async {
    final x = await _picker.pickImage(source: ImageSource.gallery, imageQuality: 85);
    if (x != null) setState(() => _picked = x);
  }

  Future<void> _pickDob() async {
    final now = DateTime.now();
    final init = _dob ?? DateTime(now.year - 18, now.month, now.day);
    final picked = await showDatePicker(
      context: context,
      initialDate: init,
      firstDate: DateTime(1900),
      lastDate: now,
    );
    if (picked != null) setState(() => _dob = picked);
  }

  Future<void> _save() async {
    if (!_form.currentState!.validate()) return;
    final me = Session.current.value;
    if (me == null) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Bạn chưa đăng nhập')));
      }
      return;
    }

    setState(() => _loading = true);
    try {
      // Server của bạn hiện nhận [FromForm] User trong editProfile ⇒
      // để an toàn, gửi kèm Username/Role và các trường có thể Required.
      final updated = await AccountService.editProfile(
        EditProfilePayload(
          username: me.username, // cần cho model hiện tại của server
          role: me.role ?? 'USER',
          name: _name.text.trim(),
          gender: _gender.value,
          address: _address.text.trim(),
          phone: _phone.text.trim(),
          email: _email.text.trim(),
          dateOfBirth: _dob,
          imageFile: _picked != null ? File(_picked!.path) : null,
        ),
      );

      Session.set(updated);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Cập nhật thành công')));
      Navigator.of(context).maybePop();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Cập nhật thất bại: $e')));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final me = Session.current.value;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Hồ sơ của tôi'),
        actions: [
          TextButton(
            onPressed: _loading ? null : _save,
            child: _loading
                ? const Padding(
              padding: EdgeInsets.symmetric(horizontal: 12.0),
              child: SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2)),
            )
                : const Text('Lưu'),
          ),
        ],
      ),
      body: (me == null)
          ? const Center(child: Text('Bạn chưa đăng nhập'))
          : SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _form,
          child: Column(
            children: [
              // Avatar
              Center(
                child: Stack(
                  children: [
                    CircleAvatar(
                      radius: 48,
                      backgroundImage: (_picked != null)
                          ? FileImage(File(_picked!.path))
                          : (me.image != null && me.image!.isNotEmpty)
                          ? NetworkImage(_avatarUrl(me.image!)!) as ImageProvider
                          : null,
                      child: (me.image == null && _picked == null) ? const Icon(Icons.person, size: 48) : null,
                    ),
                    Positioned(
                      right: -4,
                      bottom: -4,
                      child: IconButton.filledTonal(
                        onPressed: _pickImage,
                        icon: const Icon(Icons.camera_alt_outlined),
                        tooltip: 'Chọn ảnh',
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 16),

              TextFormField(
                controller: _name,
                decoration: const InputDecoration(
                  labelText: 'Họ tên',
                  prefixIcon: Icon(Icons.badge_outlined),
                ),
                validator: (v) => (v == null || v.trim().isEmpty) ? 'Nhập họ tên' : null,
              ),
              const SizedBox(height: 12),

              Row(
                children: [
                  const Text('Giới tính:'),
                  const SizedBox(width: 12),
                  ValueListenableBuilder<String>(
                    valueListenable: _gender,
                    builder: (_, val, __) => Row(
                      children: [
                        ChoiceChip(
                          label: const Text('Nam'),
                          selected: val == 'Nam',
                          onSelected: (_) => _gender.value = 'Nam',
                        ),
                        const SizedBox(width: 8),
                        ChoiceChip(
                          label: const Text('Nữ'),
                          selected: val == 'Nữ',
                          onSelected: (_) => _gender.value = 'Nữ',
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _email,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                  labelText: 'Email',
                  prefixIcon: Icon(Icons.email_outlined),
                ),
                validator: (v) => (v == null || v.trim().isEmpty) ? 'Nhập email' : null,
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _phone,
                keyboardType: TextInputType.phone,
                decoration: const InputDecoration(
                  labelText: 'Số điện thoại',
                  prefixIcon: Icon(Icons.phone_outlined),
                ),
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _address,
                decoration: const InputDecoration(
                  labelText: 'Địa chỉ',
                  prefixIcon: Icon(Icons.home_outlined),
                ),
              ),
              const SizedBox(height: 12),

              SizedBox(
                width: double.infinity,
                child: OutlinedButton.icon(
                  onPressed: _pickDob,
                  icon: const Icon(Icons.calendar_today_outlined),
                  label: Text(
                    _dob == null
                        ? 'Chọn ngày sinh'
                        : '${_dob!.day.toString().padLeft(2, '0')}/${_dob!.month.toString().padLeft(2, '0')}/${_dob!.year}',
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
