using ChatApi.Core.Entities.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ChatApi.Bases.DataSeeding {
    public static class ApplicationRoleSeeder {
        public static async Task SeedAsync(RoleManager<ApplicationRole> _roleManager) {
            string rolesJson = await File.ReadAllTextAsync("ApplicationRoles.json");
            List<string>? roles = JsonSerializer.Deserialize<List<string>>(rolesJson);

            int rolesInDb = await _roleManager.Roles.CountAsync();
            if (roles is null || rolesInDb > 0)
                return;

            foreach (var role in roles) {
                await _roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        //public async Task SeedApplicationUsers()
        //{
        //    string rolesJson = await File.ReadAllTextAsync("ApplicationRoles.json");
        //    List<string>? roles = JsonSerializer.Deserialize<List<string>>(rolesJson);

        //    int rolesInDb = await _roleManager.Roles.CountAsync();
        //    if (roles is null || rolesInDb > 0)
        //        return;

        //    foreach (var role in roles)
        //    {
        //        await _roleManager.CreateAsync(new ApplicationRole(role));
        //    }
        //}
    }
}
