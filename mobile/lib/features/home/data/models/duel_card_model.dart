/// Duel Card Model - Düello kartı modeli
class DuelCardModel {
  final String duelId;
  final String opponentId;
  final String opponentUsername;
  final String? opponentAvatarUrl;
  final String activityType;
  final String targetMetric;
  final int targetValue;
  final int myProgress;
  final int opponentProgress;
  final DateTime validUntil;
  final String status;

  const DuelCardModel({
    required this.duelId,
    required this.opponentId,
    required this.opponentUsername,
    this.opponentAvatarUrl,
    required this.activityType,
    required this.targetMetric,
    required this.targetValue,
    required this.myProgress,
    required this.opponentProgress,
    required this.validUntil,
    required this.status,
  });

  /// Kazanıyorum mu?
  bool get isWinning => myProgress > opponentProgress;

  /// Berabere mi?
  bool get isTied => myProgress == opponentProgress;

  /// Kaybediyor muyum?
  bool get isLosing => myProgress < opponentProgress;

  /// Benim ilerleme yüzdesi
  double get myProgressPercentage =>
      targetValue > 0 ? (myProgress / targetValue).clamp(0.0, 1.0) : 0.0;

  /// Rakip ilerleme yüzdesi
  double get opponentProgressPercentage =>
      targetValue > 0 ? (opponentProgress / targetValue).clamp(0.0, 1.0) : 0.0;

  /// Düello aktif mi?
  bool get isActive => status == 'ACTIVE';

  /// Kalan süre
  Duration get timeRemaining => validUntil.difference(DateTime.now());

  /// Süresi dolmuş mu?
  bool get isExpired => DateTime.now().isAfter(validUntil);

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

  /// Kalan süre formatı
  String get formattedTimeRemaining {
    final remaining = timeRemaining;
    if (remaining.isNegative) return 'Süresi doldu';
    if (remaining.inDays > 0) return '${remaining.inDays} gün';
    if (remaining.inHours > 0) return '${remaining.inHours} saat';
    if (remaining.inMinutes > 0) return '${remaining.inMinutes} dk';
    return 'Sonlanıyor';
  }

  factory DuelCardModel.fromJson(Map<String, dynamic> json) {
    return DuelCardModel(
      duelId: json['duelId'] as String? ?? '',
      opponentId: json['opponentId'] as String? ?? '',
      opponentUsername: json['opponentUsername'] as String? ?? 'Rakip',
      opponentAvatarUrl: json['opponentAvatarUrl'] as String?,
      activityType: json['activityType'] as String? ?? 'STEPS',
      targetMetric: json['targetMetric'] as String? ?? 'STEPS',
      targetValue: json['targetValue'] as int? ?? 0,
      myProgress: json['myProgress'] as int? ?? 0,
      opponentProgress: json['opponentProgress'] as int? ?? 0,
      validUntil: DateTime.tryParse(json['validUntil'] as String? ?? '') ?? DateTime.now(),
      status: json['status'] as String? ?? 'ACTIVE',
    );
  }

  /// Mock data
  static DuelCardModel? mock() {
    return DuelCardModel(
      duelId: '1',
      opponentId: 'opp1',
      opponentUsername: 'AyşeKoşar',
      activityType: 'STEPS',
      targetMetric: 'STEPS',
      targetValue: 10000,
      myProgress: 6500,
      opponentProgress: 5200,
      validUntil: DateTime.now().add(const Duration(hours: 18)),
      status: 'ACTIVE',
    );
  }
}
