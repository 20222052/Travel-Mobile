class OrderDetailModel {
  final int? id;
  final int? orderId;
  final int? tourId;
  final int? quantity;

  OrderDetailModel({
    this.id,
    this.orderId,
    this.tourId,
    this.quantity,
  });

  factory OrderDetailModel.fromJson(Map<String, dynamic> j) {
    int? _toInt(dynamic v) {
      if (v == null) return null;
      if (v is int) return v;
      if (v is num) return v.toInt();
      if (v is String) return int.tryParse(v);
      return null;
    }

    return OrderDetailModel(
      id: _toInt(j['id'] ?? j['Id']),
      orderId: _toInt(j['orderId'] ?? j['OrderId']),
      tourId: _toInt(j['tourId'] ?? j['TourId']),
      quantity: _toInt(j['quantity'] ?? j['Quantity']),
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'orderId': orderId,
    'tourId': tourId,
    'quantity': quantity,
  };

  OrderDetailModel copyWith({
    int? id,
    int? orderId,
    int? tourId,
    int? quantity,
  }) {
    return OrderDetailModel(
      id: id ?? this.id,
      orderId: orderId ?? this.orderId,
      tourId: tourId ?? this.tourId,
      quantity: quantity ?? this.quantity,
    );
  }
}
