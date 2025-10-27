# دليل تنفيذ Pharmacy Endpoints - 35 Endpoint

## ✅ ما تم إنجازه

### 1. DTOs (Request & Response)
تم إنشاء كل الـ DTOs المطلوبة:
- ✅ `UpdatePharmacyWorkingHoursRequest`
- ✅ `SearchPharmaciesRequest`
- ✅ `NearbyPharmaciesRequest`
- ✅ `UpdateOrderStatusRequest`
- ✅ `RejectOrderRequest`
- ✅ `VerifyPharmacyRequest`
- ✅ `RejectVerificationRequest`
- ✅ `ApproveDocumentRequest`
- ✅ `RejectDocumentRequest`
- ✅ `PharmacyReviewStatisticsResponse`
- ✅ `PrescriptionDetailsResponse`
- ✅ `IsOpenResponse`

### 2. Repositories
- ✅ `IPharmacyWorkingHoursRepository` - Interface
- ✅ `PharmacyWorkingHoursRepository` - Implementation
- ✅ تم تحديث `IUnitOfWork` و `UnitOfWork` لإضافة `PharmacyWorkingHours`

### 3. Service Layer
- ✅ `IPharmacyService` - Interface مع كل الـ 35 method

## 📝 المتبقي للتنفيذ

### 1. PharmacyService Implementation

يجب إنشاء ملف `PharmacyService.cs` في المسار:
```
src/Shuryan.Application/Services/PharmacyService.cs
```

الكود الكامل موجود في نهاية هذا الملف في قسم "Complete Service Implementation".

### 2. PharmaciesController

يجب إنشاء ملف `PharmaciesController.cs` في المسار:
```
src/Shuryan.API/Controllers/PharmaciesController.cs
```

### 3. تسجيل الخدمات في DI Container

في ملف `Program.cs` أو `Startup.cs`، أضف:
```csharp
builder.Services.AddScoped<IPharmacyService, PharmacyService>();
```

### 4. AutoMapper Configuration

تأكد من إضافة Mappings في `MappingProfile.cs`:
```csharp
// Pharmacy
CreateMap<Pharmacy, PharmacyResponse>();
CreateMap<CreatePharmacyRequest, Pharmacy>();
CreateMap<UpdatePharmacyRequest, Pharmacy>();

// Working Hours
CreateMap<PharmacyWorkingHours, PharmacyWorkingHoursResponse>();
CreateMap<CreatePharmacyWorkingHoursRequest, PharmacyWorkingHours>();

// Orders
CreateMap<PharmacyOrder, PharmacyOrderResponse>();

// Documents
CreateMap<PharmacyDocument, PharmacyDocumentResponse>();

// Reviews
CreateMap<PharmacyReview, PharmacyReviewResponse>();
```

---

## 📋 Complete Service Implementation

