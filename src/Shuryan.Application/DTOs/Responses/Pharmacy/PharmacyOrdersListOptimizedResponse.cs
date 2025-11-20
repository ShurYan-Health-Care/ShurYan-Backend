namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrdersListOptimizedResponse
    {
        public List<PharmacyOrderListItemResponse> Orders { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
