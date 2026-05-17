using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FRAProject.Services
{
    /// <summary>
    /// Reusable uniqueness validation — injectable into any controller.
    ///
    /// Usage (Create):
    ///   await _validator.CheckUniqueAsync&lt;AircraftVersion&gt;(ModelState, excludeId: null,
    ///       new UniqueField&lt;AircraftVersion&gt;(x => x.Code == upper,
    ///           nameof(dto.Code), $"Le code «{upper}» est déjà utilisé."),
    ///       new UniqueField&lt;AircraftVersion&gt;(x => x.Name == name,
    ///           nameof(dto.Name), $"Le nom «{name}» est déjà utilisé.")
    ///   );
    ///
    /// Usage (Edit — same call, just pass the id):
    ///   await _validator.CheckUniqueAsync&lt;AircraftVersion&gt;(ModelState, excludeId: id, ...);
    /// </summary>
    public interface IValidationService
    {
        Task CheckUniqueAsync<T>(
            ModelStateDictionary     modelState,
            int?                     excludeId,
            params UniqueField<T>[]  fields
        ) where T : class;
    }

    /// <summary>
    /// One field descriptor — fully Expression-based so EF
    /// translates the WHERE clause entirely to SQL.
    /// </summary>
    public class UniqueField<T> where T : class
    {
        /// <summary>SQL WHERE predicate — e.g. x => x.Code == value</summary>
        public Expression<Func<T, bool>> Predicate     { get; }

        /// <summary>ModelState key — use nameof(dto.FieldName)</summary>
        public string                    ModelStateKey  { get; }

        /// <summary>French error message shown under the field</summary>
        public string                    ErrorMessage   { get; }

        public UniqueField(
            Expression<Func<T, bool>> predicate,
            string                    modelStateKey,
            string                    errorMessage)
        {
            Predicate    = predicate;
            ModelStateKey = modelStateKey;
            ErrorMessage  = errorMessage;
        }
    }
}
