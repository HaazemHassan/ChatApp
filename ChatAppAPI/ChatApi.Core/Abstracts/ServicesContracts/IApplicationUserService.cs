using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums;
using System.Linq.Expressions;

namespace ChatApi.Core.Abstracts.ServicesContracts {
    public interface IApplicationUserService {
        public Task<bool> IsUserExist(Expression<Func<ApplicationUser, bool>> predicate);
        public Task<ServiceOperationStatus> AddApplicationUser(ApplicationUser user, string password);
        //public Task<bool> SendConfirmationEmailAsync(ApplicationUser user);
        public Task<ServiceOperationStatus> ConfirmEmailAsync(int userId, string code);
        public Task<ServiceOperationStatus> ResetPasswordAsync(ApplicationUser user, string newPassword);
        public Task<string?> GetFullName(int userId);
        public Task<ApplicationUser?> GetByUsernameAsync(string username);
        public Task<List<ApplicationUser>> SearchUsersByUsernameAsync(string username);
    }
}
