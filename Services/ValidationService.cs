using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;

namespace FRAProject.Services
{
    /// <summary>
    /// Runs uniqueness checks fully in SQL — no in-memory loading.
    ///
    /// For Edit, it combines the caller's predicate with an automatic
    /// "AND Id != excludeId" using Expression tree composition,
    /// so everything runs as one SQL query per field.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly FRAContext _context;

        public ValidationService(FRAContext context)
        {
            _context = context;
        }

        public async Task CheckUniqueAsync<T>(
            ModelStateDictionary     modelState,
            int?                     excludeId,
            params UniqueField<T>[]  fields
        ) where T : class
        {
            var dbSet = _context.Set<T>().AsNoTracking();

            foreach (var field in fields)
            {
                // Build the final predicate:
                //   Create → caller's predicate only
                //   Edit   → caller's predicate AND Id != excludeId
                var predicate = excludeId.HasValue
                    ? CombineWithIdExclusion(field.Predicate, excludeId.Value)
                    : field.Predicate;

                var exists = await dbSet.AnyAsync(predicate);

                if (exists)
                    modelState.AddModelError(field.ModelStateKey, field.ErrorMessage);
            }
        }

        // ── Expression tree helper ────────────────────────────────────────
        //
        // Takes:   x => x.Code == "F5E"          (caller's predicate)
        // Builds:  x => x.Code == "F5E" && x.Id != 7   (for Edit)
        //
        // Both sides share the same parameter (x) so EF can translate
        // the combined expression to a single SQL WHERE clause.
        //
        private static Expression<Func<T, bool>> CombineWithIdExclusion<T>(
            Expression<Func<T, bool>> predicate,
            int                       excludeId)
        {
            // Reuse the same parameter 'x' from the caller's expression
            var param = predicate.Parameters[0];

            // Build:  x.Id != excludeId
            var idProperty = Expression.Property(param, "Id");
            var idValue    = Expression.Constant(excludeId);
            var idCheck    = Expression.NotEqual(idProperty, idValue);

            // Combine: (caller's body) && (x.Id != excludeId)
            var combined = Expression.AndAlso(predicate.Body, idCheck);

            return Expression.Lambda<Func<T, bool>>(combined, param);
        }
    }
}
