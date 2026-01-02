import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import '../../../data/models/duel_card_model.dart';
import 'base_card.dart';

/// Duel Card - Düello kartı (Rekabet bölümü)
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
    if (!duel.isActive) {
      return EmptyCard(
        title: 'Düello',
        message: 'Aktif düello yok.',
        icon: Icons.sports_kabaddi,
        accentColor: AppColors.accentDuels,
        onTap: onTap,
      );
    }

    // Title generation
    String titleValue = '${duel.targetValue}';
    if (duel.targetValue >= 1000) {
      // 10000 -> 10.000 formatting if needed, or just simplistic
    }
    
    String typeLabel = 'Birim';
    switch (duel.activityType.toUpperCase()) {
      case 'STEPS': typeLabel = 'Adım'; break;
      case 'DISTANCE': typeLabel = 'Metre'; break;
      case 'CALORIES': typeLabel = 'Kalori'; break;
      case 'DURATION': typeLabel = 'Dakika'; break;
      default: typeLabel = duel.activityType;
    }

    final String displayTitle = '$titleValue $typeLabel'; // Örn: 10000 Adım

    return BaseCard(
      title: displayTitle,
      subtitle: 'Rakip',
      icon: Icons.sports_kabaddi,
      accentColor: AppColors.accentDuels,
      timeRemaining: duel.formattedTimeRemaining,
      showProgress: true,
      onTap: onTap,
      customProgressWidget: _buildSplitProgressBar(),
    );
  }

  Widget _buildSplitProgressBar() {
    // İlerleme yüzdeleri (Max 1.0)
    final double myPercent = duel.myProgressPercentage.clamp(0.0, 1.0);
    final double opponentPercent = duel.opponentProgressPercentage.clamp(0.0, 1.0);

    return Container(
      height: 8,
      decoration: BoxDecoration(
        color: AppColors.divider.withOpacity(0.3),
        borderRadius: BorderRadius.circular(4),
      ),
      child: Row(
        children: [
          // SOL TARAF (BENİM) - Yeşile doğru doluyor (Sağa doğru)
          Expanded(
            child: Container(
              alignment: Alignment.centerRight,
              child: FractionallySizedBox(
                widthFactor: myPercent,
                child: Container(
                  decoration: const BoxDecoration(
                    color: AppColors.primary, // Yeşil
                    borderRadius: BorderRadius.horizontal(left: Radius.circular(4)),
                  ),
                ),
              ),
            ),
          ),
          
          // ORTA ÇİZGİ (HEDEF)
          Container(
            width: 2,
            color: Colors.white,
          ),
          
          // SAĞ TARAF (RAKİP) - Kırmızıdan sola doğru doluyor
          Expanded(
            child: Container(
              alignment: Alignment.centerLeft,
              child: FractionallySizedBox(
                widthFactor: opponentPercent,
                child: Container(
                  decoration: const BoxDecoration(
                    color: Colors.red, // Kırmızı
                    borderRadius: BorderRadius.horizontal(right: Radius.circular(4)),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
