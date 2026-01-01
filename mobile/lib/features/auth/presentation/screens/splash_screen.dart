/// HealthVerse Splash Screen
/// Uygulama açılışında gösterilen yükleme ekranı
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/auth/presentation/screens/auth_selection_screen.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen>
    with SingleTickerProviderStateMixin {
  late AnimationController _spinController;

  @override
  void initState() {
    super.initState();
    _spinController = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 1),
    )..repeat();

    // 2 saniye sonra Auth seçim ekranına geç
    Future.delayed(const Duration(seconds: 2), () {
      if (mounted) {
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (context) => const AuthSelectionScreen()),
        );
      }
    });
  }

  @override
  void dispose() {
    _spinController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.offWhite,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(32.0),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              // Üst boşluk
              const SizedBox(),

              // Orta kısım - Logo ve başlık
              Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  // Logo
                  Image.asset(
                    'assets/images/logo.png',
                    width: 192,
                    height: 192,
                    fit: BoxFit.contain,
                    errorBuilder: (context, error, stackTrace) {
                      // Logo yüklenemezse placeholder
                      return Container(
                        width: 192,
                        height: 192,
                        decoration: BoxDecoration(
                          color: AppColors.primaryLight.withValues(alpha: 0.3),
                          borderRadius: BorderRadius.circular(24),
                        ),
                        child: const Icon(
                          Icons.fitness_center,
                          size: 80,
                          color: AppColors.primary,
                        ),
                      );
                    },
                  ),

                  const SizedBox(height: 16),

                  // Uygulama adı
                  Text(
                    'HealthVerse',
                    style: AppTypography.titleMedium.copyWith(
                      color: AppColors.textPrimary,
                      fontWeight: FontWeight.w600,
                    ),
                  ),

                  const SizedBox(height: 4),

                  // Slogan
                  Text(
                    'Rekabetin en sağlıklı hali.',
                    style: AppTypography.bodyMedium.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),

              // Alt kısım - Loading ve versiyon
              Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  // Loading göstergesi
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      // Dönen loader
                      RotationTransition(
                        turns: _spinController,
                        child: Container(
                          width: 20,
                          height: 20,
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            border: Border.all(
                              color: AppColors.loaderBackground,
                              width: 2,
                            ),
                          ),
                          child: CustomPaint(
                            painter: _LoaderPainter(),
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Text(
                        'Yükleniyor...',
                        style: AppTypography.labelSmall.copyWith(
                          color: AppColors.textTertiary,
                        ),
                      ),
                    ],
                  ),

                  const SizedBox(height: 16),

                  // Versiyon numarası
                  Text(
                    'v1.0',
                    style: AppTypography.labelTiny,
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// Özel loader painter - üst kısmı yeşil çizen
class _LoaderPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = AppColors.loaderActive
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round;

    final rect = Rect.fromLTWH(0, 0, size.width, size.height);
    // Üst çeyrek daire çiz
    canvas.drawArc(rect, -1.57, 1.57, false, paint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
