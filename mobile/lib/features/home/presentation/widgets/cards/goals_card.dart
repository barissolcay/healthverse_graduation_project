import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';
import '../../../data/models/goal_card_model.dart';

/// Hedefler kartı
/// Aktif hedeflerden ilkini gösterir
class GoalsCard extends StatelessWidget {
  final List<GoalCardModel> goals;
  final VoidCallback? onTap;
  
  const GoalsCard({
    super.key,
    required this.goals,
    this.onTap,
  });
  
  @override
  Widget build(BuildContext context) {
    if (goals.isEmpty) {
      return const SizedBox.shrink();
    }
    
    final goal = goals.first; // İlk hedefi göster
    
    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(24),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.07),
              blurRadius: 15,
              offset: const Offset(0, -3),
            ),
          ],
        ),
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // İkon ve başlık
              Row(
                children: [
                  Container(
                    width: 40,
                    height: 40,
                    decoration: BoxDecoration(
                      color: AppColors.accentGoals.withValues(alpha: 0.1),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      Icons.flag_outlined,
                      color: AppColors.accentGoals,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          goal.title,
                          style: AppTypography.titleMedium.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        Text(
                          'Hedefler',
                          style: AppTypography.labelSmall.copyWith(
                            color: AppColors.textTertiary,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              
              const SizedBox(height: 16),
              
              // Progress bar
              Container(
                height: 8,
                decoration: BoxDecoration(
                  color: AppColors.divider,
                  borderRadius: BorderRadius.circular(4),
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(4),
                  child: LinearProgressIndicator(
                    value: goal.progressPercentage,
                    backgroundColor: Colors.transparent,
                    valueColor: const AlwaysStoppedAnimation<Color>(
                      AppColors.accentGoals,
                    ),
                    minHeight: 8,
                  ),
                ),
              ),
              
              const SizedBox(height: 12),
              
              // Kalan süre
              Row(
                children: [
                  Icon(
                    Icons.hourglass_top_outlined,
                    color: goal.isExpired ? AppColors.error : AppColors.accentGoals,
                    size: 16,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    _formatDuration(goal.timeRemaining),
                    style: AppTypography.labelSmall.copyWith(
                      color: goal.isExpired ? AppColors.error : AppColors.accentGoals,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  if (goal.isExpired)
                    const SizedBox(width: 8),
                  if (goal.isExpired)
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: AppColors.error.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Text(
                        'Süresi doldu',
                        style: AppTypography.labelTiny.copyWith(
                          color: AppColors.error,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
  
  String _formatDuration(Duration duration) {
    if (duration.isNegative) {
      return 'Süresi doldu';
    }
    
    if (duration.inHours > 24) {
      return '${duration.inDays} gün';
    } else if (duration.inHours > 0) {
      return '${duration.inHours} gün';
    } else if (duration.inMinutes > 0) {
      return '${duration.inMinutes} saat';
    } else {
      return 'Sonlanıyor';
    }
  }
}

