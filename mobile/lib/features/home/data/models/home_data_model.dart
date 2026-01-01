/// Home Data Model - Ana sayfa verileri
import 'user_stats_model.dart';
import 'task_card_model.dart';
import 'goal_card_model.dart';
import 'duel_card_model.dart';
import 'league_card_model.dart';
import 'partner_mission_model.dart';
import 'global_mission_model.dart';

class HomeDataModel {
  final UserStatsModel userStats;
  final List<TaskCardModel> activeTasks;
  final List<GoalCardModel> activeGoals;
  final DuelCardModel? activeDuel;
  final LeagueCardModel? leagueInfo;
  final PartnerMissionModel? partnerMission;
  final GlobalMissionModel? globalMission;
  final int unreadNotificationCount;

  const HomeDataModel({
    required this.userStats,
    this.activeTasks = const [],
    this.activeGoals = const [],
    this.activeDuel,
    this.leagueInfo,
    this.partnerMission,
    this.globalMission,
    this.unreadNotificationCount = 0,
  });

  /// Gösterilecek görevler (ilk 3)
  List<TaskCardModel> get displayTasks => activeTasks.take(3).toList();

  /// Gösterilecek hedefler (ilk 2)
  List<GoalCardModel> get displayGoals => activeGoals.take(2).toList();

  /// Tamamlanmamış görev sayısı
  int get pendingTaskCount =>
      activeTasks.where((t) => t.status == 'ACTIVE').length;

  factory HomeDataModel.fromJson(Map<String, dynamic> json) {
    return HomeDataModel(
      userStats: UserStatsModel.fromJson(json['userStats'] as Map<String, dynamic>? ?? {}),
      activeTasks: (json['activeTasks'] as List<dynamic>?)
              ?.map((e) => TaskCardModel.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      activeGoals: (json['activeGoals'] as List<dynamic>?)
              ?.map((e) => GoalCardModel.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      activeDuel: json['activeDuel'] != null
          ? DuelCardModel.fromJson(json['activeDuel'] as Map<String, dynamic>)
          : null,
      leagueInfo: json['leagueInfo'] != null
          ? LeagueCardModel.fromJson(json['leagueInfo'] as Map<String, dynamic>)
          : null,
      partnerMission: json['partnerMission'] != null
          ? PartnerMissionModel.fromJson(json['partnerMission'] as Map<String, dynamic>)
          : null,
      globalMission: json['globalMission'] != null
          ? GlobalMissionModel.fromJson(json['globalMission'] as Map<String, dynamic>)
          : null,
      unreadNotificationCount: json['unreadNotificationCount'] as int? ?? 0,
    );
  }

  /// Mock data
  factory HomeDataModel.mock() {
    return HomeDataModel(
      userStats: UserStatsModel.mock(),
      activeTasks: TaskCardModel.mockList(),
      activeGoals: GoalCardModel.mockList(),
      activeDuel: DuelCardModel.mock(),
      leagueInfo: LeagueCardModel.mock(),
      partnerMission: PartnerMissionModel.mock(),
      globalMission: GlobalMissionModel.mock(),
      unreadNotificationCount: 3,
    );
  }
}
