import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';

/// Base Card Widget - Tüm kartlar için ortak tasarım
/// Gölgeli, yuvarlak köşeli, tutarlı yapı
class BaseCard extends StatelessWidget {
  final String title;
  final String? subtitle;
  final IconData icon;
  final Color accentColor;
  final String? timeRemaining;
  final double? progress; // 0.0 - 1.0
  final bool showProgress;
  final Widget? customProgressWidget;
  final VoidCallback? onTap;
  final Widget? trailing;

  const BaseCard({
    super.key,
    required this.title,
    this.subtitle,
    required this.icon,
    required this.accentColor,
    this.timeRemaining,
    this.progress,
    this.showProgress = false,
    this.customProgressWidget,
    this.onTap,
    this.trailing,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: double.infinity,
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: Colors.black.withAlpha(10),
            width: 1,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withAlpha(20),
              blurRadius: 16,
              offset: const Offset(0, 6),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Üst kısım: İkon + Başlık + Kalan süre
            Row(
              children: [
                // İkon
                Container(
                  width: 36,
                  height: 36,
                  decoration: BoxDecoration(
                    color: accentColor.withAlpha(25),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Icon(
                    icon,
                    size: 20,
                    color: accentColor,
                  ),
                ),
                
                const SizedBox(width: 12),
                
                // Başlık + Alt yazı
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        title,
                        style: AppTypography.titleSmall.copyWith(
                          color: AppColors.textPrimary,
                          fontWeight: FontWeight.bold,
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      if (subtitle != null) ...[
                        const SizedBox(height: 2),
                        Text(
                          subtitle!,
                          style: AppTypography.bodySmall.copyWith(
                            color: AppColors.textSecondary,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ],
                    ],
                  ),
                ),
                
                // Trailing (sağ üst)
                if (trailing != null) trailing!,
              ],
            ),
            
            const Spacer(),
            
            // Alt kısım: Kalan süre + Progress bar
            if (timeRemaining != null && timeRemaining!.isNotEmpty) ...[
              Text(
                'Kalan: $timeRemaining',
                style: AppTypography.labelSmall.copyWith(
                  color: accentColor,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 6),
            ],
            
            // Progress bar
            if (showProgress)
              if (customProgressWidget != null)
                customProgressWidget!
              else if (progress != null)
                Container(
                  height: 6,
                  width: double.infinity,
                  decoration: BoxDecoration(
                    color: AppColors.divider,
                    borderRadius: BorderRadius.circular(3),
                  ),
                  child: FractionallySizedBox(
                    alignment: Alignment.centerLeft,
                    widthFactor: progress!.clamp(0.0, 1.0),
                    child: Container(
                      decoration: BoxDecoration(
                        color: accentColor,
                        borderRadius: BorderRadius.circular(3),
                      ),
                    ),
                  ),
                ),
          ],
        ),
      ),
    );
  }
}

/// Empty Card Widget - Boş kart placeholder
class EmptyCard extends StatelessWidget {
  final String title;
  final String message;
  final IconData icon;
  final Color accentColor;
  final VoidCallback? onTap;

  const EmptyCard({
    super.key,
    required this.title,
    required this.message,
    required this.icon,
    required this.accentColor,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: double.infinity,
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: Colors.black.withAlpha(10), // Varla yok arası çizgi
            width: 1,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withAlpha(20), // Daha gövdeli gölge
              blurRadius: 16,
              offset: const Offset(0, 6),
            ),
          ],
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: accentColor.withAlpha(15),
                shape: BoxShape.circle,
              ),
              child: Icon(
                icon,
                size: 28,
                color: accentColor.withAlpha(150),
              ),
            ),
            const SizedBox(height: 12),
            Text(
              title,
              style: AppTypography.titleSmall.copyWith(
                color: AppColors.textPrimary,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              message,
              style: AppTypography.bodySmall.copyWith(
                color: AppColors.textTertiary,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }
}
