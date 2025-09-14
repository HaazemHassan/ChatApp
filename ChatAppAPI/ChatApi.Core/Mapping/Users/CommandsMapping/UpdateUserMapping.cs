//using School.Core.Features.User.Commands.Models;
//using School.Data.Entities.IdentityEntities;

//namespace School.Core.Mapping.User
//{
//    public partial class UserProfile
//    {
//        public void UpdateUserMapping()
//        {
//            CreateMap<UpdateUserCommand, ApplicationUser>()
//                .ForMember(dest => dest.Id, opt => opt.Ignore())
//                .ForSourceMember(src => src.Password, opt => opt.DoNotValidate())
//                .ForAllMembers(opts => opts.Condition((src, dest, srcMember)
//                 => !string.IsNullOrWhiteSpace(srcMember as string)));


//        }
//    }
//}
