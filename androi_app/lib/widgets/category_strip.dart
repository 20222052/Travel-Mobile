import 'package:flutter/material.dart';

class CategoryStrip extends StatefulWidget {
  final void Function(String? key)? onTap;
  final String? selectedKey;

  const CategoryStrip({super.key, this.onTap, this.selectedKey});

  @override
  State<CategoryStrip> createState() => _CategoryStripState();
}

class _CategoryStripState extends State<CategoryStrip> {
  @override
  Widget build(BuildContext context) {
    final items = <_CatItem>[
      _CatItem('adventure', 'Thám hiểm', Icons.terrain_outlined),
      _CatItem('relax',     'Nghỉ dưỡng', Icons.spa_outlined),
      _CatItem('culture',   'Văn hóa', Icons.account_balance_outlined),
      _CatItem('food',      'Ăn uống', Icons.restaurant_menu),
    ];

    return SizedBox(
      height: 92,
      child: ListView.separated(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
        scrollDirection: Axis.horizontal,
        itemCount: items.length,
        separatorBuilder: (_, __) => const SizedBox(width: 10),
        itemBuilder: (context, i) {
          final it = items[i];
          final isSelected = widget.selectedKey == it.key;
          
          return Material(
            color: Colors.transparent,
            child: InkWell(
              onTap: () {
                // Toggle: nếu đang chọn thì bỏ chọn (gửi null), nếu chưa chọn thì chọn
                widget.onTap?.call(isSelected ? null : it.key);
              },
              borderRadius: BorderRadius.circular(12),
              child: Container(
                width: 110,
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: isSelected ? Colors.blue.shade50 : Colors.grey.shade100,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(
                    color: isSelected ? Colors.blue : Colors.black12,
                    width: isSelected ? 2 : 1,
                  ),
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      it.icon, 
                      size: 26, 
                      color: isSelected ? Colors.blue : Colors.blueGrey,
                    ),
                    const SizedBox(height: 6),
                    Text(
                      it.label,
                      textAlign: TextAlign.center,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: TextStyle(
                        fontSize: 12, 
                        fontWeight: FontWeight.w600,
                        color: isSelected ? Colors.blue : Colors.black87,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}

class _CatItem {
  final String key;
  final String label;
  final IconData icon;
  _CatItem(this.key, this.label, this.icon);
}
