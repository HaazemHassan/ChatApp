using ChatApi.Core.Bases;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums;
using System.Linq.Expressions;

namespace ChatApi.Core.Abstracts.ServicesContracts {
    public interface IApplicationUserService {
        public Task<bool> IsUserExist(Expression<Func<ApplicationUser, bool>> predicate);
        public Task<ServiceOperationResult<string?>> AddApplicationUser(ApplicationUser user, string password);
        //public Task<bool> SendConfirmationEmailAsync(ApplicationUser user);
        public Task<ServiceOperationResult<string?>> ConfirmEmailAsync(int userId, string code);
        public Task<ServiceOperationResult<string?>> ResetPasswordAsync(ApplicationUser user, string newPassword);
        public Task<string?> GetFullName(int userId);
        public Task<ApplicationUser?> GetByUsernameAsync(string username);
        public Task<List<ApplicationUser>> SearchUsersByUsernameAsync(string username);
    }
}
