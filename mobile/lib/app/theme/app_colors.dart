/// HealthVerse Renk Paleti
/// Tüm renkler DESIGN_SYSTEM.md ile senkronize
import 'package:flutter/material.dart';

class AppColors {
  AppColors._();

  // ═══════════════════════════════════════════════════════════════════════════
  // ANA RENKLER (PRIMARY)
  // Kullanıcı tarafından belirlendi: #0F9124
  // ═══════════════════════════════════════════════════════════════════════════
  
  /// Ana yeşil - butonlar, başarı durumları, streak
  /// Kullanıcı tarafından belirlenen resmi renk: #0F9124
  static const Color primary = Color(0xFF0F9124);
  
  /// Koyu yeşil - AppBar, vurgular
  static const Color primaryDark = Color(0xFF0A7019);
  
  /// Açık yeşil - arka plan vurguları
  static const Color primaryLight = Color(0xFF7ED68E);
  
  /// Yeşil buton üzerindeki metin rengi - krem/beyaz
  /// Siyah DEĞİL, uygulama ile uyumlu olması için
  static const Color onPrimary = Color(0xFFFFFFF5); // Krem beyaz

  // ═══════════════════════════════════════════════════════════════════════════
  // MODÜL ACCENT RENKLERİ (Tasarımdan)
  // ═══════════════════════════════════════════════════════════════════════════
  
  /// Home - Ana yeşil
  static const Color accentHome = primary;
  
  /// Tasks/Görevler - Emerald (tasarımdan)
  static const Color accentTasks = Color(0xFF10B981);
  
  /// Goals/Hedefler - Orange (tasarımdan)
  static const Color accentGoals = Color(0xFFF97316);
  
  /// League/Lig - Yellow/Gold (tasarımdan)
  static const Color accentLeague = Color(0xFFEAB308);
  
  /// Duels/Düello - Purple (tasarımdan)
  static const Color accentDuels = Color(0xFF8B5CF6);
  
  /// Partner Mission/Ortak Görev - Pink (tasarımdan)
  static const Color accentPartner = Color(0xFFEC4899);
  
  /// Global Mission/Dünya Görevi - Blue (tasarımdan)
  static const Color accentGlobal = Color(0xFF2563EB);
  
  /// Profile - Indigo
  static const Color accentProfile = Color(0xFF303F9F);
  
  /// Missions - Teal (genel görevler)
  static const Color accentMissions = Color(0xFF00796B);

  // ═══════════════════════════════════════════════════════════════════════════
  // NÖTR RENKLER (LIGHT MODE)
  // ═══════════════════════════════════════════════════════════════════════════
  
  /// Splash ve özel arkaplanlar için off-white
  static const Color offWhite = Color(0xFFF9FAF6);
  
  /// Sayfa arka planı
  static const Color background = Color(0xFFF6F8F6);
  
  /// Kart arka planları
  static const Color surface = Color(0xFFFFFFFF);
  
  /// Ana metin rengi
  static const Color onSurface = Color(0xFF212121);
  
  /// Slate 800 - başlıklar için
  static const Color textPrimary = Color(0xFF1E293B);
  
  /// Slate 600 - alt başlıklar
  static const Color textSecondary = Color(0xFF475569);
  
  /// Slate 500 - açıklama metinleri
  static const Color textTertiary = Color(0xFF64748B);
  
  /// Slate 400 - ipucu metinleri
  static const Color textHint = Color(0xFF94A3B8);
  
  /// Slate 300 - ayırıcılar, borderlar
  static const Color divider = Color(0xFFCBD5E1);
  
  /// Hata rengi
  static const Color error = Color(0xFFD32F2F);
  
  /// Başarı rengi - primary ile aynı olabilir
  static const Color success = Color(0xFF0F9124);

  // ═══════════════════════════════════════════════════════════════════════════
  // DARK MODE RENKLERİ
  // ═══════════════════════════════════════════════════════════════════════════
  
  /// Dark mode arka plan
  static const Color backgroundDark = Color(0xFF122014);
  
  /// Dark mode surface
  static const Color surfaceDark = Color(0xFF1E1E1E);
  
  /// Dark mode metin
  static const Color onSurfaceDark = Color(0xFFFFFFFF);
  
  /// Dark mode için daha açık yeşil
  static const Color primaryDarkMode = Color(0xFF66BB6A);

  // ═══════════════════════════════════════════════════════════════════════════
  // LOADER RENKLERİ
  // ═══════════════════════════════════════════════════════════════════════════
  
  /// Loading spinner aktif rengi - primary ile aynı
  static const Color loaderActive = Color(0xFF0F9124);
  
  /// Loading spinner arka plan
  static const Color loaderBackground = Color(0xFFCBD5E1);

  // ═══════════════════════════════════════════════════════════════════════════
  // STREAK FIRE COLORS
  // ═══════════════════════════════════════════════════════════════════════════
  
  /// Streak başlangıç rengi - tamamlanmış seri için gradient başlangıcı
  /// Turuncu tonu
  static const Color streakFireStart = Color(0xFFE38D0B);
  
  /// Streak bitiş rengi - tamamlanmış seri için gradient sonu
  /// Kırmızı tonu
  static const Color streakFireEnd = Color(0xFFE3530B);
  
  /// Streak alev ikonu - tamamlanmamış seri için (sönük)
  /// Opacity %25
  static const Color streakFireDimmed = Color(0x40E38D0B);

  // ═══════════════════════════════════════════════════════════════════════════
  // NAVIGATION COLORS
  // ═══════════════════════════════════════════════════════════════════════════
  
  /// Bottom navigation - seçili olmayan ikonlar için gri-yeşil tonu
  static const Color navInactive = Color(0xFF9DB8A8);
}
