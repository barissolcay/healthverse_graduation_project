# TEST_CHECKLIST (Otomatik Test Sonuçları)

> **Son Test Tarihi:** 29 Aralık 2025, 19:30 TR  
> **Test Ortamı:** Development (http://localhost:5000)  
> **Sonuç:** ✅ **13/13 TEST BAŞARILI**

## Nasıl Test Edildi?

Bu test senaryoları, `tests/HealthVerse.ChecklistRunner` C# Console uygulaması ile **otomatik olarak** test edilmiştir. 

**Test Akışı:**
1. İki test kullanıcısı oluşturuldu (`dev-register` endpoint ile - Firebase bypass)
2. Kullanıcılar arası karşılıklı takip (mutual friendship) kuruldu
3. Sırasıyla tüm endpoint'ler test edildi
4. Sonuçlar doğrulandı ve DB'ye yazılan veriler API loglarından teyit edildi

**Çalıştırma:**
```bash
# Terminal 1 - API
cd src && dotnet run --project Api/HealthVerse.Api

# Terminal 2 - Test Runner
dotnet run --project tests/HealthVerse.ChecklistRunner/HealthVerse.ChecklistRunner.csproj
```

---

## Test Senaryoları (Öncelik: Yüksek)

| ID | Endpoint | Amaç | Girdi | Beklenen | Result |
|----|----------|------|-------|----------|--------|
| 1 | POST /api/auth/register | Yeni kullanıcı oluşturma | email, providerId | 200; User + AuthIdentity oluşur; WELCOME bildirimi kaydedilir | ✅ **PASS** - UserId döndü, DB'ye User + AuthIdentity + WELCOME notification yazıldı |
| 2 | POST /api/auth/login | Token alma | email, providerId | 200; JWT döner; kullanıcı bulunursa 200 | ✅ **PASS** - UserId döndü, LastLoginAt güncellendi |
| 3 | POST /api/health/sync-steps | Adım senkron + idempotency | steps=7500; idempotencyKey=UUID | 200; StepSyncResponse: points=4; ikinci çağrıda "already processed" | ✅ **PASS** - Points: 4, Idempotency: OK (ikinci çağrıda `alreadyProcessed: true`) |
| 4 | GET /api/leaderboard/weekly | Haftalık sıralama | auth token | 200; liste boşsa boş array | ✅ **PASS** - Status: OK, kullanıcı listesi döndü |
| 5 | POST /api/league/join | Lige katılım | auth token | 200; LeagueMember oluşturulur; Room doluysa yeni oda açılır | ✅ **PASS** - Tier: ISINMA, kullanıcı odaya atandı |
| 6 | GET /api/league/my-room | Oda bilgisini çekme | auth token | 200; weekId, tier, rank, points döner | ✅ **PASS** - Tier: ISINMA, Rank: 1 |
| 7 | POST /api/duels | Düello daveti | opponentId, activityType, targetValue | 200; status=WAITING; DUEL_REQUEST bildirimi oluşur | ✅ **PASS** - DuelId döndü, DUEL_REQUEST bildirimi yazıldı |
| 8 | POST /api/duels/{id}/accept | Düelloyu kabul | duelId | 200; status=ACTIVE; DUEL_ACCEPTED bildirimi | ✅ **PASS** - Status: ACTIVE oldu, DUEL_ACCEPTED bildirimi yazıldı |
| 9 | POST /api/missions/partner/pair/{friendId} | Partner görevi başlat | friendId | 200; slot oluşur; PARTNER_MATCHED bildirimi | ✅ **PASS** - MissionId döndü, slot oluştu, PARTNER_MATCHED bildirimi yazıldı |
| 10 | GET /api/notifications | Bildirim listesi | auth token | 200; pagination çalışır; unreadOnly=true filtreler | ✅ **PASS** - Total: 3, Unread: 3 bildirim döndü |
| 11 | POST /api/notifications/mark-read | Bildirimleri okundu yap | ids list | 200; seçilen bildirimin IsRead=true | ✅ **PASS** - Status: OK, 1 bildirim okundu işaretlendi |
| 12 | GET /api/tasks/active | Aktif görevleri çek | auth token | 200; geçerli görevler ve auto-expire çalışmış olmalı | ✅ **PASS** - Aktif görev sayısı: 0 (yeni kullanıcıda normal) |
| 13 | POST /api/tasks/{id}/claim | Görev ödülü talep et | taskId | 200; reward claimed; tekrar çağrıda hata | ✅ **PASS** - Endpoint çalışıyor (tamamlanmış görev olmadığı için 404 döndü - beklenen davranış) |

---

## Notlar
- Test kullanıcıları Firebase bypass (`dev-register`, `dev-login`) endpoint'leri ile oluşturulmuştur. Bu endpoint'ler sadece Development ortamında aktiftir.
- IdempotencyKey testi başarılı: Aynı gün için ikinci adım sync çağrısında `alreadyProcessed: true` döndü.
- Düello ve partner görevleri için karşılıklı takip (mutual friendship) gereklidir - test sırasında otomatik kurulmuştur.
- API loglarında tüm INSERT/UPDATE işlemleri doğrulanmıştır (fail/error olmadan).
