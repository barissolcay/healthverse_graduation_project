import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import '../providers/home_screen_provider.dart';
import '../widgets/shared/home_app_bar.dart';
import '../widgets/shared/bottom_nav_bar.dart';

/// Kısıtlı home screen
/// Health permission verilmemiş kullanıcılar için
class HomeScreenRestricted extends ConsumerWidget {
  const HomeScreenRestricted({super.key});
  
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Stack(
          children: [
            // Arka plan: Blur'lı placeholder kartlar
            _buildBlurredBackground(),
            
            // Ön plan: AppBar + BottomNav
            Column(
              children: [
                // Üst bar
                HomeAppBar(
                  username: 'HealthVerse',
                  avatarUrl: null,
                  unreadNotificationCount: 0,
                  onNotificationTap: () {},
                ),
                
                // Spacer
                const Spacer(),
                
                // Alt navigation
                HomeBottomNavBar(
                  currentIndex: 2,
                  onTap: (index) {},
                ),
              ],
            ),
            
            // Modal overlay: Permission iste
            _buildPermissionModal(context, ref),
          ],
        ),
      ),
    );
  }
  
  Widget _buildBlurredBackground() {
    return Container(
      color: AppColors.background,
      child: Opacity(
        opacity: 0.3,
        child: SingleChildScrollView(
          physics: const NeverScrollableScrollPhysics(),
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              children: [
                const SizedBox(height: 100),
                
                // Hero card placeholder
                Container(
                  height: 140,
                  decoration: BoxDecoration(
                    gradient: const LinearGradient(
                      colors: [AppColors.primary, AppColors.primaryLight],
                      begin: Alignment.topLeft,
                      end: Alignment.bottomRight,
                    ),
                    borderRadius: BorderRadius.circular(24),
                  ),
                ),
                
                const SizedBox(height: 24),
                
                // Grid placeholders
                GridView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    crossAxisSpacing: 16,
                    mainAxisSpacing: 16,
                    childAspectRatio: 1.0,
                  ),
                  itemCount: 6,
                  itemBuilder: (context, index) => _buildPlaceholderCard(),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
  
  Widget _buildPlaceholderCard() {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(24),
      ),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: const BoxDecoration(
                color: AppColors.divider,
                shape: BoxShape.circle,
              ),
            ),
            const SizedBox(height: 12),
            Container(
              width: double.infinity,
              height: 20,
              decoration: BoxDecoration(
                color: AppColors.divider,
                borderRadius: BorderRadius.circular(4),
              ),
            ),
            const SizedBox(height: 8),
            Container(
              width: 80,
              height: 14,
              decoration: BoxDecoration(
                color: AppColors.divider,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ],
        ),
      ),
    );
  }
  
  Widget _buildPermissionModal(BuildContext context, WidgetRef ref) {
    return Center(
      child: Container(
        margin: const EdgeInsets.symmetric(horizontal: 20, vertical: 32),
        padding: const EdgeInsets.all(24),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(24),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.3),
              blurRadius: 20,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Kilit ikonu
            Container(
              width: 80,
              height: 80,
              decoration: const BoxDecoration(
                color: AppColors.background,
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.lock_outline,
                color: AppColors.textSecondary,
                size: 48,
              ),
            ),
            
            const SizedBox(height: 24),
            
            // Başlık
            Text(
              'İzin gerekli',
              style: AppTypography.headlineLarge.copyWith(
                color: AppColors.textPrimary,
              ),
              textAlign: TextAlign.center,
            ),
            
            const SizedBox(height: 12),
            
            // Açıklama
            Text(
              'Sağlık verilerine erişim izni vermediğin için ilerlemeni gösteremiyoruz. Uygulamanın çalışması için sağlığına bağlı veri akışı gerekiyor.',
              style: AppTypography.bodyMedium.copyWith(
                color: AppColors.textSecondary,
              ),
              textAlign: TextAlign.center,
            ),
            
            const SizedBox(height: 32),
            
            // İzin ver butonu
            SizedBox(
              width: double.infinity,
              height: 52,
              child: ElevatedButton(
                onPressed: () {
                  ref.read(homeScreenProvider.notifier).requestHealthPermission();
                },
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: AppColors.onPrimary,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                child: Text(
                  'İzin Ver',
                  style: AppTypography.labelLarge.copyWith(
                    color: AppColors.onPrimary,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
