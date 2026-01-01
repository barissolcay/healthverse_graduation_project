/// User Stats Model - Kullanıcı istatistikleri
class UserStatsModel {
  final String username;
  final String? avatarUrl;
  final int currentStreak;
  final int todaySteps;
  final int todayCalories;
  final double todayDistance;
  final int totalPoints;
  final int freezeCount;
  final bool hasHealthPermission;

  const UserStatsModel({
    required this.username,
    this.avatarUrl,
    required this.currentStreak,
    required this.todaySteps,
    required this.todayCalories,
    required this.todayDistance,
    required this.totalPoints,
    required this.freezeCount,
    required this.hasHealthPermission,
  });

  /// Seri tamamlandı mı? (3000+ adım)
  bool get isStreakCompleted => todaySteps >= 3000;

  /// Adım puanı hesapla (3000 üstü her 1000 adım = 1 puan)
  int get stepPoints {
    if (todaySteps <= 3000) return 0;
    return ((todaySteps - 3000) / 1000).floor();
  }

  /// Adım ilerleme yüzdesi (0-100)
  double get stepProgressPercentage => (todaySteps / 3000).clamp(0.0, 1.0);

  /// Mock data factory
  factory UserStatsModel.mock() {
    return const UserStatsModel(
      username: 'TestUser',
      avatarUrl: null,
      currentStreak: 10,
      todaySteps: 4500,
      todayCalories: 320,
      todayDistance: 3.2,
      totalPoints: 1250,
      freezeCount: 2,
      hasHealthPermission: true,
    );
  }

  factory UserStatsModel.fromJson(Map<String, dynamic> json) {
    return UserStatsModel(
      username: json['username'] as String? ?? 'User',
      avatarUrl: json['avatarUrl'] as String?,
      currentStreak: json['currentStreak'] as int? ?? 0,
      todaySteps: json['todaySteps'] as int? ?? 0,
      todayCalories: json['todayCalories'] as int? ?? 0,
      todayDistance: (json['todayDistance'] as num?)?.toDouble() ?? 0.0,
      totalPoints: json['totalPoints'] as int? ?? 0,
      freezeCount: json['freezeCount'] as int? ?? 0,
      hasHealthPermission: json['hasHealthPermission'] as bool? ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'username': username,
      'avatarUrl': avatarUrl,
      'currentStreak': currentStreak,
      'todaySteps': todaySteps,
      'todayCalories': todayCalories,
      'todayDistance': todayDistance,
      'totalPoints': totalPoints,
      'freezeCount': freezeCount,
      'hasHealthPermission': hasHealthPermission,
    };
  }

  UserStatsModel copyWith({
    String? username,
    String? avatarUrl,
    int? currentStreak,
    int? todaySteps,
    int? todayCalories,
    double? todayDistance,
    int? totalPoints,
    int? freezeCount,
    bool? hasHealthPermission,
  }) {
    return UserStatsModel(
      username: username ?? this.username,
      avatarUrl: avatarUrl ?? this.avatarUrl,
      currentStreak: currentStreak ?? this.currentStreak,
      todaySteps: todaySteps ?? this.todaySteps,
      todayCalories: todayCalories ?? this.todayCalories,
      todayDistance: todayDistance ?? this.todayDistance,
      totalPoints: totalPoints ?? this.totalPoints,
      freezeCount: freezeCount ?? this.freezeCount,
      hasHealthPermission: hasHealthPermission ?? this.hasHealthPermission,
    );
  }
}
