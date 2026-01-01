import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';

/// Streak kartı (Hero section)
/// Günlük adım ilerlemesi ve streak gösterir
class StreakCard extends StatelessWidget {
  final int streakCount;
  final int currentSteps;
  final int freezeInventory;
  final VoidCallback? onTap;
  
  const StreakCard({
    super.key,
    required this.streakCount,
    required this.currentSteps,
    required this.freezeInventory,
    this.onTap,
  });
  
  @override
  Widget build(BuildContext context) {
    final isCompleted = currentSteps >= 3000;
    final progress = (currentSteps / 3000).clamp(0.0, 1.0);
    
    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          gradient: const LinearGradient(
            colors: [AppColors.primary, AppColors.primaryLight],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(24),
          boxShadow: [
            BoxShadow(
              color: AppColors.primary.withValues(alpha: 0.3),
              blurRadius: 30,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Row(
            children: [
              // Sol: Alev ikonu ve streak sayısı
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Alev ikonu
                    Icon(
                      Icons.local_fire_department,
                      color: isCompleted
                          ? Colors.white
                          : AppColors.streakFireDimmed,
                      size: 40,
                    ),
                    const SizedBox(height: 8),
                    
                    // Streak sayısı
                    Text(
                      '$streakCount Gün',
                      style: AppTypography.displayLarge.copyWith(
                        color: Colors.white,
                        fontWeight: FontWeight.w800,
                      ),
                    ),
                  ],
                ),
              ),
              
              const SizedBox(width: 24),
              
              // Sağ: İlerleme
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      'Günlük Adım',
                      style: AppTypography.labelMedium.copyWith(
                        color: Colors.white.withValues(alpha: 0.9),
                      ),
                    ),
                    
                    const SizedBox(height: 4),
                    
                    // Progress text
                    Text(
                      '$currentSteps / 3000',
                      style: AppTypography.titleLarge.copyWith(
                        color: Colors.white,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    
                    const SizedBox(height: 8),
                    
                    // Progress bar
                    Container(
                      height: 8,
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.2),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(4),
                        child: LinearProgressIndicator(
                          value: progress,
                          backgroundColor: Colors.transparent,
                          valueColor: const AlwaysStoppedAnimation<Color>(
                            Colors.white,
                          ),
                          minHeight: 8,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

