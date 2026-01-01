import 'package:flutter/material.dart';
import '../../../../../app/theme/app_colors.dart';
import '../../../../../app/theme/app_typography.dart';

/// Home app bar
/// Üstte profil, başlık ve bildirim ikonu
class HomeAppBar extends StatelessWidget implements PreferredSizeWidget {
  final String username;
  final String? avatarUrl;
  final int unreadNotificationCount;
  final VoidCallback? onNotificationTap;
  
  const HomeAppBar({
    super.key,
    required this.username,
    this.avatarUrl,
    this.unreadNotificationCount = 0,
    this.onNotificationTap,
  });
  
  @override
  Size get preferredSize => const Size.fromHeight(kToolbarHeight + 16);
  
  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: AppColors.background,
        border: null,
      ),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
          child: Row(
            children: [
              // Sol: Profil
              _buildProfileAvatar(),
              
              const Spacer(),
              
              // Orta: Başlık
              Text(
                'Ana Sayfa',
                style: AppTypography.titleLarge.copyWith(
                  color: AppColors.textPrimary,
                  fontWeight: FontWeight.w700,
                ),
              ),
              
              const Spacer(),
              
              // Sağ: Bildirim
              _buildNotificationButton(),
            ],
          ),
        ),
      ),
    );
  }
  
  Widget _buildProfileAvatar() {
    return Container(
      width: 40,
      height: 40,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        border: Border.all(
          color: AppColors.surface,
          width: 2,
        ),
      ),
      child: ClipOval(
        child: avatarUrl != null
            ? Image.network(
                avatarUrl!,
                fit: BoxFit.cover,
                errorBuilder: (context, error, stackTrace) {
                  return Container(
                    color: AppColors.primaryLight.withValues(alpha: 0.3),
                    child: Icon(
                      Icons.person,
                      color: AppColors.primary,
                      size: 20,
                    ),
                  );
                },
              )
            : Container(
                color: AppColors.primaryLight.withValues(alpha: 0.3),
                child: const Icon(
                  Icons.person,
                  color: AppColors.primary,
                  size: 20,
                ),
              ),
      ),
    );
  }
  
  Widget _buildNotificationButton() {
    return Stack(
      children: [
        // Ana buton
        GestureDetector(
          onTap: onNotificationTap,
          child: Container(
            width: 40,
            height: 40,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: AppColors.surface,
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.05),
                  blurRadius: 10,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: Icon(
              Icons.notifications_outlined,
              color: AppColors.textSecondary,
              size: 24,
            ),
          ),
        ),
        
        // Badge (okunmamış sayısı)
        if (unreadNotificationCount > 0)
          Positioned(
            top: 0,
            right: 0,
            child: Container(
              width: 16,
              height: 16,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: AppColors.error,
              ),
              child: Center(
                child: Text(
                  unreadNotificationCount > 9
                      ? '9+'
                      : unreadNotificationCount.toString(),
                  style: AppTypography.labelTiny.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
            ),
          ),
      ],
    );
  }
}

