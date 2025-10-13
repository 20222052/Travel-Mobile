import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../config/api_config.dart';
import '../models/tour.dart';

class TourCard extends StatelessWidget {
  final Tour tour;
  final VoidCallback? onTap;
  final VoidCallback? onAdd;

  TourCard({super.key, required this.tour, this.onTap, this.onAdd});

  final _fmtCurrency = NumberFormat("#,##0", "vi_VN");

  @override
  Widget build(BuildContext context) {
    final imgUrl = _resolveImageUrl(tour.imageUrl);

    return Card(
      clipBehavior: Clip.antiAlias,
      elevation: 3,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: SizedBox(
        // KÍCH THƯỚC CỐ ĐỊNH - Nội dung bên trong sẽ co lại để vừa
        height: 220, // Tổng chiều cao cố định
        child: InkWell(
          onTap: onTap,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Phần 1: Ảnh - Chiều cao cố định
              SizedBox(
                height: 160,
                width: double.infinity,
                child: imgUrl != null && imgUrl.isNotEmpty
                    ? Ink.image(
                        image: NetworkImage(imgUrl),
                        fit: BoxFit.cover, // Đổi từ cover thành contain để hiển thị toàn bộ ảnh
                        alignment: Alignment.center,
                      )
                    : Container(
                        color: Colors.grey[200],
                        child: const Icon(Icons.image, size: 40, color: Colors.grey),
                      ),
              ),

              // Phần 2: Nội dung - Chiều cao cố định (còn lại)
              Expanded(
                child: Padding(
                  padding: const EdgeInsets.all(8),
                  child: Column(
                    mainAxisSize: MainAxisSize.max,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Tên tour - 1 dòng
                      Text(
                        tour.name ?? '(Chưa có tên)',
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          fontSize: 13,
                          fontWeight: FontWeight.w600,
                          height: 1.1,
                        ),
                      ),
                      const SizedBox(height: 4),
                      
                      // Phần dưới: Giá + Địa điểm + Nút
                      Expanded(
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            // Bên trái: Giá và địa điểm
                            Expanded(
                              child: Column(
                                mainAxisSize: MainAxisSize.min,
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  // Giá
                                  if (tour.price != null)
                                    Text(
                                      '${_fmtCurrency.format(tour.price)} đ',
                                      maxLines: 1,
                                      overflow: TextOverflow.ellipsis,
                                      style: const TextStyle(
                                        fontWeight: FontWeight.w700,
                                        fontSize: 12,
                                        color: Colors.redAccent,
                                        height: 1.1,
                                      ),
                                    ),
                                  
                                  // Địa điểm
                                  if ((tour.location ?? '').isNotEmpty) ...[
                                    const SizedBox(height: 2),
                                    Row(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        const Icon(
                                          Icons.place_outlined,
                                          size: 12,
                                          color: Colors.grey,
                                        ),
                                        const SizedBox(width: 2),
                                        Expanded(
                                          child: Text(
                                            tour.location!,
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                                            style: const TextStyle(
                                              fontSize: 10,
                                              color: Colors.black54,
                                              height: 1.1,
                                            ),
                                          ),
                                        ),
                                      ],
                                    ),
                                  ],
                                ],
                              ),
                            ),
                            
                            // Bên phải: Nút Add
                            SizedBox(
                              width: 32,
                              height: 32,
                              child: IconButton(
                                tooltip: 'Thêm vào giỏ',
                                onPressed: onAdd,
                                icon: const Icon(Icons.add_circle),
                                iconSize: 24,
                                color: Colors.blueAccent,
                                padding: EdgeInsets.zero,
                                constraints: const BoxConstraints(),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  String? _resolveImageUrl(String? url) {
    if (url == null || url.isEmpty) return null;
    if (url.startsWith("http://") || url.startsWith("https://")) return url;
    final clean = url.startsWith("/") ? url.substring(1) : url;
    return "${ApiConfig.uploadsPath}/$clean";
  }
}
