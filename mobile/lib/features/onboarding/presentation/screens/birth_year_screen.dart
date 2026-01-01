/// Doğum Yılı Ekranı - Soru 1/9
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/gender_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';

class BirthYearScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const BirthYearScreen({super.key, required this.stateNotifier});

  @override
  State<BirthYearScreen> createState() => _BirthYearScreenState();
}

class _BirthYearScreenState extends State<BirthYearScreen> {
  int? _selectedYear;
  bool _skipSelected = false;
  
  final int _minYear = 1940;
  final int _maxYear = 2015;

  @override
  Widget build(BuildContext context) {
    return OnboardingScaffold(
      title: 'Hangi yılda doğdun?',
      subtitle: 'Yaşına uygun öneriler sunmak için kullanacağız.',
      currentStep: 1,
      totalSteps: 9,
      isNextEnabled: _selectedYear != null || _skipSelected,
      onNext: _handleNext,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Yıl seçici
          Container(
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppColors.divider),
            ),
            child: ListTile(
              title: Text(
                _selectedYear?.toString() ?? 'Yıl seç',
                style: TextStyle(
                  fontSize: 16,
                  color: _selectedYear != null 
                      ? AppColors.textPrimary 
                      : AppColors.textHint,
                ),
              ),
              trailing: const Icon(Icons.keyboard_arrow_down),
              onTap: _showYearPicker,
            ),
          ),
          const SizedBox(height: 24),

          // Belirtmek istemiyorum seçeneği
          GestureDetector(
            onTap: () {
              setState(() {
                _skipSelected = !_skipSelected;
                if (_skipSelected) _selectedYear = null;
              });
            },
            child: Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: _skipSelected 
                    ? AppColors.divider.withOpacity(0.3) 
                    : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: _skipSelected ? AppColors.textTertiary : AppColors.divider,
                ),
              ),
              child: Row(
                children: [
                  Icon(
                    _skipSelected ? Icons.check_box : Icons.check_box_outline_blank,
                    color: _skipSelected ? AppColors.textTertiary : AppColors.divider,
                  ),
                  const SizedBox(width: 12),
                  Text(
                    'Belirtmek istemiyorum',
                    style: TextStyle(
                      fontSize: 16,
                      color: AppColors.textTertiary,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  void _showYearPicker() {
    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) {
        return SizedBox(
          height: 300,
          child: Column(
            children: [
              // Handle bar
              Container(
                margin: const EdgeInsets.only(top: 12),
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: AppColors.divider,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              // Başlık
              Padding(
                padding: const EdgeInsets.all(16),
                child: Text(
                  'Doğum yılını seç',
                  style: TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.w600,
                    color: AppColors.textPrimary,
                  ),
                ),
              ),
              // Yıl listesi
              Expanded(
                child: ListView.builder(
                  itemCount: _maxYear - _minYear + 1,
                  itemBuilder: (context, index) {
                    final year = _maxYear - index;
                    final isSelected = year == _selectedYear;
                    return ListTile(
                      title: Text(
                        year.toString(),
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: isSelected ? FontWeight.w600 : FontWeight.w400,
                          color: isSelected ? AppColors.primary : AppColors.textPrimary,
                        ),
                        textAlign: TextAlign.center,
                      ),
                      onTap: () {
                        setState(() {
                          _selectedYear = year;
                          _skipSelected = false;
                        });
                        Navigator.pop(context);
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  void _handleNext() {
    widget.stateNotifier.updateBirthYear(
      _selectedYear,
      isSkipped: _skipSelected,
    );
    
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => GenderScreen(stateNotifier: widget.stateNotifier),
      ),
    );
  }
}
