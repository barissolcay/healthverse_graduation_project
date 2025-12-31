# DEV_TODO

## Amaç / Kapsam
- Hexagonal hedefi için Controller → Application → Ports ayrımını tesis etmek.
- Competition → Social → Duels → Tasks/Missions modüllerinde repository port’ları ve use-case akışını kurmak.
- Test edilebilirliği koruyarak parça parça refaktör yapmak.

## Öncelikli Görev Listesi
- [x] Competition port tasarımı tamamla (interface imzaları netleştir)
- [x] Use-case akışı/handler taslağını Competition için yaz
- [x] Competition.Application → Infrastructure referansını kaldırma planı hazırla (repo ihtiyaçları)
- [x] Competition.Application → Infrastructure referansını kaldır (Doğrulandı) ✅
- [x] Social + Duels port kapsamını çıkar
- [x] Tasks/Missions port kapsamını çıkar
- [x] IClock kapsam haritası (UtcNow kullanılan yerler) çıkar
- [x] NotificationDelivery migration + push sender paket planı yaz
- [x] Integration test iskeleti planı (test projesi yapısı)
- [x] MediatR Command/Query migration for remaining controllers (Tasks, Missions, Gamification, Notifications, Identity, Health) ✅

## Devam Eden İşler
- (boş)

## Tamamlananlar
- [x] Manuel test checklist ve koşum (13/13 PASS)
- [x] Migration & DbSet eşleşmesi doğrulandı (13 migration, 20+ DbSet)
- [x] NotificationDelivery'nin push sender ile birlikte ekleneceği kararı
- [x] TEST_CHECKLIST.md oluşturuldu
- [x] Competition port taslakları çıkarıldı (29 Dec)
- [x] Competition port imza dokümanı yazıldı (COMPETITION_PORT_PLAN.md)
- [x] Competition use-case/handler akış taslağı yazıldı (COMPETITION_PORT_PLAN.md)
- [x] Competition.Application → Infrastructure referans kırma planı yazıldı (COMPETITION_PORT_PLAN.md)
- [x] Social + Duels port kapsamı yazıldı (SOCIAL_DUELS_PORT_PLAN.md)
- [x] Tasks/Missions port kapsamı yazıldı (TASKS_MISSIONS_PORT_PLAN.md)
- [x] IClock kapsam haritası çıkarıldı (ICLOCK_COVERAGE.md)
- [x] NotificationDelivery push sender planı yazıldı (NOTIFICATION_PUSH_PLAN.md)
- [x] Integration test iskeleti kuruldu (WebApplicationFactory + InMemory DB, Status smoke test)
- [x] Competition port implementasyonu tamamlandı (ports, EF repos, DI, handlers)
- [x] LeagueController MediatR handler'larına bağlandı
- [x] Integration test hataları düzeltildi (29 Dec Evening):
  - Firebase singleton hatası: Test environment'ta Firebase atlanıyor, `TestAuthHandler` kullanılıyor
  - InMemory uyumluluğu: `LeagueRoom.IncrementUserCount()` domain method eklendi
  - 4/4 test geçiyor (StatusTests, LeagueTests)
- [x] **Social + Duels port implementasyonu tamamlandı** (29 Dec Evening):
  - Port interfaces: `IFriendshipRepository`, `IUserBlockRepository`, `IDuelRepository`, `ISocialUserRepository`, `ISocialUnitOfWork`, `INotificationPort`
  - Infrastructure: 6 EF Core repository implementations (`FriendshipRepository`, `UserBlockRepository`, `DuelRepository`, `SocialUserRepository`, `SocialUnitOfWork`, `NotificationAdapter`)
  - DI: `AddSocialInfrastructure()` registered in `Program.cs`
- [x] **Tasks + Missions port implementasyonu tamamlandı** (29 Dec Evening):
  - **Tasks:** `IUserTaskRepository`, `ITaskTemplateRepository`, `IUserGoalRepository`, `IUserInterestRepository`, `ITasksUnitOfWork` (5 repos + UoW)
  - **Missions:** `IPartnerMissionRepository`, `IPartnerMissionSlotRepository`, `IGlobalMissionRepository`, `IGlobalMissionParticipantRepository`, `IGlobalMissionContributionRepository`, `IMissionsUnitOfWork` (6 repos + UoW)
  - DI: `AddTasksInfrastructure()` + `AddMissionsInfrastructure()` registered in `Program.cs`
- [x] **Gamification + Notifications + Identity port implementasyonu tamamlandı** (29 Dec Night):
  - **Modules:** Gamification, Notifications, Identity
  - **Coverage:** 3 modules, 7 repositories, 3 UnitOfWork
  - **DI:** Fully registered and verified.

