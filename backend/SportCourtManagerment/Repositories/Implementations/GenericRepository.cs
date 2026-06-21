using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.Repositories.Interfaces;

namespace SportCourtManagerment.Repositories.Implementations;

/// <summary>
/// Generic EF Core repository implementation providing standard CRUD
/// operations against the <see cref="ApplicationDbContext"/>.
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
  protected readonly ApplicationDbContext _context;
  protected readonly DbSet<T> _dbSet;

  public GenericRepository(ApplicationDbContext context)
  {
    _context = context;
    _dbSet   = context.Set<T>();
  }

  /// <inheritdoc/>
  public virtual async Task<T?> GetByIdAsync(int id)
    => await _dbSet.FindAsync(id);

  /// <inheritdoc/>
  public virtual IQueryable<T> GetAll()
    => _dbSet.AsNoTracking();

  /// <inheritdoc/>
  public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
    => _dbSet.AsNoTracking().Where(predicate);

  /// <inheritdoc/>
  public virtual async Task AddAsync(T entity)
    => await _dbSet.AddAsync(entity);

  /// <inheritdoc/>
  public virtual void Update(T entity)
    => _dbSet.Update(entity);

  /// <inheritdoc/>
  public virtual void Delete(T entity)
    => _dbSet.Remove(entity);

  /// <inheritdoc/>
  public virtual async Task<int> SaveChangesAsync()
    => await _context.SaveChangesAsync();
}
