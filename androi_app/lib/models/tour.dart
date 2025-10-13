class Tour {
  final int? id;
  final String? name;
  final int? price;
  final String? location;
  final String? imageUrl;
  final int? categoryId;
  final DateTime? createdDate;
  final int? view;
  final String? description; // Thêm field mô tả chi tiết

  Tour({
    this.id,
    this.name,
    this.price,
    this.location,
    this.imageUrl,
    this.categoryId,
    this.createdDate,
    this.view,
    this.description, // Thêm vào constructor
  });

  factory Tour.fromJson(Map<String, dynamic> json) {
    return Tour(
      id: json['id'] as int?,
      name: json['name'] as String?,
      price: json['price'] is int
          ? json['price'] as int
          : (json['price'] is double ? (json['price'] as double).toInt() : null),
      location: json['location'] as String?,
      imageUrl: json['imageUrl'] as String? ?? json['image'] as String?,
      categoryId: json['categoryId'] as int?,
      createdDate: json['createdDate'] != null
          ? DateTime.tryParse(json['createdDate'].toString())
          : null,
      view: json['view'] as int?,
      description: json['description'] as String?, // Thêm parse description
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'name': name,
    'price': price,
    'location': location,
    'imageUrl': imageUrl,
    'categoryId': categoryId,
    'createdDate': createdDate?.toIso8601String(),
    'view': view,
    'description': description, // Thêm vào JSON serialization
  };
}
