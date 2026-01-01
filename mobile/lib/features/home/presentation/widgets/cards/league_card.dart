import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';
import '../../../data/models/league_card_model.dart';

/// Lig durumu kartı
/// Lig odasını ve sıralamayı gösterir
class LeagueCard extends StatelessWidget {
  final LeagueCardModel league;
  final VoidCallback? onTap;
  
  const LeagueCard({
    super.key,
    required this.league,
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
                      color: AppColors.accentLeague.withValues(alpha: 0.1),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      Icons.emoji_events_outlined,
                      color: AppColors.accentLeague,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          league.tierDisplayName,
                          style: AppTypography.titleMedium.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        Text(
                          'Lig Durumu',
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
                height: 6,
                decoration: BoxDecoration(
                  color: AppColors.divider,
                  borderRadius: BorderRadius.circular(3),
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(3),
                  child: LinearProgressIndicator(
                    value: league.rankPercentage,
                    backgroundColor: Colors.transparent,
                    valueColor: const AlwaysStoppedAnimation<Color>(
                      AppColors.accentLeague,
                    ),
                    minHeight: 6,
                  ),
                ),
              ),
              
              const SizedBox(height: 12),
              
              // Sıralama
              Row(
                children: [
                  Text(
                    '${league.myRank} / ${league.totalMembers}',
                    style: AppTypography.labelSmall.copyWith(
                      color: AppColors.accentLeague,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const Spacer(),
                  Icon(
                    league.canPromote
                        ? Icons.arrow_upward_outlined
                        : league.canDemote
                            ? Icons.arrow_downward_outlined
                            : Icons.horizontal_rule_outlined,
                    color: league.canPromote
                        ? AppColors.accentLeague
                        : league.canDemote
                            ? AppColors.error
                            : AppColors.textSecondary,
                    size: 16,
                  ),
                ],
              ),
              
              const SizedBox(height: 8),
              
              // Kalan süre
              Row(
                children: [
                  Icon(
                    Icons.access_time_outlined,
                    color: AppColors.accentLeague,
                    size: 16,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    _formatDuration(league.timeRemaining),
                    style: AppTypography.labelSmall.copyWith(
                      color: AppColors.accentLeague,
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

