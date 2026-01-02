import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/api_client.dart';
import '../../data/models/home_data_model.dart';
import '../../data/models/user_stats_model.dart';
import '../../data/repositories/home_repository_impl.dart';

/// Home screen status'ları
enum HomeStatus { initial, loading, loaded, error }

/// Home state - Tüm home ekranı verilerini tutar
class HomeState {
  final HomeStatus status;
  final HomeDataModel? homeData;
  final UserStatsModel? userStats;
  final String? errorMessage;
  final bool isHealthPermissionGranted;
  final DateTime? lastSyncTime;

  const HomeState({
    this.status = HomeStatus.initial,
    this.homeData,
    this.userStats,
    this.errorMessage,
    this.isHealthPermissionGranted = false,
    this.lastSyncTime,
  });

  /// Loading state check
  bool get isLoading => status == HomeStatus.loading;

  /// Error check
  String? get error => errorMessage;

  HomeState copyWith({
    HomeStatus? status,
    HomeDataModel? homeData,
    UserStatsModel? userStats,
    String? errorMessage,
    bool? isHealthPermissionGranted,
    DateTime? lastSyncTime,
  }) {
    return HomeState(
      status: status ?? this.status,
      homeData: homeData ?? this.homeData,
      userStats: userStats ?? this.userStats,
      errorMessage: errorMessage,
      isHealthPermissionGranted: isHealthPermissionGranted ?? this.isHealthPermissionGranted,
      lastSyncTime: lastSyncTime ?? this.lastSyncTime,
    );
  }
}

/// Home screen notifier - Riverpod StateNotifier
class HomeScreenNotifier extends StateNotifier<HomeState> {
  final HomeRepositoryImpl _repository;
  final bool _initialHealthPermission;

  Timer? _autoSyncTimer;

  HomeScreenNotifier(this._repository, {bool initialHealthPermission = false})
      : _initialHealthPermission = initialHealthPermission,
        super(const HomeState()) {
    _initialize();
  }

  /// İlk başlatma
  Future<void> _initialize() async {
    // Health permission durumuna göre yükle
    if (_initialHealthPermission) {
      await loadMockData();
    } else {
      // İzin verilmemiş - kısıtlı mod
      state = state.copyWith(
        status: HomeStatus.loaded,
        isHealthPermissionGranted: false,
      );
    }
  }

  /// Mock data yükle (test için)
  Future<void> loadMockData() async {
    state = state.copyWith(status: HomeStatus.loading);

    try {
      final homeData = await _repository.getMockHomeData();
      final userStats = UserStatsModel.mock();

      state = state.copyWith(
        status: HomeStatus.loaded,
        homeData: homeData,
        userStats: userStats,
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
      final homeData = await _repository.getAllHomeData(userId);
      final userStats = UserStatsModel.mock();

      state = state.copyWith(
        status: HomeStatus.loaded,
        homeData: homeData,
        userStats: userStats,
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
    if (state.isHealthPermissionGranted) {
      await loadMockData();
    }
  }

  /// Health permission iste
  Future<void> requestHealthPermission() async {
    state = state.copyWith(status: HomeStatus.loading);

    try {
      // Gerçek uygulamada burada platform permission istenecek
      await Future.delayed(const Duration(milliseconds: 500));
      
      // İzin verildi, verileri yükle
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

/// Provider: Health permission durumu (gerçek uygulamada SharedPreferences'tan okunur)
final healthPermissionProvider = StateProvider<bool>((ref) {
  // TODO: Gerçek uygulamada kontrol edilecek
  // Şimdilik TRUE olarak başla (test için)
  // FALSE yaparak kısıtlı modu test edebilirsin
  return true;
});

/// Provider: Home screen notifier
final homeScreenProvider = StateNotifierProvider<HomeScreenNotifier, HomeState>((ref) {
  final repository = ref.watch(homeRepositoryProvider);
  final hasHealthPermission = ref.watch(healthPermissionProvider);
  return HomeScreenNotifier(repository, initialHealthPermission: hasHealthPermission);
});
