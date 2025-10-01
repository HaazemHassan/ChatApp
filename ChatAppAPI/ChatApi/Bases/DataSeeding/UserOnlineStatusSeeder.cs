using ChatApi.Core.Entities.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Bases.DataSeeding {
    public static class UserOnlineStatusResetter {
        public static async Task ResetAsync(UserManager<ApplicationUser> _userManager) {
            var onlineUsers = await _userManager.Users.Where(u => u.IsOnline).ToListAsync();

            if (onlineUsers.Any()) {
                foreach (var user in onlineUsers) {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.UtcNow;
                }

                foreach (var user in onlineUsers) {
                    await _userManager.UpdateAsync(user);
                }
            }
        }
    }
}