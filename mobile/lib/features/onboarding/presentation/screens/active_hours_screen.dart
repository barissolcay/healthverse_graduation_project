/// En Aktif Saatler Ekranı - Soru 8/9 (Multi-select)
import 'package:flutter/material.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/referral_source_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/survey_option_tile.dart';

class ActiveHoursScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const ActiveHoursScreen({super.key, required this.stateNotifier});

  @override
  State<ActiveHoursScreen> createState() => _ActiveHoursScreenState();
}

class _ActiveHoursScreenState extends State<ActiveHoursScreen> {
  final Set<String> _selectedHours = {};

  final List<Map<String, String>> _options = [
    {
      'value': 'morning',
      'label': 'Sabah',
      'description': '06:00 - 12:00',
    },
    {
      'value': 'afternoon',
      'label': 'Öğle',
      'description': '12:00 - 17:00',
    },
    {
      'value': 'evening',
      'label': 'Akşam',
      'description': '17:00 - 21:00',
    },
    {
      'value': 'night',
      'label': 'Gece',
      'description': '21:00 - 00:00',
    },
    {
      'value': 'variable',
      'label': 'Değişken',
      'description': 'Sabit bir rutinim yok',
    },
  ];

  @override
  Widget build(BuildContext context) {
    return OnboardingScaffold(
      title: 'Günün hangi saatlerinde en aktifsin?',
      subtitle: 'Bu bilgiyi sana özel bildirimler göndermek için kullanacağız.',
      currentStep: 8,
      totalSteps: 9,
      isNextEnabled: true, // Boş geçilebilir
      onNext: _handleNext,
      child: Column(
        children: _options.map((option) {
          final isSelected = _selectedHours.contains(option['value']);
          final isVariable = option['value'] == 'variable';
          
          // "Değişken" seçiliyse diğerleri disabled
          final isDisabled = !isVariable && _selectedHours.contains('variable');
          
          return Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: SurveyCheckboxTile(
              label: option['label']!,
              description: option['description'],
              isSelected: isSelected,
              isDisabled: isDisabled,
              onTap: () {
                setState(() {
                  if (isVariable) {
                    // "Değişken" seçilince diğerlerini temizle
                    if (isSelected) {
                      _selectedHours.remove('variable');
                    } else {
                      _selectedHours.clear();
                      _selectedHours.add('variable');
                    }
                  } else {
                    // Normal seçenek - Değişken'i kaldır
                    _selectedHours.remove('variable');
                    if (isSelected) {
                      _selectedHours.remove(option['value']);
                    } else {
                      _selectedHours.add(option['value']!);
                    }
                  }
                });
              },
            ),
          );
        }).toList(),
      ),
    );
  }

  void _handleNext() {
    widget.stateNotifier.updateActiveHours(
      _selectedHours.toList(),
      isSkipped: _selectedHours.isEmpty,
    );
    
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => ReferralSourceScreen(stateNotifier: widget.stateNotifier),
      ),
    );
  }
}
