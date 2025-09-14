using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Connection.Commands.RequestsModels {
    public class JoinGroupCommand : IRequest<Response<string>> {
        public string ConnectionId { get; set; }
        public string GroupName { get; set; }
    }
}