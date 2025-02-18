namespace MovieBot.Data.IRepositories;

public interface IRepository<TEntity>
{
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task<bool> RemoveAsync(long id);
    Task<TEntity> GetByIdAsync(long id);
    IQueryable<TEntity> GetAllAsync();
}
