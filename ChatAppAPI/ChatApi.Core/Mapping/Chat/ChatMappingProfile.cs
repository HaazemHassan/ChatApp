using AutoMapper;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using ChatApi.Core.Features.Chat.Commands.Responses;
using ChatApi.Core.Features.Chat.Queries.Responses;

namespace ChatApi.Core.Mapping.Chat {
    public class ChatMappingProfile : Profile {
        public ChatMappingProfile() {
            CreateMap<CreateConversationCommand, Conversation>();
            CreateMap<SendMessageCommand, Message>();

            // Entity to Command Response mappings
            CreateMap<Conversation, CreateConversationResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<Message, SendMessageResponse>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender != null ? src.Sender.UserName : ""))
                .ForMember(dest => dest.SenderFullName, opt => opt.MapFrom(src => src.Sender != null ? src.Sender.FullName : ""));



            // Entity to Query Response mappings
            CreateMap<Conversation, GetUserConversationsResponse>()
                .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => 
                    src.Participants.Any(p => p.User.IsOnline)));
            //.ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : ""))
            //.ForMember(dest => dest.UnreadCount, opt => opt.Ignore())
            //.ForMember(dest => dest.LastMessage, opt => opt.Ignore());

            CreateMap<Conversation, GetConversationByIdResponse>();
            //.ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : ""))
            //.ForMember(dest => dest.UnreadCount, opt => opt.Ignore());

            CreateMap<ConversationParticipant, ConversationParticipantResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => src.User.IsOnline));

            CreateMap<Conversation, GetNewConversationResponse>();


            CreateMap<Message, MessageResponse>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.UserName))
                .ForMember(dest => dest.SenderFullName, opt => opt.MapFrom(src => src.Sender.FullName));
            //.ForMember(dest => dest.IsRead, opt => opt.Ignore())
            //.ForMember(dest => dest.Replies, opt => opt.Ignore());

            CreateMap<TypingIndicator, GetTypingIndicatorsResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName));
        }
    }
}