/// HealthVerse Tipografi Stilleri
/// Font: Literata (tasarımda kullanılan)
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'app_colors.dart';

class AppTypography {
  AppTypography._();

  /// Ana font ailesi - Literata (tasarımda kullanılan)
  static String get fontFamily => GoogleFonts.literata().fontFamily!;

  // ═══════════════════════════════════════════════════════════════════════════
  // DISPLAY STİLLERİ
  // ═══════════════════════════════════════════════════════════════════════════

  /// En büyük başlık - hero alanları
  static TextStyle get displayLarge => GoogleFonts.literata(
        fontSize: 32,
        fontWeight: FontWeight.w700,
        color: AppColors.textPrimary,
        height: 1.2,
      );

  // ═══════════════════════════════════════════════════════════════════════════
  // HEADLINE STİLLERİ
  // ═══════════════════════════════════════════════════════════════════════════

  /// Sayfa başlıkları
  static TextStyle get headlineLarge => GoogleFonts.literata(
        fontSize: 28,
        fontWeight: FontWeight.w600,
        color: AppColors.textPrimary,
        height: 1.3,
      );

  /// Kart başlıkları
  static TextStyle get headlineMedium => GoogleFonts.literata(
        fontSize: 24,
        fontWeight: FontWeight.w600,
        color: AppColors.textPrimary,
        height: 1.3,
      );

  // ═══════════════════════════════════════════════════════════════════════════
  // TITLE STİLLERİ
  // ═══════════════════════════════════════════════════════════════════════════

  /// Section başlıkları
  static TextStyle get titleLarge => GoogleFonts.literata(
        fontSize: 20,
        fontWeight: FontWeight.w600,
        color: AppColors.textPrimary,
        height: 1.4,
      );

  /// Splash ekran başlığı
  static TextStyle get titleMedium => GoogleFonts.literata(
        fontSize: 20,
        fontWeight: FontWeight.w600,
        color: AppColors.textPrimary,
        height: 1.4,
      );

  /// Liste başlıkları
  static TextStyle get titleSmall => GoogleFonts.literata(
        fontSize: 16,
        fontWeight: FontWeight.w500,
        color: AppColors.textPrimary,
        height: 1.4,
      );

  // ═══════════════════════════════════════════════════════════════════════════
  // BODY STİLLERİ
  // ═══════════════════════════════════════════════════════════════════════════

  /// Ana metin
  static TextStyle get bodyLarge => GoogleFonts.literata(
        fontSize: 16,
        fontWeight: FontWeight.w400,
        color: AppColors.textPrimary,
        height: 1.5,
      );

  /// İkincil metin - açıklamalar
  static TextStyle get bodyMedium => GoogleFonts.literata(
        fontSize: 14,
        fontWeight: FontWeight.w400,
        color: AppColors.textSecondary,
        height: 1.5,
      );

  /// Küçük metin
  static TextStyle get bodySmall => GoogleFonts.literata(
        fontSize: 12,
        fontWeight: FontWeight.w400,
        color: AppColors.textTertiary,
        height: 1.5,
      );

  // ═══════════════════════════════════════════════════════════════════════════
  // LABEL STİLLERİ
  // ═══════════════════════════════════════════════════════════════════════════

  /// Buton metni
  static TextStyle get labelLarge => GoogleFonts.literata(
        fontSize: 14,
        fontWeight: FontWeight.w500,
        color: AppColors.textPrimary,
        height: 1.4,
      );

  /// Medium label - kart alt metinleri
  static TextStyle get labelMedium => GoogleFonts.literata(
        fontSize: 13,
        fontWeight: FontWeight.w500,
        color: AppColors.textSecondary,
        height: 1.4,
      );

  /// Badge, caption
  static TextStyle get labelSmall => GoogleFonts.literata(
        fontSize: 12,
        fontWeight: FontWeight.w500,
        color: AppColors.textTertiary,
        height: 1.4,
      );

  /// Çok küçük metin - versiyon numarası vb.
  static TextStyle get labelTiny => GoogleFonts.literata(
        fontSize: 12,
        fontWeight: FontWeight.w400,
        color: AppColors.textHint,
        height: 1.4,
      );
}
