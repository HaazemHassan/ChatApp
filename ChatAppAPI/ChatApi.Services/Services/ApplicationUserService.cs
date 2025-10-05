using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
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

        public async Task<ServiceOperationResult<string?>> AddApplicationUser(ApplicationUser user, string password) {

            if (user is null || password is null)
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.InvalidParameters, "User or password is invalid");


            await using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
                try {
                    if (await IsUserExist(x => x.Email == user.Email || x.UserName == user.UserName))
                        return ServiceOperationResult<string>.Failure(ServiceOperationStatus.AlreadyExists, "Email or username already exists");

                    var createResult = await _userManager.CreateAsync(user, password);

                    if (!createResult.Succeeded)
                        return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to create user");

                    var addToRoleresult = await _userManager.AddToRoleAsync(user, ApplicationUserRole.User.ToString());
                    if (!addToRoleresult.Succeeded)
                        return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to assign user role");

                    //var succedded = await SendConfirmationEmailAsync(user);
                    //if (!succedded)
                    //    return ServiceOperationResult.Failed;
                    await transaction.CommitAsync();
                    return ServiceOperationResult<string>.Success("User created successfully");
                }
                catch (Exception ex) {
                    await transaction.RollbackAsync();
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, $"An error occurred: {ex.Message}");

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

        public async Task<ServiceOperationResult<string?>> ConfirmEmailAsync(int userId, string code) {
            if (code is null)
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.InvalidParameters, "Confirmation code is required");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "User not found");

            if (user.EmailConfirmed)
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Email already confirmed");

            var confirmEmail = await _userManager.ConfirmEmailAsync(user, code);
            return confirmEmail.Succeeded 
                ? ServiceOperationResult<string>.Success("Email confirmed successfully")
                : ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to confirm email");

        }

        public async Task<ServiceOperationResult<string?>> ResetPasswordAsync(ApplicationUser? user, string newPassword) {
            if (user is null || newPassword is null)
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.InvalidParameters, "User or password is invalid");

            await using var trans = await _dbContext.Database.BeginTransactionAsync();
            try {
                await _userManager.RemovePasswordAsync(user);
                var result = await _userManager.AddPasswordAsync(user, newPassword);
                if (!result.Succeeded) {
                    await trans.RollbackAsync();
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to reset password");
                }

                await trans.CommitAsync();
                return ServiceOperationResult<string>.Success("Password reset successfully");
            }
            catch (Exception ex) {
                await trans.RollbackAsync();
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, $"An error occurred: {ex.Message}");

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

        public async Task<List<ApplicationUser>> SearchUsersByUsernameAsync(string username) {
            try {
                var users = await _userManager.Users
                    .Where(u => u.UserName.Contains(username))
                    .OrderBy(u => u.UserName)
                    .Take(10)
                    .ToListAsync();

                return users;
            }
            catch (Exception) {
                return new List<ApplicationUser>();
            }
        }
    }
}
