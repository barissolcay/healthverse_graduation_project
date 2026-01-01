/// Email Giriş Ekranı - Email kontrol sonrası kayıt/giriş yönlendirmesi
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/auth/presentation/screens/email_register_screen.dart';
import 'package:healthverse_app/features/auth/presentation/screens/forgot_password_screen.dart';
import 'package:healthverse_app/features/auth/presentation/screens/health_permission_screen.dart';

class EmailEntryScreen extends StatefulWidget {
  const EmailEntryScreen({super.key});

  @override
  State<EmailEntryScreen> createState() => _EmailEntryScreenState();
}

class _EmailEntryScreenState extends State<EmailEntryScreen> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  
  bool _isLoading = false;
  bool _emailChecked = false;
  bool _emailExists = false;
  bool _obscurePassword = true;
  String? _errorMessage;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _checkEmail() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    // TODO: Backend'e email kontrolü yap
    // Şimdilik simülasyon - email "@test" içeriyorsa mevcut hesap
    await Future.delayed(const Duration(milliseconds: 800));
    
    final email = _emailController.text.trim();
    final exists = email.contains('@test'); // Simülasyon

    setState(() {
      _isLoading = false;
      _emailChecked = true;
      _emailExists = exists;
    });
  }

  Future<void> _handleLogin() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    // TODO: Firebase email/password login
    await Future.delayed(const Duration(milliseconds: 1000));
    
    setState(() {
      _isLoading = false;
      // Simülasyon - her zaman başarılı
    });

    if (mounted) {
      // Simülasyon: Giriş başarılı - Sağlık izni ekranına yönlendir
      // (Gerçek uygulamada: sağlık izni verilmiş mi kontrol et, yoksa Home'a git)
      Navigator.pushAndRemoveUntil(
        context,
        MaterialPageRoute(builder: (context) => const HealthPermissionScreen()),
        (route) => false, // Tüm geçmişi temizle
      );
    }
  }

  void _navigateToRegister() {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => EmailRegisterScreen(
          email: _emailController.text.trim(),
        ),
      ),
    );
  }

  void _navigateToForgotPassword() {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => ForgotPasswordScreen(
          initialEmail: _emailController.text.trim(),
        ),
      ),
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
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24.0),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Başlık
                Text(
                  _emailChecked && _emailExists ? 'Tekrar hoş geldin!' : 'E-posta ile devam et',
                  style: AppTypography.headlineLarge,
                ),
                const SizedBox(height: 8),
                Text(
                  _emailChecked && _emailExists 
                    ? 'Şifreni girerek hesabına giriş yap.'
                    : 'E-posta adresini gir, hesabını kontrol edelim.',
                  style: AppTypography.bodyMedium,
                ),
                const SizedBox(height: 32),

                // Email alanı
                _buildLabel('E-posta'),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _emailController,
                  keyboardType: TextInputType.emailAddress,
                  enabled: !_emailChecked, // Email kontrol edildiyse kilitle
                  decoration: _inputDecoration(
                    hint: 'ornek@mail.com',
                    suffixIcon: _emailChecked
                        ? Icon(
                            _emailExists ? Icons.check_circle : Icons.info_outline,
                            color: _emailExists ? AppColors.success : AppColors.accentGoals,
                          )
                        : null,
                  ),
                  validator: (value) {
                    if (value == null || value.isEmpty) {
                      return 'E-posta adresi gerekli';
                    }
                    if (!value.contains('@') || !value.contains('.')) {
                      return 'Geçerli bir e-posta adresi girin';
                    }
                    return null;
                  },
                ),

                // Email kontrolünden sonra duruma göre göster
                if (_emailChecked) ...[
                  const SizedBox(height: 8),
                  Text(
                    _emailExists 
                        ? 'Bu e-posta ile bir hesap bulundu.'
                        : 'Bu e-posta ile hesap bulunamadı.',
                    style: TextStyle(
                      fontSize: 14,
                      color: _emailExists ? AppColors.success : AppColors.accentGoals,
                    ),
                  ),
                ],

                // Şifre alanı (sadece mevcut hesap varsa)
                if (_emailChecked && _emailExists) ...[
                  const SizedBox(height: 24),
                  _buildLabel('Şifre'),
                  const SizedBox(height: 8),
                  TextFormField(
                    controller: _passwordController,
                    obscureText: _obscurePassword,
                    decoration: _inputDecoration(
                      hint: 'Şifreni gir',
                      suffixIcon: IconButton(
                        icon: Icon(
                          _obscurePassword ? Icons.visibility_off : Icons.visibility,
                          color: AppColors.textTertiary,
                        ),
                        onPressed: () {
                          setState(() => _obscurePassword = !_obscurePassword);
                        },
                      ),
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Şifre gerekli';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 12),
                  Align(
                    alignment: Alignment.centerRight,
                    child: TextButton(
                      onPressed: _navigateToForgotPassword,
                      child: Text(
                        'Şifremi unuttum',
                        style: TextStyle(
                          color: AppColors.primary,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ),
                ],

                // Hata mesajı
                if (_errorMessage != null) ...[
                  const SizedBox(height: 16),
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: AppColors.error.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.error_outline, color: AppColors.error, size: 20),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            _errorMessage!,
                            style: const TextStyle(color: AppColors.error, fontSize: 14),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],

                const SizedBox(height: 32),

                // Ana buton
                SizedBox(
                  width: double.infinity,
                  height: 56,
                  child: ElevatedButton(
                    onPressed: _isLoading
                        ? null
                        : () {
                            if (!_emailChecked) {
                              _checkEmail();
                            } else if (_emailExists) {
                              _handleLogin();
                            } else {
                              _navigateToRegister();
                            }
                          },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: Colors.white,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    child: _isLoading
                        ? const SizedBox(
                            width: 24,
                            height: 24,
                            child: CircularProgressIndicator(
                              color: Colors.white,
                              strokeWidth: 2,
                            ),
                          )
                        : Text(
                            !_emailChecked
                                ? 'Devam et'
                                : _emailExists
                                    ? 'Giriş yap'
                                    : 'Kayıt ol',
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                  ),
                ),

                // Email değiştir butonu
                if (_emailChecked) ...[
                  const SizedBox(height: 16),
                  Center(
                    child: TextButton(
                      onPressed: () {
                        setState(() {
                          _emailChecked = false;
                          _emailExists = false;
                          _passwordController.clear();
                        });
                      },
                      child: const Text('Farklı e-posta kullan'),
                    ),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildLabel(String text) {
    return Text(
      text,
      style: const TextStyle(
        fontSize: 14,
        fontWeight: FontWeight.w500,
        color: AppColors.textPrimary,
      ),
    );
  }

  InputDecoration _inputDecoration({required String hint, Widget? suffixIcon}) {
    return InputDecoration(
      hintText: hint,
      hintStyle: TextStyle(color: AppColors.textHint),
      filled: true,
      fillColor: Colors.white,
      suffixIcon: suffixIcon,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: AppColors.divider),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: AppColors.divider),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: AppColors.primary, width: 2),
      ),
      disabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: AppColors.divider.withOpacity(0.5)),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: AppColors.error),
      ),
    );
  }
}
