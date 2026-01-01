import '../../data/models/home_data_model.dart';
import '../../data/models/user_stats_model.dart';
import '../../data/models/task_card_model.dart';
import '../../data/models/goal_card_model.dart';
import '../../data/models/duel_card_model.dart';
import '../../data/models/league_card_model.dart';
import '../../data/models/partner_mission_model.dart';
import '../../data/models/global_mission_model.dart';

/// Home repository interface
/// API çağrılarının soyutlanmış arayüzü
abstract class HomeRepository {
  /// Kullanıcı istatistiklerini getir
  Future<UserStatsModel> getUserStats(String userId);
  
  /// Aktif görevleri getir
  Future<List<TaskCardModel>> getActiveTasks();
  
  /// Aktif hedefleri getir
  Future<List<GoalCardModel>> getActiveGoals();
  
  /// Lig odasını getir
  Future<LeagueCardModel?> getMyLeagueRoom();
  
  /// Aktif düelloyu getir
  Future<DuelCardModel?> getActiveDuel();
  
  /// Aktif partner görevini getir
  Future<PartnerMissionModel?> getActivePartnerMission();
  
  /// Aktif global görevi getir
  Future<GlobalMissionModel?> getActiveGlobalMission();
  
  /// Okunmamış bildirim sayısını getir
  Future<int> getUnreadNotificationCount();
  
  /// Tüm home verilerini getir (parallel fetch)
  Future<HomeDataModel> getAllHomeData(String userId);
}
