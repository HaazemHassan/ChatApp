using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace ChatApi.Services.Services;

public class TokenService : ITokenService {

    private readonly IHostEnvironment _environment;
    private readonly JwtSettings _jwtSettings;
    public TokenService(IHostEnvironment environment, JwtSettings jwtSettings) {
        _environment = environment;
        _jwtSettings = jwtSettings;
    }
    public CookieOptions GetRefreshTokenCookieOptions() {
        return new CookieOptions {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/api/authentication",
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };
    }
}
