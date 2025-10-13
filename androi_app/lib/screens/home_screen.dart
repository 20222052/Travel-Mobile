import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../services/cart_service.dart';
import '../services/tour_service.dart';
import '../models/tour.dart';
import '../widgets/app_topbar.dart';
import '../widgets/category_strip.dart';
import '../widgets/button_nav.dart';
import '../widgets/tour_card.dart';

import '../state/session.dart';
import '../models/user.dart';
import '../services/account_service.dart';

// blog
import 'blog_list_tab.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});
  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  int _tabIndex = 0;

  // TOUR (tab 0)
  late Future<List<Tour>> _future;
  final _searchCtrl = TextEditingController();
  String? _currentQuery;
  int? _selectedCatId;
  String _sort = 'desc';

  // BLOG (tab 1) – dùng chung _searchCtrl nhưng state riêng
  final _blogCtrl = BlogListTabController();
  String? _blogQuery;
  String _blogSort = 'desc';

  final _homeKey = const PageStorageKey('home-scroll');
  final _accountKey = const PageStorageKey('account-scroll');

  @override
  void initState() {
    super.initState();
    _future = TourService.getTours();
  }

  // ---- TOUR reload
  void _reloadTours({String? query, int? categoryId, String? sort}) {
    _currentQuery  = (query ?? _currentQuery)?.trim();
    _selectedCatId = categoryId ?? _selectedCatId;
    _sort          = (sort ?? _sort);
    final fut = TourService.getTours(
      name: (_currentQuery?.isEmpty ?? true) ? null : _currentQuery,
      categoryId: _selectedCatId,
      sort: _sort,
    );
    if (!mounted) return;
    setState(() { _future = fut; });
  }

  // reset từng tab
  void _resetHomeFilters() {
    _searchCtrl.clear();
    _currentQuery = null;
    _selectedCatId = null;
    _sort = 'desc';
    _reloadTours();
  }

  void _resetBlogFilters() {
    _searchCtrl.clear();
    _blogQuery = null;
    _blogSort = 'desc';
    _blogCtrl.apply(query: '', sort: 'desc'); // trigger reload reset
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final isBlog = _tabIndex == 1;

    final PreferredSizeWidget appBar = AppTopBar(
      searchCtrl: _searchCtrl,
      onCart: () {
        if (Session.current.value == null) {
          context.push('/login'); return;
        }
        context.push('/cart');
      },
      onSearchSubmitted: (q) {
        if (isBlog) {
          _blogQuery = q;
          _blogCtrl.apply(query: q);
        } else {
          _reloadTours(query: q);
        }
      },
      currentSort: isBlog ? _blogSort : _sort,
      onSortChanged: (v) {
        if (isBlog) {
          _blogSort = v;
          _blogCtrl.apply(sort: v);
        } else {
          _reloadTours(sort: v);
        }
      },
      hintText: isBlog ? 'Tìm bài viết…' : 'Tìm tour…',
    );

    return Scaffold(
      appBar: appBar,
      body: IndexedStack(
        index: _tabIndex,
        children: [
          _buildHomeTab(),
          // Tab bài viết dùng external controls
          BlogListTab(useExternalControls: true, controller: _blogCtrl),
          _placeholder('Diễn đàn'),
          _buildAccountTab(),
        ],
      ),
      bottomNavigationBar: BottomNav(
        currentIndex: _tabIndex,
        onTap: (i) async {
          // Nhấn lại chính tab -> reset filter của tab đó
          if (_tabIndex == i) {
            if (i == 0) _resetHomeFilters();
            if (i == 1) _resetBlogFilters();
            return;
          }

          // Diễn đàn/Tài khoản yêu cầu đăng nhập
          if ((i == 2 || i == 3) && !Session.isLoggedIn) {
            await context.push('/login');
            if (!Session.isLoggedIn) return;
          }

          if (!mounted) return;
          setState(() => _tabIndex = i);
        },
      ),
    );
  }

  // ---------- HOME TAB (tour list) ----------
  Widget _buildHomeTab() {
    return RefreshIndicator(
      onRefresh: () async => _reloadTours(),
      child: FutureBuilder<List<Tour>>(
        future: _future,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return ListView(
              key: _homeKey, padding: const EdgeInsets.all(16),
              children: [
                const SizedBox(height: 120),
                const Align(child: Icon(Icons.error_outline, size: 56, color: Colors.redAccent)),
                const SizedBox(height: 12),
                Align(child: Text('Lỗi tải dữ liệu:\n${snap.error}', textAlign: TextAlign.center)),
                const SizedBox(height: 16),
                Align(
                  child: FilledButton.icon(
                    onPressed: () => _reloadTours(),
                    icon: const Icon(Icons.refresh),
                    label: const Text('Thử lại'),
                  ),
                ),
              ],
            );
          }

          final tours = snap.data ?? [];
          return CustomScrollView(
            key: _homeKey,
            slivers: [
              SliverToBoxAdapter(
                child: CategoryStrip(
                  onTap: (key) {
                    final id = _mapCategoryKeyToId(key);
                    _reloadTours(categoryId: id);
                  },
                ),
              ),
              if (tours.isEmpty)
                const SliverToBoxAdapter(
                  child: Padding(
                    padding: EdgeInsets.symmetric(vertical: 120),
                    child: Center(child: Text('Không có tour nào')),
                  ),
                )
              else
                SliverPadding(
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                  sliver: SliverGrid.builder(
                    gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 2,
                      mainAxisSpacing: 12,
                      crossAxisSpacing: 12,
                      childAspectRatio: 0.78,
                    ),
                    itemCount: tours.length,
                    itemBuilder: (context, i) {
                      final t = tours[i];
                      return TourCard(
                        tour: t,
                        onTap: () {
                          if (t.id != null) context.push('/tour/${t.id}', extra: t);
                        },
                        onAdd: () async {
                          if (Session.current.value == null) {
                            context.push('/login'); return;
                          }
                          try {
                            await CartService.addToCart(t.id!);
                            if (!context.mounted) return;
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(content: Text('Đã thêm: ${t.name ?? 'Tour'}')),
                            );
                          } catch (e) {
                            if (!context.mounted) return;
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(content: Text('Không thêm được: $e')),
                            );
                          }
                        },
                      );
                    },
                  ),
                ),
            ],
          );
        },
      ),
    );
  }

  // ---------- ACCOUNT TAB ----------
  Widget _buildAccountTab() {
    return ValueListenableBuilder<User?>(
      valueListenable: Session.current,
      builder: (context, me, _) {
        if (me == null) {
          return ListView(
            key: _accountKey,
            padding: const EdgeInsets.all(16),
            children: [
              const SizedBox(height: 32),
              const Icon(Icons.person_outline, size: 72, color: Colors.blueGrey),
              const SizedBox(height: 12),
              const Center(child: Text('Bạn chưa đăng nhập')),
              const SizedBox(height: 12),
              FilledButton(onPressed: () => context.push('/login'), child: const Text('Đăng nhập')),
              TextButton(onPressed: () => context.push('/register'), child: const Text('Chưa có tài khoản? Đăng ký')),
            ],
          );
        }
        return ListView(
          key: _accountKey,
          padding: const EdgeInsets.all(16),
          children: [
            Row(
              children: [
                CircleAvatar(
                  radius: 28,
                  backgroundImage: (me.image?.isNotEmpty == true)
                      ? NetworkImage('https://10.0.2.2:5014/Uploads/${me.image}')
                      : null,
                  child: (me.image?.isNotEmpty == true)
                      ? null
                      : Text((me.name ?? me.username ?? 'U').characters.first.toUpperCase()),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(me.name ?? me.username ?? '',
                          style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
                      if (me.email != null) Text(me.email!, style: const TextStyle(color: Colors.black54)),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            ListTile(leading: const Icon(Icons.person_outline), title: const Text('Hồ sơ của tôi'),
                trailing: const Icon(Icons.chevron_right), onTap: () => context.push('/account/profile')),
            ListTile(leading: const Icon(Icons.history), title: const Text('Lịch sử đơn hàng'),
                trailing: const Icon(Icons.chevron_right), onTap: () => context.push('/account/history')),
            const Divider(),
            ListTile(leading: const Icon(Icons.person), title: const Text('Hồ sơ'), subtitle: Text(me.username ?? '')),
            if (me.phone != null) ListTile(leading: const Icon(Icons.phone), title: Text(me.phone!)),
            if (me.address != null) ListTile(leading: const Icon(Icons.home), title: Text(me.address!)),
            const SizedBox(height: 12),
            FilledButton.icon(
              onPressed: () async {
                try { await AccountService.logout(); } catch (_) {}
                Session.clear();
                if (!context.mounted) return;
                ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Đã đăng xuất')));
                if (!mounted) return;
                setState(() {});
              },
              icon: const Icon(Icons.logout), label: const Text('Đăng xuất'),
            ),
          ],
        );
      },
    );
  }

  Widget _placeholder(String title) {
    return ListView(
      children: [
        const SizedBox(height: 24),
        Center(child: Text('Đây là $title', style: const TextStyle(fontSize: 16))),
      ],
    );
  }

  int? _mapCategoryKeyToId(String key) {
    switch (key) {
      case 'adventure': return 1;
      case 'relax':     return 2;
      case 'culture':   return 3;
      case 'food':      return 5;
      default:          return null;
    }
  }
}
