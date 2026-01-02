import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/app/theme/app_typography.dart';
import 'package:healthverse_app/features/home/presentation/providers/home_screen_provider.dart';
import 'package:healthverse_app/features/home/presentation/widgets/cards/streak_card.dart';
import 'package:healthverse_app/features/home/presentation/widgets/cards/tasks_card.dart';
import 'package:healthverse_app/features/home/presentation/widgets/cards/goals_card.dart';
import 'package:healthverse_app/features/home/presentation/widgets/cards/league_card.dart';
import 'package:healthverse_app/features/home/presentation/widgets/cards/duel_card.dart';
import 'package:healthverse_app/features/home/presentation/widgets/cards/partner_mission_card.dart';
import 'package:healthverse_app/features/home/presentation/widgets/cards/global_mission_card.dart';
import 'package:healthverse_app/features/home/presentation/widgets/modals/streak_detail_widget.dart';
import '../widgets/cards/base_card.dart';
import 'package:healthverse_app/features/home/presentation/widgets/shared/bottom_nav_bar.dart';

/// Normal Home Screen - Sağlık verisine izin verilmiş
class HomeScreenNormal extends ConsumerStatefulWidget {
  const HomeScreenNormal({super.key});

  @override
  ConsumerState<HomeScreenNormal> createState() => _HomeScreenNormalState();
}

class _HomeScreenNormalState extends ConsumerState<HomeScreenNormal> {
  int _currentNavIndex = 2; // Ana Sayfa seçili

