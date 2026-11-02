using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    public Repository(AppDbContext context)
    {
        Context = context;
    }

    protected AppDbContext Context { get; }

    protected DbSet<TEntity> Entities => Context.Set<TEntity>();

    // Tracked on purpose: callers load an entity by id to update or delete it.
    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Entities.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken)
    {
        return await Entities.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await Entities.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        Entities.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        Entities.Remove(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }
}
