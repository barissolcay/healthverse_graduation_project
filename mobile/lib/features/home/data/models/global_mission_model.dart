/// Global Mission Model - Global görev modeli
class GlobalMissionModel {
  final String missionId;
  final String title;
  final String? description;
  final int targetValue;
  final int totalProgress;
  final int myContribution;
  final int participantCount;
  final DateTime deadline;
  final String status;
  final List<TopContributorModel> topContributors;

  const GlobalMissionModel({
    required this.missionId,
    required this.title,
    this.description,
    required this.targetValue,
    required this.totalProgress,
    required this.myContribution,
    required this.participantCount,
    required this.deadline,
    required this.status,
    this.topContributors = const [],
  });

  /// İlerleme yüzdesi
  double get progressPercentage =>
      targetValue > 0 ? (totalProgress / targetValue).clamp(0.0, 1.0) : 0.0;

  /// Tamamlandı mı?
  bool get isCompleted => status == 'COMPLETED';

  /// Kalan süre
  Duration get timeRemaining => deadline.difference(DateTime.now());

  /// Süresi dolmuş mu?
  bool get isExpired => DateTime.now().isAfter(deadline);

  factory GlobalMissionModel.fromJson(Map<String, dynamic> json) {
    return GlobalMissionModel(
      missionId: json['missionId'] as String? ?? '',
      title: json['title'] as String? ?? '',
      description: json['description'] as String?,
      targetValue: json['targetValue'] as int? ?? 0,
      totalProgress: json['totalProgress'] as int? ?? 0,
      myContribution: json['myContribution'] as int? ?? 0,
      participantCount: json['participantCount'] as int? ?? 0,
      deadline: DateTime.tryParse(json['deadline'] as String? ?? '') ?? DateTime.now(),
      status: json['status'] as String? ?? 'ACTIVE',
      topContributors: (json['topContributors'] as List<dynamic>?)
              ?.map((e) => TopContributorModel.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }

  /// Mock data
  static GlobalMissionModel? mock() {
    return GlobalMissionModel(
      missionId: '1',
      title: 'Türkiye 10M Adım Yarışı',
      description: 'Hep birlikte 10 milyon adım atalım!',
      targetValue: 10000000,
      totalProgress: 7200000,
      myContribution: 15000,
      participantCount: 1250,
      deadline: DateTime.now().add(const Duration(days: 5)),
      status: 'ACTIVE',
      topContributors: [
        const TopContributorModel(username: 'SuperWalker', contribution: 85000, rank: 1),
        const TopContributorModel(username: 'FitGirl42', contribution: 72000, rank: 2),
        const TopContributorModel(username: 'StepMaster', contribution: 68000, rank: 3),
      ],
    );
  }
}

/// Top Contributor Model
class TopContributorModel {
  final String username;
  final int contribution;
  final int rank;
  final String? avatarUrl;

  const TopContributorModel({
    required this.username,
    required this.contribution,
    required this.rank,
    this.avatarUrl,
  });

  factory TopContributorModel.fromJson(Map<String, dynamic> json) {
    return TopContributorModel(
      username: json['username'] as String? ?? '',
      contribution: json['contribution'] as int? ?? 0,
      rank: json['rank'] as int? ?? 0,
      avatarUrl: json['avatarUrl'] as String?,
    );
  }
}
