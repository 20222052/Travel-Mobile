import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:go_router/go_router.dart';

import '../services/cart_service.dart';
import '../models/cart_item.dart';
import '../state/session.dart';

class CheckoutScreen extends StatefulWidget {
  const CheckoutScreen({super.key});

  @override
  State<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends State<CheckoutScreen> {
  late Future<List<CartItemModel>> _future;
  final _fmt = NumberFormat("#,##0", "vi_VN");
  final _form = GlobalKey<FormState>();

  final _name = TextEditingController();
  final _phone = TextEditingController();
  final _address = TextEditingController();

  bool _submitting = false;

  @override
  void initState() {
    super.initState();

    if (Session.current.value == null) {
      WidgetsBinding.instance.addPostFrameCallback((_) => context.go('/login'));
    } else {
      final me = Session.current.value!;
      _name.text = me.name ?? '';
      _phone.text = me.phone ?? '';
      _address.text = me.address ?? '';
      _future = CartService.getCart();
    }
  }

  @override
  void dispose() {
    _name.dispose();
    _phone.dispose();
    _address.dispose();
    super.dispose();
  }

  Future<void> _submit(List<CartItemModel> items) async {
    if (!_form.currentState!.validate()) return;
    if (items.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Giỏ hàng trống')));
      return;
    }

    setState(() => _submitting = true);
    try {
      final orderId = await CartService.checkout(
        name: _name.text.trim(),
        phone: _phone.text.trim(),
        address: _address.text.trim(),
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Đặt hàng thành công (#$orderId)')),
      );
      context.go('/'); // hoặc /account/history
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Đặt hàng thất bại: $e')),
      );
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Đặt hàng')),
      body: FutureBuilder<List<CartItemModel>>(
        future: _future,
        builder: (_, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('Lỗi tải giỏ: ${snap.error}'),
                  const SizedBox(height: 8),
                  OutlinedButton.icon(
                    onPressed: () => setState(() => _future = CartService.getCart()),
                    icon: const Icon(Icons.refresh),
                    label: const Text('Thử lại'),
                  ),
                ],
              ),
            );
          }

          final items = snap.data ?? [];
          final total = items.fold<num>(0, (p, e) => p + (e.tour?.price ?? 0) * e.quantity);

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              children: [
                // Tóm tắt đơn
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(12),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('Tóm tắt đơn hàng', style: TextStyle(fontWeight: FontWeight.w600)),
                        const SizedBox(height: 8),
                        if (items.isEmpty)
                          const Text('Giỏ hàng trống')
                        else
                          ...items.map((it) {
                            final name = it.tour?.name ?? 'Tour';
                            final price = it.tour?.price ?? 0;
                            return Padding(
                              padding: const EdgeInsets.symmetric(vertical: 4),
                              child: Row(
                                children: [
                                  Expanded(child: Text('$name x${it.quantity}', maxLines: 1, overflow: TextOverflow.ellipsis)),
                                  Text('${_fmt.format(price * it.quantity)} đ'),
                                ],
                              ),
                            );
                          }),
                        const Divider(),
                        Row(
                          children: [
                            const Expanded(child: Text('Tổng', style: TextStyle(fontWeight: FontWeight.bold))),
                            Text('${_fmt.format(total)} đ', style: const TextStyle(fontWeight: FontWeight.bold)),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 12),

                // Form người nhận
                Form(
                  key: _form,
                  child: Column(
                    children: [
                      TextFormField(
                        controller: _name,
                        decoration: const InputDecoration(
                          labelText: 'Họ tên người nhận',
                          prefixIcon: Icon(Icons.person_outline),
                        ),
                        validator: (v) => (v == null || v.trim().isEmpty) ? 'Nhập họ tên' : null,
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _phone,
                        keyboardType: TextInputType.phone,
                        decoration: const InputDecoration(
                          labelText: 'Số điện thoại',
                          prefixIcon: Icon(Icons.phone_outlined),
                        ),
                        validator: (v) => (v == null || v.trim().isEmpty) ? 'Nhập số điện thoại' : null,
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _address,
                        decoration: const InputDecoration(
                          labelText: 'Địa chỉ nhận hàng',
                          prefixIcon: Icon(Icons.location_on_outlined),
                        ),
                        validator: (v) => (v == null || v.trim().isEmpty) ? 'Nhập địa chỉ' : null,
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 16),

                SizedBox(
                  width: double.infinity,
                  child: FilledButton.icon(
                    onPressed: _submitting ? null : () => _submit(items),
                    icon: _submitting
                        ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                        : const Icon(Icons.check),
                    label: const Text('Xác nhận đặt hàng'),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
