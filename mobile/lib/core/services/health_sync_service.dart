import 'package:health/health.dart';
import '../network/api_client.dart';
import '../constants/api_constants.dart';

/// Health verilerini backend'e senkronize eden servis
class HealthSyncService {
  final Health _health = Health();
  final ApiClient _apiClient;

  // Izin istenen veri tipleri
  static const List<HealthDataType> _dataTypes = [
    HealthDataType.STEPS,
    HealthDataType.DISTANCE_WALKING_RUNNING,
    HealthDataType.ACTIVE_ENERGY_BURNED,
    HealthDataType.WORKOUT,
  ];

  HealthSyncService(this._apiClient);

  /// Saglik verisi izinlerini kontrol et ve iste
  Future<bool> requestPermissions() async {
    try {
      // Health Connect / HealthKit izni iste
      final granted = await _health.requestAuthorization(
        _dataTypes,
        permissions: _dataTypes.map((_) => HealthDataAccess.READ).toList(),
      );

      return granted;
    } catch (e) {
      return false;
    }
  }

  /// Izinlerin verilip verilmedigini kontrol et
  Future<bool> hasPermissions() async {
    try {
      return await _health.hasPermissions(_dataTypes) ?? false;
    } catch (e) {
      return false;
    }
  }

  /// Bugunun saglik verilerini al ve backend'e gonder
  Future<HealthSyncResult> syncTodayData() async {
    try {
      // Izin kontrolu
      final hasPerms = await hasPermissions();
      if (!hasPerms) {
        final granted = await requestPermissions();
        if (!granted) {
          return HealthSyncResult(
            success: false,
            message: 'Saglik verisi izni verilmedi',
          );
        }
      }

      // Bugunun baslangici ve simdi
      final now = DateTime.now();
      final midnight = DateTime(now.year, now.month, now.day);

      // Saglik verilerini al
      final healthData = await _health.getHealthDataFromTypes(
        types: _dataTypes,
        startTime: midnight,
        endTime: now,
      );

      if (healthData.isEmpty) {
        return HealthSyncResult(
          success: true,
          message: 'Bugun icin saglik verisi bulunamadi',
          totalSteps: 0,
        );
      }

      // Verileri aktivite listesine donustur
      final activities = _convertToActivities(healthData);

      if (activities.isEmpty) {
        return HealthSyncResult(
          success: true,
          message: 'Gonderilecek gecerli veri yok',
          totalSteps: 0,
        );
      }

      // Backend'e gonder
      final response = await _apiClient.post(
        ApiConstants.healthSync,
        data: {'activities': activities},
      );

      final data = response.data as Map<String, dynamic>;

      return HealthSyncResult(
        success: data['success'] ?? false,
        message: data['message'] ?? 'Senkronizasyon tamamlandi',
        totalSteps: data['totalSteps'] ?? 0,
        stepPointsEarned: data['stepPointsEarned'] ?? 0,
        taskPointsEarned: data['taskPointsEarned'] ?? 0,
        goalsCompleted: data['goalsCompleted'] ?? 0,
        tasksCompleted: data['tasksCompleted'] ?? 0,
        duelsUpdated: data['duelsUpdated'] ?? 0,
      );
    } catch (e) {
      return HealthSyncResult(
        success: false,
        message: 'Senkronizasyon hatasi: $e',
      );
    }
  }

  /// HealthDataPoint listesini API formatina cevir
  List<Map<String, dynamic>> _convertToActivities(
    List<HealthDataPoint> healthData,
  ) {
    final Map<String, Map<String, dynamic>> aggregated = {};

    for (final point in healthData) {
      final metric = _mapDataTypeToMetric(point.type);
      if (metric == null) continue;

      // Workout tipi varsa al (sadece WORKOUT tipi icin)
      String activityType = 'WALKING';
      if (point.type == HealthDataType.WORKOUT && point.value is WorkoutHealthValue) {
        final workoutValue = point.value as WorkoutHealthValue;
        activityType = _mapWorkoutType(workoutValue.workoutActivityType);
      }

      final key = '$activityType:$metric';

      // Ayni aktivite+metrik icin degerleri topla
      if (aggregated.containsKey(key)) {
        final current = aggregated[key]!;
        final currentValue = current['value'] as int;
        final newValue = _extractValue(point);

        // En yuksek degeri al
        current['value'] = currentValue > newValue ? currentValue : newValue;
      } else {
        aggregated[key] = {
          'activityType': activityType,
          'targetMetric': metric,
          'value': _extractValue(point),
          'recordingMethod': _mapRecordingMethod(point.recordingMethod),
        };
      }
    }

    return aggregated.values.toList();
  }

  /// HealthDataType -> Backend metric string
  String? _mapDataTypeToMetric(HealthDataType type) {
    switch (type) {
      case HealthDataType.STEPS:
        return 'STEPS';
      case HealthDataType.DISTANCE_WALKING_RUNNING:
        return 'DISTANCE';
      case HealthDataType.ACTIVE_ENERGY_BURNED:
        return 'CALORIES';
      case HealthDataType.WORKOUT:
        return 'DURATION';
      default:
        return null;
    }
  }

  /// WorkoutActivityType -> Backend activity type
  String _mapWorkoutType(HealthWorkoutActivityType type) {
    switch (type) {
      case HealthWorkoutActivityType.RUNNING:
        return 'RUNNING';
      case HealthWorkoutActivityType.BIKING:
        return 'CYCLING';
      case HealthWorkoutActivityType.SWIMMING:
        return 'SWIMMING';
      case HealthWorkoutActivityType.BASKETBALL:
        return 'BASKETBALL';
      case HealthWorkoutActivityType.SOCCER:
        return 'SOCCER';
      case HealthWorkoutActivityType.TENNIS:
        return 'TENNIS';
      case HealthWorkoutActivityType.YOGA:
        return 'YOGA';
      default:
        return 'WALKING';
    }
  }

  /// RecordingMethod -> Backend string
  String _mapRecordingMethod(RecordingMethod method) {
    switch (method) {
      case RecordingMethod.automatic:
        return 'AUTOMATIC';
      case RecordingMethod.active:
        return 'ACTIVE';
      case RecordingMethod.manual:
        return 'MANUAL';
      default:
        return 'UNKNOWN';
    }
  }

  /// HealthDataPoint'ten deger cikar
  int _extractValue(HealthDataPoint point) {
    final value = point.value;
    if (value is NumericHealthValue) {
      return value.numericValue.toInt();
    } else if (value is WorkoutHealthValue) {
      // Workout icin sure (dakika)
      return value.totalEnergyBurned?.toInt() ?? 0;
    }
    return 0;
  }
}

/// Sync sonucu
class HealthSyncResult {
  final bool success;
  final String message;
  final int totalSteps;
  final int stepPointsEarned;
  final int taskPointsEarned;
  final int goalsCompleted;
  final int tasksCompleted;
  final int duelsUpdated;

  HealthSyncResult({
    required this.success,
    required this.message,
    this.totalSteps = 0,
    this.stepPointsEarned = 0,
    this.taskPointsEarned = 0,
    this.goalsCompleted = 0,
    this.tasksCompleted = 0,
    this.duelsUpdated = 0,
  });

  int get totalPointsEarned => stepPointsEarned + taskPointsEarned;
}
