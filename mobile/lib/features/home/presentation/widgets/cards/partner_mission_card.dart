import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';
import '../../../data/models/partner_mission_model.dart';

/// Partner görevi kartı
/// Aktif partner görevini gösterir
class PartnerMissionCard extends StatelessWidget {
  final PartnerMissionModel mission;
  final VoidCallback? onTap;
  
  const PartnerMissionCard({
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
                      color: AppColors.accentMissions.withValues(alpha: 0.1),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      Icons.handshake_outlined,
                      color: AppColors.accentMissions,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Partner: ${mission.partnerUsername}',
                          style: AppTypography.titleMedium.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        Text(
                          'Ortak Görev',
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
                      AppColors.accentMissions,
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
                    'İlerleme',
                    style: AppTypography.labelSmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                  const Spacer(),
                  Text(
                    '%${(mission.progressPercentage * 100).toInt()}',
                    style: AppTypography.labelSmall.copyWith(
                      color: AppColors.accentMissions,
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
                    color: mission.isExpired ? AppColors.error : AppColors.accentMissions,
                    size: 16,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    _formatDuration(mission.timeRemaining),
                    style: AppTypography.labelSmall.copyWith(
                      color: mission.isExpired ? AppColors.error : AppColors.accentMissions,
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

