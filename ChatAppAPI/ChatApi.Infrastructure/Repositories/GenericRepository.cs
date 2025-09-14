using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace ChatApi.Infrastructure.Repositories {
    public class GenericRepository<T> : IGenericRepository<T> where T : class {


        protected readonly AppDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext dbContext) {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }


        public virtual async Task<T?> GetByIdAsync(int id) {
            //return await _dbSet.FindAsync(id);
            return await _dbContext.Set<T>().FindAsync(id);
        }


        public IQueryable<T> GetTableNoTracking(Expression<Func<T, bool>>? predicate = null) {
            var query = _dbContext.Set<T>().AsNoTracking();

            return predicate is not null ? query.Where(predicate) : query;
        }


        public virtual async Task AddRangeAsync(ICollection<T> entities) {
            await _dbContext.Set<T>().AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();

        }
        public virtual async Task<T> AddAsync(T entity) {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public virtual async Task UpdateAsync(T entity) {
            _dbContext.Set<T>().Update(entity);
            await _dbContext.SaveChangesAsync();

        }

        public virtual async Task DeleteAsync(T entity) {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        public virtual async Task DeleteRangeAsync(ICollection<T> entities) {
            foreach (var entity in entities) {
                _dbContext.Entry(entity).State = EntityState.Deleted;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) {

            return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync() {
            await _dbContext.Database.CommitTransactionAsync();

        }

        public async Task RollBackAsync() {
            await _dbContext.Database.RollbackTransactionAsync();

        }

        public IQueryable<T> GetTableAsTracking(Expression<Func<T, bool>>? predicate = null) {
            var query = _dbContext.Set<T>();
            return predicate is not null ? query.Where(predicate) : query;

        }

        public virtual async Task UpdateRangeAsync(ICollection<T> entities) {
            _dbContext.Set<T>().UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate) {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) {
            return await _dbContext.Set<T>().AnyAsync(predicate);
        }
    }
}
