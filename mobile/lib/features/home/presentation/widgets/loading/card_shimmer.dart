import 'package:flutter/material.dart';
import 'package:shimmer/shimmer.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';

/// Kart shimmer placeholder'ı
/// Loading durumunda kullanılır
class CardShimmer extends StatelessWidget {
  const CardShimmer({super.key});
  
  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.07),
            blurRadius: 15,
            offset: const Offset(0, -3),
          ),
        ],
      ),
      child: Shimmer.fromColors(
        baseColor: AppColors.divider,
        highlightColor: AppColors.background,
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // İkon shimmer
              Container(
                width: 40,
                height: 40,
                decoration: const BoxDecoration(
                  color: Colors.white,
                  shape: BoxShape.circle,
                ),
              ),
              
              const SizedBox(height: 12),
              
              // Başlık shimmer
              Container(
                width: double.infinity,
                height: 20,
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(4),
                ),
              ),
              
              const SizedBox(height: 8),
              
              // Alt başlık shimmer
              Container(
                width: 80,
                height: 14,
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              
              const Spacer(),
              
              // Progress bar shimmer
              Container(
                width: double.infinity,
                height: 8,
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(4),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
