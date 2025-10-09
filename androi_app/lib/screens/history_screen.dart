import 'package:flutter/material.dart';

import '../state/session.dart';
import '../models/order.dart';
import '../models/order_detail.dart';
import '../services/order_service.dart';
import '../models/tour.dart';
import '../services/tour_service.dart';

class HistoryScreen extends StatefulWidget {
  const HistoryScreen({super.key});

  @override
  State<HistoryScreen> createState() => _HistoryScreenState();
}

class _HistoryScreenState extends State<HistoryScreen> {
  final _searchCtrl = TextEditingController();
  String _sort = 'desc';
  late Future<List<OrderModel>> _future;

  @override
  void initState() {
    super.initState();
    final uid = Session.current.value?.id;
    _future = (uid == null) ? Future.value([]) : OrderService.getOrders(uid);
  }

  Future<void> _reload() async {
    final uid = Session.current.value?.id;
    if (uid == null) return;
    setState(() {
      _future = OrderService.getOrders(
        uid,
        search: _searchCtrl.text.trim().isEmpty ? null : _searchCtrl.text.trim(),
        sort: _sort,
      );
    });
    await _future;
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  String _statusText(int? s) {
    switch (s) {
      case 0: return 'Mới tạo';
      case 1: return 'Đã xác nhận';
      case 2: return 'Đang xử lý';
      case 3: return 'Hoàn tất';
      case 4: return 'Đã hủy';
      default: return 'Không rõ';
    }
  }

  String _formatDate(DateTime? d) {
    if (d == null) return '';
    final x = d.toLocal();
    return '${x.day.toString().padLeft(2, '0')}/${x.month.toString().padLeft(2, '0')}/${x.year} '
        '${x.hour.toString().padLeft(2, '0')}:${x.minute.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    final me = Session.current.value;
    return Scaffold(
      appBar: AppBar(title: const Text('Lịch sử đơn hàng')),
      body: (me == null)
          ? const Center(child: Text('Bạn chưa đăng nhập'))
          : Column(
        children: [
          // thanh tìm kiếm + sort
          Padding(
            padding: const EdgeInsets.fromLTRB(12, 12, 12, 8),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _searchCtrl,
                    textInputAction: TextInputAction.search,
                    onSubmitted: (_) => _reload(),
                    decoration: InputDecoration(
                      hintText: 'Tìm theo tên/địa chỉ/điện thoại…',
                      prefixIcon: const Icon(Icons.search),
                      filled: true,
                      fillColor: Colors.grey.shade100,
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: BorderSide.none,
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                DropdownButtonHideUnderline(
                  child: DropdownButton<String>(
                    value: _sort,
                    items: const [
                      DropdownMenuItem(value: 'desc', child: Text('Mới nhất')),
                      DropdownMenuItem(value: 'asc', child: Text('Cũ nhất')),
                    ],
                    onChanged: (v) {
                      if (v == null) return;
                      setState(() => _sort = v);
                      _reload();
                    },
                  ),
                ),
                const SizedBox(width: 4),
                IconButton(onPressed: _reload, icon: const Icon(Icons.search)),
              ],
            ),
          ),
          const Divider(height: 1),

          // danh sách đơn
          Expanded(
            child: RefreshIndicator(
              onRefresh: _reload,
              child: FutureBuilder<List<OrderModel>>(
                future: _future,
                builder: (context, snap) {
                  if (snap.connectionState == ConnectionState.waiting) {
                    return const Center(child: CircularProgressIndicator());
                  }
                  if (snap.hasError) {
                    return ListView(
                      children: [
                        const SizedBox(height: 120),
                        const Icon(Icons.error_outline, size: 56, color: Colors.redAccent),
                        const SizedBox(height: 8),
                        Center(child: Text('Lỗi: ${snap.error}')),
                        const SizedBox(height: 8),
                        Center(
                          child: FilledButton.icon(
                            onPressed: _reload,
                            icon: const Icon(Icons.refresh),
                            label: const Text('Thử lại'),
                          ),
                        ),
                      ],
                    );
                  }

                  final orders = snap.data ?? [];
                  if (orders.isEmpty) {
                    return const Center(child: Text('Chưa có đơn nào'));
                  }

                  return ListView.separated(
                    itemCount: orders.length,
                    separatorBuilder: (_, __) => const Divider(height: 1),
                    itemBuilder: (context, i) {
                      final o = orders[i];
                      final dateText = _formatDate(o.date);
                      final statusText = _statusText(o.status);

                      return ExpansionTile(
                        title: Text(o.name ?? 'Đơn #${o.id ?? ''}'),
                        subtitle: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            if ((o.address ?? '').isNotEmpty) Text('Địa chỉ: ${o.address}'),
                            Row(
                              children: [
                                if ((o.phone ?? '').isNotEmpty) Text('ĐT: ${o.phone}'),
                                if (dateText.isNotEmpty) ...[
                                  const SizedBox(width: 12),
                                  Text(dateText, style: const TextStyle(color: Colors.black54)),
                                ],
                              ],
                            ),
                          ],
                        ),
                        trailing: Text(statusText, style: const TextStyle(color: Colors.black54, fontSize: 12)),
                        children: [
                          _OrderDetails(orderId: o.id),
                        ],
                      );
                    },
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _OrderDetails extends StatefulWidget {
  final int? orderId;
  const _OrderDetails({required this.orderId});

  @override
  State<_OrderDetails> createState() => _OrderDetailsState();
}

class _OrderDetailsState extends State<_OrderDetails> {
  Future<List<OrderDetailModel>>? _future;

  @override
  void initState() {
    super.initState();
    if (widget.orderId != null) {
      _future = OrderService.getOrderDetails(widget.orderId!);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_future == null) return const SizedBox.shrink();

    return FutureBuilder<List<OrderDetailModel>>(
      future: _future,
      builder: (context, snap) {
        if (snap.connectionState == ConnectionState.waiting) {
          return const Padding(
            padding: EdgeInsets.all(12.0),
            child: Center(child: CircularProgressIndicator()),
          );
        }
        if (snap.hasError) {
          return Padding(
            padding: const EdgeInsets.all(12.0),
            child: Text('Lỗi tải chi tiết: ${snap.error}'),
          );
        }
        final items = snap.data ?? [];
        if (items.isEmpty) {
          return const Padding(
            padding: EdgeInsets.all(12.0),
            child: Text('Không có chi tiết'),
          );
        }

        return Padding(
          padding: const EdgeInsets.only(left: 12, right: 12, bottom: 12),
          child: Column(
            children: items.map((it) => _OrderDetailRow(item: it)).toList(),
          ),
        );
      },
    );
  }
}

class _OrderDetailRow extends StatelessWidget {
  final OrderDetailModel item;
  const _OrderDetailRow({required this.item});

  @override
  Widget build(BuildContext context) {
    if (item.tourId == null) {
      return ListTile(
        contentPadding: EdgeInsets.zero,
        leading: const Icon(Icons.tour_outlined),
        title: Text('Tour: (không rõ)'),
        subtitle: Text('Số lượng: ${item.quantity ?? 1}'),
      );
    }

    // lấy tên tour (và giá nếu server có) để hiển thị đẹp hơn
    return FutureBuilder<Tour>(
      future: TourService.getTour(item.tourId!),
      builder: (context, snap) {
        final qty = item.quantity ?? 1;

        if (snap.connectionState == ConnectionState.waiting) {
          return const ListTile(
            contentPadding: EdgeInsets.zero,
            leading: SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2)),
            title: Text('Đang tải tour...'),
          );
        }
        if (snap.hasError || !snap.hasData) {
          return ListTile(
            contentPadding: EdgeInsets.zero,
            leading: const Icon(Icons.tour_outlined),
            title: Text('Tour #${item.tourId}'),
            subtitle: Text('Số lượng: $qty'),
          );
        }

        final tour = snap.data!;
        final name = tour.name ?? 'Tour #${item.tourId}';
        String subtitle = 'Số lượng: $qty';
        if ((tour.location ?? '').isNotEmpty) {
          subtitle += '  •  ${tour.location}';
        }

        return ListTile(
          contentPadding: EdgeInsets.zero,
          leading: const Icon(Icons.tour_outlined),
          title: Text(name),
          subtitle: Text(subtitle),
        );
      },
    );
  }
}
