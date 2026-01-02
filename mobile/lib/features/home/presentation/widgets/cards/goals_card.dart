import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import '../../../data/models/goal_card_model.dart';
import 'base_card.dart';

/// Hedefler kartı - Orange renk (Aktiviteler bölümü)
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
      return EmptyCard(
        title: 'Hedefler',
        message: 'Aktif hedef yok',
        icon: Icons.flag_outlined,
        accentColor: AppColors.accentGoals,
        onTap: onTap,
      );
    }

    final goal = goals.first;
    final int percent = (goal.progressPercentage * 100).toInt();
    final remaining = goal.timeRemaining;
    String? timeString;
    
    // Süre formatı
    if (!remaining.isNegative && remaining.inMinutes > 0) {
      if (remaining.inDays > 0) {
        timeString = '${remaining.inDays} gün';
      } else if (remaining.inHours > 0) {
        timeString = '${remaining.inHours} saat';
      } else {
        timeString = '${remaining.inMinutes} dk';
      }
    }

    return BaseCard(
      title: goal.title,
      // Subtitle kaldırıldı
      icon: Icons.flag,
      accentColor: AppColors.accentGoals,
      timeRemaining: timeString,
      progress: goal.progressPercentage,
      showProgress: true,
      onTap: onTap,
      trailing: Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        decoration: BoxDecoration(
          color: AppColors.accentGoals.withAlpha(25),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Text(
          '%$percent',
          style: TextStyle(
            color: AppColors.accentGoals,
            fontWeight: FontWeight.bold,
            fontSize: 12,
          ),
        ),
      ),
    );
  }
}
