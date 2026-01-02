import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import '../../../data/models/league_card_model.dart';
import 'base_card.dart';

/// League Card - Lig kartı (Rekabet bölümü)
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
    if (league.leagueName.isEmpty) {
      return EmptyCard(
        title: 'Lig Durumu',
        message: 'Henüz bir lige katılmadın.',
        icon: Icons.emoji_events,
        accentColor: AppColors.accentLeague,
        onTap: onTap,
      );
    }

    return BaseCard(
      title: '${league.tierName} Ligi',
      subtitle: '${league.currentRank}. sırada (${league.currentRank}/${league.totalParticipants})',
      icon: Icons.emoji_events,
      accentColor: AppColors.accentLeague,
      timeRemaining: league.formattedTimeRemaining,
      showProgress: false,
      onTap: onTap,
    );
  }
}
