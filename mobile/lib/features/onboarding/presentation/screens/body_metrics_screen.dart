/// Boy & Kilo Ekranı - Soru 5/9 (BMI hesaplama ile)
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/goals_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';

class BodyMetricsScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const BodyMetricsScreen({super.key, required this.stateNotifier});

  @override
  State<BodyMetricsScreen> createState() => _BodyMetricsScreenState();
}

class _BodyMetricsScreenState extends State<BodyMetricsScreen> {
  double _height = 170;
  double _weight = 70;
  bool _skipSelected = false;

  // BMI hesaplama
  double get _bmi {
    final heightInMeters = _height / 100;
    return _weight / (heightInMeters * heightInMeters);
  }

  String get _bmiCategory {
    if (_bmi < 18.5) return 'Zayıf';
    if (_bmi < 25) return 'Normal';
    if (_bmi < 30) return 'Fazla Kilolu';
    if (_bmi < 35) return '1. Derece Obez';
    if (_bmi < 40) return '2. Derece Obez';
    return 'Morbid Obez';
  }

  Color get _bmiColor {
    if (_bmi < 18.5) return Colors.blue;
    if (_bmi < 25) return AppColors.primary;
    if (_bmi < 30) return Colors.amber;
    if (_bmi < 35) return Colors.orange;
    if (_bmi < 40) return Colors.deepOrange;
    return Colors.red.shade900;
  }

  String get _bmiEmoji {
    if (_bmi < 18.5) return '🔵';
    if (_bmi < 25) return '🟢';
    if (_bmi < 30) return '🟡';
    if (_bmi < 35) return '🟠';
    if (_bmi < 40) return '🔴';
    return '⚫';
  }

  @override
  Widget build(BuildContext context) {
    return OnboardingScaffold(
      title: 'Boyun ve kilonu girebilir misin?',
      subtitle: 'Bu bilgiler sağlık hedeflerini belirlemene yardımcı olur.',
      currentStep: 5,
      totalSteps: 9,
      isNextEnabled: true,
      onNext: _handleNext,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Boy slider
          _buildMetricCard(
            title: 'Boy',
            value: '${_height.toInt()} cm',
            emoji: '📏',
            child: Column(
              children: [
                Slider(
                  value: _height,
                  min: 100,
                  max: 220,
                  divisions: 120,
                  activeColor: AppColors.primary,
                  inactiveColor: AppColors.divider,
                  onChanged: _skipSelected ? null : (value) {
                    setState(() => _height = value);
                  },
                ),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text('100 cm', style: TextStyle(color: AppColors.textTertiary, fontSize: 12)),
                    Text('220 cm', style: TextStyle(color: AppColors.textTertiary, fontSize: 12)),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // Kilo slider
          _buildMetricCard(
            title: 'Kilo',
            value: '${_weight.toInt()} kg',
            emoji: '⚖️',
            child: Column(
              children: [
                Slider(
                  value: _weight,
                  min: 25,
                  max: 200,
                  divisions: 175,
                  activeColor: AppColors.primary,
                  inactiveColor: AppColors.divider,
                  onChanged: _skipSelected ? null : (value) {
                    setState(() => _weight = value);
                  },
                ),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text('25 kg', style: TextStyle(color: AppColors.textTertiary, fontSize: 12)),
                    Text('200 kg', style: TextStyle(color: AppColors.textTertiary, fontSize: 12)),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),

          // BMI gösterimi
          if (!_skipSelected)
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: _bmiColor.withOpacity(0.1),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: _bmiColor.withOpacity(0.3)),
              ),
              child: Row(
                children: [
                  Text(_bmiEmoji, style: const TextStyle(fontSize: 32)),
                  const SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Vücut Kitle İndeksi (BMI)',
                          style: TextStyle(
                            fontSize: 12,
                            color: AppColors.textSecondary,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Row(
                          children: [
                            Text(
                              _bmi.toStringAsFixed(1),
                              style: TextStyle(
                                fontSize: 24,
                                fontWeight: FontWeight.w700,
                                color: _bmiColor,
                              ),
                            ),
                            const SizedBox(width: 8),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                              decoration: BoxDecoration(
                                color: _bmiColor.withOpacity(0.2),
                                borderRadius: BorderRadius.circular(6),
                              ),
                              child: Text(
                                _bmiCategory,
                                style: TextStyle(
                                  fontSize: 12,
                                  fontWeight: FontWeight.w600,
                                  color: _bmiColor,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          const SizedBox(height: 16),

          // Belirtmek istemiyorum
          GestureDetector(
            onTap: () {
              setState(() => _skipSelected = !_skipSelected);
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

  Widget _buildMetricCard({
    required String title,
    required String value,
    required String emoji,
    required Widget child,
  }) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.divider),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(emoji, style: const TextStyle(fontSize: 20)),
              const SizedBox(width: 8),
              Text(
                title,
                style: TextStyle(
                  fontSize: 14,
                  color: AppColors.textSecondary,
                ),
              ),
              const Spacer(),
              Text(
                value,
                style: TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.w700,
                  color: _skipSelected ? AppColors.textTertiary : AppColors.primary,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          child,
        ],
      ),
    );
  }

  void _handleNext() {
    if (_skipSelected) {
      widget.stateNotifier.updateBodyMetrics(null, null, isSkipped: true);
    } else {
      widget.stateNotifier.updateBodyMetrics(_height.toInt(), _weight.toInt());
    }
    
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => GoalsScreen(stateNotifier: widget.stateNotifier),
      ),
    );
  }
}