  @override
  Widget build(BuildContext context) {
    final homeState = ref.watch(homeScreenProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // App Bar
            _buildAppBar(homeState),

            // Content
            Expanded(
              child: homeState.isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : homeState.error != null
                      ? _buildErrorState(homeState.error!)
                      : _buildContent(homeState),
            ),
          ],
        ),
      ),
      bottomNavigationBar: HomeBottomNavBar(
        currentIndex: _currentNavIndex,
        onTap: (index) {
          setState(() {
            _currentNavIndex = index;
          });
          // TODO: Navigate to different screens
        },
      ),
    );
  }

  Widget _buildAppBar(HomeState state) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
      child: Row(
        children: [
          // Avatar
          GestureDetector(
            onTap: () {
              // TODO: Navigate to profile
            },
            child: Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                border: Border.all(color: AppColors.primary, width: 2),
              ),
              child: ClipOval(
                child: state.userStats?.avatarUrl != null
                    ? Image.network(
                        state.userStats!.avatarUrl!,
                        fit: BoxFit.cover,
                      )
                    : Container(
                        color: AppColors.primary.withAlpha(30),
                        child: const Icon(
                          Icons.person,
                          color: AppColors.primary,
                          size: 24,
                        ),
                      ),
              ),
            ),
          ),

          // Boşluk - ortada hiçbir şey yok, minimal tasarım
          const Spacer(),

          // Notification button
          GestureDetector(
            onTap: () {
              // TODO: Navigate to notifications
            },
            child: Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: AppColors.surface,
                shape: BoxShape.circle,
                border: Border.all(color: AppColors.divider),
              ),
              child: Stack(
                children: [
                  const Center(
                    child: Icon(
                      Icons.notifications_outlined,
                      color: AppColors.textSecondary,
                      size: 24,
                    ),
                  ),
                  // Bildirim badge'i
                  if ((state.homeData?.unreadNotificationCount ?? 0) > 0)
                    Positioned(
                      top: 8,
                      right: 8,
                      child: Container(
                        width: 10,
                        height: 10,
                        decoration: const BoxDecoration(
                          color: AppColors.error,
                          shape: BoxShape.circle,
                        ),
                      ),
                    ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildContent(HomeState state) {
    final userStats = state.userStats;
    final homeData = state.homeData;

    if (userStats == null) {
      return const Center(child: Text('Veri yükleniyor...'));
    }

    return RefreshIndicator(
      onRefresh: () async {
        ref.read(homeScreenProvider.notifier).refresh();
      },
      color: AppColors.primary,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(20, 8, 20, 100),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Streak Card (Hero)
            StreakCard(
              streakCount: userStats.currentStreak,
              currentSteps: userStats.todaySteps,
              freezeCount: userStats.freezeCount,
              onTap: () => _showStreakDetail(userStats),
            ),

            const SizedBox(height: 24),

            // Bölüm 1: Aktiviteler
            _buildSection(
              title: 'Aktiviteler',
              icon: Icons.assignment_outlined,
              children: [
                TasksCard(
                  tasks: homeData?.displayTasks ?? [],
                  onTap: () {
                    // TODO: Navigate to tasks
                  },
                ),
                GoalsCard(
                  goals: homeData?.activeGoals ?? [],
                  onTap: () {
                    // TODO: Navigate to goals
                  },
                ),
              ],
            ),

            const SizedBox(height: 20),

            // Bölüm 2: Rekabet
            _buildSection(
              title: 'Rekabet',
              icon: Icons.emoji_events_outlined,
              children: [
                if (homeData?.leagueCard != null)
                  LeagueCard(
                    league: homeData!.leagueCard!,
                    onTap: () {
                      // TODO: Navigate to league
                    },
                  )
                else
                  _buildEmptyLeagueCard(),
                if (homeData?.activeDuel != null)
                  DuelCard(
                    duel: homeData!.activeDuel!,
                    onTap: () {
                      // TODO: Navigate to duel
                    },
                  )
                else
                  _buildEmptyDuelCard(),
              ],
            ),

            const SizedBox(height: 20),

            // Bölüm 3: Sosyal
            _buildSection(
              title: 'Sosyal',
              icon: Icons.group_outlined,
              children: [
                if (homeData?.activePartnerMission != null)
                  PartnerMissionCard(
                    mission: homeData!.activePartnerMission!,
                    onTap: () {
                      // TODO: Navigate to partner mission
                    },
                  )
                else
                  _buildEmptyPartnerCard(),
                if (homeData?.activeGlobalMission != null)
                  GlobalMissionCard(
                    mission: homeData!.activeGlobalMission!,
                    onTap: () {
                      // TODO: Navigate to global mission
                    },
                  )
                else
                  _buildEmptyGlobalCard(),
              ],
            ),
          ],
        ),
      ),
    );
  }

  /// Yatay kaydırmalı bölüm
  Widget _buildSection({
    required String title,
    required IconData icon,
    required List<Widget> children,
  }) {
    final pageController = PageController(viewportFraction: 0.92);
    
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Bölüm başlığı + ok ikonları
        Row(
          children: [
            Icon(icon, size: 20, color: AppColors.textSecondary),
            const SizedBox(width: 8),
            Text(
              title,
              style: AppTypography.titleMedium.copyWith(
                color: AppColors.textSecondary,
                fontWeight: FontWeight.w600,
              ),
            ),
          ],
        ),
        const SizedBox(height: 12),
        
        // Yatay kaydırmalı kartlar
        SizedBox(
          height: 140,
          child: PageView.builder(
            controller: pageController,
            itemCount: children.length,
            itemBuilder: (context, index) {
              return Padding(
                padding: EdgeInsets.only(
                  right: index < children.length - 1 ? 12 : 0,
                ),
                child: children[index],
              );
            },
          ),
        ),
      ],
    );
  }

  void _showStreakDetail(userStats) {
    showStreakDetail(
      context,
      currentStreak: userStats.currentStreak,
      longestStreak: userStats.longestStreak,
      todaySteps: userStats.todaySteps,
      freezeCount: userStats.freezeCount,
    );
  }

  Widget _buildEmptyLeagueCard() {
    return EmptyCard(
      icon: Icons.emoji_events,
      accentColor: AppColors.accentLeague,
      title: 'Lig Durumu',
      message: 'Henüz bir lige katılmadın.',
    );
  }

  Widget _buildEmptyDuelCard() {
    return EmptyCard(
      icon: Icons.sports_martial_arts,
      accentColor: AppColors.accentDuels,
      title: 'Düello',
      message: 'Aktif düello yok.',
    );
  }

  Widget _buildEmptyPartnerCard() {
    return EmptyCard(
      icon: Icons.handshake,
      accentColor: AppColors.accentPartner,
      title: 'Ortak Görev',
      message: 'Partnerinle birlikte başar!',
    );
  }

  Widget _buildEmptyGlobalCard() {
    return EmptyCard(
      icon: Icons.public,
      accentColor: AppColors.accentGlobal,
      title: 'Dünya Görevi',
      message: 'Dünya geneli görevlere katıl.',
    );
  }

  Widget _buildErrorState(String error) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.error_outline,
              size: 64,
              color: AppColors.error,
            ),
            const SizedBox(height: 16),
            Text(
              'Bir hata oluştu',
              style: AppTypography.titleLarge.copyWith(
                color: AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              error,
              style: AppTypography.bodyMedium.copyWith(
                color: AppColors.textSecondary,
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: () {
                ref.read(homeScreenProvider.notifier).refresh();
              },
              child: const Text('Tekrar Dene'),
            ),
          ],
        ),
      ),
    );
  }
}


