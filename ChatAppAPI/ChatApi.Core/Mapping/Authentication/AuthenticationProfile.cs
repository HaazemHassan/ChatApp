using AutoMapper;

namespace ChatApi.Core.Mapping {
    public partial class AuthenticationProfile : Profile {
        public AuthenticationProfile() {
            RegisterMapping();
        }
    }
}
