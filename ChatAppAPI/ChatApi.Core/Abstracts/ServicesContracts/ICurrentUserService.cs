using ChatApi.Core.Entities.IdentityEntities;

namespace ChatApi.Core.Abstracts.ServicesContracts {
    public interface ICurrentUserService {
        int? UserId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
        Task<ApplicationUser?> GetCurrentUserAsync();
        Task<bool> IsOnline();
    }
}