using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Infrastructure.Repositories {
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository {

        private readonly DbSet<RefreshToken> _refreshTokens;


        public RefreshTokenRepository(AppDbContext context) : base(context) {
            _refreshTokens = context.Set<RefreshToken>();
        }

    }
}
