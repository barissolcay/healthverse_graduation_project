/// Task Card Model - Görev kartı modeli
class TaskCardModel {
  final String taskId;
  final String title;
  final String? description;
  final String activityType;
  final String targetMetric;
  final int targetValue;
  final int currentProgress;
  final int pointValue;
  final String status; // ACTIVE, COMPLETED, CLAIMED
  final DateTime? deadline;

  const TaskCardModel({
    required this.taskId,
    required this.title,
    this.description,
    required this.activityType,
    required this.targetMetric,
    required this.targetValue,
    required this.currentProgress,
    required this.pointValue,
    required this.status,
    this.deadline,
  });

  /// İlerleme yüzdesi (0-1)
  double get progressPercentage =>
      targetValue > 0 ? (currentProgress / targetValue).clamp(0.0, 1.0) : 0.0;

  /// Tamamlandı mı?
  bool get isCompleted => status == 'COMPLETED' || status == 'CLAIMED';

  /// Claim edilebilir mi?
  bool get isClaimable => status == 'COMPLETED';

  /// Süresi dolmuş mu?
  bool get isExpired => deadline != null && DateTime.now().isAfter(deadline!);

  /// Kalan süre
  Duration get timeRemaining => deadline?.difference(DateTime.now()) ?? Duration.zero;

  /// Aktivite türü Türkçe
  String get activityTypeLabel {
    switch (activityType.toUpperCase()) {
      case 'WALKING':
        return 'Yürüme';
      case 'RUNNING':
        return 'Koşu';
      case 'STEPS':
        return 'Adım';
      default:
        return activityType;
    }
  }

  factory TaskCardModel.fromJson(Map<String, dynamic> json) {
    return TaskCardModel(
      taskId: json['taskId'] as String? ?? '',
      title: json['title'] as String? ?? '',
      description: json['description'] as String?,
      activityType: json['activityType'] as String? ?? 'STEPS',
      targetMetric: json['targetMetric'] as String? ?? 'STEPS',
      targetValue: json['targetValue'] as int? ?? 0,
      currentProgress: json['currentProgress'] as int? ?? 0,
      pointValue: json['pointValue'] as int? ?? 0,
      status: json['status'] as String? ?? 'ACTIVE',
      deadline: json['deadline'] != null 
          ? DateTime.tryParse(json['deadline'] as String) 
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'taskId': taskId,
      'title': title,
      'description': description,
      'activityType': activityType,
      'targetMetric': targetMetric,
      'targetValue': targetValue,
      'currentProgress': currentProgress,
      'pointValue': pointValue,
      'status': status,
      'deadline': deadline?.toIso8601String(),
    };
  }

  /// Mock data
  static List<TaskCardModel> mockList() {
    return [
      const TaskCardModel(
        taskId: '1',
        title: '5000 Adım At',
        activityType: 'STEPS',
        targetMetric: 'STEPS',
        targetValue: 5000,
        currentProgress: 3200,
        pointValue: 50,
        status: 'ACTIVE',
      ),
      const TaskCardModel(
        taskId: '2',
        title: '2km Koş',
        activityType: 'RUNNING',
        targetMetric: 'DISTANCE',
        targetValue: 2000,
        currentProgress: 2000,
        pointValue: 75,
        status: 'COMPLETED',
      ),
    ];
  }
}
