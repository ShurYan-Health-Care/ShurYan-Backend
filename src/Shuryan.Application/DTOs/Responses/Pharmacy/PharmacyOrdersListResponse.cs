using Shuryan.Application.DTOs.Common.Pagination;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrdersListResponse : PaginatedResponse<PharmacyOrderResponse>
    {
        // الـ class بيورث من PaginatedResponse فمش محتاج properties إضافية
        // كل الـ pagination metadata موجودة في الـ base class
    }
}
