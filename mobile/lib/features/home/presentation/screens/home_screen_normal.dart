import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import '../providers/home_screen_provider.dart';
import '../../data/models/home_data_model.dart';
import '../../data/models/user_stats_model.dart';
import '../widgets/shared/home_app_bar.dart';
import '../widgets/shared/bottom_nav_bar.dart';
import '../widgets/cards/streak_card.dart';
import '../widgets/cards/tasks_card.dart';
import '../widgets/cards/goals_card.dart';
import '../widgets/cards/league_card.dart';
import '../widgets/cards/duel_card.dart';
import '../widgets/cards/partner_mission_card.dart';
import '../widgets/cards/global_mission_card.dart';
import '../widgets/loading/card_shimmer.dart';
import '../widgets/loading/hero_shimmer.dart';
import '../widgets/modals/streak_detail_widget.dart';

/// Normal home screen
/// Health permission verilmiş kullanıcılar için
class HomeScreenNormal extends ConsumerWidget {
  const HomeScreenNormal({super.key});
  
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(homeScreenProvider);
    
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // Üst bar
            HomeAppBar(
              username: state.data?.userStats.username ?? 'HealthVerse',
              avatarUrl: state.data?.userStats.avatarUrl,
              unreadNotificationCount: state.data?.unreadNotificationCount ?? 0,
              onNotificationTap: () {
                // Bildirim ekranına yönlendir
              },
            ),
            
            // İçerik
            Expanded(
              child: _buildContent(context, ref, state),
            ),
            
            // Alt navigation
            HomeBottomNavBar(
              currentIndex: 2,
              onTap: (index) {
                // Navigation logic
              },
            ),
          ],
        ),
      ),
    );
  }
  
  Widget _buildContent(BuildContext context, WidgetRef ref, HomeState state) {
    if (state.status == HomeStatus.loading) {
      return _buildLoadingShimmer();
    }
    
    if (state.status == HomeStatus.error) {
      return _buildError(context, ref, state.errorMessage ?? 'Bilinmeyen hata');
    }
    
    if (state.status == HomeStatus.loaded && state.data != null) {
      return RefreshIndicator(
        onRefresh: () async {
          await ref.read(homeScreenProvider.notifier).refresh();
        },
        child: _buildLoadedContent(context, ref, state.data!),
      );
    }
    
    return _buildLoadingShimmer();
  }
  
  Widget _buildLoadingShimmer() {
    return SingleChildScrollView(
      physics: const BouncingScrollPhysics(),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            const SizedBox(height: 16),
            const HeroShimmer(),
            const SizedBox(height: 24),
            GridView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                crossAxisSpacing: 16,
                mainAxisSpacing: 16,
                childAspectRatio: 1.0,
              ),
              itemCount: 6,
              itemBuilder: (context, index) => const CardShimmer(),
            ),
          ],
        ),
      ),
    );
  }
  
  Widget _buildLoadedContent(BuildContext context, WidgetRef ref, HomeDataModel data) {
    return SingleChildScrollView(
      physics: const AlwaysScrollableScrollPhysics(),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            const SizedBox(height: 16),
            
            // Hero: Streak kartı
            GestureDetector(
              onTap: () => _showStreakDetail(context, data.userStats),
              child: StreakCard(
                streakCount: data.userStats.currentStreak,
                currentSteps: data.userStats.todaySteps,
                freezeInventory: data.userStats.freezeCount,
              ),
            ),
            
            const SizedBox(height: 24),
            
            // Grid kartlar
            _buildCardGrid(context, data),
          ],
        ),
      ),
    );
  }
  
  Widget _buildCardGrid(BuildContext context, HomeDataModel data) {
    final cards = <Widget>[];
    
    // Görevler
    if (data.activeTasks.isNotEmpty) {
      cards.add(TasksCard(
        tasks: data.displayTasks,
        onTap: () {},
      ));
    }
    
    // Hedefler
    if (data.activeGoals.isNotEmpty) {
      cards.add(GoalsCard(
        goals: data.displayGoals,
        onTap: () {},
      ));
    }
    
    // Lig
    if (data.leagueInfo != null) {
      cards.add(LeagueCard(
        league: data.leagueInfo!,
        onTap: () {},
      ));
    }
    
    // Düello
    if (data.activeDuel != null) {
      cards.add(DuelCard(
        duel: data.activeDuel!,
        onTap: () {},
      ));
    }
    
    // Partner
    if (data.partnerMission != null) {
      cards.add(PartnerMissionCard(
        mission: data.partnerMission!,
        onTap: () {},
      ));
    }
    
    // Global
    if (data.globalMission != null) {
      cards.add(GlobalMissionCard(
        mission: data.globalMission!,
        onTap: () {},
      ));
    }
    
    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        crossAxisSpacing: 16,
        mainAxisSpacing: 16,
        childAspectRatio: 1.0,
      ),
      itemCount: cards.length,
      itemBuilder: (context, index) => cards[index],
    );
  }
  
  void _showStreakDetail(BuildContext context, UserStatsModel userStats) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) {
        return StreakDetailWidget(
          streakCount: userStats.currentStreak,
          longestStreakCount: userStats.currentStreak, // TODO: add longestStreak to model
          currentSteps: userStats.todaySteps,
          freezeInventory: userStats.freezeCount,
          onClose: () => Navigator.of(context).pop(),
        );
      },
    );
  }
  
  Widget _buildError(BuildContext context, WidgetRef ref, String message) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline, color: AppColors.error, size: 64),
            const SizedBox(height: 16),
            Text('Bir hata oluştu', style: AppTypography.titleLarge),
            const SizedBox(height: 8),
            Text(message, style: AppTypography.bodyMedium, textAlign: TextAlign.center),
            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: () => ref.read(homeScreenProvider.notifier).refresh(),
              child: Text('Tekrar Dene', style: AppTypography.labelLarge.copyWith(color: AppColors.onPrimary)),
            ),
          ],
        ),
      ),
    );
  }
}
