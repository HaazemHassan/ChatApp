using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Connection.Commands.RequestsModels {
    public class AddUserConnectionCommand : IRequest<Response<string>> {
        public int UserId { get; set; }
        public string ConnectionId { get; set; }
    }
}