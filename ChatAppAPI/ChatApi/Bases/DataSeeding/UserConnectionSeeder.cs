using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Bases.DataSeeding {
    public static class UserConnectionsRemover {
        public static async Task RemoveAsync(IUserConnectionRepository _userConnectionRepository) {
            var allConnections = await _userConnectionRepository.GetTableNoTracking().ToListAsync();

            if (allConnections.Any()) {
                await _userConnectionRepository.DeleteRangeAsync(allConnections);
            }
        }
    }
}