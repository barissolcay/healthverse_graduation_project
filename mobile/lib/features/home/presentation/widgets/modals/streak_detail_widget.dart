import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';

/// Seri detay modal
/// Bottom sheet olarak açılan streak detay modalı
class StreakDetailWidget extends StatelessWidget {
  final int streakCount;
  final int longestStreakCount;
  final int currentSteps;
  final int freezeInventory;
  final VoidCallback onClose;
  
  const StreakDetailWidget({
    super.key,
    required this.streakCount,
    required this.longestStreakCount,
    required this.currentSteps,
    required this.freezeInventory,
    required this.onClose,
  });
  
  @override
  Widget build(BuildContext context) {
    final progress = (currentSteps / 3000).clamp(0.0, 1.0);
    final maxPossiblePoints = currentSteps > 3000 ? (currentSteps - 3000) ~/ 1000 : 0;
    
    return Container(
      padding: EdgeInsets.only(
        bottom: MediaQuery.of(context).viewInsets.bottom,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          // Üst bar: Başlık ve kapat butonu
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
            child: Row(
              children: [
                const SizedBox(width: 48),
                const Spacer(),
                Text(
                  'Seri Detayı',
                  style: AppTypography.headlineMedium.copyWith(
                    color: AppColors.textPrimary,
                  ),
                ),
                const Spacer(),
                SizedBox(
                  width: 48,
                  height: 48,
                  child: IconButton(
                    onPressed: onClose,
                    icon: const Icon(Icons.close, size: 24),
                  ),
                ),
              ],
            ),
          ),
          
          const Divider(height: 1),
          
          // İçerik
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
            child: Column(
              children: [
                // Alev ikonu ve seri sayısı
                Icon(
                  Icons.local_fire_department,
                  color: AppColors.streakFireStart,
                  size: 64,
                ),
                const SizedBox(height: 16),
                Text(
                  'Seri: $streakCount gün',
                  style: AppTypography.displayLarge.copyWith(
                    color: AppColors.textPrimary,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  'En uzun seri: $longestStreakCount gün',
                  style: AppTypography.bodyMedium.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
                
                const SizedBox(height: 32),
                
                // Günlük ilerleme
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Bugünkü ilerleme',
                      style: AppTypography.labelMedium.copyWith(
                        color: AppColors.textTertiary,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const SizedBox(height: 12),
                    
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
                          value: progress,
                          backgroundColor: Colors.transparent,
                          valueColor: const AlwaysStoppedAnimation<Color>(
                            AppColors.streakFireStart,
                          ),
                          minHeight: 8,
                        ),
                      ),
                    ),
                    
                    const SizedBox(height: 12),
                    
                    // Progress text
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          '$currentSteps / 3000 adım',
                          style: AppTypography.bodyMedium.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        Text(
                          'Hedef: 3000 adım',
                          style: AppTypography.labelSmall.copyWith(
                            color: AppColors.textTertiary,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
                
                const SizedBox(height: 24),
                
                // Dondurma hakkı
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: AppColors.background,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(
                      color: AppColors.divider,
                      width: 1,
                    ),
                  ),
                  child: Row(
                    children: [
                      Icon(
                        Icons.ac_unit_outlined,
                        color: AppColors.textSecondary,
                        size: 24,
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Dondurma hakkın',
                              style: AppTypography.labelMedium.copyWith(
                                color: AppColors.textTertiary,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              'Kalan dondurma hakkın: $freezeInventory',
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.textPrimary,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
                
                const SizedBox(height: 24),
                
                // Puan kazanma
                if (maxPossiblePoints > 0)
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Icon(
                        Icons.stars_outlined,
                        color: AppColors.accentLeague,
                        size: 24,
                      ),
                      const SizedBox(height: 12),
                      Text(
                        '$maxPossiblePoints puan kazanılabilecek',
                        style: AppTypography.bodyMedium.copyWith(
                          color: AppColors.textPrimary,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ],
                  ),
                
                const SizedBox(height: 24),
                
                // Seri kuralları
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Seri kuralları',
                      style: AppTypography.labelMedium.copyWith(
                        color: AppColors.textTertiary,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const SizedBox(height: 16),
                    
                    // Kural 1
                    _buildRuleItem(
                      icon: Icons.check_circle_outline,
                      iconColor: AppColors.primary,
                      text: 'Her gün 3.000 adım ve üzeri atarsan serin +1 gün artar.',
                    ),
                    
                    const SizedBox(height: 12),
                    
                    // Kural 2
                    _buildRuleItem(
                      icon: Icons.ac_unit_outlined,
                      iconColor: AppColors.streakFireStart,
                      text: '3.000 adımın altında kalırsan ve dondurma hakkın varsa: Seri bozulmaz, gün sayısı değişmez, dondurma hakkın 1 azalır.',
                    ),
                    
                    const SizedBox(height: 12),
                    
                    // Kural 3
                    _buildRuleItem(
                      icon: Icons.star_outline,
                      iconColor: AppColors.accentLeague,
                      text: '3.000 adımın üstünde attığın her 1.000 adım için +1 puan kazanırsın.',
                    ),
                  ],
                ),
                
                const SizedBox(height: 32),
                
                // Kapat butonu
                SizedBox(
                  width: double.infinity,
                  height: 52,
                  child: ElevatedButton(
                    onPressed: onClose,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: AppColors.onPrimary,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    child: Text(
                      'Tamam',
                      style: AppTypography.labelLarge.copyWith(
                        color: AppColors.onPrimary,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
  
  Widget _buildRuleItem({
    required IconData icon,
    required Color iconColor,
    required String text,
  }) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(
          icon,
          color: iconColor,
          size: 20,
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Text(
            text,
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textPrimary,
            ),
          ),
        ),
      ],
    );
  }
}

