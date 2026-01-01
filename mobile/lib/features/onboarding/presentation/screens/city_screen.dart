/// Şehir Seçim Ekranı - Soru 3/9
import 'package:flutter/material.dart';
import 'package:healthverse_app/app/theme/app_colors.dart';
import 'package:healthverse_app/features/onboarding/presentation/screens/employment_screen.dart';
import 'package:healthverse_app/features/onboarding/presentation/state/onboarding_state.dart';
import 'package:healthverse_app/features/onboarding/presentation/widgets/onboarding_scaffold.dart';

class CityScreen extends StatefulWidget {
  final OnboardingStateNotifier stateNotifier;
  
  const CityScreen({super.key, required this.stateNotifier});

  @override
  State<CityScreen> createState() => _CityScreenState();
}

class _CityScreenState extends State<CityScreen> {
  String? _selectedCity;
  bool _skipSelected = false;
  String _searchQuery = '';
  final TextEditingController _searchController = TextEditingController();

  // Türkiye'nin 81 ili (alfabetik)
  static const List<String> _cities = [
    'Adana', 'Adıyaman', 'Afyonkarahisar', 'Ağrı', 'Aksaray', 'Amasya', 
    'Ankara', 'Antalya', 'Ardahan', 'Artvin', 'Aydın', 'Balıkesir', 
    'Bartın', 'Batman', 'Bayburt', 'Bilecik', 'Bingöl', 'Bitlis', 
    'Bolu', 'Burdur', 'Bursa', 'Çanakkale', 'Çankırı', 'Çorum', 
    'Denizli', 'Diyarbakır', 'Düzce', 'Edirne', 'Elazığ', 'Erzincan', 
    'Erzurum', 'Eskişehir', 'Gaziantep', 'Giresun', 'Gümüşhane', 
    'Hakkari', 'Hatay', 'Iğdır', 'Isparta', 'İstanbul', 'İzmir', 
    'Kahramanmaraş', 'Karabük', 'Karaman', 'Kars', 'Kastamonu', 
    'Kayseri', 'Kırıkkale', 'Kırklareli', 'Kırşehir', 'Kilis', 
    'Kocaeli', 'Konya', 'Kütahya', 'Malatya', 'Manisa', 'Mardin', 
    'Mersin', 'Muğla', 'Muş', 'Nevşehir', 'Niğde', 'Ordu', 'Osmaniye', 
    'Rize', 'Sakarya', 'Samsun', 'Siirt', 'Sinop', 'Sivas', 'Şanlıurfa', 
    'Şırnak', 'Tekirdağ', 'Tokat', 'Trabzon', 'Tunceli', 'Uşak', 'Van', 
    'Yalova', 'Yozgat', 'Zonguldak',
  ];

  List<String> get _filteredCities {
    if (_searchQuery.isEmpty) return _cities;
    return _cities
        .where((city) => city.toLowerCase().contains(_searchQuery.toLowerCase()))
        .toList();
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return OnboardingScaffold(
      title: 'Hangi şehirde yaşıyorsun?',
      currentStep: 3,
      totalSteps: 9,
      isNextEnabled: _selectedCity != null || _skipSelected,
      onNext: _handleNext,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Arama kutusu
          TextField(
            controller: _searchController,
            onChanged: (value) => setState(() => _searchQuery = value),
            decoration: InputDecoration(
              hintText: 'Şehir ara...',
              hintStyle: TextStyle(color: AppColors.textHint),
              prefixIcon: Icon(Icons.search, color: AppColors.textTertiary),
              filled: true,
              fillColor: Colors.white,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: BorderSide(color: AppColors.divider),
              ),
              enabledBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: BorderSide(color: AppColors.divider),
              ),
              focusedBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: BorderSide(color: AppColors.primary, width: 2),
              ),
            ),
          ),
          const SizedBox(height: 16),

          // Seçili şehir gösterimi
          if (_selectedCity != null) ...[
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: AppColors.primary.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Row(
                children: [
                  Icon(Icons.location_on, color: AppColors.primary, size: 20),
                  const SizedBox(width: 8),
                  Text(
                    _selectedCity!,
                    style: TextStyle(
                      fontWeight: FontWeight.w600,
                      color: AppColors.primary,
                    ),
                  ),
                  const Spacer(),
                  GestureDetector(
                    onTap: () => setState(() => _selectedCity = null),
                    child: Icon(Icons.close, color: AppColors.primary, size: 20),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
          ],

          // Şehir listesi
          Container(
            height: 200,
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppColors.divider),
            ),
            child: ListView.builder(
              itemCount: _filteredCities.length,
              itemBuilder: (context, index) {
                final city = _filteredCities[index];
                final isSelected = city == _selectedCity;
                return ListTile(
                  title: Text(
                    city,
                    style: TextStyle(
                      fontWeight: isSelected ? FontWeight.w600 : FontWeight.w400,
                      color: isSelected ? AppColors.primary : AppColors.textPrimary,
                    ),
                  ),
                  trailing: isSelected 
                      ? Icon(Icons.check, color: AppColors.primary)
                      : null,
                  onTap: () {
                    setState(() {
                      _selectedCity = city;
                      _skipSelected = false;
                    });
                  },
                );
              },
            ),
          ),
          const SizedBox(height: 16),

          // Belirtmek istemiyorum
          GestureDetector(
            onTap: () {
              setState(() {
                _skipSelected = !_skipSelected;
                if (_skipSelected) _selectedCity = null;
              });
            },
            child: Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: _skipSelected 
                    ? AppColors.divider.withOpacity(0.3) 
                    : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: _skipSelected ? AppColors.textTertiary : AppColors.divider,
                ),
              ),
              child: Row(
                children: [
                  Icon(
                    _skipSelected ? Icons.check_box : Icons.check_box_outline_blank,
                    color: _skipSelected ? AppColors.textTertiary : AppColors.divider,
                  ),
                  const SizedBox(width: 12),
                  Text(
                    'Belirtmek istemiyorum',
                    style: TextStyle(
                      fontSize: 16,
                      color: AppColors.textTertiary,
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

  void _handleNext() {
    widget.stateNotifier.updateCity(
      _selectedCity,
      isSkipped: _skipSelected,
    );
    
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => EmploymentScreen(stateNotifier: widget.stateNotifier),
      ),
    );
  }
}
