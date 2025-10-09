import 'dart:convert';
import 'package:http/http.dart' as http;

import 'api_client.dart';
import '../models/blog.dart';

class BlogPage {
  final List<Blog> items;
  final int totalPages;
  BlogPage(this.items, this.totalPages);

  factory BlogPage.fromJson(dynamic json) {
    if (json is List) {

      return BlogPage(json.map((e) => Blog.fromJson(e)).toList(), 1);
    }
    if (json is Map<String, dynamic>) {
      final list = (json['items'] ?? json['data'] ?? json['result']) as List?;
      final items = (list ?? []).map((e) => Blog.fromJson(e as Map<String, dynamic>)).toList();
      final total = (json['totalPages'] ?? 1);
      final tp = (total is num) ? total.toInt() : int.tryParse('$total') ?? 1;
      return BlogPage(items, tp);
    }
    return BlogPage(const [], 1);
  }
}

class BlogService {
  static final _api = ApiClient.instance;
  static const _timeout = Duration(seconds: 20);

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

  /// GET /api/ApiBlog?page=&pageSize=&categoryId=&title=&sort=
  static Future<BlogPage> getBlogs({
    int page = 1,
    int pageSize = 6,
    int? categoryId,
    String? title,
    String sort = 'desc',
  }) async {
    final uri = _api.uri('/api/ApiBlog', {
      'page': '$page',
      'pageSize': '$pageSize',
      if (categoryId != null) 'categoryId': '$categoryId',
      if (title != null && title.trim().isNotEmpty) 'title': title.trim(),
      'sort': sort,
    });

    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) throw _ex(res);
    return BlogPage.fromJson(jsonDecode(utf8.decode(res.bodyBytes)));
  }

  /// GET /api/ApiBlog/{id}
  static Future<Blog> getBlog(int id) async {
    final res = await _api.client
        .get(_api.uri('/api/ApiBlog/$id'), headers: _api.jsonHeaders)
        .timeout(_timeout);
    if (res.statusCode != 200) throw _ex(res);
    final map = jsonDecode(utf8.decode(res.bodyBytes)) as Map<String, dynamic>;
    return Blog.fromJson(map);
  }
}
