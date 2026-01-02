import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import '../../../data/models/task_card_model.dart';
import 'base_card.dart';

/// Görevler kartı - Emerald renk (Aktiviteler bölümü)
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
      return EmptyCard(
        title: 'Görevler',
        message: 'Aktif görev yok',
        icon: Icons.assignment_outlined,
        accentColor: AppColors.accentTasks,
        onTap: onTap,
      );
    }

    final task = tasks.first;
    final int percent = (task.progressPercentage * 100).toInt();

    return BaseCard(
      title: task.title,
      // Subtitle kaldırıldı
      icon: Icons.check_circle,
      accentColor: AppColors.accentTasks,
      // Kalan süre kaldırıldı
      progress: task.progressPercentage,
      showProgress: true,
      onTap: onTap,
      trailing: Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        decoration: BoxDecoration(
          color: AppColors.accentTasks.withAlpha(25),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Text(
          '%$percent',
          style: TextStyle(
            color: AppColors.accentTasks,
            fontWeight: FontWeight.bold,
            fontSize: 12,
          ),
        ),
      ),
    );
  }
}
