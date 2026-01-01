import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';
import '../../../data/models/duel_card_model.dart';

/// Düello kartı
/// Aktif düellolardan ilkini gösterir
class DuelCard extends StatelessWidget {
  final DuelCardModel duel;
  final VoidCallback? onTap;
  
  const DuelCard({
    super.key,
    required this.duel,
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
                      color: AppColors.accentDuels.withValues(alpha: 0.1),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      Icons.sports_martial_arts_outlined,
                      color: AppColors.accentDuels,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Rakip: ${duel.opponentUsername}',
                          style: AppTypography.titleMedium.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        Text(
                          'Düello',
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
              
              // Dual progress bar
              Container(
                height: 6,
                decoration: BoxDecoration(
                  color: AppColors.divider,
                  borderRadius: BorderRadius.circular(3),
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(3),
                  child: LinearProgressIndicator(
                    value: duel.myProgressPercentage,
                    backgroundColor: Colors.transparent,
                    valueColor: const AlwaysStoppedAnimation<Color>(
                      AppColors.accentDuels,
                    ),
                    minHeight: 6,
                  ),
                ),
              ),
              
              const SizedBox(height: 4),
              
              // Opponent progress bar
              Container(
                height: 6,
                decoration: BoxDecoration(
                  color: AppColors.divider,
                  borderRadius: BorderRadius.circular(3),
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(3),
                  child: LinearProgressIndicator(
                    value: duel.opponentProgressPercentage,
                    backgroundColor: Colors.transparent,
                    valueColor: AlwaysStoppedAnimation<Color>(
                      Colors.grey,
                    ),
                    minHeight: 6,
                  ),
                ),
              ),
              
              const SizedBox(height: 12),
              
              // Durum
              Row(
                children: [
                  Icon(
                    duel.isWinning
                        ? Icons.trending_up_outlined
                        : duel.isTied
                            ? Icons.horizontal_rule_outlined
                            : duel.isLosing
                                ? Icons.trending_down_outlined
                                : Icons.help_outline,
                    color: duel.isWinning
                        ? AppColors.accentDuels
                        : duel.isTied
                            ? AppColors.textSecondary
                            : duel.isLosing
                                ? AppColors.error
                                : AppColors.textSecondary,
                    size: 16,
                  ),
                  const SizedBox(width: 4),
                  Expanded(
                    child: Text(
                      duel.isWinning
                          ? 'Kazanıyorum'
                          : duel.isTied
                              ? 'Berabere'
                              : duel.isLosing
                                  ? 'Kaybediyorum'
                                  : 'Bekleniyor',
                      style: AppTypography.labelSmall.copyWith(
                        color: duel.isWinning
                            ? AppColors.accentDuels
                            : duel.isTied
                                ? AppColors.textSecondary
                                : duel.isLosing
                                    ? AppColors.error
                                    : AppColors.textSecondary,
                        fontWeight: FontWeight.w600,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
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
                    color: AppColors.accentDuels,
                    size: 16,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    _formatDuration(duel.timeRemaining),
                    style: AppTypography.labelSmall.copyWith(
                      color: AppColors.accentDuels,
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

