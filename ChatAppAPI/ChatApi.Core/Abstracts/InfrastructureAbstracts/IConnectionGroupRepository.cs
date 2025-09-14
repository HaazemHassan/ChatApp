using ChatApi.Core.Entities.ChatEntities;

namespace ChatApi.Core.Abstracts.InfrastructureAbstracts {
    public interface IConnectionGroupRepository : IGenericRepository<ConnectionGroup> {
        Task<IEnumerable<ConnectionGroup>> GetGroupConnectionsAsync(string groupName);
    }
}