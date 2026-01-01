/// Takma Ad Seçim Ekranı - Benzersiz kullanıcı adı belirleme
import 'dart:async';
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/auth/presentation/screens/health_permission_screen.dart';

class UsernameSelectionScreen extends StatefulWidget {
  const UsernameSelectionScreen({super.key});

  @override
  State<UsernameSelectionScreen> createState() => _UsernameSelectionScreenState();
}

class _UsernameSelectionScreenState extends State<UsernameSelectionScreen> {
  final _usernameController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  
  bool _isChecking = false;
  bool _isAvailable = false;
  bool _hasChecked = false;
  String? _errorMessage;
  Timer? _debounce;

  @override
  void initState() {
    super.initState();
    _usernameController.addListener(_onUsernameChanged);
  }

  @override
  void dispose() {
    _debounce?.cancel();
    _usernameController.dispose();
    super.dispose();
  }

  void _onUsernameChanged() {
    final username = _usernameController.text.trim();
    
    // Debounce - 500ms bekle
    _debounce?.cancel();
    
    if (username.length < 3) {
      setState(() {
        _hasChecked = false;
        _isAvailable = false;
        _errorMessage = null;
      });
      return;
    }

    setState(() {
      _isChecking = true;
      _hasChecked = false;
    });

    _debounce = Timer(const Duration(milliseconds: 500), () {
      _checkUsername(username);
    });
  }

  Future<void> _checkUsername(String username) async {
    // Username validation
    if (!RegExp(r'^[a-zA-Z0-9_]+$').hasMatch(username)) {
      setState(() {
        _isChecking = false;
        _hasChecked = true;
        _isAvailable = false;
        _errorMessage = 'Sadece harf, rakam ve alt çizgi kullanabilirsin.';
      });
      return;
    }

    // TODO: Backend'e username kontrolü yap
    await Future.delayed(const Duration(milliseconds: 300));
    
    // Simülasyon - "admin", "test" gibi isimler alınmış
    final reserved = ['admin', 'test', 'healthverse', 'moderator'];
    final isAvailable = !reserved.contains(username.toLowerCase());

    if (mounted) {
      setState(() {
        _isChecking = false;
        _hasChecked = true;
        _isAvailable = isAvailable;
        _errorMessage = isAvailable ? null : 'Bu takma ad zaten kullanılıyor.';
      });
    }
  }

  void _handleContinue() {
    if (!_isAvailable || _usernameController.text.trim().length < 3) return;

    // TODO: Backend'e username kaydet
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => const HealthPermissionScreen(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final username = _usernameController.text.trim();
    final canContinue = _hasChecked && _isAvailable && username.length >= 3;

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
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Başlık
                Text(
                  'Takma adını seç',
                  style: AppTypography.displayLarge,
                ),
                const SizedBox(height: 12),
                Text(
                  'Arkadaşların ve rakiplerin seni bu isimle görecek.',
                  style: AppTypography.bodyMedium,
                ),
                const SizedBox(height: 32),

                // Username alanı
                Text(
                  'Takma ad',
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w500,
                    color: AppColors.textPrimary,
                  ),
                ),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _usernameController,
                  decoration: InputDecoration(
                    hintText: 'fitcan',
                    hintStyle: TextStyle(color: AppColors.textHint),
                    filled: true,
                    fillColor: Colors.white,
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
                    suffixIcon: _buildSuffixIcon(),
                  ),
                ),

                // Durum mesajı
                if (_hasChecked || _errorMessage != null) ...[
                  const SizedBox(height: 8),
                  Text(
                    _isAvailable 
                        ? 'Bu takma adı kullanabilirsin.' 
                        : _errorMessage ?? 'Bu takma ad zaten kullanılıyor.',
                    style: TextStyle(
                      fontSize: 14,
                      color: _isAvailable ? AppColors.success : AppColors.error,
                    ),
                  ),
                ],

                const Spacer(),

                // Alt bilgi
                Center(
                  child: Padding(
                    padding: const EdgeInsets.only(bottom: 16.0),
                    child: Text(
                      'Bu takma ad liderlikte ve düellolarda görünecek.',
                      style: TextStyle(
                        fontSize: 14,
                        color: AppColors.textTertiary,
                      ),
                      textAlign: TextAlign.center,
                    ),
                  ),
                ),

                // Devam butonu
                SizedBox(
                  width: double.infinity,
                  height: 56,
                  child: ElevatedButton(
                    onPressed: canContinue ? _handleContinue : null,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: AppColors.onPrimary,
                      disabledBackgroundColor: AppColors.divider,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(28),
                      ),
                    ),
                    child: const Text(
                      'Devam et',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 32),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget? _buildSuffixIcon() {
    if (_isChecking) {
      return const Padding(
        padding: EdgeInsets.all(12.0),
        child: SizedBox(
          width: 24,
          height: 24,
          child: CircularProgressIndicator(strokeWidth: 2),
        ),
      );
    }
    
    if (_hasChecked) {
      return Icon(
        _isAvailable ? Icons.check_circle : Icons.cancel,
        color: _isAvailable ? AppColors.primary : AppColors.error,
        size: 24,
      );
    }
    
    return null;
  }
}