## Notlar / Kararlar / Riskler
- Controller'lar doğrudan DbContext kullanıyor → parça parça port + service eklenmeli (Competition ✅, Social ✅, Tasks ✅, Missions ✅, Gamification ✅, Notifications ✅, Identity ✅).
- **MILESTONE:** Tüm modüller için Port/Adapter (Hexagonal) altyapısı tamamlandı. Artık lojik Application katmanına taşınmalı.
- Competition.Application csproj Infrastructure referansı kaldırıldı; sadece SharedKernel + Identity.Domain bağımlılığı kaldı.
- NotificationDelivery migration'ı push sender ile birlikte eklenecek (yaygın pratik).
- UtcNow → IClock geçişi yaygın; önce kapsam haritası çıkarılacak.
- Refaktör öncesi mevcut davranış testlerle doğrulandı (13/13 PASS) → regresyon riski düşük ama yine de dilim dilim ilerle.

### Competition Port Taslakları (29 Dec)
- `ILeagueRoomRepository`: Haftaya göre işlenmemiş odaları çek (`GetUnprocessedByWeek`), oda kimliğine göre getir (`GetById`), mevcut tier için kapasitesi uygun oda bul (`FindAvailableForTier`), oda ekle (`Add`), işaretle (`MarkProcessed`), sayaç artır (`IncrementUserCount`).
- `ILeagueMemberRepository`: Üyelik sorgula (`GetMembershipByUserAndWeek`), oda üyelerini puana göre sırala (`GetMembersByRoomOrdered`), üye say (`CountByRoom`), kullanıcıyı ekle (`Add`), kullanıcının odadaki sırasını hesapla (`GetRankForUser`).
- `ILeagueConfigRepository`: Tier'ı getir (`GetByTierName`), tüm tier'ları sırayla listele (`GetAllOrdered`), tierOrder'a göre komşu tier'ları getir (`GetNextByOrder`, `GetPrevByOrder`).
- `IUserRepository` (Competition ihtiyacı kadar): Kullanıcıyı getir (`GetById`), toplu kullanıcı sözlüğü al (`GetByIds`), tier güncellemesi uygula (`UpdateTier`).
- `IUserPointsHistoryRepository`: Haftalık kayıt ekle (`AddWeeklyHistory`), kullanıcıya ait geçmişi sırala (`GetWeeklyHistory` limitli).
- `IUnitOfWork` (veya `ICompetitionDbContext` benzeri): Transactional save için ortak giriş noktası; EF izolasyonu için gereken abstractions burada toplanacak.
- `IClock` kullanımı korunacak; UtcNow ihtiyacı finalize ve join işlemlerinde mevcut.

## Sonraki Adımlar
1) ~~LeagueController'ı MediatR handler'larına bağla~~ ✅ TAMAMLANDI
2) ~~Social + Duels port implementasyonu başlat~~ ✅ TAMAMLANDI (29 Dec Evening)
   - Port interfaces: `IFriendshipRepository`, `IUserBlockRepository`, `IDuelRepository`, `ISocialUserRepository`, `ISocialUnitOfWork`, `INotificationPort`
   - Infrastructure: 6 EF Core repository implementation
   - DI: `AddSocialInfrastructure()` registered in `Program.cs`
3) ~~Tasks/Missions port implementasyonu başlat~~ ✅ TAMAMLANDI (29 Dec Evening)
   - **Tasks Module:** `IUserTaskRepository`, `ITaskTemplateRepository`, `IUserGoalRepository`, `IUserInterestRepository`, `ITasksUnitOfWork`
   - **Missions Module:** `IPartnerMissionRepository`, `IPartnerMissionSlotRepository`, `IGlobalMissionRepository`, `IGlobalMissionParticipantRepository`, `IGlobalMissionContributionRepository`, `IMissionsUnitOfWork`
   - Infrastructure: 11 EF Core repository implementations (5 Tasks + 6 Missions)
   - DI: `AddTasksInfrastructure()` + `AddMissionsInfrastructure()` registered in `Program.cs`
4) ~~Gamification/Notifications/Identity port implementasyonu~~ ✅ TAMAMLANDI (29 Dec Night)
   - **Gamification:** `IPointTransactionRepository`, `IUserDailyStatsRepository`, `IMilestoneRepository`, `IGamificationUnitOfWork`
   - **Notifications:** `INotificationRepository`, `IUserDeviceRepository`, `INotificationsUnitOfWork`
   - **Identity:** `IUserRepository`, `IAuthIdentityRepository`, `IIdentityUnitOfWork`
   - All infrastructures registered in `Program.cs` under the Hexagonal pattern.

5) MediatR Command/Query migration for remaining controllers (Tasks, Missions, etc.)

---

### İlerleme Kuralı (Her adımda)
1. Ne yapacağını yaz (neden + beklenen çıktı)
2. Uygula
3. Kontrol et (doğrulama yöntemi)
4. Dokümante et (bu dosya + rapor)
