using ChatApi.Core.Bases;
using ChatApi.Core.Bases.Authentication;
using ChatApi.Core.Entities.IdentityEntities;
using System.Security.Claims;

namespace ChatApi.Core.Abstracts.ServicesContracts {
    public interface IAuthenticationService {
        public Task<JwtResult?> AuthenticateAsync(ApplicationUser user, DateTime? refreshTokenExpDate = null);
        public ClaimsPrincipal? GetPrincipalFromAcessToken(string token, bool validateLifetime = true);
        public Task<JwtResult?> ReAuthenticateAsync(string refreshToken, string accessToken);
        public Task<ServiceOperationResult<JwtResult?>> GoogleAuthenticateAsync(string idToken);
        //public Task<ServiceOperationResult> SendResetPasswordCodeAsync(string email);
        //public Task<JwtResult?> VerifyResetPasswordCodeAsync(string email, string code);
    }
}
