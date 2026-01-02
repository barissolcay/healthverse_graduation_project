import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';

/// Ortak Modal Stilleri
/// - Blur + koyu arkaplan
/// - Ortada yuvarlak köşeli kart
/// - X butonu ile kapatma
/// - Boşluğa tıklayınca kapatma

/// Center modal göster
Future<T?> showCenterModal<T>({
  required BuildContext context,
  required Widget child,
  String? title,
  bool barrierDismissible = true,
}) {
  return showGeneralDialog<T>(
    context: context,
    barrierDismissible: barrierDismissible,
    barrierLabel: 'Dismiss',
    barrierColor: Colors.black.withAlpha(75), // %30 siyahlık
    transitionDuration: const Duration(milliseconds: 200),
    pageBuilder: (context, animation, secondaryAnimation) {
      return _CenterModalWrapper(
        title: title,
        child: child,
      );
    },
    transitionBuilder: (context, animation, secondaryAnimation, child) {
      return BackdropFilter(
        filter: ImageFilter.blur(
          sigmaX: 4 * animation.value,
          sigmaY: 4 * animation.value,
        ),
        child: FadeTransition(
          opacity: animation,
          child: ScaleTransition(
            scale: Tween<double>(begin: 0.9, end: 1.0).animate(
              CurvedAnimation(parent: animation, curve: Curves.easeOutCubic),
            ),
            child: child,
          ),
        ),
      );
    },
  );
}

class _CenterModalWrapper extends StatelessWidget {
  final String? title;
  final Widget child;

  const _CenterModalWrapper({
    this.title,
    required this.child,
  });

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Material(
          color: Colors.transparent,
          child: Container(
            constraints: const BoxConstraints(maxWidth: 340),
            decoration: BoxDecoration(
              color: AppColors.surface,
              borderRadius: BorderRadius.circular(20),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withAlpha(40),
                  blurRadius: 24,
                  offset: const Offset(0, 8),
                ),
              ],
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                // Header with X button
                if (title != null)
                  Padding(
                    padding: const EdgeInsets.fromLTRB(24, 20, 12, 0),
                    child: Row(
                      children: [
                        Expanded(
                          child: Text(
                            title!,
                            style: AppTypography.titleLarge.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                            textAlign: TextAlign.center,
                          ),
                        ),
                        IconButton(
                          onPressed: () => Navigator.pop(context),
                          icon: const Icon(Icons.close),
                          color: AppColors.textTertiary,
                          iconSize: 24,
                        ),
                      ],
                    ),
                  )
                else
                  Align(
                    alignment: Alignment.topRight,
                    child: Padding(
                      padding: const EdgeInsets.all(8),
                      child: IconButton(
                        onPressed: () => Navigator.pop(context),
                        icon: const Icon(Icons.close),
                        color: AppColors.textTertiary,
                        iconSize: 24,
                      ),
                    ),
                  ),
                
                // Content
                Flexible(
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.fromLTRB(24, 8, 24, 24),
                    child: child,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

/// Info tooltip göster (dondurma hakkı açıklaması gibi)
void showInfoTooltip(BuildContext context, {required String message}) {
  showDialog(
    context: context,
    barrierColor: Colors.transparent,
    builder: (context) => AlertDialog(
      backgroundColor: AppColors.textPrimary,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      contentPadding: const EdgeInsets.all(16),
      content: Text(
        message,
        style: AppTypography.bodyMedium.copyWith(
          color: AppColors.surface,
        ),
        textAlign: TextAlign.center,
      ),
    ),
  );
}
