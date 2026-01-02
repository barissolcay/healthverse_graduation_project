import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';

/// Bottom Navigation Bar - Material 3 Standartlarına Uygun
/// Tüm butonlar eşit boyutta, seçili olan yeşil
class HomeBottomNavBar extends StatelessWidget {
  final int currentIndex;
  final void Function(int)? onTap;

  const HomeBottomNavBar({
    super.key,
    required this.currentIndex,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: const Border(
          top: BorderSide(color: AppColors.divider, width: 1),
        ),
      ),
      child: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 8),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceAround,
            children: [
              _NavItem(
                icon: Icons.assignment_outlined,
                selectedIcon: Icons.assignment,
                label: 'Görevler',
                isSelected: currentIndex == 0,
                onTap: onTap != null ? () => onTap!(0) : null,
              ),
              _NavItem(
                icon: Icons.flag_outlined,
                selectedIcon: Icons.flag,
                label: 'Hedefler',
                isSelected: currentIndex == 1,
                onTap: onTap != null ? () => onTap!(1) : null,
              ),
              _NavItem(
                icon: Icons.home_outlined,
                selectedIcon: Icons.home,
                label: 'Ana Sayfa',
                isSelected: currentIndex == 2,
                onTap: onTap != null ? () => onTap!(2) : null,
              ),
              _NavItem(
                icon: Icons.emoji_events_outlined,
                selectedIcon: Icons.emoji_events,
                label: 'Liderlik',
                isSelected: currentIndex == 3,
                onTap: onTap != null ? () => onTap!(3) : null,
              ),
              _NavItem(
                icon: Icons.group_outlined,
                selectedIcon: Icons.group,
                label: 'Sosyal',
                isSelected: currentIndex == 4,
                onTap: onTap != null ? () => onTap!(4) : null,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// Navigation Item - Eşit boyutlu
class _NavItem extends StatelessWidget {
  final IconData icon;
  final IconData selectedIcon;
  final String label;
  final bool isSelected;
  final VoidCallback? onTap;

  const _NavItem({
    required this.icon,
    required this.selectedIcon,
    required this.label,
    required this.isSelected,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      behavior: HitTestBehavior.opaque,
      child: SizedBox(
        width: 64,
        height: 56,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              isSelected ? selectedIcon : icon,
              color: isSelected ? AppColors.primary : AppColors.navInactive,
              size: 24,
            ),
            const SizedBox(height: 4),
            Text(
              label,
              style: AppTypography.labelSmall.copyWith(
                color: isSelected ? AppColors.primary : AppColors.navInactive,
                fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
                fontSize: 10,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ],
        ),
      ),
    );
  }
}
