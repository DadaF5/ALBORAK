using FRAProject.Data;
using System.Linq.Expressions;

namespace FRAProject.Infrastructure.Interfaces
{
    /// <summary>
    /// Generic repository interface.
    /// Provides standard data access operations for any entity type.
    ///
    /// Existing methods (unchanged):
    ///   GetByIdAsync, GetAllAsync, FindAsync, AddAsync,
    ///   Update, Delete, ExistsAsync
    ///
    /// Added methods (required by controllers and services):
    ///   GetWhereAsync       — alias for FindAsync (used throughout codebase)
    ///   GetFirstOrDefaultAsync — single entity by predicate
    ///   GetPagedAsync       — paged + sorted + filtered results
    ///   AnyAsync            — alias for ExistsAsync (used throughout codebase)
    ///   CountAsync          — count matching rows
    ///   Add                 — sync add (controllers use Add not AddAsync)
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        // ════════════════════════════════════════════════════════════════
        //  EXISTING METHODS — unchanged
        // ════════════════════════════════════════════════════════════════

        /// <summary>Find entity by primary key.</summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>Return all rows — use with caution on large tables.</summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>Return all rows matching predicate.</summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>Add entity asynchronously.</summary>
        Task AddAsync(T entity);

        /// <summary>Mark entity as modified — EF will UPDATE on SaveChanges.</summary>
        void Update(T entity);

        /// <summary>Hard delete — removes row from DB. Use soft delete (IsActive=false) instead where possible.</summary>
        void Delete(T entity);

        /// <summary>Returns true if any row matches predicate.</summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        // ════════════════════════════════════════════════════════════════
        //  ADDED METHODS — required by controllers and services
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Return all rows matching predicate.
        /// Same as FindAsync — added for consistency with codebase naming.
        /// Controllers and services call GetWhereAsync throughout.
        /// </summary>
        Task<IEnumerable<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Return first entity matching predicate, or null if none found.
        /// Used in UploadDocument to check for existing active upload.
        /// </summary>
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Return paged, sorted and filtered results.
        /// Used by all Index actions (search + sort + paging).
        /// </summary>
        Task<PagedResult<T>> GetPagedAsync(
            Expression<Func<T, bool>>? filter,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// Returns true if any row matches predicate.
        /// Same as ExistsAsync — added for consistency with codebase naming.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Count rows matching predicate.
        /// Used in DossierService to generate DossierNumber sequence.
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Add entity synchronously (stages for save — does not commit).
        /// Controllers call Add() not AddAsync() for consistency.
        /// Commit with UnitOfWork.CompleteAsync().
        /// </summary>
        void Add(T entity);
    }
}