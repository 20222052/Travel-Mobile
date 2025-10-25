import 'package:flutter/foundation.dart';

/// Global state quản lý số lượng item trong giỏ hàng
class CartState extends ChangeNotifier {
  static final CartState _instance = CartState._internal();
  factory CartState() => _instance;
  CartState._internal();

  int _itemCount = 0;

  int get itemCount => _itemCount;

  /// Cập nhật số lượng item trong giỏ hàng
  void updateCount(int count) {
    if (_itemCount != count) {
      _itemCount = count;
      notifyListeners();
    }
  }

  /// Tăng số lượng (khi thêm sản phẩm mới)
  void increment() {
    _itemCount++;
    notifyListeners();
  }

  /// Giảm số lượng (khi xóa sản phẩm)
  void decrement() {
    if (_itemCount > 0) {
      _itemCount--;
      notifyListeners();
    }
  }

  /// Reset về 0
  void reset() {
    _itemCount = 0;
    notifyListeners();
  }
}
