/// Sağlık Verilerine İzin Ekranı - Health Connect / HealthKit izin açıklaması
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/onboarding_welcome_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';

class HealthPermissionScreen extends StatefulWidget {
  const HealthPermissionScreen({super.key});

  @override
  State<HealthPermissionScreen> createState() => _HealthPermissionScreenState();
}

class _HealthPermissionScreenState extends State<HealthPermissionScreen> {
  bool _isLoading = false;

  Future<void> _handleGrantPermission() async {
    setState(() => _isLoading = true);

    // TODO: Health package ile izin iste
    // final granted = await _healthService.requestPermissions();
    await Future.delayed(const Duration(milliseconds: 1000));

    setState(() => _isLoading = false);

    if (mounted) {
      // Onboarding anketine yönlendir
      _navigateToOnboarding();
    }
  }

  void _handleSkip() {
    // Onboarding anketine yönlendir (izin olmadan)
    _navigateToOnboarding();
  }

  void _navigateToOnboarding() {
    final stateNotifier = OnboardingStateNotifier();
    Navigator.pushAndRemoveUntil(
      context,
      MaterialPageRoute(
        builder: (context) => OnboardingWelcomeScreen(stateNotifier: stateNotifier),
      ),
      (route) => false, // Tüm geçmişi temizle
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: AppColors.textPrimary),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: SafeArea(
        child: Column(
          children: [
            // İçerik
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Başlık
                    Text(
                      'Sağlık verilerine erişim izni',
                      style: AppTypography.displayLarge,
                    ),
                    const SizedBox(height: 12),
                    Text(
                      'Adımlarını ve egzersizlerini otomatik takip edebilmemiz için telefonundaki sağlık verilerine erişim iznine ihtiyacımız var.',
                      style: AppTypography.bodyMedium,
                    ),
                    const SizedBox(height: 32),

                    // Bilgi kartları
                    Container(
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(16),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withOpacity(0.05),
                            blurRadius: 10,
                            offset: const Offset(0, 2),
                          ),
                        ],
                      ),
                      child: Column(
                        children: [
                          _InfoItem(
                            icon: Icons.verified_user_outlined,
                            text: 'Uygulamanın çalışması için gerekli minimum sağlık verilerine erişiriz.',
                          ),
                          const SizedBox(height: 16),
                          _InfoItem(
                            icon: Icons.lock_outline,
                            text: 'Verilerini asla üçüncü kişilerle paylaşmayız.',
                          ),
                          const SizedBox(height: 16),
                          _InfoItem(
                            icon: Icons.sell_outlined,
                            text: 'Verilerini ticari amaçla satmayız.',
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),

            // Alt butonlar
            Container(
              padding: const EdgeInsets.all(24),
              color: AppColors.background,
              child: Column(
                children: [
                  // İzin ver butonu
                  SizedBox(
                    width: double.infinity,
                    height: 52,
                    child: ElevatedButton(
                      onPressed: _isLoading ? null : _handleGrantPermission,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        foregroundColor: AppColors.onPrimary,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      child: _isLoading
                          ? const SizedBox(
                              width: 24,
                              height: 24,
                              child: CircularProgressIndicator(
                                color: AppColors.onPrimary,
                                strokeWidth: 2,
                              ),
                            )
                          : const Text(
                              'İzin ver',
                              style: TextStyle(
                                fontSize: 16,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                    ),
                  ),
                  const SizedBox(height: 12),

                  // Şimdilik atla butonu
                  SizedBox(
                    width: double.infinity,
                    height: 52,
                    child: TextButton(
                      onPressed: _handleSkip,
                      child: Text(
                        'Şimdilik atla',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.w700,
                          color: AppColors.primary,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Uyarı metinleri
                  Text(
                    'Bu izni vermezsen uygulamanın bazı özellikleri tam olarak çalışmayabilir.',
                    style: TextStyle(
                      fontSize: 12,
                      color: AppColors.textTertiary,
                    ),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 12),
                  Text(
                    "Bu izni daha sonra 'Veri ve İzinler' menüsünden değiştirebilirsin.",
                    style: TextStyle(
                      fontSize: 12,
                      color: AppColors.textTertiary,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Bilgi öğesi widget'ı
class _InfoItem extends StatelessWidget {
  final IconData icon;
  final String text;

  const _InfoItem({
    required this.icon,
    required this.text,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: 40,
          height: 40,
          decoration: BoxDecoration(
            color: AppColors.primary.withOpacity(0.15),
            borderRadius: BorderRadius.circular(10),
          ),
          child: Icon(
            icon,
            color: AppColors.primary,
            size: 22,
          ),
        ),
        const SizedBox(width: 16),
        Expanded(
          child: Text(
            text,
            style: const TextStyle(
              fontSize: 15,
              color: AppColors.textPrimary,
              height: 1.4,
            ),
          ),
        ),
      ],
    );
  }
}
