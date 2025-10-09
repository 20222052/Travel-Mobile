class User {
  final int? id;
  final String? username;
  final String? name;
  final String? role;
  final String? image;

  final String? gender;
  final String? address;
  final String? phone;
  final String? email;

  final DateTime? createdDate;
  final DateTime? dateOfBirth;

  User({
    this.id,
    this.username,
    this.name,
    this.role,
    this.image,
    this.gender,
    this.address,
    this.phone,
    this.email,
    this.createdDate,
    this.dateOfBirth,
  });

  factory User.fromJson(Map<String, dynamic> j) {
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

    // ASP.NET mặc định camelCase: id, username, name, ...
    return User(
      id: _toInt(j['id'] ?? j['Id']),
      username: (j['username'] ?? j['Username'])?.toString(),
      name: (j['name'] ?? j['Name'])?.toString(),
      role: (j['role'] ?? j['Role'])?.toString(),
      image: (j['image'] ?? j['Image'])?.toString(),
      gender: (j['gender'] ?? j['Gender'])?.toString(),
      address: (j['address'] ?? j['Address'])?.toString(),
      phone: (j['phone'] ?? j['Phone'])?.toString(),
      email: (j['email'] ?? j['Email'])?.toString(),
      createdDate: _toDate(j['createdDate'] ?? j['CreatedDate']),
      dateOfBirth: _toDate(j['dateOfBirth'] ?? j['DateOfBirth']),
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'username': username,
    'name': name,
    'role': role,
    'image': image,
    'gender': gender,
    'address': address,
    'phone': phone,
    'email': email,
    'createdDate': createdDate?.toIso8601String(),
    'dateOfBirth': dateOfBirth?.toIso8601String(),
  };
}
