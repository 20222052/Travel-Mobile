import 'tour.dart';

class CartItemModel {
  final int? id;
  final int? cartId;
  final int? tourId;
  final int quantity;
  final Tour? tour;

  CartItemModel({
    this.id,
    this.cartId,
    this.tourId,
    required this.quantity,
    this.tour,
  });

  // Chịu được cả (Id) lẫn (id)
  factory CartItemModel.fromJson(Map<String, dynamic> json) {
    int? _i(dynamic v) => (v is num) ? v.toInt() : int.tryParse('$v');
    Map<String, dynamic>? _m(dynamic v) => (v is Map<String, dynamic>) ? v : null;

    return CartItemModel(
      id: _i(json['id'] ?? json['Id']),
      cartId: _i(json['cartId'] ?? json['CartId']),
      tourId: _i(json['tourId'] ?? json['TourId']),
      quantity: _i(json['quantity'] ?? json['Quantity']) ?? 0,
      tour: _m(json['tour'] ?? json['Tour']) != null ? Tour.fromJson(_m(json['tour'] ?? json['Tour'])!) : null,
    );
  }
}
