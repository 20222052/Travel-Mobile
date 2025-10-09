import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
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
      child: InkWell(
        onTap: onTap,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Ảnh trên cùng (ripple mượt)
            if (imgUrl != null && imgUrl.isNotEmpty)
              Ink.image(
                image: NetworkImage(imgUrl),
                fit: BoxFit.cover,
                height: 120,
                child: const SizedBox.expand(),
                alignment: Alignment.center,
              )
            else
              const SizedBox(
                height: 140,
                child: Center(child: Icon(Icons.image, size: 50)),
              ),

            // Nội dung
            Padding(
              padding: const EdgeInsets.all(12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    tour.name ?? '(Chưa có tên)',
                    maxLines: 2,
                    softWrap: true,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      // Trái
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            if (tour.price != null)
                              Text(
                                '${_fmtCurrency.format(tour.price)} đ',
                                style: const TextStyle(
                                  fontWeight: FontWeight.w700,
                                  fontSize: 15,
                                  color: Colors.redAccent,
                                ),
                              ),
                            if ((tour.location ?? '').isNotEmpty) ...[
                              const SizedBox(height: 4),
                              Row(
                                children: [
                                  const Icon(Icons.place_outlined, size: 18, color: Colors.grey),
                                  const SizedBox(width: 4),
                                  Expanded(
                                    child: Text(
                                      tour.location!,
                                      maxLines: 1,
                                      overflow: TextOverflow.ellipsis,
                                      style: const TextStyle(fontSize: 13.5, color: Colors.black54),
                                    ),
                                  ),
                                ],
                              ),
                            ],
                          ],
                        ),
                      ),

                      // Phải: nút +
                      IconButton(
                        tooltip: 'Thêm vào giỏ',
                        onPressed: onAdd,
                        icon: const Icon(Icons.add_circle),
                        iconSize: 30,
                        color: Colors.blueAccent,
                        splashRadius: 22,
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  String? _resolveImageUrl(String? url) {
    const baseUrl = "https://10.0.2.2:44364/Uploads";
    if (url == null || url.isEmpty) return null;
    if (url.startsWith("http://") || url.startsWith("https://")) return url;
    final clean = url.startsWith("/") ? url.substring(1) : url;
    return "$baseUrl/$clean";
  }
}
