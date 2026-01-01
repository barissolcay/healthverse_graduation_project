/// Bizi Nereden Duydun Ekranı - Soru 9/9
import 'package:flutter/material.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/onboarding_completion_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/survey_option_tile.dart';

class ReferralSourceScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const ReferralSourceScreen({super.key, required this.stateNotifier});

  @override
  State<ReferralSourceScreen> createState() => _ReferralSourceScreenState();
}

class _ReferralSourceScreenState extends State<ReferralSourceScreen> {
  String? _selectedSource;

  final List<Map<String, String>> _options = [
    {'value': 'friend', 'label': 'Arkadaş/Aile önerisi'},
    {'value': 'social_media', 'label': 'Sosyal medya (Instagram, TikTok vb.)'},
    {'value': 'app_store', 'label': 'App Store / Play Store keşif'},
    {'value': 'search', 'label': 'İnternet araması'},
    {'value': 'blog', 'label': 'Blog/Haber sitesi'},
    {'value': 'other', 'label': 'Diğer'},
    {'value': 'skip', 'label': 'Belirtmek istemiyorum'},
  ];

  @override
  Widget build(BuildContext context) {
    return OnboardingScaffold(
      title: 'HealthVerse\'ü nereden duydun?',
      subtitle: 'Son soru! Uygulamamızı tanıtmamıza yardımcı ol.',
      currentStep: 9,
      totalSteps: 9,
      nextButtonText: 'Tamamla',
      isNextEnabled: _selectedSource != null,
      onNext: _handleNext,
      child: Column(
        children: _options.map((option) {
          final isSelected = _selectedSource == option['value'];
          final isSkipOption = option['value'] == 'skip';
          
          return Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: SurveyOptionTile(
              label: option['label']!,
              isSelected: isSelected,
              isSkipOption: isSkipOption,
              onTap: () {
                setState(() => _selectedSource = option['value']);
              },
            ),
          );
        }).toList(),
      ),
    );
  }

  void _handleNext() {
    final isSkipped = _selectedSource == 'skip';
    widget.stateNotifier.updateReferralSource(
      isSkipped ? null : _selectedSource,
      isSkipped: isSkipped,
    );
    
    // Anketi tamamla
    widget.stateNotifier.completeOnboarding();
    
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => OnboardingCompletionScreen(
          stateNotifier: widget.stateNotifier,
        ),
      ),
    );
  }
}
