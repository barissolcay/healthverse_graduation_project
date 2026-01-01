/// Email Doğrulama Ekranı - 6 haneli OTP girişi
import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/auth/presentation/screens/username_selection_screen.dart';

class EmailVerificationScreen extends StatefulWidget {
  final String email;
  
  const EmailVerificationScreen({super.key, required this.email});

  @override
  State<EmailVerificationScreen> createState() => _EmailVerificationScreenState();
}

class _EmailVerificationScreenState extends State<EmailVerificationScreen> {
  final List<TextEditingController> _controllers = List.generate(6, (_) => TextEditingController());
  final List<FocusNode> _focusNodes = List.generate(6, (_) => FocusNode());
  
  bool _isLoading = false;
  bool _isResending = false;
  String? _errorMessage;
  int _resendCooldown = 0;
  Timer? _cooldownTimer;

  @override
  void initState() {
    super.initState();
    _startResendCooldown();
  }

  @override
  void dispose() {
    for (var controller in _controllers) {
      controller.dispose();
    }
    for (var node in _focusNodes) {
      node.dispose();
    }
    _cooldownTimer?.cancel();
    super.dispose();
  }

  void _startResendCooldown() {
    _resendCooldown = 60;
    _cooldownTimer?.cancel();
    _cooldownTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_resendCooldown > 0) {
        setState(() => _resendCooldown--);
      } else {
        timer.cancel();
      }
    });
  }

  String get _code => _controllers.map((c) => c.text).join();

  void _onDigitChanged(int index, String value) {
    if (value.length == 1) {
      // Sonraki kutuya geç
      if (index < 5) {
        _focusNodes[index + 1].requestFocus();
      } else {
        // Son kutu - klavyeyi kapat ve doğrula
        _focusNodes[index].unfocus();
        if (_code.length == 6) {
          _verifyCode();
        }
      }
    }
  }

  void _onKeyPressed(int index, RawKeyEvent event) {
    if (event is RawKeyDownEvent) {
      if (event.logicalKey == LogicalKeyboardKey.backspace) {
        if (_controllers[index].text.isEmpty && index > 0) {
          _controllers[index - 1].clear();
          _focusNodes[index - 1].requestFocus();
        }
      }
    }
  }

  Future<void> _verifyCode() async {
    if (_code.length != 6) return;

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    // TODO: Firebase email verification
    await Future.delayed(const Duration(milliseconds: 1500));

    // Simülasyon - 123456 doğru kod
    final isValid = _code == '123456';

    setState(() => _isLoading = false);

    if (mounted) {
      if (isValid) {
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (context) => const UsernameSelectionScreen()),
        );
      } else {
        setState(() {
          _errorMessage = 'Girdiğin kod yanlış. Lütfen tekrar dene.';
        });
        // Kutuları temizle
        for (var controller in _controllers) {
          controller.clear();
        }
        _focusNodes[0].requestFocus();
      }
    }
  }

  Future<void> _resendCode() async {
    if (_resendCooldown > 0) return;

    setState(() => _isResending = true);

    // TODO: Firebase resend verification email
    await Future.delayed(const Duration(milliseconds: 1000));

    setState(() => _isResending = false);

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Doğrulama kodu tekrar gönderildi!'),
          backgroundColor: AppColors.success,
        ),
      );
      _startResendCooldown();
    }
  }

  void _changeEmail() {
    // Kayıt ekranına geri dön
    Navigator.pop(context);
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
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Başlık
              Text('E-postanı doğrula', style: AppTypography.headlineLarge),
              const SizedBox(height: 8),
              RichText(
                text: TextSpan(
                  style: AppTypography.bodyMedium,
                  children: [
                    const TextSpan(text: '6 haneli doğrulama kodunu '),
                    TextSpan(
                      text: widget.email,
                      style: const TextStyle(
                        fontWeight: FontWeight.w700,
                        color: AppColors.textPrimary,
                      ),
                    ),
                    const TextSpan(text: ' adresine gönderdik.'),
                  ],
                ),
              ),
              const SizedBox(height: 32),

              // OTP Kutuları
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: List.generate(6, (index) {
                  return Container(
                    width: 48,
                    height: 56,
                    margin: EdgeInsets.only(right: index < 5 ? 12 : 0),
                    child: RawKeyboardListener(
                      focusNode: FocusNode(),
                      onKey: (event) => _onKeyPressed(index, event),
                      child: TextField(
                        controller: _controllers[index],
                        focusNode: _focusNodes[index],
                        textAlign: TextAlign.center,
                        keyboardType: TextInputType.number,
                        maxLength: 1,
                        style: const TextStyle(
                          fontSize: 24,
                          fontWeight: FontWeight.w600,
                          color: AppColors.textPrimary,
                        ),
                        decoration: InputDecoration(
                          counterText: '',
                          filled: true,
                          fillColor: AppColors.background,
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
                          errorBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12),
                            borderSide: BorderSide(color: AppColors.error),
                          ),
                        ),
                        inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                        onChanged: (value) => _onDigitChanged(index, value),
                      ),
                    ),
                  );
                }),
              ),
              const SizedBox(height: 16),

              // Spam uyarısı
              Center(
                child: Text(
                  'Kodu göremiyorsan spam klasörünü de kontrol et.',
                  style: TextStyle(
                    fontSize: 14,
                    color: AppColors.textTertiary,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),

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

              const Spacer(),

              // Devam butonu
              SizedBox(
                width: double.infinity,
                height: 56,
                child: ElevatedButton(
                  onPressed: _isLoading || _code.length != 6 ? null : _verifyCode,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    foregroundColor: AppColors.onPrimary,
                    disabledBackgroundColor: AppColors.divider,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  child: _isLoading
                      ? const SizedBox(
                          width: 24,
                          height: 24,
                          child: CircularProgressIndicator(
                            color: AppColors.backgroundDark,
                            strokeWidth: 2,
                          ),
                        )
                      : const Text(
                          'Devam et',
                          style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
                        ),
                ),
              ),
              const SizedBox(height: 24),

              // Tekrar gönder linki
              Center(
                child: _isResending
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : GestureDetector(
                        onTap: _resendCooldown == 0 ? _resendCode : null,
                        child: RichText(
                          text: TextSpan(
                            style: TextStyle(fontSize: 14, color: AppColors.textTertiary),
                            children: [
                              const TextSpan(text: 'Kodu almadın mı? '),
                              TextSpan(
                                text: _resendCooldown > 0
                                    ? 'Tekrar gönder (${_resendCooldown}s)'
                                    : 'Tekrar gönder',
                                style: TextStyle(
                                  fontWeight: FontWeight.w600,
                                  color: _resendCooldown > 0
                                      ? AppColors.textTertiary
                                      : AppColors.primary,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
              ),
              const SizedBox(height: 12),

              // Email değiştir linki
              Center(
                child: GestureDetector(
                  onTap: _changeEmail,
                  child: RichText(
                    text: TextSpan(
                      style: TextStyle(fontSize: 14, color: AppColors.textTertiary),
                      children: [
                        const TextSpan(text: 'Yanlış e-posta adresi mi? '),
                        TextSpan(
                          text: 'E-postayı değiştir',
                          style: TextStyle(
                            fontWeight: FontWeight.w600,
                            color: AppColors.primary,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 16),
            ],
          ),
        ),
      ),
    );
  }
}
