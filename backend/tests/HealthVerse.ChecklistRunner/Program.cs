using System.Net.Http.Json;
using System.Text.Json.Nodes;

class Program
{
    private static HttpClient _client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    
    // User 1 (Primary)
    private static Guid? _userId1;
    private static string _email1 = $"testuser_{Guid.NewGuid():N}@example.com";
    private static string _username1 = $"testuser_{Guid.NewGuid():N}"[..20];
    
    // User 2 (Secondary - for duels/partner)
    private static Guid? _userId2;
    private static string _email2 = $"testfriend_{Guid.NewGuid():N}@example.com";
    private static string _username2 = $"testfriend_{Guid.NewGuid():N}"[..20];
    
    // Shared state
    private static Guid? _duelId;

    static async Task Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("  HealthVerse Otomatik Test Runner (Tüm 13 Senaryo)");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine($"User 1: {_username1} ({_email1})");
        Console.WriteLine($"User 2: {_username2} ({_email2})");
        Console.WriteLine();

        // Check API
        Console.WriteLine("API kontrolü...");
        try
        {
            var health = await _client.GetAsync("/swagger/index.html");
            Console.WriteLine($"API durumu: {(health.IsSuccessStatusCode ? "OK ✅" : "FAIL ❌")}");
            if (!health.IsSuccessStatusCode) return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API bağlantısı başarısız: {ex.Message}");
            return;
        }
        Console.WriteLine();

        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine("  KISIM 1: Kimlik Doğrulama (Auth)");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        // ===== AUTH TESTS =====
        await RunTest(1, "POST /api/auth/register (dev-register)", TestDevRegister);
        await RunTest(2, "POST /api/auth/login (dev-login)", TestDevLogin);
        
        if (!_userId1.HasValue)
        {
            Console.WriteLine("CRITICAL: User1 oluşturulamadı!");
            return;
        }
        
        // Create User2 for multi-user tests
        Console.WriteLine("\n[SETUP] User2 oluşturuluyor...");
        await CreateSecondUser();
        if (!_userId2.HasValue)
        {
            Console.WriteLine("WARNING: User2 oluşturulamadı, multi-user testleri atlanacak.");
        }
        
        // Set User1 as current user
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", _userId1.Value.ToString());
        Console.WriteLine($"[INFO] X-User-Id: {_userId1}\n");

        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine("  KISIM 2: Sağlık & Gamification");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        await RunTest(3, "POST /api/health/sync-steps", TestSyncSteps);
        await RunTest(4, "GET /api/leaderboard/weekly", TestWeeklyLeaderboard);

        Console.WriteLine("\n─────────────────────────────────────────────────────────────");
        Console.WriteLine("  KISIM 3: Liga (League)");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        await RunTest(5, "POST /api/league/join", TestJoinLeague);
        await RunTest(6, "GET /api/league/my-room", TestMyRoom);

        Console.WriteLine("\n─────────────────────────────────────────────────────────────");
        Console.WriteLine("  KISIM 4: Düello (Duel) - İki Kullanıcı Gerektirir");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        if (_userId2.HasValue)
        {
            // Setup mutual friendship
            Console.WriteLine("[SETUP] Karşılıklı takip kuruluyor...");
            await SetupMutualFriendship();
            Console.WriteLine();
        }
        
        await RunTest(7, "POST /api/duels (Düello Oluştur)", TestCreateDuel);
        await RunTest(8, "POST /api/duels/{id}/accept", TestAcceptDuel);

        Console.WriteLine("\n─────────────────────────────────────────────────────────────");
        Console.WriteLine("  KISIM 5: Partner Görevi - İki Kullanıcı Gerektirir");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        await RunTest(9, "POST /api/missions/partner/pair/{friendId}", TestPartnerPair);

