import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import '../models/tour.dart';
import '../services/tour_service.dart';
import '../state/session.dart';

class DetailTourScreen extends StatefulWidget {
  final int id;
  final Tour? initial; // dữ liệu truyền kèm (nếu có) để hiển thị nhanh

  const DetailTourScreen({
    super.key,
    required this.id,
    this.initial,
  });

  @override
  State<DetailTourScreen> createState() => _DetailTourScreenState();
}

class _DetailTourScreenState extends State<DetailTourScreen> {
  final _fmtCurrency = NumberFormat("#,##0", "vi_VN");
  Tour? _tour;
  late Future<Tour> _future;

  @override
  void initState() {
    super.initState();
    _tour = widget.initial;                // hiển thị tạm nếu có
    _future = TourService.getTour(widget.id); // fetch chi tiết từ server
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<Tour>(
      future: _future,
      builder: (context, snap) {
        // cập nhật dữ liệu khi fetch xong
        if (snap.connectionState == ConnectionState.done && snap.hasData) {
          _tour = snap.data!;
        }

        final t = _tour;
        final imgUrl = _resolveImageUrl(t?.imageUrl);

        return Scaffold(
          body: CustomScrollView(
            slivers: [
              SliverAppBar(
                pinned: true,
                stretch: true,
                expandedHeight: 260,
                leading: IconButton(
                  icon: const Icon(Icons.arrow_back),
                  onPressed: () => Navigator.of(context).maybePop(),
                ),
                actions: [
                  IconButton(
                    icon: const Icon(Icons.share_outlined),
                    onPressed: () {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Chức năng chia sẻ sẽ cập nhật sau')),
                      );
                    },
                  ),
                  IconButton(
                    icon: const Icon(Icons.favorite_border),
                    onPressed: () {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Đã thêm vào yêu thích (demo)')),
                      );
                    },
                  ),
                ],
                flexibleSpace: FlexibleSpaceBar(
                  title: Text(
                    t?.name ?? 'Chi tiết tour',
                    maxLines: 1,
                    style: Theme.of(context).textTheme.titleLarge?.copyWith(color: Colors.white),
                    overflow: TextOverflow.ellipsis,
                  ),
                  background: (imgUrl != null)
                      ? Stack(
                    fit: StackFit.expand,
                    children: [
                      Image.network(
                        imgUrl,
                        fit: BoxFit.cover,
                        errorBuilder: (_, __, ___) =>
                        const Center(child: Icon(Icons.broken_image, size: 72, color: Colors.white70)),
                      ),
                      // phủ gradient để chữ dễ đọc
                      Container(
                        decoration: const BoxDecoration(
                          gradient: LinearGradient(
                            begin: Alignment.topCenter,
                            end: Alignment.bottomCenter,
                            colors: [Colors.transparent, Colors.black38],
                          ),
                        ),
                      ),
                    ],
                  )
                      : Container(
                    color: Colors.blueGrey.shade100,
                    alignment: Alignment.center,
                    child: const Icon(Icons.image, size: 72),
                  ),
                ),
              ),

              // Nội dung chi tiết
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: _buildBody(context, t, snap),
                ),
              ),
            ],
          ),

          // Nút hành động dưới cùng
          bottomNavigationBar: SafeArea(
            top: false,
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
              child: Row(
                children: [
                  Expanded(
                    child: FilledButton.icon(
                        onPressed:  () {
                        if (Session.current.value == null) {
                          context.push('/login');
                          return;
                        }
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(content: Text('Đã thêm Tour')),
                        );
                      },
                      icon: const Icon(Icons.add),
                      label: const Text('Thêm vào giỏ hàng'),
                    ),
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }

  Widget _buildBody(BuildContext context, Tour? t, AsyncSnapshot<Tour> snap) {
    if (snap.hasError) {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const SizedBox(height: 40),
          const Icon(Icons.error_outline, size: 56, color: Colors.redAccent),
          const SizedBox(height: 12),
          Text('Lỗi tải dữ liệu:\n${snap.error}', textAlign: TextAlign.center),
          const SizedBox(height: 16),
          Center(
            child: FilledButton.icon(
              onPressed: () => setState(() => _future = TourService.getTour(widget.id)),
              icon: const Icon(Icons.refresh),
              label: const Text('Thử lại'),
            ),
          ),
        ],
      );
    }

    if (t == null && snap.connectionState == ConnectionState.waiting) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.symmetric(vertical: 80),
          child: CircularProgressIndicator(),
        ),
      );
    }

    // Hiển thị thông tin
    final rows = <Widget>[
      if (t?.price != null) _infoRow('Giá', '${_fmtCurrency.format(t!.price)} đ', bold: true, big: true),
      if ((t?.location ?? '').isNotEmpty) _infoRow('Địa điểm', t!.location!),
      if (t?.categoryId != null) _infoRow('Danh mục', '${t!.categoryId}'),
      if (t?.view != null) _infoRow('Lượt xem', '${t!.view}'),
      if (t?.createdDate != null)
        _infoRow('Ngày tạo', _formatDate(t!.createdDate!)),
      const SizedBox(height: 12),
      const Divider(),
      const SizedBox(height: 8),
      Text(
        'Giới thiệu',
        style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700),
      ),
      const SizedBox(height: 8),
      Text(
        _buildDescription(t),
        style: Theme.of(context).textTheme.bodyMedium,
      ),
      const SizedBox(height: 16),
    ];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: rows,
    );
  }

  Widget _infoRow(String label, String value, {bool bold = false, bool big = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 96,
            child: Text(label, style: const TextStyle(color: Colors.black54)),
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              value,
              style: TextStyle(
                fontWeight: bold ? FontWeight.w700 : FontWeight.w400,
                fontSize: big ? 16 : 14,
              ),
            ),
          ),
        ],
      ),
    );
  }

  String _formatDate(DateTime dt) {
    // dd/MM/yyyy HH:mm
    final d = dt.toLocal();
    return '${d.day.toString().padLeft(2, '0')}/${d.month.toString().padLeft(2, '0')}/${d.year} '
        '${d.hour.toString().padLeft(2, '0')}:${d.minute.toString().padLeft(2, '0')}';
  }

  String _buildDescription(Tour? t) {
    // Nếu backend chưa có field mô tả, hiển thị thông tin cơ bản thay thế
    final name = t?.name ?? 'Tour';
    final place = (t?.location ?? '').isNotEmpty ? t!.location! : 'điểm đến hấp dẫn';
    final price = (t?.price != null) ? '${_fmtCurrency.format(t!.price)} đ' : 'liên hệ';
    return 'Khám phá $name tại $place với mức giá $price. '
        'Lịch trình và thông tin chi tiết sẽ được cập nhật trong phiên bản tiếp theo.';
  }

  String? _resolveImageUrl(String? url) {
    const baseUrl = "https://10.0.2.2:44364/Uploads";
    if (url == null || url.isEmpty) return null;
    if (url.startsWith("http://") || url.startsWith("https://")) return url;
    final clean = url.startsWith("/") ? url.substring(1) : url;
    return "$baseUrl/$clean";
  }
}
