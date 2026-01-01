import 'package:dio/dio.dart';
import '../../../../core/network/api_client.dart';
import '../models/home_data_model.dart';
import '../models/user_stats_model.dart';
import '../models/task_card_model.dart';
import '../models/goal_card_model.dart';
import '../models/duel_card_model.dart';
import '../models/league_card_model.dart';
import '../models/partner_mission_model.dart';
import '../models/global_mission_model.dart';
import '../../domain/repositories/home_repository.dart';

/// Home repository implementation
/// API çağrılarını gerçekleştirir
class HomeRepositoryImpl implements HomeRepository {
  final ApiClient _apiClient;
  
  HomeRepositoryImpl(this._apiClient);
  
  @override
  Future<UserStatsModel> getUserStats(String userId) async {
    try {
      final response = await _apiClient.get('/api/users/$userId/stats');
      return UserStatsModel.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        throw Exception('Kullanıcı bulunamadı');
      }
      rethrow;
    }
  }
  
  @override
  Future<List<TaskCardModel>> getActiveTasks() async {
    try {
      final response = await _apiClient.get('/api/tasks/active');
      final List<dynamic> dataList = response.data['tasks'] as List<dynamic>? ?? [];
      return dataList.map((json) => TaskCardModel.fromJson(json as Map<String, dynamic>)).toList();
    } on DioException {
      rethrow;
    }
  }
  
  @override
  Future<List<GoalCardModel>> getActiveGoals() async {
    try {
      final response = await _apiClient.get('/api/goals/active');
      final List<dynamic> dataList = response.data['goals'] as List<dynamic>? ?? [];
      return dataList.map((json) => GoalCardModel.fromJson(json as Map<String, dynamic>)).toList();
    } on DioException {
      rethrow;
    }
  }
  
  @override
  Future<LeagueCardModel?> getMyLeagueRoom() async {
    try {
      final response = await _apiClient.get('/api/league/my-room');
      return LeagueCardModel.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        return null;
      }
      rethrow;
    }
  }
  
  @override
  Future<DuelCardModel?> getActiveDuel() async {
    try {
      final response = await _apiClient.get('/api/duels/active');
      final List<dynamic> dataList = response.data['duels'] as List<dynamic>? ?? [];
      if (dataList.isEmpty) return null;
      return DuelCardModel.fromJson(dataList.first as Map<String, dynamic>);
    } on DioException {
      rethrow;
    }
  }
  
  @override
  Future<PartnerMissionModel?> getActivePartnerMission() async {
    try {
      final response = await _apiClient.get('/api/missions/partner/active');
      final List<dynamic> dataList = response.data['missions'] as List<dynamic>? ?? [];
      if (dataList.isEmpty) return null;
      return PartnerMissionModel.fromJson(dataList.first as Map<String, dynamic>);
    } on DioException {
      rethrow;
    }
  }
  
  @override
  Future<GlobalMissionModel?> getActiveGlobalMission() async {
    try {
      final response = await _apiClient.get('/api/missions/global/active');
      final List<dynamic> dataList = response.data['missions'] as List<dynamic>? ?? [];
      if (dataList.isEmpty) return null;
      return GlobalMissionModel.fromJson(dataList.first as Map<String, dynamic>);
    } on DioException {
      rethrow;
    }
  }
  
  @override
  Future<int> getUnreadNotificationCount() async {
    try {
      final response = await _apiClient.get('/api/notifications/unread-count');
      return response.data['count'] as int? ?? 0;
    } on DioException {
      return 0;
    }
  }
  
  @override
  Future<HomeDataModel> getAllHomeData(String userId) async {
    // Parallel fetch - tüm API'leri aynı anda çağır
    final results = await Future.wait([
      getUserStats(userId),
      getActiveTasks(),
      getActiveGoals(),
      getMyLeagueRoom(),
      getActiveDuel(),
      getActivePartnerMission(),
      getActiveGlobalMission(),
      getUnreadNotificationCount(),
    ], eagerError: false);
    
    return HomeDataModel(
      userStats: results[0] as UserStatsModel,
      activeTasks: results[1] as List<TaskCardModel>,
      activeGoals: results[2] as List<GoalCardModel>,
      leagueInfo: results[3] as LeagueCardModel?,
      activeDuel: results[4] as DuelCardModel?,
      partnerMission: results[5] as PartnerMissionModel?,
      globalMission: results[6] as GlobalMissionModel?,
      unreadNotificationCount: results[7] as int,
    );
  }
  
  /// Mock data for testing without backend
  Future<HomeDataModel> getMockHomeData() async {
    await Future.delayed(const Duration(milliseconds: 500)); // Simüle loading
    return HomeDataModel.mock();
  }
}
