using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Response for prescription queries with pagination and metadata
    /// </summary>
    public class PrescriptionQueryResponse
    {
        public List<PrescriptionResponse> Prescriptions { get; set; } = new();
        
        // Pagination
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        // Applied filters (for debugging/transparency)
        public Dictionary<string, object?> AppliedFilters { get; set; } = new();
    }
}
