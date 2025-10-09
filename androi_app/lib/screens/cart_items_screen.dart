// lib/screens/cart_items_screen.dart
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:go_router/go_router.dart';

import '../services/cart_service.dart';
import '../models/cart_item.dart';
import '../state/session.dart';

class CartItemsScreen extends StatefulWidget {
  const CartItemsScreen({super.key});

  @override
  State<CartItemsScreen> createState() => _CartItemsScreenState();
}

class _CartItemsScreenState extends State<CartItemsScreen> {
  late Future<List<CartItemModel>> _future;
  final _fmt = NumberFormat("#,##0", "vi_VN");

  int? _busyItemId;
  @override
  void initState() {
    super.initState();
    if (Session.current.value == null) {
      // chưa login -> chuyển sang login
      WidgetsBinding.instance.addPostFrameCallback((_) => context.go('/login'));
    } else {
      _future = CartService.getCart();
    }
  }

  Future<void> _reload() async {
    setState(() => _future = CartService.getCart());
    await _future;
  }

  void _handleError(Object e) {
    final msg = e.toString();
    // bắt 401 -> buộc đăng nhập
    if (msg.contains('HTTP 401')) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.')),
      );
      context.push('/login');
      return;
    }
    // ScaffoldMessenger.of(context).showSnackBar(
    //   SnackBar(content: Text('Lỗi: $msg')),
    // );
  }

  Future<void> _inc(CartItemModel it) async {
    if (it.id == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Thiếu ID mục giỏ hàng')),
      );
      return;
    }
    setState(() => _busyItemId = it.id);
    try {
      await CartService.updateQuantity(it.id!, it.quantity + 1);
      await _reload();
    } catch (e) {
      _handleError(e);
    } finally {
      if (mounted) setState(() => _busyItemId = null);
    }
  }

  Future<void> _dec(CartItemModel it) async {
    if (it.id == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Thiếu ID mục giỏ hàng')),
      );
      return;
    }
    setState(() => _busyItemId = it.id);
    try {
      if (it.quantity <= 1) {
        await CartService.deleteItem(it.id!);
      } else {
        await CartService.updateQuantity(it.id!, it.quantity - 1);
      }
      await _reload();
    } catch (e) {
      _handleError(e);
    } finally {
      if (mounted) setState(() => _busyItemId = null);
    }
  }

  Future<void> _remove(CartItemModel it) async {
    if (it.id == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Thiếu ID mục giỏ hàng')),
      );
      return;
    }
    setState(() => _busyItemId = it.id);
    try {
      await CartService.deleteItem(it.id!);
      await _reload();
    } catch (e) {
      _handleError(e);
    } finally {
      if (mounted) setState(() => _busyItemId = null);
    }
  }

  String _imgUrl(String? path) {
    if (path == null || path.isEmpty) return '';
    if (path.startsWith('http')) return path;
    final p = path.startsWith('/') ? path : '/$path';
    const host = 'https://10.0.2.2:44364/Uploads';
    return '$host$p';
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Giỏ hàng')),
      body: FutureBuilder<List<CartItemModel>>(
        future: _future,
        builder: (ctx, snap) {
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
                    onPressed: _reload,
                    icon: const Icon(Icons.refresh),
                    label: const Text('Thử lại'),
                  ),
                ],
              ),
            );
          }

          final items = snap.data ?? [];
          if (items.isEmpty) {
            return const Center(child: Text('Giỏ hàng trống'));
          }

          final total = items.fold<num>(0, (p, e) => p + (e.tour?.price ?? 0) * e.quantity);

          return Column(
            children: [
              Expanded(
                child: RefreshIndicator(
                  onRefresh: _reload,
                  child: ListView.separated(
                    itemCount: items.length,
                    separatorBuilder: (_, __) => const Divider(height: 1),
                    itemBuilder: (_, i) {
                      final it = items[i];
                      final img = it.tour?.imageUrl ?? '';
                      final name = it.tour?.name ?? 'Tour';
                      final price = it.tour?.price ?? 0;
                      final busy = _busyItemId == it.id;

                      return ListTile(
                        leading: (img.isNotEmpty)
                            ? ClipRRect(
                          borderRadius: BorderRadius.circular(8),
                          child: Image.network(
                            _imgUrl(img),
                            width: 56,
                            height: 56,
                            fit: BoxFit.cover,
                            errorBuilder: (_, __, ___) => const Icon(Icons.image, size: 32),
                          ),
                        )
                            : const Icon(Icons.image, size: 32),
                        title: Text(name, maxLines: 1, overflow: TextOverflow.ellipsis),
                        subtitle: Text('${_fmt.format(price)} đ'),
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            IconButton(
                              onPressed: busy ? null : () => _dec(it),
                              icon: const Icon(Icons.remove_circle_outline),
                            ),
                            Text('${it.quantity}', style: const TextStyle(fontWeight: FontWeight.w600)),
                            IconButton(
                              onPressed: busy ? null : () => _inc(it),
                              icon: const Icon(Icons.add_circle_outline),
                            ),
                            IconButton(
                              onPressed: busy ? null : () => _remove(it),
                              icon: const Icon(Icons.delete_outline),
                            ),
                          ],
                        ),
                      );
                    },
                  ),
                ),
              ),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.surface,
                  border: const Border(top: BorderSide(color: Color(0x11000000))),
                ),
                child: Row(
                  children: [
                    Expanded(
                      child: Text(
                        'Tổng: ${_fmt.format(total)} đ',
                        style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                      ),
                    ),
                    FilledButton.icon(
                      onPressed: () => context.push('/checkout'),
                      icon: const Icon(Icons.arrow_forward),
                      label: const Text('Đặt hàng'),
                    ),
                  ],
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}
