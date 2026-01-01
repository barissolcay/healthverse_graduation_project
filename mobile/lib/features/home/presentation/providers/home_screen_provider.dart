import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/services/health_sync_service.dart';
import '../../data/models/home_data_model.dart';
import '../../data/repositories/home_repository_impl.dart';

/// Home screen status'ları
enum HomeStatus { initial, loading, loaded, error }

/// Home state - Tüm home ekranı verilerini tutar
class HomeState {
  final HomeStatus status;
  final HomeDataModel? data;
  final String? errorMessage;
  final bool isHealthPermissionGranted;
  final DateTime? lastSyncTime;
  
  const HomeState({
    this.status = HomeStatus.initial,
    this.data,
    this.errorMessage,
    this.isHealthPermissionGranted = false,
    this.lastSyncTime,
  });
  
  HomeState copyWith({
    HomeStatus? status,
    HomeDataModel? data,
    String? errorMessage,
    bool? isHealthPermissionGranted,
    DateTime? lastSyncTime,
  }) {
    return HomeState(
      status: status ?? this.status,
      data: data ?? this.data,
      errorMessage: errorMessage,
      isHealthPermissionGranted: isHealthPermissionGranted ?? this.isHealthPermissionGranted,
      lastSyncTime: lastSyncTime ?? this.lastSyncTime,
    );
  }
}

/// Home screen notifier - Riverpod StateNotifier
class HomeScreenNotifier extends StateNotifier<HomeState> {
  final HomeRepositoryImpl _repository;
  final HealthSyncService? _healthService;
  
  Timer? _autoSyncTimer;
  
  HomeScreenNotifier(this._repository, [this._healthService]) : super(const HomeState()) {
    _initialize();
  }
  
  /// İlk başlatma
  Future<void> _initialize() async {
    // Şimdilik mock data kullan (backend hazır değil)
    await loadMockData();
  }
  
  /// Mock data yükle (test için)
  Future<void> loadMockData() async {
    state = state.copyWith(status: HomeStatus.loading);
    
    try {
      final data = await _repository.getMockHomeData();
      state = state.copyWith(
        status: HomeStatus.loaded,
        data: data,
        isHealthPermissionGranted: true,
        lastSyncTime: DateTime.now(),
      );
    } catch (e) {
      state = state.copyWith(
        status: HomeStatus.error,
        errorMessage: 'Veriler yüklenemedi: $e',
      );
    }
  }
  
  /// Home verilerini yükle (gerçek API)
  Future<void> loadHomeData(String userId) async {
    state = state.copyWith(status: HomeStatus.loading);
    
    try {
      final data = await _repository.getAllHomeData(userId);
      state = state.copyWith(
        status: HomeStatus.loaded,
        data: data,
        isHealthPermissionGranted: true,
        lastSyncTime: DateTime.now(),
      );
    } catch (e) {
      state = state.copyWith(
        status: HomeStatus.error,
        errorMessage: 'Veriler yüklenemedi: $e',
      );
    }
  }
  
  /// Yenile (pull-to-refresh)
  Future<void> refresh() async {
    await loadMockData(); // Şimdilik mock
  }
  
  /// Health permission iste
  Future<void> requestHealthPermission() async {
    state = state.copyWith(status: HomeStatus.loading);
    
    try {
      // Şimdilik mock olarak izin verildi say
      await Future.delayed(const Duration(milliseconds: 500));
      await loadMockData();
    } catch (e) {
      state = state.copyWith(
        status: HomeStatus.error,
        errorMessage: 'İzin isteği sırasında hata: $e',
      );
    }
  }
  
  @override
  void dispose() {
    _autoSyncTimer?.cancel();
    super.dispose();
  }
}

/// Provider: API client
final apiClientProvider = Provider<ApiClient>((ref) {
  return ApiClient();
});

/// Provider: Home repository
final homeRepositoryProvider = Provider<HomeRepositoryImpl>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return HomeRepositoryImpl(apiClient);
});

/// Provider: Home screen notifier
final homeScreenProvider = StateNotifierProvider<HomeScreenNotifier, HomeState>((ref) {
  final repository = ref.watch(homeRepositoryProvider);
  return HomeScreenNotifier(repository);
});
