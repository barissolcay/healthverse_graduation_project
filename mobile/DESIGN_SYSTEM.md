# HealthVerse Mobile - Tasarım ve Geliştirme Kılavuzu

> **Son Güncelleme:** 2026-01-02  
> **Amaç:** Mobile uygulamanın tüm tasarım kuralları, mimarisi ve standartlarını içerir.  
> **Kullanım:** Herhangi bir AI asistan veya geliştirici bu dosyayı okuyarak projeye devam edebilir.

---

## 📐 1. MİMARİ VE KLASÖR YAPISI

### 1.1 Seçilen Mimari: Clean Architecture + Feature-First

Backend'deki Hexagonal Architecture ile uyumlu, Flutter için optimize edilmiş yapı:

```
lib/
├── main.dart                      # Entry point
├── app/                           # Uygulama yapılandırması
│   ├── app.dart                   # MaterialApp wrapper
│   ├── router.dart                # GoRouter yapılandırması
│   └── theme/                     # Tema dosyaları
│       ├── app_theme.dart         # Ana tema
│       ├── app_colors.dart        # Renk paleti
│       └── app_typography.dart    # Tipografi
│
├── core/                          # Paylaşılan altyapı
│   ├── network/                   # API client, interceptors
│   ├── storage/                   # Local storage
│   ├── utils/                     # Yardımcı fonksiyonlar
│   └── constants/                 # Sabitler
│
├── shared/                        # Paylaşılan UI bileşenleri
│   ├── widgets/                   # Ortak widget'lar
│   │   ├── buttons/               # Buton stilleri
│   │   ├── cards/                 # Kart bileşenleri
│   │   ├── inputs/                # Form elemanları
│   │   └── progress/              # İlerleme göstergeleri
│   └── extensions/                # Dart extensions
│
└── features/                      # Özellik modülleri
    ├── auth/                      # Kimlik doğrulama
    │   ├── data/                  # Repository, data sources
    │   ├── domain/                # Entities, use cases
    │   └── presentation/          # Screens, widgets, providers
    │
    ├── home/                      # Ana sayfa
    ├── league/                    # Lig sistemi
    ├── tasks/                     # Görevler
    ├── goals/                     # Hedefler
    ├── duels/                     # Düellolar
    ├── missions/                  # Global + Partner görevler
    ├── profile/                   # Profil ve başarılar
    ├── social/                    # Takip sistemi
    ├── notifications/             # Bildirimler
    └── settings/                  # Ayarlar
```

### 1.2 Feature Modül Yapısı (Örnek: auth)

```
features/auth/
├── data/
│   ├── repositories/
│   │   └── auth_repository_impl.dart
│   └── data_sources/
│       └── auth_remote_data_source.dart
├── domain/
│   ├── entities/
│   │   └── user.dart
│   ├── repositories/
│   │   └── auth_repository.dart
│   └── usecases/
│       ├── login_usecase.dart
│       └── register_usecase.dart
└── presentation/
    ├── screens/
    │   ├── login_screen.dart
    │   ├── register_screen.dart
    │   └── onboarding_screen.dart
    ├── widgets/
    │   └── auth_form.dart
    └── providers/
        └── auth_provider.dart
```

### 1.3 Neden Bu Yapı?

| Özellik | Avantaj |
|---------|---------|
| **Feature-first** | Her özellik kendi içinde izole, kolay navigasyon |
| **Clean layers** | Backend hexagonal ile uyumlu (domain/data/presentation) |
| **Shared widgets** | Tutarlı UI, tek yerden güncelleme |
| **Testable** | Her katman bağımsız test edilebilir |

---

## 🔄 2. STATE MANAGEMENT: RIVERPOD

### 2.1 Neden Riverpod?

| Özellik | Riverpod Avantajı |
|---------|-------------------|
| **Type-safe** | Compile-time hata yakalama |
| **Testable** | Provider override ile kolay test |
| **Scalable** | Büyük projelerde performans |
| **Modern** | Flutter 3.x ile tam uyum |
| **No BuildContext** | Provider'lara her yerden erişim |

### 2.2 Provider Kullanım Kuralları

