import 'dart:io';
import 'package:androi_app/screens/blog_detail_screen.dart';
import 'package:androi_app/screens/blog_list_tab.dart';
import 'package:androi_app/screens/cart_items_screen.dart';
import 'package:androi_app/screens/checkout_screen.dart';
import 'package:androi_app/screens/detail_tour_screen.dart';
import 'package:androi_app/screens/history_screen.dart';
import 'package:androi_app/screens/login_screen.dart';
import 'package:androi_app/screens/profile_screen.dart';
import 'package:androi_app/screens/register_screen.dart';
import 'package:androi_app/screens/verify_otp_screen.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import 'models/blog.dart';
import 'models/tour.dart';
import 'screens/home_screen.dart';
import 'screens/not_found_screen.dart';

class MyHttpOverrides extends HttpOverrides {
  @override
  HttpClient createHttpClient(SecurityContext? context) {
    return super.createHttpClient(context)
      ..badCertificateCallback = (X509Certificate cert, String host, int port) => true;
  }
}

void main() {
  // Chỉ bật bỏ kiểm tra SSL ở DEV
  if (kDebugMode) {
    HttpOverrides.global = MyHttpOverrides();
  }
  runApp(const MyApp());
}

final _router = GoRouter(
  routes: [
    GoRoute(
      path: '/',
      builder: (context, state) => const HomeScreen(),
    ),
    GoRoute(
      path: '/tour/:id',
      builder: (context, state) {
        final idStr = state.pathParameters['id'];
        final id = int.tryParse(idStr ?? '');
        final initial = state.extra is Tour ? state.extra as Tour : null;
        if (id == null) {
          return const NotFoundScreen();
        }
        return DetailTourScreen(id: id, initial: initial);
      },
    ),
    GoRoute(path: '/', builder: (context, state) => const HomeScreen()),
    GoRoute(path: '/login', builder: (_, __) => const LoginScreen()),
    GoRoute(path: '/register', builder: (_, __) => const RegisterScreen()),
    GoRoute(
      path: '/verify-otp',
      builder: (context, state) {
        final email = state.extra as String?;
        if (email == null || email.isEmpty) {
          return Scaffold(
            appBar: AppBar(title: const Text('Lỗi')),
            body: const Center(
              child: Text('Email không hợp lệ. Vui lòng đăng ký lại.'),
            ),
          );
        }
        return VerifyOtpScreen(email: email);
      },
    ),
    GoRoute(path: '/', builder: (_, __) => const HomeScreen()),
    GoRoute(path: '/cart', builder: (_, __) => const CartItemsScreen()),
    GoRoute(path: '/checkout', builder: (_, __) => const CheckoutScreen()),
    GoRoute(
      path: '/blog',
      builder: (_, __) => Scaffold(
        appBar: AppBar(title: const Text('Bài viết')),
        body: const BlogListTab(),
      ),
    ),
    GoRoute(
      path: '/blog/:id',
      builder: (context, state) {
        final id = int.tryParse(state.pathParameters['id'] ?? '');
        final initial = state.extra is Blog ? state.extra as Blog : null;
        if (id == null) {
          return const NotFoundScreen();
        }
        return BlogDetailScreen(id: id, initial: initial);
      },
    ),
    GoRoute(
      path: '/account/profile',
      builder: (_, __) => const ProfileScreen(),
    ),
    GoRoute(
      path: '/account/history',
      builder: (_, __) => const HistoryScreen(),
    ),
  ],
  errorBuilder: (_, __) => const NotFoundScreen(),
);


class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    final seed = Colors.lightBlue;

    return MaterialApp.router(
      routerConfig: _router,
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        scaffoldBackgroundColor: Colors.lightBlue.shade50,
        colorScheme: ColorScheme.fromSeed(seedColor: seed),
        appBarTheme: AppBarTheme(
          backgroundColor: Colors.lightBlue.shade100,
          foregroundColor: Colors.black,
          elevation: 0,
          centerTitle: false,
        ),
        cardTheme: CardThemeData(
          color: Colors.white,
          elevation: 2,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        ),
        iconTheme: const IconThemeData(color: Colors.black87),
        textTheme: const TextTheme().apply(
          bodyColor: Colors.black87,
          displayColor: Colors.black87,
        ),
        navigationBarTheme: NavigationBarThemeData(
          backgroundColor: Colors.lightBlue.shade100,
          indicatorColor: seed.withOpacity(0.2),
          labelTextStyle: MaterialStateProperty.all(
            const TextStyle(fontWeight: FontWeight.w600),
          ),
        ),
      ),
    );
  }
}