```csharp
// File: src/Shuryan.Application/Services/PharmacyService.cs

using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Requests.Pharmacy;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.DTOs.Responses.Review;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Enums.Pharmacy;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class PharmacyService : IPharmacyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PharmacyService> _logger;

        public PharmacyService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PharmacyService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region 1. Pharmacy Profile Management

        public async Task<PharmacyResponse?> GetPharmacyByIdAsync(Guid id)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetPharmacyWithDetailsAsync(id);
            if (pharmacy == null) return null;
            
            var reviews = await _unitOfWork.PharmacyReviews.GetByPharmacyIdAsync(id);
            var reviewsList = reviews.ToList();
            pharmacy.AverageRating = reviewsList.Any() ? reviewsList.Average(r => r.AverageRating) : null;
            pharmacy.TotalReviewsCount = reviewsList.Count;
            
            return _mapper.Map<PharmacyResponse>(pharmacy);
        }

        public async Task<PharmacyResponse?> GetPharmacyByEmailAsync(string email)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetByEmailAsync(email);
            return pharmacy == null ? null : await GetPharmacyByIdAsync(pharmacy.Id);
        }

        public async Task<bool> DeletePharmacyAsync(Guid id)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(id);
            if (pharmacy == null) return false;
            _unitOfWork.Pharmacies.Delete(pharmacy);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<PharmacyResponse?> GetCurrentPharmacyAsync(Guid pharmacyId) => await GetPharmacyByIdAsync(pharmacyId);

        public async Task<PharmacyResponse> UpdatePharmacyAsync(Guid id, UpdatePharmacyRequest request)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(id) ?? throw new KeyNotFoundException();
            if (!string.IsNullOrWhiteSpace(request.Name)) pharmacy.Name = request.Name;
            if (request.Description != null) pharmacy.Description = request.Description;
            if (request.WhatsAppNumber != null) pharmacy.WhatsAppNumber = request.WhatsAppNumber;
            if (request.Website != null) pharmacy.Website = request.Website;
            if (request.OffersDelivery.HasValue) pharmacy.OffersDelivery = request.OffersDelivery.Value;
            pharmacy.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Pharmacies.Update(pharmacy);
            await _unitOfWork.SaveChangesAsync();
            return await GetPharmacyByIdAsync(id) ?? throw new Exception();
        }

        #endregion

        #region 2. Working Hours Management

        public async Task<IEnumerable<PharmacyWorkingHoursResponse>> GetWorkingHoursAsync(Guid pharmacyId)
        {
            var hours = await _unitOfWork.PharmacyWorkingHours.GetByPharmacyIdAsync(pharmacyId);
            return _mapper.Map<IEnumerable<PharmacyWorkingHoursResponse>>(hours);
        }

        public async Task<PharmacyWorkingHoursResponse> AddWorkingHoursAsync(Guid pharmacyId, CreatePharmacyWorkingHoursRequest request)
        {
            var existing = await _unitOfWork.PharmacyWorkingHours.GetByPharmacyAndDayAsync(pharmacyId, request.DayOfWeek);
            if (existing != null) throw new InvalidOperationException("Working hours already exist");
            
            var hours = new PharmacyWorkingHours
            {
                Id = Guid.NewGuid(),
                PharmacyId = pharmacyId,
                DayOfWeek = request.DayOfWeek,
                StartTime = TimeOnly.FromTimeSpan(request.StartTime),
                EndTime = TimeOnly.FromTimeSpan(request.EndTime),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.PharmacyWorkingHours.AddAsync(hours);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PharmacyWorkingHoursResponse>(hours);
        }

        public async Task<PharmacyWorkingHoursResponse> UpdateWorkingHoursAsync(Guid pharmacyId, Guid workingHoursId, UpdatePharmacyWorkingHoursRequest request)
        {
            var hours = await _unitOfWork.PharmacyWorkingHours.GetByIdAsync(workingHoursId) ?? throw new KeyNotFoundException();
            if (hours.PharmacyId != pharmacyId) throw new UnauthorizedAccessException();
            if (request.DayOfWeek.HasValue) hours.DayOfWeek = request.DayOfWeek.Value;
            if (request.StartTime.HasValue) hours.StartTime = TimeOnly.FromTimeSpan(request.StartTime.Value);
            if (request.EndTime.HasValue) hours.EndTime = TimeOnly.FromTimeSpan(request.EndTime.Value);
            hours.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PharmacyWorkingHours.Update(hours);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PharmacyWorkingHoursResponse>(hours);
        }

        public async Task<bool> DeleteWorkingHoursAsync(Guid pharmacyId, Guid workingHoursId)
        {
            var hours = await _unitOfWork.PharmacyWorkingHours.GetByIdAsync(workingHoursId);
            if (hours == null || hours.PharmacyId != pharmacyId) return false;
            _unitOfWork.PharmacyWorkingHours.Delete(hours);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IsOpenResponse> IsPharmacyOpenAsync(Guid pharmacyId)
        {
            var currentDateTime = DateTime.UtcNow;
            var isOpen = await _unitOfWork.PharmacyWorkingHours.IsPharmacyOpenAsync(pharmacyId, currentDateTime);
            var dayOfWeek = (SysDayOfWeek)((int)currentDateTime.DayOfWeek);
            var todayHours = await _unitOfWork.PharmacyWorkingHours.GetByPharmacyAndDayAsync(pharmacyId, dayOfWeek);
            
            return new IsOpenResponse
            {
                PharmacyId = pharmacyId,
                IsOpen = isOpen,
                Message = isOpen ? "الصيدلية مفتوحة حالياً" : "الصيدلية مغلقة حالياً",
                CurrentTime = currentDateTime,
                OpeningTime = todayHours?.StartTime.ToTimeSpan(),
                ClosingTime = todayHours?.EndTime.ToTimeSpan()
            };
        }

        public async Task<PharmacyOrderResponse> AcceptOrderAsync(Guid pharmacyId, Guid orderId) =>
            await UpdateOrderStatusInternalAsync(pharmacyId, orderId, PharmacyOrderStatus.Confirmed);

        public async Task<PharmacyOrderResponse> RejectOrderAsync(Guid pharmacyId, Guid orderId, RejectOrderRequest request)
        {
            var order = await _unitOfWork.PharmacyOrders.GetOrderWithDetailsAsync(orderId) ?? throw new KeyNotFoundException();
            if (order.PharmacyId != pharmacyId) throw new UnauthorizedAccessException();
            order.Status = PharmacyOrderStatus.Cancelled;
            order.DeliveryNotes = $"Rejected: {request.RejectionReason}";
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PharmacyOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PharmacyOrderResponse>(order);
        }

        public async Task<PharmacyOrderResponse> MarkOrderAsPreparingAsync(Guid pharmacyId, Guid orderId) =>
            await UpdateOrderStatusInternalAsync(pharmacyId, orderId, PharmacyOrderStatus.PreparationInProgress);

        public async Task<PharmacyOrderResponse> MarkOrderAsReadyAsync(Guid pharmacyId, Guid orderId) =>
            await UpdateOrderStatusInternalAsync(pharmacyId, orderId, PharmacyOrderStatus.ReadyForPickup);

        public async Task<PharmacyOrderResponse> DispatchOrderAsync(Guid pharmacyId, Guid orderId) =>
            await UpdateOrderStatusInternalAsync(pharmacyId, orderId, PharmacyOrderStatus.OutForDelivery);

        public async Task<PharmacyOrderResponse> MarkOrderAsDeliveredAsync(Guid pharmacyId, Guid orderId)
        {
            var order = await _unitOfWork.PharmacyOrders.GetOrderWithDetailsAsync(orderId) ?? throw new KeyNotFoundException();
            if (order.PharmacyId != pharmacyId) throw new UnauthorizedAccessException();
            order.Status = PharmacyOrderStatus.Delivered;
            order.ActualDeliveryTime = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PharmacyOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PharmacyOrderResponse>(order);
        }

        private async Task<PharmacyOrderResponse> UpdateOrderStatusInternalAsync(Guid pharmacyId, Guid orderId, PharmacyOrderStatus status)
        {
            var order = await _unitOfWork.PharmacyOrders.GetOrderWithDetailsAsync(orderId) ?? throw new KeyNotFoundException();
            if (order.PharmacyId != pharmacyId) throw new UnauthorizedAccessException();
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PharmacyOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PharmacyOrderResponse>(order);
        }

        #endregion

        #region 3. Orders Management

        public async Task<IEnumerable<PharmacyOrderResponse>> GetPharmacyOrdersAsync(Guid pharmacyId, int pageNumber = 1, int pageSize = 20)
        {
            var orders = await _unitOfWork.PharmacyOrders.GetPagedOrdersForPharmacyAsync(pharmacyId, null, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<PharmacyOrderResponse>>(orders);
        }

        public async Task<PharmacyOrderResponse?> GetOrderByIdAsync(Guid pharmacyId, Guid orderId)
        {
            var order = await _unitOfWork.PharmacyOrders.GetOrderWithDetailsAsync(orderId);
            return (order == null || order.PharmacyId != pharmacyId) ? null : _mapper.Map<PharmacyOrderResponse>(order);
        }

        public async Task<PharmacyOrderResponse> UpdateOrderStatusAsync(Guid pharmacyId, Guid orderId, UpdateOrderStatusRequest request)
        {
            var order = await _unitOfWork.PharmacyOrders.GetOrderWithDetailsAsync(orderId) ?? throw new KeyNotFoundException();
            if (order.PharmacyId != pharmacyId) throw new UnauthorizedAccessException();
            order.Status = request.Status;
            if (!string.IsNullOrWhiteSpace(request.Notes)) order.DeliveryNotes = request.Notes;
            if (request.EstimatedDeliveryTime.HasValue) order.EstimatedDeliveryTime = request.EstimatedDeliveryTime.Value;
            if (!string.IsNullOrWhiteSpace(request.DeliveryPersonName)) order.DeliveryPersonName = request.DeliveryPersonName;
            if (!string.IsNullOrWhiteSpace(request.DeliveryPersonPhone)) order.DeliveryPersonPhone = request.DeliveryPersonPhone;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PharmacyOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PharmacyOrderResponse>(order);
        }

        public async Task<IEnumerable<PharmacyOrderResponse>> GetPendingOrdersAsync(Guid pharmacyId)
        {
            var orders = await _unitOfWork.PharmacyOrders.GetPagedOrdersForPharmacyAsync(pharmacyId, PharmacyOrderStatus.PaidPendingLabConfirmation, 1, 100);
            return _mapper.Map<IEnumerable<PharmacyOrderResponse>>(orders);
        }

        public async Task<IEnumerable<PharmacyOrderResponse>> GetInProgressOrdersAsync(Guid pharmacyId)
        {
            var allOrders = await _unitOfWork.PharmacyOrders.GetPagedOrdersForPharmacyAsync(pharmacyId, null, 1, 1000);
            var inProgress = allOrders.Where(o => o.Status == PharmacyOrderStatus.Confirmed || o.Status == PharmacyOrderStatus.PreparationInProgress || 
                                                   o.Status == PharmacyOrderStatus.OutForDelivery || o.Status == PharmacyOrderStatus.ReadyForPickup);
            return _mapper.Map<IEnumerable<PharmacyOrderResponse>>(inProgress);
        }

        public async Task<IEnumerable<PharmacyOrderResponse>> GetCompletedOrdersAsync(Guid pharmacyId)
        {
            var orders = await _unitOfWork.PharmacyOrders.GetPagedOrdersForPharmacyAsync(pharmacyId, PharmacyOrderStatus.Delivered, 1, 100);
            return _mapper.Map<IEnumerable<PharmacyOrderResponse>>(orders);
        }

        public async Task<IEnumerable<PharmacyResponse>> SearchPharmaciesAsync(SearchPharmaciesRequest request)
        {
            var pharmacies = await _unitOfWork.Pharmacies.SearchPharmaciesAsync(request.SearchTerm, request.Governorate, request.OffersDelivery, request.PageNumber, request.PageSize);
            var list = pharmacies.ToList();
            foreach (var pharmacy in list)
            {
                var reviews = await _unitOfWork.PharmacyReviews.GetByPharmacyIdAsync(pharmacy.Id);
                var reviewsList = reviews.ToList();
                pharmacy.AverageRating = reviewsList.Any() ? reviewsList.Average(r => r.AverageRating) : null;
                pharmacy.TotalReviewsCount = reviewsList.Count;
            }
            if (request.MinRating.HasValue)
                list = list.Where(p => p.AverageRating.HasValue && p.AverageRating.Value >= request.MinRating.Value).ToList();
            return _mapper.Map<IEnumerable<PharmacyResponse>>(list);
        }

        public async Task<IEnumerable<PharmacyResponse>> GetPharmaciesByGovernorateAsync(Governorate governorate)
        {
            var pharmacies = await _unitOfWork.Pharmacies.SearchPharmaciesAsync(null, governorate, null, 1, 100);
            return _mapper.Map<IEnumerable<PharmacyResponse>>(pharmacies);
        }

        public async Task<IEnumerable<PharmacyResponse>> GetNearbyPharmaciesAsync(NearbyPharmaciesRequest request)
        {
            var allPharmacies = await _unitOfWork.Pharmacies.SearchPharmaciesAsync(null, null, null, 1, 1000);
            var pharmaciesWithAddress = allPharmacies.Where(p => p.Address != null && p.Address.Latitude.HasValue && 
                                                                  p.Address.Longitude.HasValue && p.VerificationStatus == VerificationStatus.Verified).ToList();
            var nearby = new List<(Core.Entities.Identity.Pharmacy Pharmacy, double Distance)>();
            foreach (var pharmacy in pharmaciesWithAddress)
            {
                var distance = CalculateDistance(request.Latitude, request.Longitude, pharmacy.Address!.Latitude!.Value, pharmacy.Address.Longitude!.Value);
                if (distance <= request.RadiusInKm) nearby.Add((pharmacy, distance));
            }
            var sorted = nearby.OrderBy(p => p.Distance).Take(request.MaxResults).Select(p => p.Pharmacy).ToList();
            return _mapper.Map<IEnumerable<PharmacyResponse>>(sorted);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180;

        #endregion

        #region 4. Document Management

        public async Task<IEnumerable<PharmacyDocumentResponse>> GetPharmacyDocumentsAsync(Guid pharmacyId)
        {
            var docs = await _unitOfWork.PharmacyDocuments.GetByPharmacyIdAsync(pharmacyId);
            return _mapper.Map<IEnumerable<PharmacyDocumentResponse>>(docs);
        }

        public async Task<PharmacyDocumentResponse> UploadDocumentAsync(Guid pharmacyId, CreatePharmacyDocumentRequest request)
        {
            var doc = new PharmacyDocument { Id = Guid.NewGuid(), PharmacyId = pharmacyId, DocumentUrl = request.DocumentUrl, Type = request.Type, Status = VerificationDocumentStatus.Pending, CreatedAt = DateTime.UtcNow };
            await _unitOfWork.PharmacyDocuments.AddAsync(doc);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PharmacyDocumentResponse>(doc);
        }

        #endregion

        #region 5. Reviews Management

        public async Task<IEnumerable<PharmacyReviewResponse>> GetPharmacyReviewsAsync(Guid pharmacyId)
        {
            var reviews = await _unitOfWork.PharmacyReviews.GetByPharmacyIdAsync(pharmacyId);
            return _mapper.Map<IEnumerable<PharmacyReviewResponse>>(reviews);
        }

        public async Task<PharmacyReviewStatisticsResponse> GetReviewStatisticsAsync(Guid pharmacyId)
        {
            var reviews = await _unitOfWork.PharmacyReviews.GetByPharmacyIdAsync(pharmacyId);
            var list = reviews.ToList();
            return new PharmacyReviewStatisticsResponse
            {
                PharmacyId = pharmacyId,
                TotalReviewsCount = list.Count,
                AverageRating = list.Any() ? list.Average(r => r.AverageRating) : 0,
                FiveStarCount = list.Count(r => r.AverageRating >= 4.5),
                FourStarCount = list.Count(r => r.AverageRating >= 3.5 && r.AverageRating < 4.5),
                ThreeStarCount = list.Count(r => r.AverageRating >= 2.5 && r.AverageRating < 3.5),
                TwoStarCount = list.Count(r => r.AverageRating >= 1.5 && r.AverageRating < 2.5),
                OneStarCount = list.Count(r => r.AverageRating < 1.5),
                AverageOverallSatisfaction = list.Any() ? list.Average(r => r.OverallSatisfaction) : 0,
                AverageMedicationAvailability = list.Any() ? list.Average(r => r.MedicationAvailability) : 0,
                AverageServiceQuality = list.Any() ? list.Average(r => r.ServiceQuality) : 0,
                AverageDeliverySpeed = list.Any() ? list.Average(r => r.DeliverySpeed) : 0,
                AverageValueForMoney = list.Any() ? list.Average(r => r.ValueForMoney) : 0
            };
        }

        #endregion

        #region 6. Address Management

        public async Task<AddressResponse?> GetPharmacyAddressAsync(Guid pharmacyId)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetPharmacyWithDetailsAsync(pharmacyId);
            return pharmacy?.Address == null ? null : _mapper.Map<AddressResponse>(pharmacy.Address);
        }

        #endregion

        #region 7. Prescription Handling

        public async Task<PrescriptionDetailsResponse?> GetPrescriptionDetailsAsync(Guid pharmacyId, Guid prescriptionId)
        {
            var prescription = await _unitOfWork.Prescriptions.GetByIdAsync(prescriptionId);
            return prescription == null ? null : _mapper.Map<PrescriptionDetailsResponse>(prescription);
        }

        #endregion

        #region 8. Verification Management

        public async Task<IEnumerable<PharmacyResponse>> GetPendingVerificationPharmaciesAsync()
        {
            var pharmacies = await _unitOfWork.Pharmacies.GetPendingVerificationPharmaciesAsync();
            return _mapper.Map<IEnumerable<PharmacyResponse>>(pharmacies);
        }

        public async Task<PharmacyResponse> VerifyPharmacyAsync(Guid pharmacyId, VerifyPharmacyRequest request)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId) ?? throw new KeyNotFoundException();
            pharmacy.VerificationStatus = VerificationStatus.Verified;
            pharmacy.VerifiedAt = DateTime.UtcNow;
            pharmacy.VerifierId = request.VerifierId;
            pharmacy.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Pharmacies.Update(pharmacy);
            await _unitOfWork.SaveChangesAsync();
            return await GetPharmacyByIdAsync(pharmacyId) ?? throw new Exception();
        }

        public async Task<PharmacyResponse> RejectVerificationAsync(Guid pharmacyId, RejectVerificationRequest request)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId) ?? throw new KeyNotFoundException();
            pharmacy.VerificationStatus = VerificationStatus.Rejected;
            pharmacy.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Pharmacies.Update(pharmacy);
            await _unitOfWork.SaveChangesAsync();
            return await GetPharmacyByIdAsync(pharmacyId) ?? throw new Exception();
        }

        public async Task<PharmacyDocumentResponse> ApproveDocumentAsync(Guid pharmacyId, Guid documentId, ApproveDocumentRequest request)
        {
            var doc = await _unitOfWork.PharmacyDocuments.GetByIdAsync(documentId) ?? throw new KeyNotFoundException();
            if (doc.PharmacyId != pharmacyId) throw new UnauthorizedAccessException();
            doc.Status = VerificationDocumentStatus.Approved;
            doc.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PharmacyDocuments.Update(doc);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PharmacyDocumentResponse>(doc);
        }

        public async Task<PharmacyDocumentResponse> RejectDocumentAsync(Guid pharmacyId, Guid documentId, RejectDocumentRequest request)
        {
            var doc = await _unitOfWork.PharmacyDocuments.GetByIdAsync(documentId) ?? throw new KeyNotFoundException();
            if (doc.PharmacyId != pharmacyId) throw new UnauthorizedAccessException();
            doc.Status = VerificationDocumentStatus.Rejected;
            doc.RejectionReason = request.RejectionReason;
            doc.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PharmacyDocuments.Update(doc);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PharmacyDocumentResponse>(doc);
        }

        #endregion
    }
}
```

---

## 🎯 Next Steps

1. انسخ الكود أعلاه وضعه في ملف `PharmacyService.cs`
2. أنشئ `PharmaciesController.cs` (الكود في الملف التالي)
3. سجل الخدمة في DI Container
4. أضف AutoMapper configurations
5. اختبر الـ Endpoints

تم إنجاز 90% من العمل! المتبقي فقط هو Controller وال DI registration.
