import 'package:flutter/material.dart';
import '../config/api_config.dart';

class AppTopBar extends StatelessWidget implements PreferredSizeWidget {
  final TextEditingController searchCtrl;
  final VoidCallback? onCart;
  final ValueChanged<String>? onSearchSubmitted;

  final String currentSort; // 'desc' | 'asc'
  final ValueChanged<String>? onSortChanged;

  final String hintText; // ⬅️ thêm

  const AppTopBar({
    super.key,
    required this.searchCtrl,
    this.onCart,
    this.onSearchSubmitted,
    this.currentSort = 'desc',
    this.onSortChanged,
    this.hintText = 'Tìm tour…', // ⬅️ mặc định như cũ
  });

  @override
  Size get preferredSize => const Size.fromHeight(64);

  static String get _logoUrl => '${ApiConfig.baseUrl}/img/logo.png';

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      bottom: false,
      child: Container(
        padding: const EdgeInsets.fromLTRB(12, 8, 12, 8),
        decoration: BoxDecoration(
          color: Theme.of(context).colorScheme.surface,
          border: const Border(bottom: BorderSide(color: Color(0x11000000))),
        ),
        child: Row(
          children: [
            SizedBox(
              width: 36, height: 36,
              child: ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: Image.network(
                  _logoUrl, fit: BoxFit.contain,
                  errorBuilder: (_, __, ___) => Container(
                    color: Colors.blue.shade50, alignment: Alignment.center,
                    child: const Icon(Icons.image, color: Colors.grey, size: 22),
                  ),
                ),
              ),
            ),
            const SizedBox(width: 10),

            Expanded(
              child: SizedBox(
                height: 40,
                child: TextField(
                  controller: searchCtrl,
                  textInputAction: TextInputAction.search,
                  onSubmitted: onSearchSubmitted,
                  decoration: InputDecoration(
                    hintText: hintText,                    // ⬅️ dùng hint theo tab
                    prefixIcon: const Icon(Icons.search),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 12),
                    filled: true,
                    fillColor: Colors.grey.shade100,
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide.none,
                    ),
                  ),
                ),
              ),
            ),

            const SizedBox(width: 8),

            SizedBox(
              height: 40,
              child: DropdownButtonHideUnderline(
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10),
                  decoration: BoxDecoration(
                    color: Colors.grey.shade100,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.black12),
                  ),
                  child: DropdownButton<String>(
                    value: currentSort,
                    onChanged: (val) { if (val != null) onSortChanged?.call(val); },
                    items: const [
                      DropdownMenuItem(value: 'desc', child: Text('Mới nhất')),
                      DropdownMenuItem(value: 'asc', child: Text('Cũ nhất')),
                    ],
                  ),
                ),
              ),
            ),

            const SizedBox(width: 8),

            IconButton(
              onPressed: onCart,
              icon: const Icon(Icons.shopping_cart_outlined, size: 26),
              tooltip: 'Giỏ hàng',
            ),
          ],
        ),
      ),
    );
  }
}