```dart
// ✅ DOĞRU: StateNotifierProvider kullan
final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier(ref.read(authRepositoryProvider));
});

// ✅ DOĞRU: FutureProvider async data için
final userProvider = FutureProvider<User>((ref) async {
  return ref.read(userRepositoryProvider).getCurrentUser();
});

// ❌ YANLIŞ: Global state için setState kullanma
```

### 2.3 Gerekli Paketler

```yaml
dependencies:
  flutter_riverpod: ^2.6.1
  riverpod_annotation: ^2.6.1

dev_dependencies:
  riverpod_generator: ^2.6.1
  build_runner: ^2.4.9
```

---

## 🎨 3. RENK PALETİ

### 3.1 Ana Renkler

| Renk | Hex Kodu | Kullanım Alanı |
|------|----------|----------------|
| **Primary Green** | `#0F9124` | Ana butonlar, başarı durumları, streak (**resmi renk**) |
| **Primary Dark** | `#0A7019` | AppBar, vurgular |
| **Primary Light** | `#7ED68E` | Arka plan vurguları |
| **On Primary** | `#FFFFF5` | Yeşil buton üzerindeki metin (krem beyaz, siyah DEĞİL!) |

### 3.2 Sayfa/Modül Accent Renkleri

| Modül | Accent Renk | Hex Kodu | Kullanım |
|-------|-------------|----------|----------|
| **Home** | Green | `#2E7D32` | Ana tema |
| **League** | Gold/Amber | `#FFA000` | Sıralama, ödüller |
| **Tasks** | Purple | `#7B1FA2` | Görev kartları |
| **Goals** | Blue | `#1976D2` | Hedef ilerleme |
| **Duels** | Red/Orange | `#E64A19` | Rekabet, düello |
| **Missions** | Teal | `#00796B` | Topluluk görevleri |
| **Profile** | Indigo | `#303F9F` | Profil, başarılar |

### 3.3 Nötr Renkler

| Renk | Hex Kodu | Kullanım |
|------|----------|----------|
| **Background** | `#FAFAFA` | Sayfa arka planı (light) |
| **Surface** | `#FFFFFF` | Kart arka planları |
| **On Surface** | `#212121` | Ana metin |
| **Secondary Text** | `#757575` | İkincil metin |
| **Divider** | `#E0E0E0` | Ayırıcı çizgiler |
| **Error** | `#D32F2F` | Hata durumları |

### 3.4 Dark Mode Renkleri

| Renk | Hex Kodu | Kullanım |
|------|----------|----------|
| **Background** | `#121212` | Sayfa arka planı |
| **Surface** | `#1E1E1E` | Kart arka planları |
| **On Surface** | `#FFFFFF` | Ana metin |
| **Primary Green** | `#66BB6A` | Daha açık yeşil (erişilebilirlik) |

---

## 🔤 4. TİPOGRAFİ

### 4.1 Font Ailesi: Inter

Modern, okunabilir, tüm platformlarda tutarlı.

```yaml
# pubspec.yaml
dependencies:
  google_fonts: ^6.2.1
```

### 4.2 Metin Stilleri

| Stil | Boyut | Ağırlık | Kullanım |
|------|-------|---------|----------|
| **Display Large** | 32sp | Bold (700) | Başlık ekranları |
| **Headline Large** | 28sp | SemiBold (600) | Sayfa başlıkları |
| **Headline Medium** | 24sp | SemiBold (600) | Kart başlıkları |
| **Title Large** | 20sp | Medium (500) | Section başlıkları |
| **Title Medium** | 16sp | Medium (500) | Liste başlıkları |
| **Body Large** | 16sp | Regular (400) | Ana metin |
| **Body Medium** | 14sp | Regular (400) | İkincil metin |
| **Label Large** | 14sp | Medium (500) | Buton metni |
| **Label Small** | 12sp | Medium (500) | Caption, badge |

---

## 🔘 5. BUTON STİLLERİ

### 5.1 Primary Button (Ana Buton)

