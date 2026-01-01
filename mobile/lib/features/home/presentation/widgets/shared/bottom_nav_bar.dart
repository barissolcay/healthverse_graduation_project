import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';

/// Bottom navigation bar
/// 5 sekme: Görevler, Hedefler, Ana Sayfa, Liderlik, Sosyal
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
        border: Border(
          top: BorderSide(
            color: AppColors.divider,
            width: 1,
          ),
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 20,
            offset: const Offset(0, -5),
          ),
        ],
      ),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
          child: Row(
            children: [
              // Görevler
              _NavItem(
                icon: Icons.assignment_outlined,
                label: 'Görevler',
                index: 0,
                currentIndex: currentIndex,
                onTap: onTap,
              ),
              
              // Hedefler
              _NavItem(
                icon: Icons.track_changes_outlined,
                label: 'Hedefler',
                index: 1,
                currentIndex: currentIndex,
                onTap: onTap,
              ),
              
              // Ana Sayfa (ortada büyük buton)
              const Spacer(),
              _NavCenterButton(
                index: 2,
                currentIndex: currentIndex,
                onTap: onTap,
              ),
              const Spacer(),
              
              // Liderlik
              _NavItem(
                icon: Icons.emoji_events_outlined,
                label: 'Liderlik',
                index: 3,
                currentIndex: currentIndex,
                onTap: onTap,
              ),
              
              // Sosyal
              _NavItem(
                icon: Icons.group_outlined,
                label: 'Sosyal',
                index: 4,
                currentIndex: currentIndex,
                onTap: onTap,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// Normal nav item
class _NavItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final int index;
  final int currentIndex;
  final void Function(int)? onTap;
  
  const _NavItem({
    required this.icon,
    required this.label,
    required this.index,
    required this.currentIndex,
    this.onTap,
  });
  
  @override
  Widget build(BuildContext context) {
    final isSelected = index == currentIndex;
    
    return GestureDetector(
      onTap: onTap != null ? () => onTap!(index) : null,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            icon,
            color: isSelected ? AppColors.primary : AppColors.navInactive,
            size: 24,
          ),
          const SizedBox(height: 4),
          Text(
            label,
            style: AppTypography.labelTiny.copyWith(
              color: isSelected ? AppColors.primary : AppColors.navInactive,
              fontWeight: isSelected ? FontWeight.w700 : FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}

/// Center button (Ana Sayfa)
/// Daha büyük, circular, elevated
class _NavCenterButton extends StatelessWidget {
  final int index;
  final int currentIndex;
  final void Function(int)? onTap;
  
  const _NavCenterButton({
    required this.index,
    required this.currentIndex,
    this.onTap,
  });
  
  @override
  Widget build(BuildContext context) {
    final isSelected = index == currentIndex;
    
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        // Buton
        GestureDetector(
          onTap: onTap != null ? () => onTap!(index) : null,
          child: Container(
            width: 56,
            height: 56,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: isSelected ? AppColors.primary : AppColors.surface,
              boxShadow: isSelected
                  ? [
                      BoxShadow(
                        color: AppColors.primary.withValues(alpha: 0.4),
                        blurRadius: 16,
                        offset: const Offset(0, 8),
                      ),
                    ]
                  : null,
            ),
            child: Icon(
              Icons.home,
              color: isSelected ? AppColors.onPrimary : AppColors.navInactive,
              size: 28,
            ),
          ),
        ),
        
        const SizedBox(height: 4),
        
        // Label
        Text(
          'ANA SAYFA',
          style: AppTypography.labelTiny.copyWith(
            color: isSelected ? AppColors.primary : AppColors.navInactive,
            fontWeight: FontWeight.w700,
          ),
        ),
      ],
    );
  }
}

