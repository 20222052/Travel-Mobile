// lib/services/tour_service.dart
import 'dart:convert';
import 'package:http/http.dart' as http;

import 'api_client.dart';
import '../models/tour.dart';

class PagedTours {
  final List<Tour> items;
  final int totalPages;
  final int? catId;

  PagedTours({required this.items, required this.totalPages, this.catId});
}

class TourService {
  static final _api = ApiClient.instance;
  static const _timeout = Duration(seconds: 20);

  /// GET /api/ApiTour?name=&categoryId=&sort=desc
  static Future<List<Tour>> getTours({
    String? name,
    int? categoryId,
    String sort = 'desc', // 'desc' | 'asc'
  }) async {
    final uri = _api.uri('/api/ApiTour', {
      if (name != null && name.trim().isNotEmpty) 'name': name.trim(),
      if (categoryId != null) 'categoryId': '$categoryId',
      'sort': sort,
    });

    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }

    final dyn = jsonDecode(utf8.decode(res.bodyBytes));
    if (dyn is List) {
      return dyn.map((e) => Tour.fromJson(e as Map<String, dynamic>)).toList();
    }
    if (dyn is Map<String, dynamic>) {
      final listLike = dyn['items'] ?? dyn['data'] ?? dyn['result'];
      if (listLike is List) {
        return listLike.map((e) => Tour.fromJson(e as Map<String, dynamic>)).toList();
      }
      return [Tour.fromJson(dyn)];
    }
    throw Exception('Định dạng JSON không đúng kỳ vọng');
  }

  /// GET /api/ApiTour/page?page=&pageSize=&categoryId=&name=&sort=
  static Future<PagedTours> getToursPaged({
    int page = 1,
    int pageSize = 6,
    String? name,
    int? categoryId,
    String sort = 'desc',
  }) async {
    final uri = _api.uri('/api/ApiTour/page', {
      'page': '$page',
      'pageSize': '$pageSize',
      if (name != null && name.trim().isNotEmpty) 'name': name.trim(),
      if (categoryId != null) 'categoryId': '$categoryId',
      'sort': sort,
    });

    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }

    final map = jsonDecode(utf8.decode(res.bodyBytes)) as Map<String, dynamic>;
    final listLike = map['items'] as List<dynamic>? ?? const [];
    final totalPages = (map['totalPages'] as num?)?.toInt() ?? 1;
    final catId = (map['catId'] as num?)?.toInt();

    final items = listLike.map((e) => Tour.fromJson(e as Map<String, dynamic>)).toList();
    return PagedTours(items: items, totalPages: totalPages, catId: catId);
  }

  /// GET /api/ApiTour/{id}
  static Future<Tour> getTour(int id) async {
    final uri = _api.uri('/api/ApiTour/$id');
    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }
    final map = jsonDecode(utf8.decode(res.bodyBytes)) as Map<String, dynamic>;
    return Tour.fromJson(map);
  }

  /// POST /api/ApiTour
  static Future<Tour> createTour(Tour tour) async {
    final uri = _api.uri('/api/ApiTour');
    final res = await _api.client
        .post(uri, headers: _api.jsonHeaders, body: jsonEncode(tour.toJson()))
        .timeout(_timeout);

    if (res.statusCode != 200 && res.statusCode != 201) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }
    final map = jsonDecode(utf8.decode(res.bodyBytes)) as Map<String, dynamic>;
    return Tour.fromJson(map);
  }

  /// PUT /api/ApiTour/{id}
  static Future<void> updateTour(Tour tour) async {
    if (tour.id == null) {
      throw Exception('Thiếu id để cập nhật');
    }
    final uri = _api.uri('/api/ApiTour/${tour.id}');
    final res = await _api.client
        .put(uri, headers: _api.jsonHeaders, body: jsonEncode(tour.toJson()))
        .timeout(_timeout);

    if (res.statusCode != 200 && res.statusCode != 204) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }
  }

  /// DELETE /api/ApiTour/{id}
  static Future<void> deleteTour(int id) async {
    final uri = _api.uri('/api/ApiTour/$id');
    final res = await _api.client.delete(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200 && res.statusCode != 204) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }
  }

  /// Ramdum tour
  static Future<List<Tour>> getRandomTours() async {
    final uri = _api.uri('/api/ApiTour/random');
    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }
    final data = jsonDecode(utf8.decode(res.bodyBytes));
    if (data is List) {
      return data.map((e) => Tour.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw Exception('Định dạng JSON không đúng');
  }

  static Future<List<Tour>> getTopViewedTours() async {
    final uri = _api.uri('/api/ApiTour/topviewed');
    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }
    final data = jsonDecode(utf8.decode(res.bodyBytes));
    if (data is List) {
      return data.map((e) => Tour.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw Exception('Định dạng JSON không đúng kỳ vọng (expected List)');
  }

  static Future<Tour?> getLatestTour() async {
    final uri = _api.uri('/api/ApiTour/latest');
    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }
    final data = jsonDecode(utf8.decode(res.bodyBytes));
    if (data == null) return null;
    return Tour.fromJson(data as Map<String, dynamic>);
  }

  static Future<List<Tour>> getHotelTours() async {
    final uri = _api.uri('/api/ApiTour/hotel');
    final res = await _api.client.get(uri, headers: _api.jsonHeaders).timeout(_timeout);
    if (res.statusCode != 200) {
      throw Exception('HTTP ${res.statusCode}: ${res.body}');
    }
    final data = jsonDecode(utf8.decode(res.bodyBytes));
    if (data is List) {
      return data.map((e) => Tour.fromJson(e as Map<String, dynamic>)).toList();
    }
    throw Exception('Định dạng JSON không đúng kỳ vọng (expected List)');
  }
}
