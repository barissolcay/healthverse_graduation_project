# Phase 0 Report: Guardrails + Baseline

**Tarih**: 30 Aralık 2025  
**Durum**: Tamamlandı

## Özet
Hexagonal mimari dönüşümüne başlamadan önce projenin "röntgeni çekildi" (baseline) ve mimari oyun kuralları (contract) belirlendi. Henüz kod değişikliği yapılmadı, ancak "nereye gideceğimiz" netleştirildi.

## Oluşturulan Dokümanlar
Aşağıdaki dokümanlar `docs/architecture/` yolunda oluşturulmuştur:

1. **[BASELINE_20251230.md](../BASELINE_20251230.md)**
   - Projenin build ve test durumu kaydedildi.
   - **Metrik**: 368 test passed (299 Unit, 29 Integration, 40 Architecture).
   - **Migration**: DB bağlantı hatasına rağmen 1 eksik migration tespit edildi.

2. **[DEPENDENCY_MAP.md](../DEPENDENCY_MAP.md)**
   - Modüller arası çapraz bağımlılıklar haritalandı.
   - **Bulgu**: `Competition`, `Social`, `Missions` ve `Identity` modülleri diğer modüllerin Application/Domain katmanlarına doğrudan referans veriyor.
   - **Hedef**: Faz 6'da bu referanslar temizlenecek.

3. **[HEXAGONAL_CONTRACT.md](../HEXAGONAL_CONTRACT.md)**
   - Katman kuralları ve "Allowlist" istisnaları (StatusController vb.) yazılı hale getirildi.

4. **[ARCH_TEST_BACKLOG.md](../ARCH_TEST_BACKLOG.md)**
   - İlerleyen fazlarda `tests/HealthVerse.ArchitectureTests` projesine eklenecek kurallar listelendi.

## Komut Çıktıları
Yapılan doğrulamalarda:
- `.NET SDK 10.0.101` doğrulandı.
- `dotnet sln list` ile 27 proje teyit edildi.
- `dotnet build` başarılı.

## Sonraki Adım (Faz 1)
**Auth + CurrentUser Boundary**: Kimlik yönetimi (Guid UserId) tek bir noktada standartlaştırılacak ve Controller'lardan temizlenecek.
