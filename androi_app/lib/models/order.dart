class OrderModel {
  final int? id;
  final int? userId;
  final String? name;
  final String? address;
  final String? phone;
  final DateTime? date; // server: Date
  final int? status;    // server: byte? -> dùng int? bên client

  OrderModel({
    this.id,
    this.userId,
    this.name,
    this.address,
    this.phone,
    this.date,
    this.status,
  });

  factory OrderModel.fromJson(Map<String, dynamic> j) {
    int? _toInt(dynamic v) {
      if (v == null) return null;
      if (v is int) return v;
      if (v is num) return v.toInt();
      if (v is String) return int.tryParse(v);
      return null;
    }

    DateTime? _toDate(dynamic v) {
      if (v == null) return null;
      if (v is DateTime) return v;
      if (v is String) return DateTime.tryParse(v);
      return null;
    }

    // chấp nhận cả camelCase và PascalCase từ server
    return OrderModel(
      id: _toInt(j['id'] ?? j['Id']),
      userId: _toInt(j['userId'] ?? j['UserId']),
      name: (j['name'] ?? j['Name'])?.toString(),
      address: (j['address'] ?? j['Address'])?.toString(),
      phone: (j['phone'] ?? j['Phone'])?.toString(),
      date: _toDate(j['date'] ?? j['Date']),
      status: _toInt(j['status'] ?? j['Status']),
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'userId': userId,
    'name': name,
    'address': address,
    'phone': phone,
    'date': date?.toIso8601String(),
    'status': status,
  };

  OrderModel copyWith({
    int? id,
    int? userId,
    String? name,
    String? address,
    String? phone,
    DateTime? date,
    int? status,
  }) {
    return OrderModel(
      id: id ?? this.id,
      userId: userId ?? this.userId,
      name: name ?? this.name,
      address: address ?? this.address,
      phone: phone ?? this.phone,
      date: date ?? this.date,
      status: status ?? this.status,
    );
  }
}
