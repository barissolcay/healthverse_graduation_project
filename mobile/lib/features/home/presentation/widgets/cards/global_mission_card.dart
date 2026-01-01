import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';
import '../../../data/models/global_mission_model.dart';

/// Dünya görevi kartı
/// Aktif global görevlerden birini gösterir
class GlobalMissionCard extends StatelessWidget {
  final GlobalMissionModel mission;
  final VoidCallback? onTap;
  
  const GlobalMissionCard({
    super.key,
    required this.mission,
    this.onTap,
  });
  
  @override
  Widget build(BuildContext context) {
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
                      color: AppColors.accentProfile.withValues(alpha: 0.1),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      Icons.public_outlined,
                      color: AppColors.accentProfile,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          mission.title,
                          style: AppTypography.titleMedium.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        Text(
                          'Dünya Görevi',
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
                    value: mission.progressPercentage,
                    backgroundColor: Colors.transparent,
                    valueColor: const AlwaysStoppedAnimation<Color>(
                      AppColors.accentProfile,
                    ),
                    minHeight: 8,
                  ),
                ),
              ),
              
              const SizedBox(height: 12),
              
              // İlerleme text
              Row(
                children: [
                  Text(
                    'Topluluk',
                    style: AppTypography.labelSmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                  const Spacer(),
                  Text(
                    '%${(mission.progressPercentage * 100).toInt()}',
                    style: AppTypography.labelSmall.copyWith(
                      color: AppColors.accentProfile,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ],
              ),
              
              const SizedBox(height: 8),
              
              // Kalan süre
              Row(
                children: [
                  Icon(
                    Icons.access_time_outlined,
                    color: mission.isExpired ? AppColors.error : AppColors.accentProfile,
                    size: 16,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    _formatDuration(mission.timeRemaining),
                    style: AppTypography.labelSmall.copyWith(
                      color: mission.isExpired ? AppColors.error : AppColors.accentProfile,
                      fontWeight: FontWeight.w600,
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

