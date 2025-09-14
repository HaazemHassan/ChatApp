using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Features.Users.Queries.Responses;

namespace ChatApi.Core.Mapping.Users {
    public partial class UserProfile {
        public void GetUserByIdMapping() {
            CreateMap<ApplicationUser, GetUserByIdResponse>();
            //.ForMember(dest => dest.Phone,
            //   opt => opt.MapFrom(src => src.PhoneNumber));

        }
    }
}