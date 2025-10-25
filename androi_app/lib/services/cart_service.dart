import 'dart:convert';
import 'package:http/http.dart' as http;

import 'api_client.dart';
import '../models/cart_item.dart';
import '../state/cart_state.dart';

class CartService {
  static final _api = ApiClient.instance;
  static const _timeout = Duration(seconds: 20);
  static final _cartState = CartState();

  static Exception _ex(http.Response res) {
    final body = utf8.decode(res.bodyBytes);
    try {
      final m = jsonDecode(body);
      final msg = (m is Map && m['message'] != null) ? m['message'].toString() : body;
      return Exception('HTTP ${res.statusCode}: $msg');
    } catch (_) {
      return Exception('HTTP ${res.statusCode}: $body');
    }
  }

  /// GET /api/ApiCart
  static Future<List<CartItemModel>> getCart() async {
    final res = await _api.client
        .get(_api.uri('/api/ApiCart'), headers: _api.authJsonHeaders())
        .timeout(_timeout);
    if (res.statusCode != 200) throw _ex(res);

    final data = jsonDecode(utf8.decode(res.bodyBytes));
    if (data is List) {
      final items = data.map((e) => CartItemModel.fromJson(e as Map<String, dynamic>)).toList();
      // Cập nhật tổng số lượng sản phẩm (sum của quantity)
      final totalQuantity = items.fold<int>(0, (sum, item) => sum + item.quantity);
      _cartState.updateCount(totalQuantity);
      return items;
    }
    throw Exception('Định dạng JSON không đúng (expected List)');
  }

  /// POST /api/ApiCart/add  body: {tourId, quantity}
  static Future<void> addToCart(int tourId, {int quantity = 1}) async {
    final res = await _api.client
        .post(
      _api.uri('/api/ApiCart/add'),
      headers: _api.authJsonHeaders(),
      body: jsonEncode({'tourId': tourId, 'quantity': quantity}),
    )
        .timeout(_timeout);
    if (res.statusCode != 200) throw _ex(res);
    
    // Tăng số lượng trong state theo số lượng thêm vào
    for (int i = 0; i < quantity; i++) {
      _cartState.increment();
    }
  }

  /// PUT /api/ApiCart/{id}  body: {quantity}
  static Future<void> updateQuantity(int cartItemId, int quantity) async {
    final res = await _api.client
        .put(
      _api.uri('/api/ApiCart/$cartItemId'),
      headers: _api.authJsonHeaders(),
      body: jsonEncode({'quantity': quantity}),
    )
        .timeout(_timeout);
    if (res.statusCode != 200) throw _ex(res);
    
    // Sau khi update quantity, reload lại giỏ hàng để cập nhật tổng số lượng chính xác
    await getCart();
  }

  /// DELETE /api/ApiCart/{id}
  static Future<void> deleteItem(int cartItemId) async {
    final res = await _api.client
        .delete(_api.uri('/api/ApiCart/$cartItemId'), headers: _api.authJsonHeaders())
        .timeout(_timeout);
    if (res.statusCode != 200) throw _ex(res);
    
    // Sau khi xóa, reload lại giỏ hàng để cập nhật tổng số lượng chính xác
    await getCart();
  }

  /// POST /api/ApiCart/checkout  body: {name, phone, address}
  static Future<int> checkout({required String name, required String phone, required String address}) async {
    final res = await _api.client
        .post(
      _api.uri('/api/ApiCart/checkout'),
      headers: _api.authJsonHeaders(),
      body: jsonEncode({'name': name, 'phone': phone, 'address': address}),
    )
        .timeout(_timeout);
    if (res.statusCode != 200) throw _ex(res);

    final map = jsonDecode(utf8.decode(res.bodyBytes)) as Map<String, dynamic>;
    final id = map['orderId'] ?? map['OrderId'] ?? map['id'] ?? map['Id'];
    
    // Reset giỏ hàng sau khi checkout
    _cartState.reset();
    
    return (id as num).toInt();
  }
}
