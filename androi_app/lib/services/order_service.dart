// lib/services/order_service.dart
import 'dart:convert';
import 'package:http/http.dart' as http;

import '../models/order.dart';
import '../models/order_detail.dart';
import 'api_client.dart';

class OrderService {
  static final _api = ApiClient.instance;
  static const _timeout = Duration(seconds: 20);

  static Future<List<OrderModel>> getOrders(
      int userId, {
        String? search,
        String sort = 'desc',
      }) async {
    final uri = _api.uri('/api/ApiOrder', {
      'id': '$userId',
      if (search != null && search.trim().isNotEmpty) 'search': search.trim(),
      'sort': sort,
    });

    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }

    final data = jsonDecode(utf8.decode(res.bodyBytes));
    if (data is List) {
      return data.map((e) => OrderModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw Exception('Định dạng JSON không đúng kỳ vọng (expected List)');
  }

  /// GET /api/ApiOrder/orderDetail/{orderId}
  static Future<List<OrderDetailModel>> getOrderDetails(int orderId) async {
    final uri = _api.uri('/api/ApiOrder/orderDetail/$orderId');

    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }

    final data = jsonDecode(utf8.decode(res.bodyBytes));
    if (data is List) {
      return data.map((e) => OrderDetailModel.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw Exception('Định dạng JSON không đúng kỳ vọng (expected List)');
  }
}
