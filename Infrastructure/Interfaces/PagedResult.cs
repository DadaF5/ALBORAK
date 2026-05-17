namespace FRAProject.Infrastructure.Interfaces
{
    /// <summary>
    /// Return type for IGenericRepository.GetPagedAsync().
    /// Carries the current page of items plus paging metadata.
    ///
    /// Place this file in:
    ///   Infrastructure/Interfaces/PagedResult.cs
    ///
    /// Used by every controller Index action to build the IndexVm.
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>Current page of entities.</summary>
        public List<T> Items { get; set; } = [];

        /// <summary>Total rows matching the filter — before paging.</summary>
        public int TotalCount { get; set; }

        /// <summary>Total number of pages = ceiling(TotalCount / PageSize).</summary>
        public int TotalPages { get; set; }
    }
}
