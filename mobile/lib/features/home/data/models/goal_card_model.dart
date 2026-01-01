/// Goal Card Model - Hedef kartı modeli
class GoalCardModel {
  final String goalId;
  final String title;
  final String? description;
  final String activityType;
  final String targetMetric;
  final int targetValue;
  final int currentProgress;
  final String frequency; // DAILY, WEEKLY, MONTHLY
  final DateTime startDate;
  final DateTime endDate;
  final String status;

  const GoalCardModel({
    required this.goalId,
    required this.title,
    this.description,
    required this.activityType,
    required this.targetMetric,
    required this.targetValue,
    required this.currentProgress,
    required this.frequency,
    required this.startDate,
    required this.endDate,
    required this.status,
  });

  /// İlerleme yüzdesi
  double get progressPercentage =>
      targetValue > 0 ? (currentProgress / targetValue).clamp(0.0, 1.0) : 0.0;

  /// Tamamlandı mı?
  bool get isCompleted => status == 'COMPLETED';

  /// Frekans Türkçe
  String get frequencyLabel {
    switch (frequency.toUpperCase()) {
      case 'DAILY':
        return 'Günlük';
      case 'WEEKLY':
        return 'Haftalık';
      case 'MONTHLY':
        return 'Aylık';
      default:
        return frequency;
    }
  }

  /// Süresi dolmuş mu?
  bool get isExpired => DateTime.now().isAfter(endDate);

  /// Kalan süre
  Duration get timeRemaining => endDate.difference(DateTime.now());

  factory GoalCardModel.fromJson(Map<String, dynamic> json) {
    return GoalCardModel(
      goalId: json['goalId'] as String? ?? '',
      title: json['title'] as String? ?? '',
      description: json['description'] as String?,
      activityType: json['activityType'] as String? ?? 'STEPS',
      targetMetric: json['targetMetric'] as String? ?? 'STEPS',
      targetValue: json['targetValue'] as int? ?? 0,
      currentProgress: json['currentProgress'] as int? ?? 0,
      frequency: json['frequency'] as String? ?? 'DAILY',
      startDate: DateTime.tryParse(json['startDate'] as String? ?? '') ?? DateTime.now(),
      endDate: DateTime.tryParse(json['endDate'] as String? ?? '') ?? DateTime.now(),
      status: json['status'] as String? ?? 'ACTIVE',
    );
  }

  /// Mock data
  static List<GoalCardModel> mockList() {
    return [
      GoalCardModel(
        goalId: '1',
        title: 'Haftalık 50km',
        activityType: 'WALKING',
        targetMetric: 'DISTANCE',
        targetValue: 50000,
        currentProgress: 32000,
        frequency: 'WEEKLY',
        startDate: DateTime.now().subtract(const Duration(days: 3)),
        endDate: DateTime.now().add(const Duration(days: 4)),
        status: 'ACTIVE',
      ),
    ];
  }
}
