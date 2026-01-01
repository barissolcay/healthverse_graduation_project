/// Aktivite Seviyesi Ekranı - Soru 7/9
import 'package:flutter/material.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/active_hours_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/survey_option_tile.dart';

class ActivityLevelScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const ActivityLevelScreen({super.key, required this.stateNotifier});

  @override
  State<ActivityLevelScreen> createState() => _ActivityLevelScreenState();
}

class _ActivityLevelScreenState extends State<ActivityLevelScreen> {
  String? _selectedLevel;

  final List<Map<String, String?>> _options = [
    {
      'value': 'sedentary',
      'label': 'Hareketsiz',
      'description': 'Çoğunlukla oturuyorum',
    },
    {
      'value': 'low',
      'label': 'Düşük',
      'description': 'Hafif yürüyüşler, az hareket',
    },
    {
      'value': 'moderate',
      'label': 'Orta',
      'description': 'Düzenli yürüyüş, bazen egzersiz',
    },
    {
      'value': 'active',
      'label': 'Aktif',
      'description': 'Haftada 3-4 gün spor',
    },
    {
      'value': 'very_active',
      'label': 'Çok Aktif',
      'description': 'Her gün yoğun aktivite',
    },
    {
      'value': 'skip',
      'label': 'Belirtmek istemiyorum',
      'description': null,
    },
  ];

  @override
  Widget build(BuildContext context) {
    return OnboardingScaffold(
      title: 'Günlük aktivite seviyen nasıl?',
      currentStep: 7,
      totalSteps: 9,
      isNextEnabled: _selectedLevel != null,
      onNext: _handleNext,
      child: Column(
        children: _options.map((option) {
          final isSelected = _selectedLevel == option['value'];
          final isSkipOption = option['value'] == 'skip';
          
          return Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: SurveyOptionTile(
              label: option['label']!,
              description: option['description'],
              isSelected: isSelected,
              isSkipOption: isSkipOption,
              onTap: () {
                setState(() => _selectedLevel = option['value']);
              },
            ),
          );
        }).toList(),
      ),
    );
  }

  void _handleNext() {
    final isSkipped = _selectedLevel == 'skip';
    widget.stateNotifier.updateActivityLevel(
      isSkipped ? null : _selectedLevel,
      isSkipped: isSkipped,
    );
    
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => ActiveHoursScreen(stateNotifier: widget.stateNotifier),
      ),
    );
  }
}
