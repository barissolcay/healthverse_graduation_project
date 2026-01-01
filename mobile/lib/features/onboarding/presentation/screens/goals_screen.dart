/// Hedefler Ekranı - Soru 6/9 (Multi-select, max 2)
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/activity_level_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/survey_option_tile.dart';

class GoalsScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const GoalsScreen({super.key, required this.stateNotifier});

  @override
  State<GoalsScreen> createState() => _GoalsScreenState();
}

class _GoalsScreenState extends State<GoalsScreen> {
  final Set<String> _selectedGoals = {};
  static const int _maxSelection = 2;

  final List<Map<String, String>> _options = [
    {'value': 'active', 'label': 'Daha aktif olmak'},
    {'value': 'lose_weight', 'label': 'Kilo vermek'},
    {'value': 'build_muscle', 'label': 'Kas kazanmak'},
    {'value': 'maintain', 'label': 'Formumu korumak'},
    {'value': 'compete', 'label': 'Rekabet & Motivasyon'},
    {'value': 'unknown', 'label': 'Henüz bilmiyorum'},
  ];

  @override
  Widget build(BuildContext context) {
    return OnboardingScaffold(
      title: 'Asıl hedefin ne?',
      subtitle: 'En fazla $_maxSelection seçenek işaretleyebilirsin.',
      currentStep: 6,
      totalSteps: 9,
      isNextEnabled: true, // Boş geçilebilir
      onNext: _handleNext,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Seçim sayısı göstergesi
          if (_selectedGoals.isNotEmpty)
            Padding(
              padding: const EdgeInsets.only(bottom: 16),
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: AppColors.primary.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  '${_selectedGoals.length} / $_maxSelection seçildi',
                  style: TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                    color: AppColors.primary,
                  ),
                ),
              ),
            ),
          
          // Seçenekler
          ..._options.map((option) {
            final isSelected = _selectedGoals.contains(option['value']);
            final isDisabled = !isSelected && _selectedGoals.length >= _maxSelection;
            
            return Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: SurveyCheckboxTile(
                label: option['label']!,
                isSelected: isSelected,
                isDisabled: isDisabled,
                onTap: () {
                  setState(() {
                    if (isSelected) {
                      _selectedGoals.remove(option['value']);
                    } else if (_selectedGoals.length < _maxSelection) {
                      _selectedGoals.add(option['value']!);
                    }
                  });
                },
              ),
            );
          }),
        ],
      ),
    );
  }

  void _handleNext() {
    widget.stateNotifier.updateGoals(
      _selectedGoals.toList(),
      isSkipped: _selectedGoals.isEmpty,
    );
    
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => ActivityLevelScreen(stateNotifier: widget.stateNotifier),
      ),
    );
  }
}
