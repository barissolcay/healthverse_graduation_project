import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import '../../../data/models/partner_mission_model.dart';
import 'base_card.dart';

/// Partner Mission Card - Ortak görev kartı (Sosyal bölümü)
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
    if (mission.missionId.isEmpty) {
      return EmptyCard(
        title: 'Ortak Görev',
        message: 'Partner bul',
        icon: Icons.handshake,
        accentColor: AppColors.accentPartner,
        onTap: onTap,
      );
    }

    final int percent = (mission.progressPercentage * 100).toInt();

    return BaseCard(
      title: mission.title,
      subtitle: 'Partner: ${mission.partnerUsername}',
      icon: Icons.handshake,
      accentColor: AppColors.accentPartner,
      timeRemaining: _formatDuration(mission.timeRemaining),
      progress: mission.progressPercentage,
      showProgress: true,
      onTap: onTap,
      trailing: Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        decoration: BoxDecoration(
          color: AppColors.accentPartner.withAlpha(25),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Text(
          '%$percent',
          style: TextStyle(
            color: AppColors.accentPartner,
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