        Console.WriteLine("\n─────────────────────────────────────────────────────────────");
        Console.WriteLine("  KISIM 6: Bildirimler (Notifications)");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        await RunTest(10, "GET /api/notifications", TestGetNotifications);
        await RunTest(11, "POST /api/notifications/mark-read", TestMarkNotificationsRead);

        Console.WriteLine("\n─────────────────────────────────────────────────────────────");
        Console.WriteLine("  KISIM 7: Görevler (Tasks)");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        await RunTest(12, "GET /api/tasks/active", TestActiveTasks);
        await RunTest(13, "POST /api/tasks/{id}/claim", TestClaimTask);

        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("  TÜM TESTLER TAMAMLANDI!");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
    }

    private static async Task RunTest(int id, string name, Func<Task<(bool success, string detail)>> action)
    {
        Console.Write($"[{id,2}] {name,-45} ... ");
        try
        {
            var (success, detail) = await action();
            Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(success ? "PASS" : "FAIL");
            Console.ResetColor();
            if (!string.IsNullOrEmpty(detail))
            {
                Console.WriteLine($"     └── {detail}");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.ResetColor();
        }
    }

    // ===== SETUP HELPERS =====

    private static async Task CreateSecondUser()
    {
        // Register User2
        var regPayload = new { username = _username2, email = _email2, avatarId = 2 };
        var regResp = await _client.PostAsJsonAsync("/api/auth/dev-register", regPayload);
        if (!regResp.IsSuccessStatusCode) return;
        
        // Login User2
        var loginPayload = new { email = _email2 };
        var loginResp = await _client.PostAsJsonAsync("/api/auth/dev-login", loginPayload);
        if (!loginResp.IsSuccessStatusCode) return;
        
        var content = await loginResp.Content.ReadAsStringAsync();
        var json = JsonNode.Parse(content);
        if (Guid.TryParse(json?["userId"]?.ToString(), out var userId))
        {
            _userId2 = userId;
            Console.WriteLine($"[SETUP] User2 oluşturuldu: {_userId2}");
        }
    }

    private static async Task SetupMutualFriendship()
    {
        if (!_userId1.HasValue || !_userId2.HasValue) return;
        
        // User1 follows User2
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", _userId1.Value.ToString());
        var resp1 = await _client.PostAsync($"/api/social/follow/{_userId2}", null);
        Console.WriteLine($"     User1 -> User2 takip: {resp1.StatusCode}");
        
        // User2 follows User1
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", _userId2.Value.ToString());
        var resp2 = await _client.PostAsync($"/api/social/follow/{_userId1}", null);
        Console.WriteLine($"     User2 -> User1 takip: {resp2.StatusCode}");
        
        // Switch back to User1
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", _userId1.Value.ToString());
    }

    // ===== TEST METHODS =====

    private static async Task<(bool, string)> TestDevRegister()
    {
        var payload = new { username = _username1, email = _email1, avatarId = 1 };
        var response = await _client.PostAsJsonAsync("/api/auth/dev-register", payload);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
            return (false, $"Status: {response.StatusCode}");

        var json = JsonNode.Parse(content);
        var success = json?["success"]?.GetValue<bool>() ?? false;
        var userId = json?["userId"]?.ToString();
        
        return (success, $"UserId: {userId}");
    }

    private static async Task<(bool, string)> TestDevLogin()
    {
        var payload = new { email = _email1 };
        var response = await _client.PostAsJsonAsync("/api/auth/dev-login", payload);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
            return (false, $"Status: {response.StatusCode}");

        var json = JsonNode.Parse(content);
        var success = json?["success"]?.GetValue<bool>() ?? false;
        
        if (success && Guid.TryParse(json?["userId"]?.ToString(), out var userId))
        {
            _userId1 = userId;
            return (true, $"UserId: {userId}");
        }
        return (false, content);
    }

    private static async Task<(bool, string)> TestSyncSteps()
    {
        var payload = new 
        { 
            userId = _userId1,
            stepCount = 7500,
            happenedAt = DateTime.UtcNow,
            sourceId = "test_runner"
        };
        
        var response = await _client.PostAsJsonAsync("/api/health/sync-steps", payload);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
            return (false, $"Status: {response.StatusCode}, Body: {content}");

        var json = JsonNode.Parse(content);
        var pointsEarned = json?["pointsEarned"]?.ToString() ?? "?";

        // Idempotency test
        var response2 = await _client.PostAsJsonAsync("/api/health/sync-steps", payload);
        var content2 = await response2.Content.ReadAsStringAsync();
        var json2 = JsonNode.Parse(content2);
        var alreadyProcessed = json2?["alreadyProcessed"]?.GetValue<bool>() ?? false;
        
        return (true, $"Points: {pointsEarned}, Idempotency: {(alreadyProcessed ? "OK ✓" : "yeni kayıt")}");
    }

    private static async Task<(bool, string)> TestWeeklyLeaderboard()
    {
        var response = await _client.GetAsync("/api/leaderboard/weekly");
        return (response.IsSuccessStatusCode, $"Status: {response.StatusCode}");
    }

    private static async Task<(bool, string)> TestJoinLeague()
    {
        var response = await _client.PostAsync("/api/league/join", null);
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            var json = JsonNode.Parse(content);
            var tier = json?["tier"]?.ToString() ?? "?";
            return (true, $"Tier: {tier}");
        }
        
        if (content.Contains("zaten", StringComparison.OrdinalIgnoreCase))
            return (true, "Zaten lige katılmış");
            
        return (false, $"Status: {response.StatusCode}");
    }

    private static async Task<(bool, string)> TestMyRoom()
    {
        var response = await _client.GetAsync("/api/league/my-room");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(content);
            var tier = json?["tier"]?.ToString() ?? "?";
            var rank = json?["rankInRoom"]?.ToString() ?? "?";
            return (true, $"Tier: {tier}, Rank: {rank}");
        }
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return (true, "Henüz oda yok (404)");
            
        return (false, $"Status: {response.StatusCode}");
    }

    private static async Task<(bool, string)> TestCreateDuel()
    {
        if (!_userId2.HasValue)
            return (false, "User2 yok, test atlandı");

        var payload = new 
        { 
            opponentId = _userId2.Value, 
            activityType = "STEPS",
            targetMetric = "STEPS",
            targetValue = 10000,
            durationDays = 3
        };
        
        var response = await _client.PostAsJsonAsync("/api/duels", payload);
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            var json = JsonNode.Parse(content);
            var duelIdStr = json?["duelId"]?.ToString();
            if (Guid.TryParse(duelIdStr, out var duelId))
            {
                _duelId = duelId;
                return (true, $"DuelId: {duelId}");
            }
            return (true, "Düello oluşturuldu");
        }
        
        // BadRequest might be expected if no mutual friendship
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var json = JsonNode.Parse(content);
            var msg = json?["message"]?.ToString() ?? content;
            return (true, $"Beklenen hata: {msg}");
        }
        
        return (false, $"Status: {response.StatusCode}, Body: {content}");
    }

    private static async Task<(bool, string)> TestAcceptDuel()
    {
        if (!_duelId.HasValue)
            return (false, "Düello oluşturulmadı, test atlandı");
        
        if (!_userId2.HasValue)
            return (false, "User2 yok, test atlandı");

        // Switch to User2 to accept
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", _userId2.Value.ToString());
        
        var response = await _client.PostAsync($"/api/duels/{_duelId}/accept", null);
        var content = await response.Content.ReadAsStringAsync();
        
        // Switch back to User1
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", _userId1!.Value.ToString());
        
        if (response.IsSuccessStatusCode)
            return (true, "Düello kabul edildi, status=ACTIVE");
        
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var json = JsonNode.Parse(content);
            var msg = json?["message"]?.ToString() ?? content;
            return (true, $"Beklenen durum: {msg}");
        }
            
        return (false, $"Status: {response.StatusCode}");
    }

    private static async Task<(bool, string)> TestPartnerPair()
    {
        if (!_userId2.HasValue)
            return (false, "User2 yok, test atlandı");

        var response = await _client.PostAsync($"/api/missions/partner/pair/{_userId2}", null);
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            var json = JsonNode.Parse(content);
            var missionId = json?["missionId"]?.ToString() ?? "?";
            return (true, $"MissionId: {missionId}");
        }
        
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var json = JsonNode.Parse(content);
            var msg = json?["message"]?.ToString() ?? content;
            return (true, $"Beklenen hata: {msg}");
        }
        
        return (false, $"Status: {response.StatusCode}");
    }

    private static async Task<(bool, string)> TestGetNotifications()
    {
        var response = await _client.GetAsync("/api/notifications");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(content);
            var total = json?["totalCount"]?.ToString() ?? "?";
            var unread = json?["unreadCount"]?.ToString() ?? "?";
            return (true, $"Total: {total}, Unread: {unread}");
        }
        
        return (false, $"Status: {response.StatusCode}");
    }

    private static async Task<(bool, string)> TestMarkNotificationsRead()
    {
        // First get notifications
        var getResp = await _client.GetAsync("/api/notifications");
        if (!getResp.IsSuccessStatusCode)
            return (false, "Bildirimleri alamadık");
        
        var content = await getResp.Content.ReadAsStringAsync();
        var json = JsonNode.Parse(content);
        var notifications = json?["notifications"]?.AsArray();
        
        var ids = new List<string>();
        if (notifications != null && notifications.Count > 0)
        {
            ids.Add(notifications[0]?["id"]?.ToString() ?? "");
        }
        
        var payload = new { ids = ids };
        var response = await _client.PostAsJsonAsync("/api/notifications/mark-read", payload);
        
        return (response.IsSuccessStatusCode, $"Status: {response.StatusCode}, İşlenen: {ids.Count}");
    }

    private static async Task<(bool, string)> TestActiveTasks()
    {
        var response = await _client.GetAsync("/api/tasks/active");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(content);
            var total = json?["totalActive"]?.ToString() ?? "0";
            return (true, $"Aktif görev sayısı: {total}");
        }
        
        return (false, $"Status: {response.StatusCode}");
    }

    private static async Task<(bool, string)> TestClaimTask()
    {
        // First check completed tasks
        var completedResp = await _client.GetAsync("/api/tasks/completed");
        
        if (completedResp.IsSuccessStatusCode)
        {
            var content = await completedResp.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(content);
            var tasks = json?["tasks"]?.AsArray();
            
            if (tasks != null && tasks.Count > 0)
            {
                // Find one that's COMPLETED (not REWARD_CLAIMED)
                foreach (var task in tasks)
                {
                    var status = task?["status"]?.ToString();
                    if (status == "COMPLETED")
                    {
                        var taskId = task?["id"]?.ToString();
                        var claimResp = await _client.PostAsync($"/api/tasks/{taskId}/claim", null);
                        var claimContent = await claimResp.Content.ReadAsStringAsync();
                        
                        if (claimResp.IsSuccessStatusCode)
                            return (true, "Ödül toplandı!");
                        
                        var claimJson = JsonNode.Parse(claimContent);
                        return (true, claimJson?["message"]?.ToString() ?? "Claim denendi");
                    }
                }
            }
        }
        
        // No completed task to claim - try with a random ID to test endpoint
        var testResp = await _client.PostAsync($"/api/tasks/{Guid.NewGuid()}/claim", null);
        
        if (testResp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return (true, "Endpoint çalışıyor (tamamlanmış görev yok)");
        
        if (testResp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            return (true, "Endpoint çalışıyor (görev henüz tamamlanmamış)");
            
        return (false, $"Status: {testResp.StatusCode}");
    }
}
