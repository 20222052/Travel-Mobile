import 'package:flutter/foundation.dart';
import '../models/user.dart';

class Session {
  static final ValueNotifier<User?> current = ValueNotifier<User?>(null);
  static bool get isLoggedIn => current.value != null;
  static void set(User? u) => current.value = u;
  static void clear() => current.value = null;
}
