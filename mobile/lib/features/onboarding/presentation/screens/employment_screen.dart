/// Çalışma Durumu Ekranı - Soru 4/9
import 'package:flutter/material.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/work_type_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/body_metrics_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/survey_option_tile.dart';

class EmploymentScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const EmploymentScreen({super.key, required this.stateNotifier});

  @override
  State<EmploymentScreen> createState() => _EmploymentScreenState();
}

class _EmploymentScreenState extends State<EmploymentScreen> {
  String? _selectedStatus;

  final List<Map<String, String>> _options = [
    {'value': 'employed', 'label': 'Evet, çalışıyorum'},
    {'value': 'unemployed', 'label': 'Hayır, çalışmıyorum'},
    {'value': 'student', 'label': 'Öğrenciyim'},
    {'value': 'skip', 'label': 'Belirtmek istemiyorum'},
  ];

  @override
  Widget build(BuildContext context) {
    return OnboardingScaffold(
      title: 'Şu an bir işte çalışıyor musun?',
      currentStep: 4,
      totalSteps: 9,
      isNextEnabled: _selectedStatus != null,
      onNext: _handleNext,
      child: Column(
        children: _options.map((option) {
          final isSelected = _selectedStatus == option['value'];
          final isSkipOption = option['value'] == 'skip';
          
          return Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: SurveyOptionTile(
              label: option['label']!,
              isSelected: isSelected,
              isSkipOption: isSkipOption,
              onTap: () {
                setState(() => _selectedStatus = option['value']);
              },
            ),
          );
        }).toList(),
      ),
    );
  }

  void _handleNext() {
    final isSkipped = _selectedStatus == 'skip';
    widget.stateNotifier.updateEmploymentStatus(
      isSkipped ? null : _selectedStatus,
      isSkipped: isSkipped,
    );
    
    // Eğer çalışıyorsa → İş türü ekranına git
    // Değilse → Boy/Kilo ekranına atla
    if (_selectedStatus == 'employed') {
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => WorkTypeScreen(stateNotifier: widget.stateNotifier),
        ),
      );
    } else {
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => BodyMetricsScreen(stateNotifier: widget.stateNotifier),
        ),
      );
    }
  }
}
