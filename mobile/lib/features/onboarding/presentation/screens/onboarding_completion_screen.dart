/// Onboarding Tamamlandı Ekranı - Başarılı veya Yetersiz Bilgi
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/home/presentation/screens/home_screen_wrapper.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/birth_year_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';

class OnboardingCompletionScreen extends StatelessWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const OnboardingCompletionScreen({
    super.key,
    required this.stateNotifier,
  });

  @override
  Widget build(BuildContext context) {
    final isEligible = stateNotifier.data.isEligibleForReward;
    
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
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: isEligible 
              ? _buildSuccessContent(context) 
              : _buildInsufficientContent(context),
        ),
      ),
    );
  }

  Widget _buildSuccessContent(BuildContext context) {
    return Column(
      children: [
        const Spacer(),
        
        // Başarı ikonu (emoji değil, icon)
        Container(
          width: 100,
          height: 100,
          decoration: BoxDecoration(
            color: AppColors.primary.withOpacity(0.15),
            shape: BoxShape.circle,
          ),
          child: Icon(
            Icons.check_circle_outline,
            size: 60,
            color: AppColors.primary,
          ),
        ),
        const SizedBox(height: 24),
        
        // Başlık
        Text(
          'Harika, hazırsın!',
          style: AppTypography.displayLarge,
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 16),
        
        // Alt başlık
        Text(
          'Anket tamamlandı, teşekkürler!',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 32),
        
        // Ödül kartı
        Container(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            gradient: LinearGradient(
              colors: [
                AppColors.primary,
                AppColors.primary.withOpacity(0.8),
              ],
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
            ),
            borderRadius: BorderRadius.circular(16),
            boxShadow: [
              BoxShadow(
                color: AppColors.primary.withOpacity(0.3),
                blurRadius: 12,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: Row(
            children: [
              Container(
                width: 56,
                height: 56,
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(
                  Icons.ac_unit,
                  size: 28,
                  color: Colors.white,
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      '2 Seri Dondurma Hakkı',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.w700,
                        color: Colors.white,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Hesabına eklendi!',
                      style: TextStyle(
                        fontSize: 14,
                        color: Colors.white.withOpacity(0.9),
                      ),
                    ),
                  ],
                ),
              ),
              const Icon(Icons.check_circle, color: Colors.white, size: 32),
            ],
          ),
        ),
        const SizedBox(height: 24),
        
        // Bilgi kartları
        _InfoRow(
          icon: Icons.notifications_active,
          text: 'Artık sana özel bildirimler gönderebileceğiz!',
        ),
        const SizedBox(height: 12),
        _InfoRow(
          icon: Icons.settings,
          text: 'Cevaplarını Ayarlar → Profil\'den güncelleyebilirsin.',
        ),
        
        const Spacer(),
        
        // Devam butonu
        SizedBox(
          width: double.infinity,
          height: 56,
          child: ElevatedButton(
            onPressed: () => _navigateToHome(context),
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
                  'Uygulamaya Geç',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
                ),
                SizedBox(width: 8),
                Icon(Icons.arrow_forward, size: 20),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildInsufficientContent(BuildContext context) {
    final skipCount = stateNotifier.data.skipCount;
    
    return Column(
      children: [
        const Spacer(),
        
        // Üzgün emoji (sadece bu ekranda emoji var)
        const Text('😔', style: TextStyle(fontSize: 80)),
        const SizedBox(height: 24),
        
        // Başlık
        Text(
          'Üzgünüz...',
          style: AppTypography.displayLarge,
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 24),
        
        // Açıklama kartı
        Container(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: AppColors.divider),
          ),
          child: Column(
            children: [
              Text(
                'Seni daha iyi tanımak istiyoruz çünkü ilerleyen zamanlarda bildirim sistemini sana özel hale getirmek istiyoruz.',
                style: TextStyle(
                  fontSize: 15,
                  color: AppColors.textPrimary,
                  height: 1.5,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 16),
              Text(
                'Seri dondurma hakkı kazanabilmek için en fazla 3 soruyu atlayabilirsin. Sen $skipCount soruyu atladın.',
                style: TextStyle(
                  fontSize: 14,
                  color: AppColors.error,
                  height: 1.4,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 16),
              Text(
                'Seri dondurma hakkı kazanmak ve uygulamanın geliştirilmesine katkıda bulunmak istersen birkaç sorumuzu daha yanıtlayabilirsin.',
                style: TextStyle(
                  fontSize: 14,
                  color: AppColors.textSecondary,
                  height: 1.4,
                ),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
        
        const Spacer(),
        
        // Geri dön butonu
        SizedBox(
          width: double.infinity,
          height: 56,
          child: ElevatedButton(
            onPressed: () => _goBackToSurvey(context),
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
                Icon(Icons.arrow_back, size: 20),
                SizedBox(width: 8),
                Text(
                  'Geri Dön ve Yanıtla',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 12),
        
        // Yine de devam et butonu
        SizedBox(
          width: double.infinity,
          height: 56,
          child: OutlinedButton(
            onPressed: () => _navigateToHome(context),
            style: OutlinedButton.styleFrom(
              foregroundColor: AppColors.textSecondary,
              side: BorderSide(color: AppColors.divider),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
            child: const Text(
              'Yine de Devam Et',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
            ),
          ),
        ),
      ],
    );
  }

  void _navigateToHome(BuildContext context) {
    // Home ekranına yönlendir (tüm stack'i temizle)
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(
        builder: (context) => const ProviderScope(
          child: HomeScreenWrapper(),
        ),
      ),
      (route) => false,
    );
    
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(
          stateNotifier.data.isEligibleForReward
              ? 'Onboarding tamamlandı! 2 Freeze hakkı kazandınız!'
              : 'Onboarding tamamlandı.',
        ),
        backgroundColor: stateNotifier.data.isEligibleForReward 
            ? AppColors.primary 
            : AppColors.textSecondary,
      ),
    );
  }

  void _goBackToSurvey(BuildContext context) {
    // State'i sıfırla ve baştan başla
    stateNotifier.reset();
    
    Navigator.pushAndRemoveUntil(
      context,
      MaterialPageRoute(
        builder: (context) => BirthYearScreen(stateNotifier: stateNotifier),
      ),
      (route) => route.isFirst,
    );
  }
}

class _InfoRow extends StatelessWidget {
  final IconData icon;
  final String text;

  const _InfoRow({required this.icon, required this.text});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: AppColors.divider),
      ),
      child: Row(
        children: [
          Icon(icon, color: AppColors.primary, size: 20),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              text,
              style: TextStyle(
                fontSize: 14,
                color: AppColors.textSecondary,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
