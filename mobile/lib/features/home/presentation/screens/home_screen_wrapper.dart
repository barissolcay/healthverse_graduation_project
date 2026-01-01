import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/home_screen_provider.dart';
import 'home_screen_normal.dart';
import 'home_screen_restricted.dart';

/// Home screen wrapper
/// Health permission kontrolü yapar ve uygun ekrana yönlendirir
class HomeScreenWrapper extends ConsumerWidget {
  const HomeScreenWrapper({super.key});
  
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(homeScreenProvider);
    
    // Health permission kontrolü
    if (!state.isHealthPermissionGranted) {
      // Kısıtlı mode - permission verilmemiş
      return const HomeScreenRestricted();
    }
    
    // Normal mode - permission verilmiş
    return const HomeScreenNormal();
  }
}
