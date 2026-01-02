import 'package:flutter/material.dart';
import 'dart:ui';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/home/presentation/widgets/shared/bottom_nav_bar.dart';

/// Kısıtlı Home Screen - Sağlık verisine izin verilmemiş
/// Arka plan blurlu, üstünde izin modal'ı
class HomeScreenRestricted extends StatelessWidget {
  final VoidCallback onPermissionRequest;

  const HomeScreenRestricted({
    super.key,
    required this.onPermissionRequest,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: Stack(
        children: [
          // Blurred background content
          _buildBlurredBackground(),

          // Permission modal overlay
          _buildPermissionModal(context),
        ],
      ),
      bottomNavigationBar: HomeBottomNavBar(
        currentIndex: 2,
        onTap: null, // Disabled in restricted mode
      ),
    );
  }

  Widget _buildBlurredBackground() {
    return IgnorePointer(
      child: ImageFiltered(
        imageFilter: ImageFilter.blur(sigmaX: 8, sigmaY: 8),
        child: ColorFiltered(
          colorFilter: ColorFilter.mode(
            Colors.grey.withOpacity(0.5),
            BlendMode.saturation,
          ),
          child: Opacity(
            opacity: 0.7,
            child: SafeArea(
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  children: [
                    // Fake App Bar
                    Row(
                      children: [
                        Container(
                          width: 40,
                          height: 40,
                          decoration: const BoxDecoration(
                            shape: BoxShape.circle,
                            color: AppColors.divider,
                          ),
                        ),
                        const Spacer(),
                        Text(
                          'Ana Sayfa',
                          style: AppTypography.titleLarge.copyWith(
                            color: AppColors.textPrimary,
                          ),
                        ),
                        const Spacer(),
                        Container(
                          width: 40,
                          height: 40,
                          decoration: const BoxDecoration(
                            shape: BoxShape.circle,
                            color: AppColors.divider,
                          ),
                        ),
                      ],
                    ),

                    const SizedBox(height: 20),

                    // Fake Streak Card
                    Container(
                      height: 140,
                      width: double.infinity,
                      decoration: BoxDecoration(
                        color: AppColors.divider,
                        borderRadius: BorderRadius.circular(24),
                      ),
                      child: Center(
                        child: Icon(
                          Icons.local_fire_department,
                          size: 48,
                          color: AppColors.textHint,
                        ),
                      ),
                    ),

                    const SizedBox(height: 20),

                    // Fake Grid
                    Expanded(
                      child: GridView.count(
                        crossAxisCount: 2,
                        mainAxisSpacing: 16,
                        crossAxisSpacing: 16,
                        childAspectRatio: 0.9,
                        physics: const NeverScrollableScrollPhysics(),
                        children: List.generate(
                          6,
                          (index) => Container(
                            decoration: BoxDecoration(
                              color: AppColors.surface,
                              borderRadius: BorderRadius.circular(24),
                            ),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildPermissionModal(BuildContext context) {
    return Container(
      color: Colors.black.withOpacity(0.3),
      child: Center(
        child: Container(
          margin: const EdgeInsets.symmetric(horizontal: 24),
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            color: AppColors.surface,
            borderRadius: BorderRadius.circular(20),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.15),
                blurRadius: 30,
                offset: const Offset(0, 10),
              ),
            ],
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // İkon - kilit + yürüyüş
              Stack(
                alignment: Alignment.center,
                children: [
                  Icon(
                    Icons.lock,
                    size: 64,
                    color: AppColors.textHint,
                  ),
                  Positioned(
                    bottom: 0,
                    child: Icon(
                      Icons.directions_walk,
                      size: 28,
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 20),

              // Başlık
              Text(
                'İzin Gerekli',
                style: AppTypography.headlineMedium.copyWith(
                  color: AppColors.textPrimary,
                  fontWeight: FontWeight.bold,
                ),
              ),

              const SizedBox(height: 12),

              // Açıklama
              Text(
                'Sağlık verilerine erişim izni vermediğin için ilerlemeni gösteremiyoruz. '
                'Uygulamanın çalışması için sağlığına bağlı veri akışı gerekiyor.',
                style: AppTypography.bodyMedium.copyWith(
                  color: AppColors.textSecondary,
                ),
                textAlign: TextAlign.center,
              ),

              const SizedBox(height: 16),

              // Gizlilik notu
              Text(
                'Uygulamanın çalışması için gerekli minimum verileri okuyoruz. '
                'Verilerin üçüncü kişilerle paylaşılmıyor ve ticari amaçla satılmıyor.',
                style: AppTypography.labelSmall.copyWith(
                  color: AppColors.textTertiary,
                ),
                textAlign: TextAlign.center,
              ),

              const SizedBox(height: 24),

              // İzin ver butonu
              SizedBox(
                width: double.infinity,
                height: 52,
                child: ElevatedButton(
                  onPressed: onPermissionRequest,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    foregroundColor: AppColors.onPrimary,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                    elevation: 2,
                  ),
                  child: Text(
                    'İzin Ver',
                    style: AppTypography.labelLarge.copyWith(
                      color: AppColors.onPrimary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ),

              const SizedBox(height: 16),

              // Gizlilik politikası linki
              GestureDetector(
                onTap: () {
                  // TODO: Navigate to privacy policy
                },
                child: Text(
                  'Detaylı bilgi: Gizlilik Politikası',
                  style: AppTypography.labelSmall.copyWith(
                    color: AppColors.textTertiary,
                    decoration: TextDecoration.underline,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