```
Özellikler:
- Arka plan: Primary Green (#2E7D32)
- Metin: Beyaz
- Köşe yuvarlaklığı: 12px
- Yükseklik: 52px
- Padding: 16px horizontal
- Font: Label Large (14sp, Medium)
- Elevation: 2 (pressed: 0)
- Ripple: Beyaz %20 opacity
```

### 5.2 Secondary Button (İkincil Buton)

```
Özellikler:
- Arka plan: Transparent
- Border: 1.5px Primary Green
- Metin: Primary Green
- Köşe yuvarlaklığı: 12px
- Yükseklik: 52px
```

### 5.3 Text Button (Metin Buton)

```
Özellikler:
- Arka plan: Transparent
- Metin: Primary Green
- Padding: 8px
```

### 5.4 Icon Button

```
Özellikler:
- Boyut: 48x48px
- Icon boyutu: 24px
- Splash radius: 24px
```

### 5.5 Buton Durumları

| Durum | Değişiklik |
|-------|------------|
| **Normal** | Standart görünüm |
| **Pressed** | Opacity %80, elevation 0 |
| **Disabled** | Background #E0E0E0, text #9E9E9E |
| **Loading** | CircularProgress (beyaz, 20px) |

---

## 📦 6. KART TASARIMI

### 6.1 BaseCard (Ortak Kart)
Tüm özet kartları (Task, Goal, League, Duel, Mission) bu yapıyı kullanır.

```
Özellikler:
- Arka plan: Surface (#FFFFFF)
- Köşe yuvarlaklığı: 16px
- Border: 1px Solid (#000000 opacity %10) - Çok silik
- Shadow: BoxShadow(color: black %20, blur: 16, offset: 0,6) - Belirgin gölge
- İçerik Padding: 16px
- Sol İkon: Dairesel accent background içinde
- Sağ Badge: % veya Puan (accent color ile)
- Alt Kısım: Kalan Süre (accent color) ve Progress Bar
```

### 6.2 EmptyCard
Veri olmadığında gösterilen placeholder.

```
Özellikler:
- BaseCard ile aynı yapı (boyut, gölge, border)
- İkon: Merkezde, büyük, soluk
- Mesaj: Merkezde açıklayıcı metin
```

---

## 📊 7. PROGRESS GÖSTERİMLERİ

### 7.1 Linear Progress Bar

```
Özellikler:
- Yükseklik: 8px
- Köşe yuvarlaklığı: 4px
- Arka plan: #E0E0E0
- Dolgu: Gradient (primary → primary light)
```

### 7.2 Circular Progress (Streak Ring)

```
Özellikler:
- Boyut: 120px
- Stroke: 10px
- Arka plan: #E0E0E0
- Dolgu: Primary Green
- Merkez: Değer metni + ikon
```

---

## 📱 8. EKRAN LİSTESİ VE DURUMU

### 8.1 Auth Ekranları (8 adet - ✅ Tamamlandı)

| # | Ekran | Durum | Notlar |
|---|-------|-------|--------|
| 1 | Splash | ✅ Tamamlandı | Logo + "Yükleniyor..." + v1.0 |
| 2 | Auth Seçimi | ✅ Tamamlandı | Google / Apple / Email + DEV MODE |
| 3 | Email Giriş/Kontrol | ✅ Tamamlandı | Akıllı email kontrol + yönlendirme |
| 4 | Email Kayıt | ✅ Tamamlandı | Şifre gücü + terms checkbox |
| 5 | Email Doğrulama | ✅ Tamamlandı | 6 haneli OTP + 60s resend cooldown |
| 6 | Takma Ad | ✅ Tamamlandı | Debounced benzersizlik kontrolü |
| 7 | Sağlık İzni | ✅ Tamamlandı | İzin ver / Şimdilik atla |
| 8 | Şifremi Unuttum | ✅ Tamamlandı | Email input + success state |

### 8.2 Onboarding Anketi (12 adet - ✅ Tamamlandı)

