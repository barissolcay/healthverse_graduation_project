/// API sabitleri
class ApiConstants {
  ApiConstants._();

  // Development için localhost
  // Android Emulator: 10.0.2.2
  // iOS Simulator: localhost
  // Gerçek cihaz: bilgisayarın IP adresi
  static const String baseUrl = 'http://10.0.2.2:5000'; // Android Emulator

  // Endpoints
  static const String healthSync = '/api/health/sync';
  static const String authRegister = '/api/auth/register';
  static const String authLogin = '/api/auth/login';
  static const String authMe = '/api/auth/me';

  // Development endpoints
  static const String devRegister = '/api/auth/dev-register';
  static const String devLogin = '/api/auth/dev-login';
}
