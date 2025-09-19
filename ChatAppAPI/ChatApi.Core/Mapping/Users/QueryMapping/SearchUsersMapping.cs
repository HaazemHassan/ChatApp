using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Features.Users.Queries.Responses;

namespace ChatApi.Core.Mapping.Users {
    public partial class UserProfile {
        public void SearchUsersMapping() {
            CreateMap<ApplicationUser, SearchUsersResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }
    }
}