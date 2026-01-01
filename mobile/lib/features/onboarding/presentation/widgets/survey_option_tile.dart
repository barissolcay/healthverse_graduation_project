/// Survey Option Tile - Anket seçenekleri için ortak widget
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';

/// Radio button tarzı tek seçim tile
class SurveyOptionTile extends StatelessWidget {
  final String label;
  final String? description;
  final String? emoji;
  final bool isSelected;
  final VoidCallback onTap;
  final bool isSkipOption;

  const SurveyOptionTile({
    super.key,
    required this.label,
    this.description,
    this.emoji,
    required this.isSelected,
    required this.onTap,
    this.isSkipOption = false,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: isSelected 
              ? AppColors.primary.withOpacity(0.1) 
              : Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected ? AppColors.primary : AppColors.divider,
            width: isSelected ? 2 : 1,
          ),
        ),
        child: Row(
          children: [
            // Emoji varsa göster
            if (emoji != null) ...[
              Text(emoji!, style: const TextStyle(fontSize: 24)),
              const SizedBox(width: 12),
            ],

            // Label ve description
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    label,
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
                      color: isSkipOption 
                          ? AppColors.textTertiary 
                          : AppColors.textPrimary,
                    ),
                  ),
                  if (description != null) ...[
                    const SizedBox(height: 4),
                    Text(
                      description!,
                      style: TextStyle(
                        fontSize: 14,
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ],
              ),
            ),

            // Radio indicator
            AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              width: 24,
              height: 24,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                border: Border.all(
                  color: isSelected ? AppColors.primary : AppColors.divider,
                  width: 2,
                ),
                color: isSelected ? AppColors.primary : Colors.transparent,
              ),
              child: isSelected
                  ? const Icon(Icons.check, size: 16, color: Colors.white)
                  : null,
            ),
          ],
        ),
      ),
    );
  }
}

/// Checkbox tarzı çoklu seçim tile
class SurveyCheckboxTile extends StatelessWidget {
  final String label;
  final String? description;
  final String? emoji;
  final bool isSelected;
  final VoidCallback onTap;
  final bool isDisabled;

  const SurveyCheckboxTile({
    super.key,
    required this.label,
    this.description,
    this.emoji,
    required this.isSelected,
    required this.onTap,
    this.isDisabled = false,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: isDisabled ? null : onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: isSelected 
              ? AppColors.primary.withOpacity(0.1) 
              : isDisabled 
                  ? AppColors.divider.withOpacity(0.3)
                  : Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected ? AppColors.primary : AppColors.divider,
            width: isSelected ? 2 : 1,
          ),
        ),
        child: Row(
          children: [
            // Emoji varsa göster
            if (emoji != null) ...[
              Text(
                emoji!, 
                style: TextStyle(
                  fontSize: 24,
                  color: isDisabled ? Colors.grey : null,
                ),
              ),
              const SizedBox(width: 12),
            ],

            // Label ve description
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    label,
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
                      color: isDisabled 
                          ? AppColors.textTertiary 
                          : AppColors.textPrimary,
                    ),
                  ),
                  if (description != null) ...[
                    const SizedBox(height: 4),
                    Text(
                      description!,
                      style: TextStyle(
                        fontSize: 14,
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ],
              ),
            ),

            // Checkbox indicator
            AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              width: 24,
              height: 24,
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(6),
                border: Border.all(
                  color: isSelected ? AppColors.primary : AppColors.divider,
                  width: 2,
                ),
                color: isSelected ? AppColors.primary : Colors.transparent,
              ),
              child: isSelected
                  ? const Icon(Icons.check, size: 16, color: Colors.white)
                  : null,
            ),
          ],
        ),
      ),
    );
  }
}
