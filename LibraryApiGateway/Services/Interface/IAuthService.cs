using LibraryApiGateway.Models;

namespace LibraryApiGateway.Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResponse?> AuthenticateAsync(LoginRequest request);
        Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request);
        string GenerateJwtToken(UserInfo user);
        string GenerateRefreshToken();
    }
}
