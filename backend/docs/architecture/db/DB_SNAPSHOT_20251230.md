# PHASE 3: DB Snapshot & Migration State Report

**Tarih**: 2025-12-30

## 1. Connection Info
**Not**: `dotnet user-secrets list` komutu yanıt vermediği için connection string doğrulanamadı.
Varsayım: Development ortamı, muhtemelen `localhost`.

## 2. Repo Migration Durumu
### API Migrations (`src/Api/HealthVerse.Api/Migrations`)
Kaynak: File System Analysis
Durum: **13 Migration Dosyası**
- `20251227210129_InitialCreate`
- ...
- `20251229122919_AddMilestoneTables`

### Infrastructure Migrations (`src/Infrastructure/HealthVerse.Infrastructure/Migrations`)
Kaynak: File System Analysis
Durum: **1 Migration Dosyası**
- `20251229221633_AddNotificationDeliveries` (Daha yeni tarihli!)

## 3. Migration State Analizi (Senaryo Seçimi)
**Durum**: API ve Infrastructure arasında kopukluk var. Infrastructure'daki migration, API'deki son migration'dan SONRA oluşturulmuş. 
Ancak Infrastructure'da önceki history (InitialCreate vb.) YOK.

**Teşhis**: **Senaryo B3 (Drift/Split)**
- API projesi eski zinciri tutuyor.
- Infrastructure projesi üzerine "ekleme" yapılmış ama zincirin başı kopuk.

## 4. Çözüm Planı (Bridge Strategy)
**Hedef**: Tek bir zincir (Infrastructure) ve data kaybı yok.

1.  **Move & Merge**: API'deki 1-13 nolu migration dosyalarını Infrastructure klasörüne taşı.
2.  **Namespace Fix**: Taşınan dosyaların namespace'ini değiştirme (Riskli). Ya da `HealthVerse.Api.Migrations` namespace'ini Infrastructure içinde kabul et.
    *   *Karar*: Namespace değiştirmek `__EFMigrationsHistory`'de row mismatch yaratmaz (EF Core class adına bakar). Ancak clean code için değiştirmek iyidir. Yine de güvenli yol: **Namespace'leri olduğu gibi taşı** veya **Bulk update yap ve Snapshot'ı yenile**.
3.  **Snapshot Merge**: Infrastructure'daki snapshot ile API'deki snapshot birleştirilmeli.
    *   *Pratik Yol*: API'dekileri taşıdıktan sonra Infrastructure'daki `AddNotificationDeliveries` dosyasını geçici olarak sil, snapshot'ı regenerate et, sonra onu geri getir?
    *   *Daha Güvenli Yol*:
        1. API klasörünü Infrastructure'a kopyala.
        2. `AddNotificationDeliveries` dosyasını (ve Designer.cs) bu klasöre ekle.
        3. Infrastructure'daki `HealthVerseDbContextModelSnapshot.cs` dosyasını sil (API'den geleni kullanma, yeniden oluştur).
        4. `dotnet ef migrations remove` (Infra'da) -> Son migration'ı (Deliveries) geri alıp tekrar ekle? Hayır, bu DB ile mismatch yaratır.

**Basitleştirilmiş Strateji (Scenario Fix)**:
1.  API'deki tüm `.cs` (Migration + Designer) dosyalarını `Infrastructure/Migrations` altına kopyala.
2.  Infrastructure'daki `AddNotificationDeliveries` dosyalarını koru.
3.  Tüm dosyaların namespace'ini `HealthVerse.Infrastructure.Migrations` yap (veya `Api` kalabilir, ama Infra projesinde duracakları için Infra olması doğru).
    *   *Not*: Mevcut DB `HealthVerse.Api.Migrations.InitialCreate` olarak kayıtlı olabilir. Namespace değişirse EF Core bunu "yeni migration" sanmaz, ancak Designer dosyasındaki metadata önemlidir.
4.  `HealthVerseDbContextModelSnapshot.cs` dosyasını:
    *   API'deki snapshot eski (Deliveries yok).
    *   Infra'daki snapshot yeni (Deliveries var AMA önceki tablolar var mı?).
    *   **Kritik Kontrol**: Infra'daki snapshot dosyasının boyutunu kontrol et. Eğer büyükse (API'deki kadar), demek ki Infra snapshot'ı zaten "her şeyi" biliyor.
    *   *Kontrol*: Önceki `list_dir` çıktısına göre:
        *   API Snapshot: 58993 bytes
        *   Infra Snapshot: 62002 bytes
    *   **Sonuç**: Infra snapshot'ı API'den daha büyük ve muhtemelen tüm şemayı içeriyor! Demek ki `AddNotificationDeliveries` eklenirken EF Core tüm context'i taramış.

**Onaylanan Strateji**:
1.  API migrations dosyalarını (1-13) Infrastructure'a taşı.
2.  Infrastructure snapshot'ını KORU (O zaten güncel).
3.  API'deki Migrations klasörünü sil.
4.  Program.cs'i Infra assembly'sine yönlendir.
