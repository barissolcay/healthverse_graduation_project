import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import '../shared/center_modal.dart';

/// Streak Detail Widget - Seri detayını gösteren modal

void showStreakDetail(
  BuildContext context, {
  required int currentStreak,
  required int longestStreak,
  required int todaySteps,
  required int freezeCount,
}) {
  final stepPoints = todaySteps >= 3000 ? ((todaySteps - 3000) / 1000).floor() : 0;
  
  showCenterModal(
    context: context,
    child: _StreakDetailContent(
      currentStreak: currentStreak,
      longestStreak: longestStreak,
      todaySteps: todaySteps,
      freezeCount: freezeCount,
      stepPoints: stepPoints,
    ),
  );
}

class _StreakDetailContent extends StatefulWidget {
  final int currentStreak;
  final int longestStreak;
  final int todaySteps;
  final int freezeCount;
  final int stepPoints;

  const _StreakDetailContent({
    required this.currentStreak,
    required this.longestStreak,
    required this.todaySteps,
    required this.freezeCount,
    required this.stepPoints,
  });

  @override
  State<_StreakDetailContent> createState() => _StreakDetailContentState();
}

class _StreakDetailContentState extends State<_StreakDetailContent> {
  bool _showRules = false;

  bool get isCompleted => widget.todaySteps >= 3000;
  double get progressPercentage => (widget.todaySteps / 3000).clamp(0.0, 1.0);

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        // Alev + Seri sayısı
        _buildHeroSection(),
        
        const SizedBox(height: 24),
        
        // Progress bar + adım sayısı
        _buildProgressSection(),
        
        const SizedBox(height: 20),
        
        // Dondurma + Puan satırı
        _buildStatsRow(),
        
        const SizedBox(height: 16),
        
        // Seri Kuralları (açılır kapanır)
        _buildRulesSection(),
      ],
    );
  }

  Widget _buildHeroSection() {
    return Column(
      children: [
        // Alev ikonu
        Container(
          width: 72,
          height: 72,
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
                      size: 40,
                    ),
                  )
                : const Icon(
                    Icons.local_fire_department_outlined,
                    color: AppColors.textHint,
                    size: 40,
                  ),
          ),
        ),
        
        const SizedBox(height: 16),
        
        // Seri sayısı - büyük
        Text(
          '${widget.currentStreak} gün',
          style: AppTypography.displayLarge.copyWith(
            color: AppColors.textPrimary,
            fontWeight: FontWeight.bold,
            fontSize: 40,
          ),
        ),
        
        const SizedBox(height: 4),
        
        Text(
          'En uzun serin: ${widget.longestStreak} gün',
          style: AppTypography.bodySmall.copyWith(
            color: AppColors.textTertiary,
          ),
        ),
      ],
    );
  }

  Widget _buildProgressSection() {
    return Column(
      children: [
        // Progress bar
        Container(
          height: 10,
          width: double.infinity,
          decoration: BoxDecoration(
            color: AppColors.divider,
            borderRadius: BorderRadius.circular(5),
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
                borderRadius: BorderRadius.circular(5),
              ),
            ),
          ),
        ),
        
        const SizedBox(height: 10),
        
        // Adım bilgisi - büyük
        RichText(
          text: TextSpan(
            children: [
              TextSpan(
                text: '${widget.todaySteps}',
                style: AppTypography.titleLarge.copyWith(
                  color: AppColors.textPrimary,
                  fontWeight: FontWeight.bold,
                ),
              ),
              TextSpan(
                text: ' / 3.000 adım',
                style: AppTypography.titleMedium.copyWith(
                  color: AppColors.textTertiary,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildStatsRow() {
    return Row(
      children: [
        // Dondurma hakkı
        Expanded(
          child: GestureDetector(
            onTap: () => _showFreezeInfo(),
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: Colors.blue.shade50,
                borderRadius: BorderRadius.circular(10),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    Icons.ac_unit,
                    size: 16,
                    color: Colors.blue.shade600,
                  ),
                  const SizedBox(width: 6),
                  Text(
                    '${widget.freezeCount}',
                    style: AppTypography.labelLarge.copyWith(
                      color: Colors.blue.shade700,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
        
        const SizedBox(width: 12),
        
        // Puan
        Expanded(
          child: GestureDetector(
            onTap: () => _showPointsInfo(),
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: Colors.amber.shade50,
                borderRadius: BorderRadius.circular(10),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    Icons.star,
                    size: 16,
                    color: Colors.amber.shade600,
                  ),
                  const SizedBox(width: 6),
                  Text(
                    '+${widget.stepPoints}',
                    style: AppTypography.labelLarge.copyWith(
                      color: Colors.amber.shade700,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildRulesSection() {
    return Column(
      children: [
        // Başlık - tıklanabilir
        GestureDetector(
          onTap: () => setState(() => _showRules = !_showRules),
          child: Container(
            padding: const EdgeInsets.symmetric(vertical: 8),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(
                  'Seri kuralları',
                  style: AppTypography.labelMedium.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
                const SizedBox(width: 4),
                Icon(
                  _showRules ? Icons.expand_less : Icons.expand_more,
                  size: 20,
                  color: AppColors.textSecondary,
                ),
              ],
            ),
          ),
        ),
        
        // Kurallar (açılır kapanır)
        AnimatedCrossFade(
          firstChild: const SizedBox.shrink(),
          secondChild: _buildRulesContent(),
          crossFadeState: _showRules 
              ? CrossFadeState.showSecond 
              : CrossFadeState.showFirst,
          duration: const Duration(milliseconds: 200),
        ),
      ],
    );
  }

  Widget _buildRulesContent() {
    return Container(
      margin: const EdgeInsets.only(top: 8),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.divider.withAlpha(50),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildRuleItem(
            icon: Icons.check_circle,
            iconColor: AppColors.primary,
            text: 'Her gün 3.000 adım ve üzerini atarsan serin 1 gün artar.',
          ),
          const SizedBox(height: 12),
          _buildRuleItem(
            icon: Icons.ac_unit,
            iconColor: Colors.blue,
            text: '3.000 adımın altında kalırsan ve dondurma hakkın varsa seri bozulmaz, dondurma hakkın 1 azalır.',
          ),
          const SizedBox(height: 12),
          _buildRuleItem(
            icon: Icons.warning_amber_rounded,
            iconColor: Colors.red,
            text: '3.000 adımdan az atarsan ve dondurma hakkın yoksa maalesef serin sıfırlanır.',
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
        Icon(icon, size: 18, color: iconColor),
        const SizedBox(width: 10),
        Expanded(
          child: Text(
            text,
            style: AppTypography.bodySmall.copyWith(
              color: AppColors.textPrimary,
              height: 1.4,
            ),
          ),
        ),
      ],
    );
  }

  void _showFreezeInfo() {
    showInfoTooltip(
      context,
      message: 'Başarılar elde ettikçe dondurma hakkı kazanırsın!',
    );
  }

  void _showPointsInfo() {
    showInfoTooltip(
      context,
      message: '3.000 adımı geçtikten sonra her 1.000 adım için +1 puan!',
    );
  }
}
