using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Connection.Commands.RequestsModels {
    public class RemoveUserConnectionCommand : IRequest<Response<string>> {
        public string ConnectionId { get; set; }
    }
}