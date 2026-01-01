/// Cinsiyet Ekranı - Soru 2/9
import 'package:flutter/material.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/city_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/survey_option_tile.dart';

class GenderScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const GenderScreen({super.key, required this.stateNotifier});

  @override
  State<GenderScreen> createState() => _GenderScreenState();
}

class _GenderScreenState extends State<GenderScreen> {
  String? _selectedGender;

  final List<Map<String, String>> _options = [
    {'value': 'male', 'label': 'Erkek'},
    {'value': 'female', 'label': 'Kadın'},
    {'value': 'other', 'label': 'Diğer'},
    {'value': 'skip', 'label': 'Belirtmek istemiyorum'},
  ];

  @override
  Widget build(BuildContext context) {
    return OnboardingScaffold(
      title: 'Cinsiyetin nedir?',
      currentStep: 2,
      totalSteps: 9,
      isNextEnabled: _selectedGender != null,
      onNext: _handleNext,
      child: Column(
        children: _options.map((option) {
          final isSelected = _selectedGender == option['value'];
          final isSkipOption = option['value'] == 'skip';
          
          return Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: SurveyOptionTile(
              label: option['label']!,
              isSelected: isSelected,
              isSkipOption: isSkipOption,
              onTap: () {
                setState(() => _selectedGender = option['value']);
              },
            ),
          );
        }).toList(),
      ),
    );
  }

  void _handleNext() {
    final isSkipped = _selectedGender == 'skip';
    widget.stateNotifier.updateGender(
      isSkipped ? null : _selectedGender,
      isSkipped: isSkipped,
    );
    
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => CityScreen(stateNotifier: widget.stateNotifier),
      ),
    );
  }
}
