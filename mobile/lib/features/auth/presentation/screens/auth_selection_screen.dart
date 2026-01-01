/// Auth Seçimi Ekranı - Google / Apple / Email ile giriş seçenekleri
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/auth/presentation/screens/email_entry_screen.dart';
import 'package:healthverse_app/features/auth/presentation/screens/username_selection_screen.dart';

class AuthSelectionScreen extends StatelessWidget {
  const AuthSelectionScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24.0),
          child: Column(
            children: [
              // Üst başlık alanı
              const Padding(
                padding: EdgeInsets.only(top: 56.0),
                child: Column(
                  children: [
                    Text(
                      "HealthVerse'e devam et",
                      style: TextStyle(
                        fontSize: 28,
                        fontWeight: FontWeight.w600,
                        color: Color(0xFF1E293B), // slate-800
                        letterSpacing: -0.5,
                      ),
                      textAlign: TextAlign.center,
                    ),
                    SizedBox(height: 8),
                    Text(
                      'Başlamak için bir yöntem seç.',
                      style: TextStyle(
                        fontSize: 16,
                        color: Color(0xFF64748B), // slate-500
                      ),
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              ),

              // Orta alan - Butonlar
              Expanded(
                child: Center(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      // Google butonu
                      _AuthButton(
                        onPressed: () => _handleGoogleSignIn(context),
                        icon: _GoogleIcon(),
                        label: 'Google ile devam et',
                        backgroundColor: Colors.white,
                        textColor: const Color(0xFF1E293B),
                        borderColor: const Color(0xFFE2E8F0),
                      ),
                      const SizedBox(height: 16),

                      // Apple butonu
                      _AuthButton(
                        onPressed: () => _handleAppleSignIn(context),
                        icon: const Icon(Icons.apple, size: 24, color: Color(0xFF1F2937)),
                        label: 'Apple ile devam et',
                        backgroundColor: Colors.white,
                        textColor: const Color(0xFF1E293B),
                        borderColor: const Color(0xFFE2E8F0),
                      ),
                      const SizedBox(height: 16),

                      // Email butonu
                      _AuthButton(
                        onPressed: () => _handleEmailSignIn(context),
                        icon: const Icon(Icons.email_outlined, size: 24, color: Colors.white),
                        label: 'E-posta ile devam et',
                        backgroundColor: AppColors.primary,
                        textColor: Colors.white,
                      ),
                    ],
                  ),
                ),
              ),

              // Alt bilgi metni
              Padding(
                padding: const EdgeInsets.only(bottom: 16.0),
                child: Text(
                  'Hesabın olsun veya olmasın, buradan devam edebilirsin.',
                  style: TextStyle(
                    fontSize: 14,
                    color: Colors.grey.shade500,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
              
              // DEV MODE - UI test için hızlı bypass
              // Production'da kaldırılacak
              Padding(
                padding: const EdgeInsets.only(bottom: 16.0),
                child: TextButton(
                  onPressed: () => _handleDevModeBypass(context),
                  child: const Text(
                    '🛠️ DEV MODE - UI Test (Atla)',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.orange,
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _handleGoogleSignIn(BuildContext context) {
    // TODO: Firebase Google Sign In
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Google ile giriş - yakında')),
    );
  }

  void _handleAppleSignIn(BuildContext context) {
    // TODO: Firebase Apple Sign In
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Apple ile giriş - yakında')),
    );
  }

  void _handleEmailSignIn(BuildContext context) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => const EmailEntryScreen(),
      ),
    );
  }
  
  // DEV MODE - Tüm auth akışını atlayarak doğrudan test için
  void _handleDevModeBypass(BuildContext context) {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('DEV MODE: Auth atlandı, Takma Ad ekranına gidiliyor...'),
        backgroundColor: Colors.orange,
        duration: Duration(seconds: 1),
      ),
    );
    Navigator.pushAndRemoveUntil(
      context,
      MaterialPageRoute(builder: (context) => const UsernameSelectionScreen()),
      (route) => false,
    );
  }
}

/// Auth buton widget'ı - tutarlı stil için
class _AuthButton extends StatelessWidget {
  final VoidCallback onPressed;
  final Widget icon;
  final String label;
  final Color backgroundColor;
  final Color textColor;
  final Color? borderColor;

  const _AuthButton({
    required this.onPressed,
    required this.icon,
    required this.label,
    required this.backgroundColor,
    required this.textColor,
    this.borderColor,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: double.infinity,
      height: 56,
      child: OutlinedButton(
        onPressed: onPressed,
        style: OutlinedButton.styleFrom(
          backgroundColor: backgroundColor,
          foregroundColor: textColor,
          side: BorderSide(
            color: borderColor ?? Colors.transparent,
            width: 1,
          ),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          elevation: borderColor != null ? 1 : 0,
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            icon,
            const SizedBox(width: 12),
            Text(
              label,
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w500,
                color: textColor,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Google ikonu (SVG yerine basit CustomPaint)
class _GoogleIcon extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 24,
      height: 24,
      child: Image.network(
        'https://www.google.com/favicon.ico',
        errorBuilder: (context, error, stackTrace) => const Icon(
          Icons.g_mobiledata,
          size: 24,
          color: Color(0xFF4285F4),
        ),
      ),
    );
  }
}
