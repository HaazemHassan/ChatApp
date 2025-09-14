using ChatApi.Core.Entities.ChatEntities;

namespace ChatApi.Core.Abstracts.InfrastructureAbstracts {
    public interface IUserConnectionRepository : IGenericRepository<UserConnection> {
        Task<IEnumerable<UserConnection>> GetUserConnectionsAsync(int userId);
        Task<UserConnection?> GetConnectionByIdAsync(string connectionId);
    }
}