using Microsoft.EntityFrameworkCore;
using MovieBot.Data.DbContexts;
using MovieBot.Data.IRepositories;
using MovieBot.Domain.Commons;

namespace MovieBot.Data.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : Auditable
{
    private readonly AppDbContext dbContext;
    private readonly DbSet<TEntity> dbSet;

    public Repository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
        this.dbSet = dbContext.Set<TEntity>();
    }
    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await this.dbSet.AddAsync(entity);
        await this.dbContext.SaveChangesAsync();

        return entity;
    }

    public IQueryable<TEntity> GetAllAsync()
        => this.dbSet;
    public async Task<TEntity> GetByIdAsync(long id)
    {
        var result = await this.dbSet.Where(e => e.Id == id).FirstOrDefaultAsync();
        return result;
    }

    public async Task<bool> RemoveAsync(long id)
    {
        var entity = await this.dbSet.Where(e => e.Id == id)
            .FirstOrDefaultAsync();
        this.dbSet.Remove(entity);
        await this.dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        var result = dbContext.Update(entity).Entity;
        await dbContext.SaveChangesAsync();
        return result;
    }
}
