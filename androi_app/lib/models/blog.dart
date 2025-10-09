class Blog {
  final int? id;
  final String? title;
  final String? image;       // đường dẫn ảnh (có thể là /Uploads/...)
  final String? content;     // có thể chứa HTML
  final int? view;
  final DateTime? postedDate;
  final int? categoryId;

  Blog({
    this.id,
    this.title,
    this.image,
    this.content,
    this.view,
    this.postedDate,
    this.categoryId,
  });

  factory Blog.fromJson(Map<String, dynamic> json) {
    int? _i(dynamic v) => (v is num) ? v.toInt() : int.tryParse('$v');
    String? _s(dynamic v) => v?.toString();

    DateTime? _d(dynamic v) {
      final s = _s(v);
      if (s == null || s.isEmpty) return null;
      return DateTime.tryParse(s);
    }

    return Blog(
      id: _i(json['id'] ?? json['Id']),
      title: _s(json['title'] ?? json['Title']),
      image: _s(json['image'] ?? json['Image']),
      content: _s(json['content'] ?? json['Content']),
      view: _i(json['view'] ?? json['View']),
      postedDate: _d(json['postedDate'] ?? json['PostedDate']),
      categoryId: _i(json['categoryId'] ?? json['CategoryId']),
    );
  }
}
