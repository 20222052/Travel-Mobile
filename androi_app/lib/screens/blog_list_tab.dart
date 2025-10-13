import 'package:flutter/material.dart';
import 'package:flutter_html/flutter_html.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../config/api_config.dart';
import '../models/blog.dart';
import '../services/blog_service.dart';

class BlogListTabController {
  void Function({String? query, String? sort})? _apply;
  void apply({String? query, String? sort}) => _apply?.call(query: query, sort: sort);
}

class BlogListTab extends StatefulWidget {
  final bool useExternalControls;            // dùng topbar bên ngoài (Home)
  final BlogListTabController? controller;   // để Home điều khiển

  const BlogListTab({
    super.key,
    this.useExternalControls = false,
    this.controller,
  });

  @override
  State<BlogListTab> createState() => _BlogListTabState();
}

class _BlogListTabState extends State<BlogListTab> {
  final _fmtDate = DateFormat('dd/MM/yyyy');
  final _scroll = ScrollController();

  List<Blog> _items = [];
  int _page = 1;
  final int _pageSize = 6;
  int _totalPages = 1;
  bool _loading = false;
  bool _loadingMore = false;

  // filter điều khiển từ Home
  String? _query;
  String _sort = 'desc';

  // chỉ dùng khi không có external controls
  final _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    widget.controller?._apply = _applyFromOutside; // hook controller
    _scroll.addListener(_onScroll);
    _load(reset: true);
  }

  @override
  void dispose() {
    _scroll.dispose();
    _searchCtrl.dispose();
    super.dispose();
  }

  // Home gọi vào đây để đổi query/sort và reload
  void _applyFromOutside({String? query, String? sort}) {
    if (query != null) _query = query.trim().isEmpty ? null : query.trim();
    if (sort != null) _sort = sort;
    _load(reset: true);
  }

  void _onScroll() {
    if (_loadingMore || _loading) return;
    if (_page >= _totalPages) return;
    if (_scroll.position.pixels >= _scroll.position.maxScrollExtent - 200) {
      _loadMore();
    }
  }

  Future<void> _load({bool reset = false}) async {
    if (_loading) return;
    setState(() => _loading = true);

    try {
      final page = reset ? 1 : _page;
      final resp = await BlogService.getBlogs(
        page: page,
        pageSize: _pageSize,
        title: _query,
        sort: _sort,
      );

      if (!mounted) return;
      setState(() {
        _totalPages = resp.totalPages;
        _page = page;
        if (reset) {
          _items = List<Blog>.from(resp.items);
        } else {
          _items = List<Blog>.from(_items)..addAll(resp.items);
        }
      });
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Lỗi tải bài viết: $e')));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _loadMore() async {
    if (_loadingMore || _page >= _totalPages) return;
    setState(() => _loadingMore = true);
    try {
      final next = _page + 1;
      final resp = await BlogService.getBlogs(
        page: next,
        pageSize: _pageSize,
        title: _query,
        sort: _sort,
      );
      if (!mounted) return;
      setState(() {
        _page = next;
        _totalPages = resp.totalPages;
        _items = List<Blog>.from(_items)..addAll(resp.items);
      });
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Lỗi tải thêm: $e')));
    } finally {
      if (mounted) setState(() => _loadingMore = false);
    }
  }

  String _imgUrl(String? path) {
    if (path == null || path.isEmpty) return '';
    if (path.startsWith('http')) return path;
    final host = ApiConfig.uploadsPath;
    final p = path.startsWith('/') ? path : '/$path';
    return '$host$p';
  }

  String _plainText(String? html) {
    if (html == null) return '';
    return html
        .replaceAll(RegExp(r'<[^>]+>'), '')   // bỏ thẻ
        .replaceAll('&nbsp;', ' ')
        .trim();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        if (!widget.useExternalControls) ...[
          // Thanh tìm kiếm
          Padding(
            padding: const EdgeInsets.fromLTRB(12, 8, 12, 0),
            child: SizedBox(
              height: 40,
              child: TextField(
                controller: _searchCtrl,
                textInputAction: TextInputAction.search,
                onSubmitted: (q) => _applyFromOutside(query: q),
                decoration: InputDecoration(
                  hintText: 'Tìm bài viết…',
                  prefixIcon: const Icon(Icons.search),
                  filled: true,
                  fillColor: Colors.grey.shade100,
                  contentPadding: const EdgeInsets.symmetric(horizontal: 12),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: BorderSide.none,
                  ),
                ),
              ),
            ),
          ),
          const SizedBox(height: 8),
        ],

        Expanded(
          child: RefreshIndicator(
            onRefresh: () async {
              if (!widget.useExternalControls) _searchCtrl.clear();
              _applyFromOutside(query: '', sort: 'desc'); // reset
            },
            child: ListView.separated(
              controller: _scroll,
              itemCount: _items.length + 1,
              padding: const EdgeInsets.fromLTRB(12, 8, 12, 12),
              separatorBuilder: (_, __) => const SizedBox(height: 10),
              itemBuilder: (_, i) {
                if (i == _items.length) {
                  return (_loadingMore)
                      ? const Padding(
                    padding: EdgeInsets.symmetric(vertical: 12),
                    child: Center(child: CircularProgressIndicator()),
                  )
                      : const SizedBox.shrink();
                }

                final b = _items[i];
                final img = _imgUrl(b.image);
                final dateStr = (b.postedDate != null) ? _fmtDate.format(b.postedDate!) : '';

                return InkWell(
                  onTap: () {
                    if (b.id != null) {
                      context.push('/blog/${b.id}', extra: b);
                    }
                  },
                  borderRadius: BorderRadius.circular(12),
                  child: Container(
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.black12),
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        ClipRRect(
                          borderRadius: const BorderRadius.horizontal(left: Radius.circular(12)),
                          child: (img.isNotEmpty)
                              ? Image.network(
                            img,
                            width: 120, height: 90, fit: BoxFit.cover,
                            errorBuilder: (_, __, ___) => Container(
                              width: 120, height: 90,
                              alignment: Alignment.center,
                              color: Colors.blue.shade50,
                              child: const Icon(Icons.image, color: Colors.grey),
                            ),
                          )
                              : Container(
                            width: 120, height: 90,
                            alignment: Alignment.center,
                            color: Colors.blue.shade50,
                            child: const Icon(Icons.image, color: Colors.grey),
                          ),
                        ),
                        const SizedBox(width: 10),
                        Expanded(
                          child: Padding(
                            padding: const EdgeInsets.fromLTRB(0, 10, 10, 10),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(b.title ?? '(Không có tiêu đề)',
                                  maxLines: 2, overflow: TextOverflow.ellipsis,
                                  style: const TextStyle(fontWeight: FontWeight.w600),
                                ),
                                const SizedBox(height: 6),
                                Row(
                                  children: [
                                    if (dateStr.isNotEmpty) ...[
                                      const Icon(Icons.event, size: 14, color: Colors.black45),
                                      const SizedBox(width: 4),
                                      Text(dateStr, style: const TextStyle(fontSize: 12, color: Colors.black54)),
                                      const SizedBox(width: 12),
                                    ],
                                    const Icon(Icons.visibility, size: 14, color: Colors.black45),
                                    const SizedBox(width: 4),
                                    Text('${b.view ?? 0}', style: const TextStyle(fontSize: 12, color: Colors.black54)),
                                  ],
                                ),
                                const SizedBox(height: 6),
                                Text(
                                  'Nhấn để xem chi tiết ...',
                                  maxLines: 2,
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ],
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
        ),
      ],
    );
  }
}