| # | Ekran | Durum | Notlar |
|---|-------|-------|--------|
| 1 | Hoş Geldin | ✅ Tamamlandı | Ödül açıklaması + 4 info card |
| 2 | Doğum Yılı | ✅ Tamamlandı | 1940-2015 picker + skip |
| 3 | Cinsiyet | ✅ Tamamlandı | 4 seçenek radio |
| 4 | Şehir | ✅ Tamamlandı | 81 il arama + skip |
| 5 | Çalışma Durumu | ✅ Tamamlandı | Koşullu (5B) yönlendirme |
| 5B | İş Türü | ✅ Tamamlandı | Sadece çalışanlara |
| 6 | Boy & Kilo | ✅ Tamamlandı | Slider + canlı BMI (WHO) |
| 7 | Hedefler | ✅ Tamamlandı | Multi-select (max 2) |
| 8 | Aktivite Seviyesi | ✅ Tamamlandı | 5 seviye radio |
| 9 | Aktif Saatler | ✅ Tamamlandı | Multi-select + "Değişken" |
| 10 | Nereden Duydun | ✅ Tamamlandı | 7 kaynak radio |
| 11A | Tamamlandı (Başarılı) | ✅ Tamamlandı | Skip ≤ 3 → 2 Freeze ödül |
| 11B | Tamamlandı (Yetersiz) | ✅ Tamamlandı | Skip > 3 → ödül yok, geri dön |

### 8.3 Ana Uygulama Ekranları (Bekliyor)

| # | Ekran | Durum | Notlar |
|---|-------|-------|--------|
| 1 | Home | ✅ Tamamlandı (UI) | Tüm özet kartları (BaseCard), Sections |
| 2 | League | ⏳ Bekliyor | Sıralama + promote/demote |
| 3 | Tasks | ⏳ Bekliyor | Aktif/Tamamlanan + Claim |
| 4 | Goals | ⏳ Bekliyor | Hedef oluştur/takip |
| 5 | Duels | ⏳ Bekliyor | İstekler/Aktif/Sonuçlar |
| 6 | Global Missions | ⏳ Bekliyor | Katıl/Katkı/Top3 |
| 7 | Partner Mission | ⏳ Bekliyor | Eşleş/İlerleme |
| 8 | Profile | ⏳ Bekliyor | Başarılar/Rozetler |
| 9 | Social | ⏳ Bekliyor | Takip/Arkadaşlar |
| 10 | Notifications | ⏳ Bekliyor | Inbox + Badge |
| 11 | Settings | ⏳ Bekliyor | Tema/DND/Push |

---

## 🚀 9. GELİŞTİRME SÜRECİ

### 9.1 Başlangıç Sırası

1. **Altyapı kurulumu** - Riverpod, GoRouter, tema
2. **Auth akışı** - Splash → Login → Register → Onboarding
3. **Home ekranı** - Temel kartlar
4. **Diğer modüller** - Birer birer

### 9.2 Her Ekran İçin Checklist

- [ ] Tasarım referansı (görsel/HTML) alındı
- [ ] Riverpod provider'ları oluşturuldu
- [ ] UI widget'ları kodlandı
- [ ] API entegrasyonu yapıldı
- [ ] Loading/Error durumları eklendi
- [ ] README'ye ilerleme kaydedildi

---

## 📦 10. GEREKLİ PAKETLER

```yaml
dependencies:
  flutter:
    sdk: flutter
  
  # State Management
  flutter_riverpod: ^2.6.1
  riverpod_annotation: ^2.6.1
  
  # Navigation
  go_router: ^14.6.2
  
  # Network
  dio: ^5.9.0
  
  # Storage
  flutter_secure_storage: ^10.0.0
  shared_preferences: ^2.3.3
  
  # Health
  health: ^13.2.1
  
  # UI
  google_fonts: ^6.2.1
  flutter_svg: ^2.0.16
  cached_network_image: ^3.4.1
  shimmer: ^3.0.0
  
  # Utils
  intl: ^0.19.0
  
dev_dependencies:
  flutter_test:
    sdk: flutter
  flutter_lints: ^5.0.0
  riverpod_generator: ^2.6.1
  build_runner: ^2.4.9
```

---

## 📝 11. DEĞİŞİKLİK GEÇMİŞİ

| Tarih | Değişiklik |
|-------|------------|
| 2026-01-01 | İlk versiyon oluşturuldu |

---

*Bu doküman proje boyunca güncellenecektir.*
