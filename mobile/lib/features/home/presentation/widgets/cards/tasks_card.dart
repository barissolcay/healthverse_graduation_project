import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';
import '../../../data/models/task_card_model.dart';

/// Görevler kartı
/// Aktif görevlerden ilkini gösterir
class TasksCard extends StatelessWidget {
  final List<TaskCardModel> tasks;
  final VoidCallback? onTap;
  
  const TasksCard({
    super.key,
    required this.tasks,
    this.onTap,
  });
  
  @override
  Widget build(BuildContext context) {
    if (tasks.isEmpty) {
      return const SizedBox.shrink();
    }
    
    final task = tasks.first; // İlk görevi göster
    
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
                      color: AppColors.accentTasks.withValues(alpha: 0.1),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      Icons.check_circle_outline,
                      color: AppColors.accentTasks,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          task.title,
                          style: AppTypography.titleMedium.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        Text(
                          'Görevler',
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
                    value: task.progressPercentage,
                    backgroundColor: Colors.transparent,
                    valueColor: const AlwaysStoppedAnimation<Color>(
                      AppColors.accentTasks,
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
                    Icons.schedule_outlined,
                    color: task.isExpired ? AppColors.error : AppColors.accentTasks,
                    size: 16,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    _formatDuration(task.timeRemaining),
                    style: AppTypography.labelSmall.copyWith(
                      color: task.isExpired ? AppColors.error : AppColors.accentTasks,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  if (task.isExpired)
                    const SizedBox(width: 8),
                  if (task.isExpired)
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
      return '${duration.inHours} saat';
    } else if (duration.inMinutes > 0) {
      return '${duration.inMinutes} dk';
    } else {
      return 'Sonlanıyor';
    }
  }
}

