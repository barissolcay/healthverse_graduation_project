import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import '../../../data/models/global_mission_model.dart';
import 'base_card.dart';

/// Global Mission Card - Global görev kartı (Sosyal bölümü)
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
    if (mission.missionId.isEmpty) { // Basit check
       return EmptyCard(
        title: 'Dünya Görevi',
        message: 'Aktif görev yok',
        icon: Icons.public,
        accentColor: AppColors.accentGlobal,
        onTap: onTap,
      );
    }

    final int percent = (mission.progressPercentage * 100).toInt();

    return BaseCard(
      title: 'Aya Yürüyoruz', // Kullanıcı isteği: Sabit başlık
      subtitle: '${mission.participantCount} Katılımcı',
      icon: Icons.public,
      accentColor: AppColors.accentGlobal,
      timeRemaining: _formatDuration(mission.timeRemaining),
      progress: mission.progressPercentage,
      showProgress: true,
      onTap: onTap,
      trailing: Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        decoration: BoxDecoration(
          color: AppColors.accentGlobal.withAlpha(25),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Text(
          '%$percent',
          style: TextStyle(
            color: AppColors.accentGlobal,
            fontWeight: FontWeight.bold,
            fontSize: 12,
          ),
        ),
      ),
    );
  }

  String? _formatDuration(Duration duration) {
    if (duration.isNegative) return null;
    if (duration.inDays > 0) return '${duration.inDays} gün';
    if (duration.inHours > 0) return '${duration.inHours} saat';
    return '${duration.inMinutes} dk';
  }
}
