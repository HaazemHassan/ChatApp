using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Features.Users.Queries.Responses;

namespace ChatApi.Core.Mapping.Users {
    public partial class UserProfile {
        public void GetUsersPaginatedMapping() {
            CreateMap<ApplicationUser, GetUsersPaginatedResponse>()
                .ForMember(dest => dest.Phone,
                   opt => opt.MapFrom(src => src.PhoneNumber));

        }
    }
}


