import 'package:flutter/material.dart';
import 'core/network/api_client.dart';
import 'core/services/health_sync_service.dart';
import 'core/constants/api_constants.dart';

void main() {
  runApp(const HealthVerseApp());
}

class HealthVerseApp extends StatelessWidget {
  const HealthVerseApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'HealthVerse',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: Colors.green,
          brightness: Brightness.light,
        ),
        useMaterial3: true,
      ),
      home: const HomePage(),
    );
  }
}

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  final ApiClient _apiClient = ApiClient();
  late final HealthSyncService _healthService;

  bool _isLoading = false;
  String _statusMessage = 'Hos geldiniz!';
  String? _userId;
  HealthSyncResult? _lastSyncResult;

  @override
  void initState() {
    super.initState();
    _healthService = HealthSyncService(_apiClient);
    _loadUserId();
  }

  Future<void> _loadUserId() async {
    final userId = await _apiClient.getUserId();
    setState(() {
      _userId = userId;
      if (userId != null) {
        _statusMessage = 'Giris yapildi: ${userId.substring(0, 8)}...';
      }
    });
  }

  Future<void> _devLogin() async {
    setState(() {
      _isLoading = true;
      _statusMessage = 'Giris yapiliyor...';
    });

    try {
      final response = await _apiClient.post(
        ApiConstants.devLogin,
        data: {
          'username': 'TestUser${DateTime.now().millisecondsSinceEpoch}',
          'email': 'test${DateTime.now().millisecondsSinceEpoch}@test.com',
        },
      );

      final data = response.data as Map<String, dynamic>;
      final userId = data['userId'] as String;
      await _apiClient.saveUserId(userId);

      setState(() {
        _userId = userId;
        _statusMessage = 'Giris basarili! User: ${userId.substring(0, 8)}...';
      });
    } catch (e) {
      setState(() {
        _statusMessage = 'Giris hatasi: $e';
      });
    } finally {
      setState(() => _isLoading = false);
    }
  }

  Future<void> _requestHealthPermissions() async {
    setState(() {
      _isLoading = true;
      _statusMessage = 'Izinler isteniyor...';
    });

    try {
      final granted = await _healthService.requestPermissions();
      setState(() {
        _statusMessage = granted ? 'Saglik izinleri verildi' : 'Izinler reddedildi';
      });
    } catch (e) {
      setState(() => _statusMessage = 'Izin hatasi: $e');
    } finally {
      setState(() => _isLoading = false);
    }
  }

  Future<void> _syncHealthData() async {
    if (_userId == null) {
      setState(() => _statusMessage = 'Once giris yapin');
      return;
    }

    setState(() {
      _isLoading = true;
      _statusMessage = 'Senkronize ediliyor...';
    });

    try {
      final result = await _healthService.syncTodayData();
      setState(() {
        _lastSyncResult = result;
        _statusMessage = result.message;
      });
    } catch (e) {
      setState(() => _statusMessage = 'Senkronizasyon hatasi: $e');
    } finally {
      setState(() => _isLoading = false);
    }
  }

  Future<void> _logout() async {
    await _apiClient.clearSession();
    setState(() {
      _userId = null;
      _lastSyncResult = null;
      _statusMessage = 'Cikis yapildi';
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('HealthVerse'),
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
        actions: [
          if (_userId != null)
            IconButton(icon: const Icon(Icons.logout), onPressed: _logout),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  children: [
                    if (_isLoading) const CircularProgressIndicator()
                    else Icon(
                      _userId != null ? Icons.check_circle : Icons.info,
                      size: 48,
                      color: _userId != null ? Colors.green : Colors.grey,
                    ),
                    const SizedBox(height: 8),
                    Text(_statusMessage, textAlign: TextAlign.center),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
            if (_userId == null)
              ElevatedButton.icon(
                onPressed: _isLoading ? null : _devLogin,
                icon: const Icon(Icons.login),
                label: const Text('Development Login'),
                style: ElevatedButton.styleFrom(padding: const EdgeInsets.all(16)),
              ),
            if (_userId != null) ...[
              ElevatedButton.icon(
                onPressed: _isLoading ? null : _requestHealthPermissions,
                icon: const Icon(Icons.health_and_safety),
                label: const Text('Saglik Izinlerini Ver'),
                style: ElevatedButton.styleFrom(padding: const EdgeInsets.all(16)),
              ),
              const SizedBox(height: 8),
              ElevatedButton.icon(
                onPressed: _isLoading ? null : _syncHealthData,
                icon: const Icon(Icons.sync),
                label: const Text('Saglik Verilerini Senkronize Et'),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.all(16),
                  backgroundColor: Colors.green,
                  foregroundColor: Colors.white,
                ),
              ),
            ],
            if (_lastSyncResult != null) ...[
              const SizedBox(height: 16),
              Card(
                color: _lastSyncResult!.success ? Colors.green.shade50 : Colors.red.shade50,
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('Son Senkronizasyon', style: TextStyle(fontWeight: FontWeight.bold)),
                      const Divider(),
                      Text('Adimlar: ${_lastSyncResult!.totalSteps}'),
                      Text('Adim Puani: +${_lastSyncResult!.stepPointsEarned}'),
                      Text('Gorev Puani: +${_lastSyncResult!.taskPointsEarned}'),
                      Text('Hedefler: ${_lastSyncResult!.goalsCompleted}'),
                      Text('Gorevler: ${_lastSyncResult!.tasksCompleted}'),
                      const Divider(),
                      Text('Toplam: +${_lastSyncResult!.totalPointsEarned}', 
                           style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.green)),
                    ],
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

