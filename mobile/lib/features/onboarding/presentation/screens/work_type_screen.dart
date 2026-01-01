/// İş Türü Ekranı - Soru 4B (Koşullu - sadece çalışanlar)
import 'package:flutter/material.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/body_metrics_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/survey_option_tile.dart';

class WorkTypeScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const WorkTypeScreen({super.key, required this.stateNotifier});

  @override
  State<WorkTypeScreen> createState() => _WorkTypeScreenState();
}

class _WorkTypeScreenState extends State<WorkTypeScreen> {
  String? _selectedType;

  final List<Map<String, String?>> _options = [
    {
      'value': 'desk',
      'label': 'Masa başı / Ofis işi',
      'description': 'Çoğunlukla oturarak çalışıyorum',
    },
    {
      'value': 'physical',
      'label': 'Fiziksel iş',
      'description': 'Ayakta, hareket gerektiren bir iş',
    },
    {
      'value': 'mixed',
      'label': 'Yarı yarıya',
      'description': 'Hem oturuyorum hem hareket ediyorum',
    },
    {
      'value': 'remote',
      'label': 'Uzaktan / Evden çalışma',
      'description': 'Ev ortamında çalışıyorum',
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
      title: 'Ne tür bir işte çalışıyorsun?',
      subtitle: 'Aktivite seviyeni daha iyi anlamamıza yardımcı olur.',
      currentStep: 4, // Aynı adım numarası (koşullu soru)
      totalSteps: 9,
      isNextEnabled: _selectedType != null,
      onNext: _handleNext,
      child: Column(
        children: _options.map((option) {
          final isSelected = _selectedType == option['value'];
          final isSkipOption = option['value'] == 'skip';
          
          return Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: SurveyOptionTile(
              label: option['label']!,
              description: option['description'],
              isSelected: isSelected,
              isSkipOption: isSkipOption,
              onTap: () {
                setState(() => _selectedType = option['value']);
              },
            ),
          );
        }).toList(),
      ),
    );
  }

  void _handleNext() {
    final isSkipped = _selectedType == 'skip';
    widget.stateNotifier.updateWorkType(
      isSkipped ? null : _selectedType,
      isSkipped: isSkipped,
    );
    
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => BodyMetricsScreen(stateNotifier: widget.stateNotifier),
      ),
    );
  }
}
