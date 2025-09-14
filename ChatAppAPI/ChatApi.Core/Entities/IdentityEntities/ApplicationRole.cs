using Microsoft.AspNetCore.Identity;

namespace ChatApi.Core.Entities.IdentityEntities {
    public class ApplicationRole : IdentityRole<int> {
        public ApplicationRole() {

        }
        public ApplicationRole(string role) {
            Name = role;
        }
    }
}
