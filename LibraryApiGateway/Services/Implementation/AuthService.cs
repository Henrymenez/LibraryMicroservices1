//using LibraryApiGateway.Models;
//using Microsoft.IdentityModel.Tokens;
//using System.Security.Claims;
//using System.Text;

//namespace LibraryApiGateway.Services.Implementation
//{
//    public class AuthService
//    {
//        private readonly IConfiguration _configuration;
//        private readonly ILogger<AuthService> _logger;
//        private readonly HttpClient _httpClient;

//        // In-memory storage for demo purposes - use Redis or database in production
//        private static readonly Dictionary<string, RefreshTokenData> _refreshTokens = new();

//        public AuthService(IConfiguration configuration, ILogger<AuthService> logger, HttpClient httpClient)
//        {
//            _configuration = configuration;
//            _logger = logger;
//            _httpClient = httpClient;
//        }

//        public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
//        {
//            try
//            {
//                // In a real implementation, you would validate against a user service or database
//                // For demo purposes, we'll use hardcoded users
//                var user = ValidateUser(request.Email, request.Password);
//                if (user == null)
//                    return null;

//                var token = GenerateJwtToken(user);
//                var refreshToken = GenerateRefreshToken();

//                _refreshTokens[refreshToken] = new RefreshTokenData
//                {
//                    UserId = user.Id,
//                    ExpiresAt = DateTime.UtcNow.AddDays(7)
//                };

//                return new LoginResponse
//                {
//                    Token = token,
//                    RefreshToken = refreshToken,
//                    ExpiresAt = DateTime.UtcNow.AddHours(1),
//                    User = user
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Authentication failed for user {Email}", request.Email);
//                return null;
//            }
//        }

//        public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
//        {
//            if (!_refreshTokens.TryGetValue(request.RefreshToken, out var tokenData) ||
//                tokenData.ExpiresAt < DateTime.UtcNow)
//            {
//                return null;
//            }

//            // Get user info (in real implementation, fetch from user service)
//            var user = GetUserById(tokenData.UserId);
//            if (user == null)
//                return null;

//            var newToken = GenerateJwtToken(user);
//            var newRefreshToken = GenerateRefreshToken();

//            // Remove old refresh token and add new one
//            _refreshTokens.Remove(request.RefreshToken);
//            _refreshTokens[newRefreshToken] = new RefreshTokenData
//            {
//                UserId = user.Id,
//                ExpiresAt = DateTime.UtcNow.AddDays(7)
//            };

//            return new LoginResponse
//            {
//                Token = newToken,
//                RefreshToken = newRefreshToken,
//                ExpiresAt = DateTime.UtcNow.AddHours(1),
//                User = user
//            };
//        }

//        public string GenerateJwtToken(UserInfo user)
//        {
//            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new[]
//                {
//                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//                    new Claim(ClaimTypes.Email, user.Email),
//                    new Claim(ClaimTypes.# Library Microservices Solutio
//    }
//}
