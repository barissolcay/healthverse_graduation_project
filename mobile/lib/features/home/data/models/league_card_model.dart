/// League Card Model - Lig kartı modeli
class LeagueCardModel {
  final String leagueId;
  final String leagueName;
  final String tierName; // Bronz, Gümüş, Altın, Platin, Elmas
  final int currentRank;
  final int totalParticipants;
  final int myPoints;
  final int pointsToPromote;
  final int pointsToRelegation;
  final DateTime weekEndsAt;

  const LeagueCardModel({
    required this.leagueId,
    required this.leagueName,
    required this.tierName,
    required this.currentRank,
    required this.totalParticipants,
    required this.myPoints,
    required this.pointsToPromote,
    required this.pointsToRelegation,
    required this.weekEndsAt,
  });

  /// Yükselme bölgesinde mi? (top 3)
  bool get isInPromotionZone => currentRank <= 3;

  /// Düşme bölgesinde mi? (son 2)
  bool get isInRelegationZone => currentRank > totalParticipants - 2;

  /// Kalan süre
  Duration get timeRemaining => weekEndsAt.difference(DateTime.now());

  /// Kalan süre formatı
  String get formattedTimeRemaining {
    final remaining = timeRemaining;
    if (remaining.isNegative) return 'Bitti';
    if (remaining.inDays > 0) return '${remaining.inDays} gün';
    if (remaining.inHours > 0) return '${remaining.inHours} saat';
    return '${remaining.inMinutes} dk';
  }

  /// Tier display name (alias)
  String get tierDisplayName => tierName;

  /// My rank (alias)
  int get myRank => currentRank;

  /// Total members (alias)
  int get totalMembers => totalParticipants;

  /// Can promote?
  bool get canPromote => isInPromotionZone;

  /// Can demote?
  bool get canDemote => isInRelegationZone;

  /// Rank percentage (0-1)
  double get rankPercentage => totalParticipants > 0 
      ? (totalParticipants - currentRank + 1) / totalParticipants 
      : 0.0;

  factory LeagueCardModel.fromJson(Map<String, dynamic> json) {
    return LeagueCardModel(
      leagueId: json['leagueId'] as String? ?? '',
      leagueName: json['leagueName'] as String? ?? '',
      tierName: json['tierName'] as String? ?? 'Bronz',
      currentRank: json['currentRank'] as int? ?? 0,
      totalParticipants: json['totalParticipants'] as int? ?? 0,
      myPoints: json['myPoints'] as int? ?? 0,
      pointsToPromote: json['pointsToPromote'] as int? ?? 0,
      pointsToRelegation: json['pointsToRelegation'] as int? ?? 0,
      weekEndsAt: DateTime.tryParse(json['weekEndsAt'] as String? ?? '') ?? DateTime.now(),
    );
  }

  /// Mock data
  static LeagueCardModel mock() {
    return LeagueCardModel(
      leagueId: '1',
      leagueName: 'TEMPO Ligi #42',
      tierName: 'TEMPO',
      currentRank: 5,
      totalParticipants: 30,
      myPoints: 850,
      pointsToPromote: 200,
      pointsToRelegation: 150,
      weekEndsAt: DateTime.now().add(const Duration(days: 3)),
    );
  }
}
