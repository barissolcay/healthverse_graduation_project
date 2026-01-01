/// Onboarding State - Anket durumu ve skip sayacı
import 'package:flutter/foundation.dart';

/// Anket verilerini tutan model
class OnboardingSurveyData {
  int? birthYear;
  String? gender;
  String? city;
  String? employmentStatus;
  String? workType;
  int? height;
  int? weight;
  double? bmi;
  List<String> goals;
  String? activityLevel;
  List<String> activeHours;
  String? referralSource;
  int skipCount;
  DateTime? completedAt;
  bool rewardGiven;

  OnboardingSurveyData({
    this.birthYear,
    this.gender,
    this.city,
    this.employmentStatus,
    this.workType,
    this.height,
    this.weight,
    this.bmi,
    this.goals = const [],
    this.activityLevel,
    this.activeHours = const [],
    this.referralSource,
    this.skipCount = 0,
    this.completedAt,
    this.rewardGiven = false,
  });

  /// Skip sayısına göre ödül hakkı kontrolü
  bool get isEligibleForReward => skipCount <= 3;

  /// BMI hesapla
  void calculateBMI() {
    if (height != null && weight != null && height! > 0) {
      final heightInMeters = height! / 100;
      bmi = weight! / (heightInMeters * heightInMeters);
    }
  }

  /// BMI kategorisini döndür
  String get bmiCategory {
    if (bmi == null) return '';
    if (bmi! < 18.5) return 'Zayıf';
    if (bmi! < 25) return 'Normal';
    if (bmi! < 30) return 'Fazla Kilolu';
    if (bmi! < 35) return '1. Derece Obez';
    if (bmi! < 40) return '2. Derece Obez';
    return 'Morbid Obez';
  }

  /// JSON'a çevir (User.Metadata için)
  Map<String, dynamic> toJson() {
    return {
      'onboarding': {
        'birthYear': birthYear,
        'gender': gender,
        'city': city,
        'employmentStatus': employmentStatus,
        'workType': workType,
        'height': height,
        'weight': weight,
        'bmi': bmi?.toStringAsFixed(1),
        'goals': goals,
        'activityLevel': activityLevel,
        'activeHours': activeHours,
        'referralSource': referralSource,
        'skipCount': skipCount,
        'completedAt': completedAt?.toIso8601String(),
        'rewardGiven': rewardGiven,
      },
    };
  }

  /// Kopya oluştur
  OnboardingSurveyData copyWith({
    int? birthYear,
    String? gender,
    String? city,
    String? employmentStatus,
    String? workType,
    int? height,
    int? weight,
    double? bmi,
    List<String>? goals,
    String? activityLevel,
    List<String>? activeHours,
    String? referralSource,
    int? skipCount,
    DateTime? completedAt,
    bool? rewardGiven,
  }) {
    return OnboardingSurveyData(
      birthYear: birthYear ?? this.birthYear,
      gender: gender ?? this.gender,
      city: city ?? this.city,
      employmentStatus: employmentStatus ?? this.employmentStatus,
      workType: workType ?? this.workType,
      height: height ?? this.height,
      weight: weight ?? this.weight,
      bmi: bmi ?? this.bmi,
      goals: goals ?? this.goals,
      activityLevel: activityLevel ?? this.activityLevel,
      activeHours: activeHours ?? this.activeHours,
      referralSource: referralSource ?? this.referralSource,
      skipCount: skipCount ?? this.skipCount,
      completedAt: completedAt ?? this.completedAt,
      rewardGiven: rewardGiven ?? this.rewardGiven,
    );
  }
}

/// Onboarding State Notifier - Basit state management
class OnboardingStateNotifier extends ChangeNotifier {
  OnboardingSurveyData _data = OnboardingSurveyData();
  
  OnboardingSurveyData get data => _data;

  void updateBirthYear(int? year, {bool isSkipped = false}) {
    _data = _data.copyWith(birthYear: year);
    if (isSkipped) _incrementSkip();
    notifyListeners();
  }

  void updateGender(String? gender, {bool isSkipped = false}) {
    _data = _data.copyWith(gender: gender);
    if (isSkipped) _incrementSkip();
    notifyListeners();
  }

  void updateCity(String? city, {bool isSkipped = false}) {
    _data = _data.copyWith(city: city);
    if (isSkipped) _incrementSkip();
    notifyListeners();
  }

  void updateEmploymentStatus(String? status, {bool isSkipped = false}) {
    _data = _data.copyWith(employmentStatus: status);
    if (isSkipped) _incrementSkip();
    notifyListeners();
  }

  void updateWorkType(String? type, {bool isSkipped = false}) {
    _data = _data.copyWith(workType: type);
    if (isSkipped) _incrementSkip();
    notifyListeners();
  }

  void updateBodyMetrics(int? height, int? weight, {bool isSkipped = false}) {
    _data = _data.copyWith(height: height, weight: weight);
    _data.calculateBMI();
    if (isSkipped) _incrementSkip();
    notifyListeners();
  }

  void updateGoals(List<String> goals, {bool isSkipped = false}) {
    _data = _data.copyWith(goals: goals);
    if (isSkipped || goals.isEmpty) _incrementSkip();
    notifyListeners();
  }

  void updateActivityLevel(String? level, {bool isSkipped = false}) {
    _data = _data.copyWith(activityLevel: level);
    if (isSkipped) _incrementSkip();
    notifyListeners();
  }

  void updateActiveHours(List<String> hours, {bool isSkipped = false}) {
    _data = _data.copyWith(activeHours: hours);
    if (isSkipped || hours.isEmpty) _incrementSkip();
    notifyListeners();
  }

  void updateReferralSource(String? source, {bool isSkipped = false}) {
    _data = _data.copyWith(referralSource: source);
    if (isSkipped) _incrementSkip();
    notifyListeners();
  }

  void completeOnboarding() {
    _data = _data.copyWith(
      completedAt: DateTime.now(),
      rewardGiven: _data.isEligibleForReward,
    );
    notifyListeners();
  }

  void _incrementSkip() {
    _data = _data.copyWith(skipCount: _data.skipCount + 1);
  }

  void reset() {
    _data = OnboardingSurveyData();
    notifyListeners();
  }
}
