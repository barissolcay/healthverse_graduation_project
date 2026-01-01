/// Onboarding Welcome Screen - Hoş geldin ve ödül açıklaması
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/birth_year_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';

class OnboardingWelcomeScreen extends StatelessWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const OnboardingWelcomeScreen({
    super.key,
    required this.stateNotifier,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // Ana içerik
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const SizedBox(height: 32),
                    
                    // Başlık
                    Text(
                      'Seni tanımak istiyoruz',
                      style: AppTypography.displayLarge,
                    ),
                    const SizedBox(height: 24),

                    // Ödül kartı
                    _InfoCard(
                      icon: Icons.card_giftcard,
                      title: 'Anketi tamamla, ödül kazan!',
                      description: 'Tüm soruları yanıtla ve 2 Seri Dondurma Hakkı kazan.',
                      color: AppColors.primary,
                    ),
                    const SizedBox(height: 16),

                    // Seri dondurma açıklaması
                    _InfoCard(
                      icon: Icons.ac_unit,
                      title: 'Seri Dondurma Nedir?',
                      description: '3000 adımdan az attığın günlerde serin bozulmaz. Bu hak seni kurtarır!',
                      color: AppColors.accentGoals,
                    ),
                    const SizedBox(height: 16),

                    // Gizlilik
                    _InfoCard(
                      icon: Icons.lock_outline,
                      title: 'Bilgilerin güvende',
                      description: 'Bu bilgiler sadece sana özel. Kimse göremez, hiçbir yerde paylaşılmaz.',
                      color: AppColors.accentProfile,
                    ),
                    const SizedBox(height: 16),

                    // Amaç
                    _InfoCard(
                      icon: Icons.campaign_outlined,
                      title: 'Neden soruyoruz?',
                      description: 'Uygulamayı geliştirmek ve sana özel bildirimler sunmak için bu bilgilere ihtiyacımız var.',
                      color: AppColors.accentMissions,
                    ),
                    const SizedBox(height: 16),

                    // İpucu
                    Container(
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: AppColors.divider.withOpacity(0.3),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Row(
                        children: [
                          Icon(Icons.settings, size: 20, color: AppColors.textTertiary),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Text(
                              'Cevaplarını daha sonra Ayarlar → Profil\'den güncelleyebilirsin.',
                              style: TextStyle(
                                fontSize: 14,
                                color: AppColors.textSecondary,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),

            // Alt buton
            Container(
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: AppColors.background,
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 10,
                    offset: const Offset(0, -2),
                  ),
                ],
              ),
              child: SizedBox(
                width: double.infinity,
                height: 56,
                child: ElevatedButton(
                  onPressed: () => _startSurvey(context),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    foregroundColor: AppColors.onPrimary,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  child: const Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(
                        'Başlayalım',
                        style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
                      ),
                      SizedBox(width: 8),
                      Icon(Icons.arrow_forward, size: 20),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _startSurvey(BuildContext context) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => BirthYearScreen(stateNotifier: stateNotifier),
      ),
    );
  }
}

/// Bilgi kartı widget'ı - Icon kullanıyor (emoji değil)
class _InfoCard extends StatelessWidget {
  final IconData icon;
  final String title;
  final String description;
  final Color color;

  const _InfoCard({
    required this.icon,
    required this.title,
    required this.description,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: color.withOpacity(0.3)),
        boxShadow: [
          BoxShadow(
            color: color.withOpacity(0.1),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              color: color.withOpacity(0.15),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(icon, color: color, size: 22),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                    color: AppColors.textPrimary,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  description,
                  style: TextStyle(
                    fontSize: 14,
                    color: AppColors.textSecondary,
                    height: 1.4,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
