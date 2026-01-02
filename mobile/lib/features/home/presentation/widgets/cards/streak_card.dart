import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';

/// Streak Card - Hero section kartı
/// Border gradient ile: seri tamamlandıysa turuncu border, tamamlanmadıysa gri
class StreakCard extends StatelessWidget {
  final int streakCount;
  final int currentSteps;
  final int freezeCount;
  final VoidCallback? onTap;

  const StreakCard({
    super.key,
    required this.streakCount,
    required this.currentSteps,
    required this.freezeCount,
    this.onTap,
  });

  /// 3000+ adım = seri tamamlandı
  bool get isCompleted => currentSteps >= 3000;

  /// İlerleme yüzdesi (0-1)
  double get progressPercentage => (currentSteps / 3000).clamp(0.0, 1.0);

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(24),
          // Gradient border
          gradient: isCompleted
              ? const LinearGradient(
                  colors: [
                    AppColors.streakFireStart,
                    AppColors.streakFireEnd,
                  ],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                )
              : null,
          color: isCompleted ? null : AppColors.divider,
        ),
        padding: const EdgeInsets.all(3), // Border kalınlığı
        child: Container(
          width: double.infinity,
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: AppColors.surface,
            borderRadius: BorderRadius.circular(21),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Üst kısım: Ateş ikonu + Gün sayısı
              Row(
                children: [
                  // Alev ikonu
                  _buildFireIcon(),
                  const SizedBox(width: 16),
                  
                  // Seri bilgisi - sayı büyük, label küçük
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.baseline,
                    textBaseline: TextBaseline.alphabetic,
                    children: [
                      Text(
                        '$streakCount',
                        style: AppTypography.displayLarge.copyWith(
                          color: isCompleted
                              ? AppColors.streakFireStart
                              : AppColors.textPrimary,
                          fontWeight: FontWeight.bold,
                          fontSize: 48,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Text(
                        'Günlük Seri',
                        style: AppTypography.bodyMedium.copyWith(
                          color: AppColors.textSecondary,
                        ),
                      ),
                    ],
                  ),
                  
                  const Spacer(),
                  
                  // Freeze badge
                  if (freezeCount > 0)
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 10,
                        vertical: 6,
                      ),
                      decoration: BoxDecoration(
                        color: Colors.blue.shade50,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            Icons.ac_unit,
                            size: 16,
                            color: Colors.blue.shade600,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            '$freezeCount',
                            style: AppTypography.labelMedium.copyWith(
                              color: Colors.blue.shade600,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                    ),
                ],
              ),
              
              const SizedBox(height: 20),
              
              // Progress bar with labels
              Row(
                children: [
                  Text(
                    '$currentSteps',
                    style: AppTypography.titleMedium.copyWith(
                      color: AppColors.textPrimary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    ' / 3.000 adım',
                    style: AppTypography.bodyMedium.copyWith(
                      color: AppColors.textTertiary,
                    ),
                  ),
                ],
              ),
              
              const SizedBox(height: 8),
              
              Container(
                height: 8,
                width: double.infinity,
                decoration: BoxDecoration(
                  color: AppColors.divider,
                  borderRadius: BorderRadius.circular(4),
                ),
                child: FractionallySizedBox(
                  alignment: Alignment.centerLeft,
                  widthFactor: progressPercentage,
                  child: Container(
                    decoration: BoxDecoration(
                      gradient: isCompleted
                          ? const LinearGradient(
                              colors: [
                                AppColors.streakFireStart,
                                AppColors.streakFireEnd,
                              ],
                            )
                          : null,
                      color: isCompleted ? null : AppColors.primary,
                      borderRadius: BorderRadius.circular(4),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildFireIcon() {
    return Container(
      width: 56,
      height: 56,
      decoration: BoxDecoration(
        color: isCompleted
            ? AppColors.streakFireStart.withAlpha(25)
            : AppColors.textHint.withAlpha(25),
        shape: BoxShape.circle,
      ),
      child: Center(
        child: isCompleted
            ? ShaderMask(
                shaderCallback: (bounds) => const LinearGradient(
                  colors: [
                    AppColors.streakFireStart,
                    AppColors.streakFireEnd,
                  ],
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                ).createShader(bounds),
                child: const Icon(
                  Icons.local_fire_department,
                  color: Colors.white,
                  size: 32,
                ),
              )
            : const Icon(
                Icons.local_fire_department_outlined,
                color: AppColors.textHint,
                size: 32,
              ),
      ),
    );
  }
}
