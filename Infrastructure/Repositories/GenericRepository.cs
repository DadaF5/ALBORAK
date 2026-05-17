using FRAProject.Data;
using FRAProject.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FRAProject.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly FRAContext _context;

        public GenericRepository(FRAContext context)
        {
            _context = context;
        }

        // ════════════════════════════════════════════════════════════════
        //  EXISTING METHODS — code unchanged, only nullable T? added
        //  where compiler requires it (GetByIdAsync returns null when
        //  not found — T? makes that explicit and suppresses warnings)
        // ════════════════════════════════════════════════════════════════

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task<bool> ExistsAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }

        // ════════════════════════════════════════════════════════════════
        //  ADDED METHODS — implementations for new interface members
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Same as FindAsync — added so controllers can call
        /// GetWhereAsync() which is the name used throughout the codebase.
        /// Both methods do exactly the same thing.
        /// </summary>
        public async Task<IEnumerable<T>> GetWhereAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>()
                .Where(predicate)
                .ToListAsync();
        }

        /// <summary>
        /// Returns first matching entity or null.
        /// Used to check for an existing active document upload before
        /// saving a new one (soft-deletes the old one first).
        /// </summary>
        public async Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>()
                .Where(predicate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Paged, sorted and filtered query.
        /// Called by every Index action in every controller.
        ///
        /// filter    — WHERE clause (null = no filter)
        /// orderBy   — ORDER BY clause (null = no sort)
        /// pageNumber — 1-based page number
        /// pageSize   — rows per page
        ///
        /// Returns PagedResult containing:
        ///   Items      — the current page of entities
        ///   TotalCount — total rows matching filter
        ///   TotalPages — ceiling(TotalCount / pageSize)
        /// </summary>
        public async Task<PagedResult<T>> GetPagedAsync(
            Expression<Func<T, bool>>? filter,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
            int pageNumber,
            int pageSize)
        {
            // Start with full set
            IQueryable<T> query = _context.Set<T>();

            // Apply filter if provided
            if (filter != null)
                query = query.Where(filter);

            // Count BEFORE paging — needed for TotalPages
            var totalCount = await query.CountAsync();

            // Apply sort if provided
            if (orderBy != null)
                query = orderBy(query);

            // Apply paging
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        /// <summary>
        /// Same as ExistsAsync — added so controllers can call AnyAsync()
        /// which is the name used throughout the codebase.
        /// Both methods do exactly the same thing.
        /// </summary>
        public async Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }

        /// <summary>
        /// Count rows matching predicate.
        /// Used in DossierService.SubmitAsync() to generate the
        /// sequential DossierNumber (DAM-IMMAT-2026-0001).
        /// </summary>
        public async Task<int> CountAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().CountAsync(predicate);
        }

        /// <summary>
        /// Sync add — stages entity for insert without awaiting.
        /// Controller calls Add() then UnitOfWork.CompleteAsync()
        /// to commit. This is the pattern used throughout:
        ///   _uow.Countries.Add(entity);
        ///   await _uow.CompleteAsync();
        /// </summary>
        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }
    }
}