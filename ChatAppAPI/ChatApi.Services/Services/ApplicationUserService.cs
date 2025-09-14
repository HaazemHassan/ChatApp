using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums;
using ChatApi.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ChatApi.Services.Services {
    public class ApplicationUserService : IApplicationUserService {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly IEmailService _emailsService;
        private readonly IUrlHelper _urlHelper;



        public ApplicationUserService(UserManager<ApplicationUser> userManager, AppDbContext dbContext) {
            _userManager = userManager;
            _dbContext = dbContext;
            //_httpContextAccessor = httpContextAccessor;
            //_emailsService = emailsService;
            //_urlHelper = urlHelper;
        }


        public async Task<bool> IsUserExist(Expression<Func<ApplicationUser, bool>> predicate) {
            var user = await _userManager.Users.FirstOrDefaultAsync(predicate);
            return user is null ? false : true;
        }

        public async Task<ServiceOperationStatus> AddApplicationUser(ApplicationUser user, string password) {

            if (user is null || password is null)
                return ServiceOperationStatus.InvalidParameters;


            await using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
                try {
                    if (await IsUserExist(x => x.Email == user.Email || x.UserName == user.UserName))
                        return ServiceOperationStatus.AlreadyExists;

                    var createResult = await _userManager.CreateAsync(user, password);

                    if (!createResult.Succeeded)
                        return ServiceOperationStatus.Failed;

                    var addToRoleresult = await _userManager.AddToRoleAsync(user, ApplicationUserRole.User.ToString());
                    if (!addToRoleresult.Succeeded)
                        return ServiceOperationStatus.Failed;

                    //var succedded = await SendConfirmationEmailAsync(user);
                    //if (!succedded)
                    //    return ServiceOperationResult.Failed;
                    await transaction.CommitAsync();
                    return ServiceOperationStatus.Succeeded;
                }
                catch (Exception) {
                    await transaction.RollbackAsync();
                    return ServiceOperationStatus.Failed;

                }
            }
        }

        //public async Task<bool> SendConfirmationEmailAsync(ApplicationUser user) {
        //    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var resquestAccessor = _httpContextAccessor.HttpContext.Request;
        //    var confrimEmailActionContext = new UrlActionContext {
        //        Action = "ConfirmEmail",
        //        Controller = "Authentication",
        //        Values = new { UserId = user.Id, Code = code }
        //    };
        //    var returnUrl = resquestAccessor.Scheme + "://" + resquestAccessor.Host + _urlHelper.Action(confrimEmailActionContext);
        //    var message = $"To Confirm Email Click Link: {returnUrl}";
        //    var sendResult = await _emailsService.SendEmail(user.Email, message, "Confirm email");
        //    return sendResult;
        //}

        public async Task<ServiceOperationStatus> ConfirmEmailAsync(int userId, string code) {
            if (code is null)
                return ServiceOperationStatus.InvalidParameters;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return ServiceOperationStatus.NotFound;

            if (user.EmailConfirmed)
                return ServiceOperationStatus.Failed;

            var confirmEmail = await _userManager.ConfirmEmailAsync(user, code);
            return confirmEmail.Succeeded ? ServiceOperationStatus.Succeeded : ServiceOperationStatus.Failed;

        }

        public async Task<ServiceOperationStatus> ResetPasswordAsync(ApplicationUser? user, string newPassword) {
            if (user is null || newPassword is null)
                return ServiceOperationStatus.InvalidParameters;

            await using var trans = await _dbContext.Database.BeginTransactionAsync();
            try {
                await _userManager.RemovePasswordAsync(user);
                var result = await _userManager.AddPasswordAsync(user, newPassword);
                if (!result.Succeeded) {
                    await trans.RollbackAsync();
                    return ServiceOperationStatus.Failed;
                }

                await trans.CommitAsync();
                return ServiceOperationStatus.Succeeded;
            }
            catch {
                await trans.RollbackAsync();
                return ServiceOperationStatus.Failed;

            }

        }


        public async Task<string?> GetFullName(int userId) {

            try {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return null;

                return user.FullName;
            }
            catch {
                return null;
            }

        }

        public async Task<ApplicationUser?> GetByUsernameAsync(string username) {
            return await _userManager.FindByNameAsync(username);
        }
    }
}
