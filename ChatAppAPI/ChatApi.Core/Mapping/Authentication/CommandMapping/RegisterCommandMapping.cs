using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Features.Authentication.Commands.RequestsModels;

namespace ChatApi.Core.Mapping {
    public partial class AuthenticationProfile {
        public void RegisterMapping() {
            CreateMap<RegisterCommand, ApplicationUser>();

        }
    }
}
