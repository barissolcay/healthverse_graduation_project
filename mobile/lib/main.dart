import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'app/theme/app_theme.dart';
import 'features/home/presentation/screens/home_screen_wrapper.dart';
import 'features/auth/presentation/screens/splash_screen.dart';

/// DEV MODE - true ise direkt anasayfaya gider
const bool kDevMode = true;

void main() {
  runApp(
    const ProviderScope(
      child: HealthVerseApp(),
    ),
  );
}

class HealthVerseApp extends StatelessWidget {
  const HealthVerseApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'HealthVerse',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      darkTheme: AppTheme.dark,
      themeMode: ThemeMode.light,
      // Dev mode: direkt anasayfa, normal: splash
      home: kDevMode ? const HomeScreenWrapper() : const SplashScreen(),
    );
  }
}
