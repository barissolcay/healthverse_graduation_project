/// Partner Mission Model - Partner görevi modeli
class PartnerMissionModel {
  final String missionId;
  final String partnerId;
  final String partnerUsername;
  final String? partnerAvatarUrl;
  final String title;
  final String activityType;
  final int targetValue;
  final int myContribution;
  final int partnerContribution;
  final DateTime deadline;
  final String status;

  const PartnerMissionModel({
    required this.missionId,
    required this.partnerId,
    required this.partnerUsername,
    this.partnerAvatarUrl,
    required this.title,
    required this.activityType,
    required this.targetValue,
    required this.myContribution,
    required this.partnerContribution,
    required this.deadline,
    required this.status,
  });

  /// Toplam ilerleme
  int get totalProgress => myContribution + partnerContribution;

  /// İlerleme yüzdesi
  double get progressPercentage =>
      targetValue > 0 ? (totalProgress / targetValue).clamp(0.0, 1.0) : 0.0;

  /// Tamamlandı mı?
  bool get isCompleted => status == 'COMPLETED';

  /// Kalan süre
  Duration get timeRemaining => deadline.difference(DateTime.now());

  /// Süresi dolmuş mu?
  bool get isExpired => DateTime.now().isAfter(deadline);

  factory PartnerMissionModel.fromJson(Map<String, dynamic> json) {
    return PartnerMissionModel(
      missionId: json['missionId'] as String? ?? '',
      partnerId: json['partnerId'] as String? ?? '',
      partnerUsername: json['partnerUsername'] as String? ?? 'Partner',
      partnerAvatarUrl: json['partnerAvatarUrl'] as String?,
      title: json['title'] as String? ?? '',
      activityType: json['activityType'] as String? ?? 'STEPS',
      targetValue: json['targetValue'] as int? ?? 0,
      myContribution: json['myContribution'] as int? ?? 0,
      partnerContribution: json['partnerContribution'] as int? ?? 0,
      deadline: DateTime.tryParse(json['deadline'] as String? ?? '') ?? DateTime.now(),
      status: json['status'] as String? ?? 'ACTIVE',
    );
  }

  /// Mock data
  static PartnerMissionModel? mock() {
    return PartnerMissionModel(
      missionId: '1',
      partnerId: 'p1',
      partnerUsername: 'MehmetFit',
      title: 'Birlikte 100K Adım',
      activityType: 'STEPS',
      targetValue: 100000,
      myContribution: 42000,
      partnerContribution: 38000,
      deadline: DateTime.now().add(const Duration(days: 2)),
      status: 'ACTIVE',
    );
  }
}
