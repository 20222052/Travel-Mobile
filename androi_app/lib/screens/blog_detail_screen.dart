import 'package:flutter/material.dart';
import 'package:flutter_html/flutter_html.dart';

import '../config/api_config.dart';
import '../models/blog.dart';
import '../services/blog_service.dart';

class BlogDetailScreen extends StatefulWidget {
  final int id;
  final Blog? initial;

  const BlogDetailScreen({super.key, required this.id, this.initial});

  @override
  State<BlogDetailScreen> createState() => _BlogDetailScreenState();
}

class _BlogDetailScreenState extends State<BlogDetailScreen> {
  Blog? _blog;
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _blog = widget.initial;
    _loadIfNeeded();
  }

  Future<void> _loadIfNeeded() async {
    if (_blog != null) return;
    await _loadById(widget.id);
  }

  Future<void> _loadById(int id) async {
    setState(() => _loading = true);
    try {
      final b = await BlogService.getBlog(id);
      if (!mounted) return;
      setState(() => _blog = b);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Lỗi tải bài viết: $e')));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  // Dò bài trước/sau theo id±1, bỏ qua id trống (tối đa 20 lần)
  Future<void> _loadAdjacent(int step) async {
    if (_loading) return;
    final currentId = _blog?.id ?? widget.id;
    int candidate = currentId + step;

    setState(() => _loading = true);
    try {
      int tries = 0;
      while (tries < 20 && candidate > 0) {
        try {
          final b = await BlogService.getBlog(candidate);
          if (!mounted) return;
          setState(() => _blog = b);
          return;
        } catch (e) {
          final msg = e.toString();
          if (!msg.contains('HTTP 404')) {
            if (!mounted) return;
            ScaffoldMessenger.of(context)
                .showSnackBar(SnackBar(content: Text('Không chuyển được: $e')));
            return;
          }
          candidate += step;
          tries++;
        }
      }
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(step < 0 ? 'Không còn bài trước.' : 'Không còn bài sau.')),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  String _imgUrl(String? path) {
    if (path == null || path.isEmpty) return '';
    if (path.startsWith('http')) return path;
    final host = ApiConfig.uploadsPath;
    final p = path.startsWith('/') ? path : '/$path';
    return '$host$p';
  }

  @override
  Widget build(BuildContext context) {
    final b = _blog;

    return Scaffold(
      appBar: AppBar(
        title: Text(
          b?.title ?? 'Chi tiết bài viết',
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
      ),

      // Nội dung cuộn được
      body: _loading && b == null
          ? const Center(child: CircularProgressIndicator())
          : (b == null)
          ? Center(
        child: OutlinedButton.icon(
          onPressed: () => _loadById(widget.id),
          icon: const Icon(Icons.refresh),
          label: const Text('Thử lại'),
        ),
      )
          : SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if ((b.image ?? '').isNotEmpty) ...[
              ClipRRect(
                borderRadius: BorderRadius.circular(12),
                child: Image.network(
                  _imgUrl(b.image),
                  fit: BoxFit.cover,
                  errorBuilder: (_, __, ___) => Container(
                    height: 160,
                    alignment: Alignment.center,
                    color: Colors.blue.shade50,
                    child: const Icon(Icons.image, color: Colors.grey),
                  ),
                ),
              ),
              const SizedBox(height: 12),
            ],
            Text(
              b.title ?? '',
              style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w700),
            ),
            const SizedBox(height: 12),

            // Hiển thị HTML
            Html(
              data: b.content ?? '',
              style: {
                'body': Style(fontSize: FontSize(16.0), color: Colors.black87),
                'p': Style(margin: Margins.only(bottom: 12)),
                'img': Style(margin: Margins.symmetric(vertical: 8)),
              },
            ),
            const SizedBox(height: 80), // chừa chỗ cho bottom bar
          ],
        ),
      ),

      // Hai nút cố định dưới cùng
      bottomNavigationBar: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
          child: Row(
            children: [
              Expanded(
                child: OutlinedButton.icon(
                  onPressed: _loading ? null : () => _loadAdjacent(-1),
                  icon: const Icon(Icons.arrow_back_ios_new),
                  label: const Text('Bài trước'),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: OutlinedButton.icon(
                  onPressed: _loading ? null : () => _loadAdjacent(1),
                  icon: const Icon(Icons.arrow_forward_ios),
                  label: const Text('Bài sau'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
