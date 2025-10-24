import 'package:flutter/foundation.dart';
import '../models/user.dart';
import 'cart_state.dart';

class Session {
  static final ValueNotifier<User?> current = ValueNotifier<User?>(null);
  static bool get isLoggedIn => current.value != null;
  
  static void set(User? u) {
    current.value = u;
    // Reset cart state khi đăng nhập
    if (u != null) {
      CartState().reset();
    }
  }
  
  static void clear() {
    current.value = null;
    // Reset cart state khi logout
    CartState().reset();
  }
}
