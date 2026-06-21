using System.Linq.Expressions;

namespace SportCourtManagerment.Repositories.Interfaces;

/// <summary>
/// Generic repository providing standard CRUD operations over any entity type.
/// Returns IQueryable to allow OData and custom downstream filtering.
/// </summary>
public interface IGenericRepository<T> where T : class
{
  /// <summary>Returns a single entity by primary key, or null.</summary>
  Task<T?> GetByIdAsync(int id);

  /// <summary>Returns an un-materialized queryable for advanced filtering (OData, etc.).</summary>
  IQueryable<T> GetAll();

  /// <summary>Returns an un-materialized queryable filtered by a predicate.</summary>
  IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);

  /// <summary>Adds a new entity to the context.</summary>
  Task AddAsync(T entity);

  /// <summary>Marks an entity as modified.</summary>
  void Update(T entity);

  /// <summary>Marks an entity for deletion.</summary>
  void Delete(T entity);

  /// <summary>Persists all pending changes to the database.</summary>
  Task<int> SaveChangesAsync();
}
