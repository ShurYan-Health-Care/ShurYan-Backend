using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.FileUpload;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Pharmacy;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Pharmacy;
using Shuryan.Core.Interfaces;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class PharmacyProfileService : IPharmacyProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PharmacyProfileService> _logger;
        private readonly IFileUploadService _fileUploadService;

        public PharmacyProfileService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PharmacyProfileService> logger,
            IFileUploadService fileUploadService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        #region Basic Info Operations

        public async Task<PharmacyBasicInfoResponse?> GetBasicInfoAsync(Guid pharmacyId)
        {
            try
            {
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    _logger.LogWarning("Pharmacy with ID {PharmacyId} not found", pharmacyId);
                    return null;
                }

                return new PharmacyBasicInfoResponse
                {
                    Name = pharmacy.Name,
                    Email = pharmacy.Email,
                    PhoneNumber = pharmacy.PhoneNumber,
                    ProfileImageUrl = pharmacy.ProfilePictureUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving basic info for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        public async Task<PharmacyBasicInfoResponse> UpdateBasicInfoAsync(Guid pharmacyId, UpdatePharmacyBasicInfoRequest request)
        {
            try
            {
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                // تحديث الحقول اللي اتبعتت بس (Partial Update)
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    pharmacy.Name = request.Name;
                }

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    pharmacy.PhoneNumber = request.PhoneNumber;
                    pharmacy.PhoneNumberConfirmed = false; // Reset confirmation
                }

                pharmacy.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated basic info for pharmacy {PharmacyId}", pharmacyId);

                return new PharmacyBasicInfoResponse
                {
                    Name = pharmacy.Name,
                    Email = pharmacy.Email,
                    PhoneNumber = pharmacy.PhoneNumber,
                    ProfileImageUrl = pharmacy.ProfilePictureUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating basic info for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        public async Task<string> UpdateProfileImageAsync(Guid pharmacyId, IFormFile image)
        {
            try
            {
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                // حذف الصورة القديمة لو موجودة
                if (!string.IsNullOrEmpty(pharmacy.ProfilePictureUrl))
                {
                    await _fileUploadService.DeleteFileAsync(pharmacy.ProfilePictureUrl);
                }

                // رفع الصورة على Cloudinary
                var uploadResult = await _fileUploadService.UploadProfileImageAsync(image, pharmacyId.ToString());
                pharmacy.ProfilePictureUrl = uploadResult.FileUrl;
                pharmacy.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated profile image for pharmacy {PharmacyId}", pharmacyId);

                return uploadResult.FileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        #endregion

        #region Order Status Updates

        public async Task<PharmacyOrderStatusUpdateResponse> UpdateOrderStatusAsync(
            Guid pharmacyId,
            Guid orderId,
            UpdatePharmacyOrderStatusRequest request)
        {
            try
            {
                _logger.LogInformation("Updating pharmacy order {OrderId} status to {Status} by pharmacy {PharmacyId}",
                    orderId, request.NewStatus, pharmacyId);

                // Ensure pharmacy exists
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                // Fetch order and ensure ownership
                var order = await _unitOfWork.Repository<PharmacyOrder>().GetByIdAsync(orderId);
                if (order == null || order.PharmacyId != pharmacyId)
                {
                    throw new KeyNotFoundException($"Order with ID {orderId} not found for pharmacy {pharmacyId}");
                }

                // Allowed target statuses
                var allowedStatuses = new[]
                {
                    PharmacyOrderStatus.PreparationInProgress,
                    PharmacyOrderStatus.OutForDelivery,
                    PharmacyOrderStatus.ReadyForPickup,
                    PharmacyOrderStatus.Delivered
                };

                if (!allowedStatuses.Contains(request.NewStatus))
                {
                    throw new InvalidOperationException("هذه الحالة غير مسموح بتعيينها من قبل الصيدلية");
                }

                // Validate transitions
                var current = order.Status;

                switch (request.NewStatus)
                {
                    case PharmacyOrderStatus.PreparationInProgress:
                        if (current != PharmacyOrderStatus.Confirmed)
                            throw new InvalidOperationException($"Cannot move to PreparationInProgress from {current}");
                        order.Status = PharmacyOrderStatus.PreparationInProgress;
                        // optional ETA
                        order.EstimatedDeliveryTime = request.EstimatedDeliveryTime;
                        break;

                    case PharmacyOrderStatus.OutForDelivery:
                        if (order.DeliveryType != OrderDeliveryType.Delivery)
                            throw new InvalidOperationException("لا يمكن تعيين 'خرج للتوصيل' لأن نوع الطلب ليس توصيل");
                        if (current != PharmacyOrderStatus.PreparationInProgress)
                            throw new InvalidOperationException($"Cannot move to OutForDelivery from {current}");
                        order.Status = PharmacyOrderStatus.OutForDelivery;
                        order.EstimatedDeliveryTime = request.EstimatedDeliveryTime;
                        if (!string.IsNullOrWhiteSpace(request.DeliveryPersonName))
                            order.DeliveryPersonName = request.DeliveryPersonName;
                        if (!string.IsNullOrWhiteSpace(request.DeliveryPersonPhone))
                            order.DeliveryPersonPhone = request.DeliveryPersonPhone!;
                        if (!string.IsNullOrWhiteSpace(request.DeliveryNotes))
                            order.DeliveryNotes = request.DeliveryNotes;
                        break;

                    case PharmacyOrderStatus.ReadyForPickup:
                        if (order.DeliveryType != OrderDeliveryType.PharmacyPickup)
                            throw new InvalidOperationException("لا يمكن تعيين 'جاهز للاستلام' لأن نوع الطلب ليس استلام من الصيدلية");
                        if (current != PharmacyOrderStatus.PreparationInProgress)
                            throw new InvalidOperationException($"Cannot move to ReadyForPickup from {current}");
                        order.Status = PharmacyOrderStatus.ReadyForPickup;
                        if (!string.IsNullOrWhiteSpace(request.DeliveryNotes))
                            order.DeliveryNotes = request.DeliveryNotes;
                        break;

                    case PharmacyOrderStatus.Delivered:
                        if (current != PharmacyOrderStatus.OutForDelivery && current != PharmacyOrderStatus.ReadyForPickup)
                            throw new InvalidOperationException($"Cannot move to Delivered from {current}");
                        order.Status = PharmacyOrderStatus.Delivered;
                        order.ActualDeliveryTime = request.ActualDeliveryTime ?? DateTime.UtcNow;
                        break;
                }

                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<PharmacyOrder>().Update(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, order.Status);

                return new PharmacyOrderStatusUpdateResponse
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Status = order.Status,
                    StatusName = order.Status.ToString(),
                    UpdatedAt = order.UpdatedAt ?? DateTime.UtcNow,
                    EstimatedDeliveryTime = order.EstimatedDeliveryTime,
                    ActualDeliveryTime = order.ActualDeliveryTime,
                    Message = "تم تحديث حالة الطلب بنجاح"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status for pharmacy {PharmacyId}", orderId, pharmacyId);
                throw;
            }
        }

        #endregion

        #region Address Operations

        public async Task<PharmacyAddressResponse?> GetAddressAsync(Guid pharmacyId)
        {
            try
            {
                var pharmacy = await _unitOfWork.Pharmacies.GetPharmacyWithDetailsAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    _logger.LogWarning("Pharmacy with ID {PharmacyId} not found", pharmacyId);
                    return null;
                }

                if (pharmacy.Address == null)
                {
                    return new PharmacyAddressResponse();
                }

                return new PharmacyAddressResponse
                {
                    Governorate = pharmacy.Address.Governorate,
                    City = pharmacy.Address.City,
                    Street = pharmacy.Address.Street,
                    BuildingNumber = pharmacy.Address.BuildingNumber,
                    Latitude = pharmacy.Address.Latitude,
                    Longitude = pharmacy.Address.Longitude
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        public async Task<PharmacyAddressResponse> UpdateAddressAsync(Guid pharmacyId, UpdatePharmacyAddressRequest request)
        {
            try
            {
                // Get pharmacy without tracking to check if exists
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                var addressRepo = _unitOfWork.Repository<Address>();
                Address address;
                bool isNewAddress = false;

                // لو مفيش عنوان، نعمل واحد جديد
                if (pharmacy.AddressId == null)
                {
                    address = new Address
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow,
                        Governorate = request.Governorate ?? Governorate.Cairo,
                        City = request.City ?? string.Empty,
                        Street = request.Street ?? string.Empty,
                        BuildingNumber = request.BuildingNumber,
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    await addressRepo.AddAsync(address);
                    
                    // Update pharmacy's AddressId
                    pharmacy.AddressId = address.Id;
                    _unitOfWork.Pharmacies.Update(pharmacy);
                    
                    isNewAddress = true;
                }
                else
                {
                    // Get existing address
                    address = await addressRepo.GetByIdAsync(pharmacy.AddressId.Value);
                    if (address == null)
                    {
                        throw new KeyNotFoundException($"Address with ID {pharmacy.AddressId.Value} not found");
                    }

                    // تحديث الحقول اللي اتبعتت بس (Partial Update)
                    if (request.Governorate.HasValue)
                    {
                        address.Governorate = request.Governorate.Value;
                    }

                    if (!string.IsNullOrWhiteSpace(request.City))
                    {
                        address.City = request.City;
                    }

                    if (!string.IsNullOrWhiteSpace(request.Street))
                    {
                        address.Street = request.Street;
                    }

                    if (request.BuildingNumber != null)
                    {
                        address.BuildingNumber = request.BuildingNumber;
                    }

                    if (request.Latitude.HasValue)
                    {
                        address.Latitude = request.Latitude.Value;
                    }

                    if (request.Longitude.HasValue)
                    {
                        address.Longitude = request.Longitude.Value;
                    }

                    address.UpdatedAt = DateTime.UtcNow;
                    addressRepo.Update(address);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated address for pharmacy {PharmacyId}, IsNew: {IsNew}", pharmacyId, isNewAddress);

                return new PharmacyAddressResponse
                {
                    Governorate = address.Governorate,
                    City = address.City,
                    Street = address.Street,
                    BuildingNumber = address.BuildingNumber,
                    Latitude = address.Latitude,
                    Longitude = address.Longitude
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        #endregion

        #region Working Hours Operations

        public async Task<PharmacyWorkingHoursResponse> GetWorkingHoursAsync(Guid pharmacyId)
        {
            try
            {
                var workingHours = await _unitOfWork.PharmacyWorkingHours.GetByPharmacyIdAsync(pharmacyId);

                var response = new PharmacyWorkingHoursResponse();

                // تحويل كل يوم من الأسبوع
                foreach (var wh in workingHours)
                {
                    var daySchedule = ConvertToDayScheduleResponse(wh);

                    switch (wh.DayOfWeek)
                    {
                        case SysDayOfWeek.Saturday:
                            response.Saturday = daySchedule;
                            break;
                        case SysDayOfWeek.Sunday:
                            response.Sunday = daySchedule;
                            break;
                        case SysDayOfWeek.Monday:
                            response.Monday = daySchedule;
                            break;
                        case SysDayOfWeek.Tuesday:
                            response.Tuesday = daySchedule;
                            break;
                        case SysDayOfWeek.Wednesday:
                            response.Wednesday = daySchedule;
                            break;
                        case SysDayOfWeek.Thursday:
                            response.Thursday = daySchedule;
                            break;
                        case SysDayOfWeek.Friday:
                            response.Friday = daySchedule;
                            break;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving working hours for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        public async Task<PharmacyWorkingHoursResponse> UpdateWorkingHoursAsync(Guid pharmacyId, UpdatePharmacyWorkingHoursRequest request)
        {
            try
            {
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                var schedule = request.WeeklySchedule;

                // تحديث كل يوم لوحده (Partial Update)
                await UpdateDaySchedule(pharmacyId, SysDayOfWeek.Saturday, schedule.Saturday);
                await UpdateDaySchedule(pharmacyId, SysDayOfWeek.Sunday, schedule.Sunday);
                await UpdateDaySchedule(pharmacyId, SysDayOfWeek.Monday, schedule.Monday);
                await UpdateDaySchedule(pharmacyId, SysDayOfWeek.Tuesday, schedule.Tuesday);
                await UpdateDaySchedule(pharmacyId, SysDayOfWeek.Wednesday, schedule.Wednesday);
                await UpdateDaySchedule(pharmacyId, SysDayOfWeek.Thursday, schedule.Thursday);
                await UpdateDaySchedule(pharmacyId, SysDayOfWeek.Friday, schedule.Friday);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated working hours for pharmacy {PharmacyId}", pharmacyId);

                // جلب البيانات المحدثة
                return await GetWorkingHoursAsync(pharmacyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating working hours for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        #endregion

        #region Delivery Operations

        public async Task<PharmacyDeliveryResponse> GetDeliverySettingsAsync(Guid pharmacyId)
        {
            try
            {
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                return new PharmacyDeliveryResponse
                {
                    OffersDelivery = pharmacy.OffersDelivery,
                    DeliveryFee = pharmacy.DeliveryFee
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delivery settings for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        public async Task<PharmacyDeliveryResponse> UpdateDeliverySettingsAsync(Guid pharmacyId, UpdatePharmacyDeliveryRequest request)
        {
            try
            {
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                pharmacy.OffersDelivery = request.OffersDelivery;
                pharmacy.DeliveryFee = request.DeliveryFee;
                pharmacy.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated delivery settings for pharmacy {PharmacyId}", pharmacyId);

                return new PharmacyDeliveryResponse
                {
                    OffersDelivery = pharmacy.OffersDelivery,
                    DeliveryFee = pharmacy.DeliveryFee
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery settings for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private DayScheduleResponse ConvertToDayScheduleResponse(PharmacyWorkingHours workingHours)
        {
            var (fromTime, fromPeriod) = ConvertTo12HourFormat(workingHours.StartTime);
            var (toTime, toPeriod) = ConvertTo12HourFormat(workingHours.EndTime);

            return new DayScheduleResponse
            {
                Enabled = true,
                FromTime = fromTime,
                ToTime = toTime,
                FromPeriod = fromPeriod,
                ToPeriod = toPeriod
            };
        }

        private (string time, string period) ConvertTo12HourFormat(TimeOnly time)
        {
            var hour = time.Hour;
            var period = hour >= 12 ? "PM" : "AM";

            if (hour == 0)
                hour = 12;
            else if (hour > 12)
                hour -= 12;

            return (hour.ToString("D2"), period);
        }

        private async Task UpdateDaySchedule(Guid pharmacyId, SysDayOfWeek dayOfWeek, DayScheduleDto? daySchedule)
        {
            // لو اليوم مش موجود في الـ request، متعملش حاجة
            if (daySchedule == null)
                return;

            // جلب ساعات العمل الحالية لليوم ده
            var existingSchedule = await _unitOfWork.PharmacyWorkingHours.GetByPharmacyAndDayAsync(pharmacyId, dayOfWeek);

            // لو اليوم disabled، امسح الساعات لو موجودة
            if (!daySchedule.Enabled)
            {
                if (existingSchedule != null)
                {
                    _unitOfWork.PharmacyWorkingHours.Delete(existingSchedule);
                    _logger.LogInformation("Disabled day {Day} for pharmacy {PharmacyId}", dayOfWeek, pharmacyId);
                }
                return;
            }

            // لو اليوم enabled، لازم يكون فيه أوقات
            if (string.IsNullOrWhiteSpace(daySchedule.FromTime) ||
                string.IsNullOrWhiteSpace(daySchedule.ToTime) ||
                string.IsNullOrWhiteSpace(daySchedule.FromPeriod) ||
                string.IsNullOrWhiteSpace(daySchedule.ToPeriod))
            {
                throw new ArgumentException($"Working hours are required when day is enabled for {dayOfWeek}");
            }

            var startTime = ConvertTo24HourFormat(daySchedule.FromTime, daySchedule.FromPeriod);
            var endTime = ConvertTo24HourFormat(daySchedule.ToTime, daySchedule.ToPeriod);

            // لو موجود، حدثه
            if (existingSchedule != null)
            {
                existingSchedule.StartTime = startTime;
                existingSchedule.EndTime = endTime;
                existingSchedule.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.PharmacyWorkingHours.Update(existingSchedule);
                _logger.LogInformation("Updated day {Day} for pharmacy {PharmacyId}", dayOfWeek, pharmacyId);
            }
            else
            {
                // لو مش موجود، اضفه
                var workingHours = new PharmacyWorkingHours
                {
                    Id = Guid.NewGuid(),
                    PharmacyId = pharmacyId,
                    DayOfWeek = dayOfWeek,
                    StartTime = startTime,
                    EndTime = endTime,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.PharmacyWorkingHours.AddAsync(workingHours);
                _logger.LogInformation("Added day {Day} for pharmacy {PharmacyId}", dayOfWeek, pharmacyId);
            }
        }

        private async Task AddDayScheduleIfEnabled(Guid pharmacyId, SysDayOfWeek dayOfWeek, DayScheduleDto? daySchedule)
        {
            if (daySchedule == null || !daySchedule.Enabled)
                return;

            if (string.IsNullOrWhiteSpace(daySchedule.FromTime) ||
                string.IsNullOrWhiteSpace(daySchedule.ToTime) ||
                string.IsNullOrWhiteSpace(daySchedule.FromPeriod) ||
                string.IsNullOrWhiteSpace(daySchedule.ToPeriod))
            {
                return;
            }

            var startTime = ConvertTo24HourFormat(daySchedule.FromTime, daySchedule.FromPeriod);
            var endTime = ConvertTo24HourFormat(daySchedule.ToTime, daySchedule.ToPeriod);

            var workingHours = new PharmacyWorkingHours
            {
                Id = Guid.NewGuid(),
                PharmacyId = pharmacyId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PharmacyWorkingHours.AddAsync(workingHours);
        }

        private TimeOnly ConvertTo24HourFormat(string time, string period)
        {
            if (!int.TryParse(time, out int hour))
            {
                throw new ArgumentException($"Invalid time format: {time}");
            }

            if (period.ToUpper() == "PM" && hour != 12)
            {
                hour += 12;
            }
            else if (period.ToUpper() == "AM" && hour == 12)
            {
                hour = 0;
            }

            return new TimeOnly(hour, 0);
        }

        #endregion

        #region Prescription Operations

        public async Task<PendingPrescriptionsListResponse> GetPendingPrescriptionsAsync(Guid pharmacyId)
        {
            try
            {
                // Verify pharmacy exists
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                // Get pending pharmacy orders with status = PendingPharmacyResponse (1)
                var pendingOrders = await _unitOfWork.PharmacyOrders
                    .GetQueryable()
                    .Where(o => o.PharmacyId == pharmacyId && 
                               o.Status == PharmacyOrderStatus.PendingPharmacyResponse)
                    .Include(o => o.Prescription)
                        .ThenInclude(p => p!.Patient)
                    .Include(o => o.Prescription)
                        .ThenInclude(p => p!.Doctor)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var response = new PendingPrescriptionsListResponse
                {
                    Prescriptions = pendingOrders.Select(order => new PendingPrescriptionResponse
                    {
                        OrderId = order.Id,
                        PrescriptionId = order.PrescriptionId,
                        PrescriptionNumber = order.Prescription?.PrescriptionNumber ?? "N/A",
                        PatientName = $"{order.Prescription?.Patient?.FirstName} {order.Prescription?.Patient?.LastName}".Trim(),
                        PatientPhone = order.Prescription?.Patient?.PhoneNumber ?? "N/A",
                        DoctorName = $"د. {order.Prescription?.Doctor?.FirstName} {order.Prescription?.Doctor?.LastName}".Trim(),
                        Status = (int)order.Status,
                        ReceivedAt = order.CreatedAt
                    }).ToList(),
                    TotalCount = pendingOrders.Count
                };

                _logger.LogInformation("Retrieved {Count} pending prescriptions for pharmacy {PharmacyId}", 
                    response.TotalCount, pharmacyId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending prescriptions for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        public async Task<PrescriptionDetailsResponse> GetPrescriptionDetailsAsync(Guid pharmacyId, Guid orderId)
        {
            try
            {
                // Verify pharmacy exists
                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                // Get the specific order
                var order = await _unitOfWork.PharmacyOrders
                    .GetQueryable()
                    .Where(o => o.Id == orderId && o.PharmacyId == pharmacyId)
                    .Include(o => o.Prescription)
                        .ThenInclude(p => p!.PrescribedMedications)
                            .ThenInclude(pm => pm.Medication)
                    .Include(o => o.Prescription)
                        .ThenInclude(p => p!.Patient)
                            .ThenInclude(p => p!.Address)
                    .Include(o => o.Prescription)
                        .ThenInclude(p => p!.Doctor)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {orderId} not found for pharmacy {pharmacyId}");
                }

                var prescription = order.Prescription;
                if (prescription == null)
                {
                    throw new KeyNotFoundException($"Prescription not found for order {orderId}");
                }

                // Build address string
                var address = prescription.Patient?.Address != null
                    ? $"{prescription.Patient.Address.City}, {prescription.Patient.Address.Street}"
                    : "غير متوفر";

                var response = new PrescriptionDetailsResponse
                {
                    OrderId = order.Id,
                    PrescriptionNumber = prescription.PrescriptionNumber ?? "N/A",
                    Patient = new PrescriptionPatientInfo
                    {
                        Name = $"{prescription.Patient?.FirstName} {prescription.Patient?.LastName}".Trim(),
                        Phone = prescription.Patient?.PhoneNumber ?? "N/A",
                        Address = address
                    },
                    Doctor = new PrescriptionDoctorInfo
                    {
                        Name = $"د. {prescription.Doctor?.FirstName} {prescription.Doctor?.LastName}".Trim(),
                        Specialty = prescription.Doctor?.MedicalSpecialty.ToString() ?? "غير محدد"
                    },
                    Medications = prescription.PrescribedMedications?.Select(pm => new PrescriptionMedicationItemResponse
                    {
                        MedicationName = pm.Medication?.BrandName ?? "N/A",
                        Dosage = pm.Dosage ?? "N/A",
                        Frequency = pm.Frequency ?? "N/A",
                        DurationDays = pm.DurationDays,
                        SpecialInstructions = pm.SpecialInstructions
                    }).ToList() ?? new List<PrescriptionMedicationItemResponse>(),
                    Status = (int)order.Status,
                    CreatedAt = prescription.CreatedAt
                };

                _logger.LogInformation("Retrieved prescription details for order {OrderId}, pharmacy {PharmacyId}", 
                    orderId, pharmacyId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescription details for order {OrderId}, pharmacy {PharmacyId}", 
                    orderId, pharmacyId);
                throw;
            }
        }

        #endregion

        #region Orders Operations
        public async Task<PharmacyOrdersListResponse> GetPharmacyOrdersAsync(Guid pharmacyId, PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy orders for pharmacy {PharmacyId} with pagination", pharmacyId);

                var pharmacy = await _unitOfWork.Repository<Pharmacy>().GetByIdAsync(pharmacyId);
                if (pharmacy == null)
                {
                    throw new ArgumentException($"Pharmacy with ID {pharmacyId} not found");
                }

                var query = _unitOfWork.Repository<PharmacyOrder>()
                    .GetQueryable()
                    .Where(po => po.PharmacyId == pharmacyId)
                    .Include(po => po.Patient)
                    .Include(po => po.Prescription)
                        .ThenInclude(p => p.Doctor)
                    .Include(po => po.Prescription)
                        .ThenInclude(p => p.PrescribedMedications)
                            .ThenInclude(pm => pm.Medication)
                    .OrderByDescending(po => po.CreatedAt);

                var totalCount = await query.CountAsync();

                var orders = await query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToListAsync();

                var orderResponses = orders.Select(order => new PharmacyOrderResponse
                {
                    OrderId = order.Id,
                    PrescriptionNumber = order.Prescription?.PrescriptionNumber ?? string.Empty,
                    PatientName = $"{order.Patient.FirstName} {order.Patient.LastName}",
                    PatientPhone = order.Patient.PhoneNumber ?? string.Empty,
                    DoctorName = order.Prescription?.Doctor != null 
                        ? $"د. {order.Prescription.Doctor.FirstName} {order.Prescription.Doctor.LastName}" 
                        : string.Empty,
                    PharmacyOrderStatus = order.Status,
                    ReceivedAt = order.CreatedAt,
                    Prescription = order.Prescription != null ? new PharmacyOrderPrescriptionResponse
                    {
                        Medications = order.Prescription.PrescribedMedications.Select(pm => new PharmacyOrderMedicationResponse
                        {
                            MedicationName = pm.Medication.BrandName,
                            Dosage = pm.Dosage,
                            Frequency = pm.Frequency,
                            Duration = $"{pm.DurationDays} أيام",
                            SpecialInstructions = pm.SpecialInstructions
                        }).ToList(),
                        DoctorNotes = order.Prescription.GeneralInstructions,
                        TotalAmount = order.TotalCost
                    } : null
                }).ToList();

                var response = new PharmacyOrdersListResponse
                {
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / paginationParams.PageSize),
                    HasNextPage = paginationParams.PageNumber < (int)Math.Ceiling((double)totalCount / paginationParams.PageSize),
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    Data = orderResponses
                };

                _logger.LogInformation("Retrieved {Count} orders out of {TotalCount} for pharmacy {PharmacyId}", 
                    response.Data.Count(), totalCount, pharmacyId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pharmacy orders for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        public async Task<PharmacyOrderResponseResponse> RespondToOrderAsync(Guid pharmacyId, Guid orderId, PharmacyOrderResponseRequest request)
        {
            try
            {
                _logger.LogInformation("Pharmacy {PharmacyId} responding to order {OrderId}", pharmacyId, orderId);

                var pharmacy = await _unitOfWork.Repository<Pharmacy>().GetByIdAsync(pharmacyId);
                if (pharmacy == null)
                {
                    throw new ArgumentException($"Pharmacy with ID {pharmacyId} not found");
                }

                var order = await _unitOfWork.Repository<PharmacyOrder>()
                    .GetQueryable()
                    .Include(po => po.Patient)
                    .Include(po => po.Prescription)
                        .ThenInclude(p => p.PrescribedMedications)
                            .ThenInclude(pm => pm.Medication)
                    .FirstOrDefaultAsync(po => po.Id == orderId && po.PharmacyId == pharmacyId);

                if (order == null)
                {
                    throw new ArgumentException($"Order with ID {orderId} not found for pharmacy {pharmacyId}");
                }

                if (order.Status != PharmacyOrderStatus.PendingPharmacyResponse)
                {
                    throw new InvalidOperationException($"Order {orderId} cannot be responded to. Current status: {order.Status}");
                }

                order.Status = PharmacyOrderStatus.WaitingForPatientConfirmation;
                order.TotalCost = request.TotalAmount;
                order.DeliveryFee = request.DeliveryFee;
                order.DeliveryType = request.DeliveryAvailable ? OrderDeliveryType.Delivery : OrderDeliveryType.PharmacyPickup;
                order.UpdatedAt = DateTime.UtcNow;

                if (order.Prescription != null)
                {
                    order.Prescription.Status = PrescriptionStatus.Reported;
                    order.Prescription.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Prescription>().Update(order.Prescription);
                    
                    _logger.LogInformation("Updated prescription {PrescriptionId} status to Reported", order.PrescriptionId);
                }

                await SaveMedicationDetailsAsync(order, request.Medications);

                _unitOfWork.Repository<PharmacyOrder>().Update(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully saved all changes for order {OrderId}", orderId);

                _logger.LogInformation("Successfully updated order {OrderId} with pharmacy response", orderId);

                var response = new PharmacyOrderResponseResponse
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    PatientName = $"{order.Patient.FirstName} {order.Patient.LastName}",
                    PrescriptionNumber = order.Prescription?.PrescriptionNumber ?? string.Empty,
                    Status = (int)order.Status,
                    StatusName = order.Status.ToString(),
                    TotalAmount = order.TotalCost,
                    DeliveryFee = order.DeliveryFee,
                    DeliveryAvailable = request.DeliveryAvailable,
                    RespondedAt = order.UpdatedAt ?? DateTime.UtcNow,
                    Message = "تم إرسال رد الصيدلية بنجاح"
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to order {OrderId} for pharmacy {PharmacyId}", orderId, pharmacyId);
                throw;
            }
        }

        private async Task SaveMedicationDetailsAsync(PharmacyOrder order, List<MedicationAvailabilityRequest> medications)
        {
            try
            {
                _logger.LogInformation("Saving medication details for order {OrderId}", order.Id);

                var existingItems = await _unitOfWork.Repository<PharmacyOrderItem>()
                    .GetQueryable()
                    .Where(poi => poi.PharmacyOrderId == order.Id)
                    .ToListAsync();

                if (existingItems.Any())
                {
                    foreach (var item in existingItems)
                    {
                        _unitOfWork.Repository<PharmacyOrderItem>().Delete(item);
                    }
                }

                foreach (var medicationRequest in medications)
                {
                    var requestedMedication = await FindMedicationByNameAsync(medicationRequest.MedicationName);
                    
                    if (requestedMedication == null)
                    {
                        _logger.LogWarning("Medication '{MedicationName}' not found in database for order {OrderId}. Creating new medication.", 
                            medicationRequest.MedicationName, order.Id);
                        
                        requestedMedication = new Medication
                        {
                            Id = Guid.NewGuid(),
                            BrandName = medicationRequest.MedicationName,
                            GenericName = null,
                            Strength = null,
                            DosageForm = DosageForm.Tablet, 
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        
                        await _unitOfWork.Repository<Medication>().AddAsync(requestedMedication);
                        _logger.LogInformation("Created new medication: {MedicationName} with ID {MedicationId}", 
                            requestedMedication.BrandName, requestedMedication.Id);
                    }

                    var status = medicationRequest.IsAvailable 
                        ? PharmacyItemStatus.Available 
                        : PharmacyItemStatus.NotAvailable;

                    Medication? alternativeMedication = null;
                    if (medicationRequest.AlternativeOne != null && !string.IsNullOrWhiteSpace(medicationRequest.AlternativeOne.MedicationName))
                    {
                        alternativeMedication = await FindMedicationByNameAsync(medicationRequest.AlternativeOne.MedicationName);
                        if (alternativeMedication == null)
                        {
                            alternativeMedication = new Medication
                            {
                                Id = Guid.NewGuid(),
                                BrandName = medicationRequest.AlternativeOne.MedicationName,
                                GenericName = null,
                                Strength = null,
                                DosageForm = DosageForm.Tablet,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            
                            await _unitOfWork.Repository<Medication>().AddAsync(alternativeMedication);
                            _logger.LogInformation("Created new alternative medication: {MedicationName} with ID {MedicationId}", 
                                alternativeMedication.BrandName, alternativeMedication.Id);
                        }
                        
                        if (alternativeMedication != null)
                        {
                            status = PharmacyItemStatus.AlternativeOffered;
                        }
                    }

                    // إنشاء PharmacyOrderItem
                    var orderItem = new PharmacyOrderItem
                    {
                        Id = Guid.NewGuid(),
                        PharmacyOrderId = order.Id,
                        RequestedMedicationId = requestedMedication.Id,
                        Status = status,
                        AvailableQuantity = medicationRequest.IsAvailable ? medicationRequest.AvailableQuantity : null,
                        UnitPrice = medicationRequest.IsAvailable ? medicationRequest.UnitPrice : null,
                        TotalPrice = medicationRequest.IsAvailable 
                            ? medicationRequest.AvailableQuantity * medicationRequest.UnitPrice 
                            : null,
                        AlternativeMedicationId = alternativeMedication?.Id,
                        AlternativeUnitPrice = medicationRequest.AlternativeOne?.UnitPrice,
                        AlternativeNotes = alternativeMedication != null 
                            ? $"بديل متاح: {alternativeMedication.BrandName}" 
                            : null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<PharmacyOrderItem>().AddAsync(orderItem);
                    _logger.LogInformation("Added PharmacyOrderItem for medication '{MedicationName}' with status {Status}", 
                        medicationRequest.MedicationName, status);
                }

                _logger.LogInformation("Successfully saved {Count} medication details for order {OrderId}", 
                    medications.Count, order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving medication details for order {OrderId}", order.Id);
                throw;
            }
        }

        private async Task<Medication?> FindMedicationByNameAsync(string medicationName)
        {
            try
            {
                return await _unitOfWork.Repository<Medication>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(m => m.BrandName.ToLower() == medicationName.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding medication by name: {MedicationName}", medicationName);
                return null;
            }
        }

        public async Task<PharmacyStatisticsResponse> GetPharmacyStatisticsAsync(Guid pharmacyId)
        {
            try
            {
                _logger.LogInformation("Getting statistics for pharmacy {PharmacyId}", pharmacyId);

                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null || pharmacy.IsDeleted)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                var today = DateTime.UtcNow.Date;
                var currentYear = DateTime.UtcNow.Year;
                var currentMonth = DateTime.UtcNow.Month;

                // Execute queries sequentially to avoid DbContext threading issues
                var newOrdersToday = await _unitOfWork.PharmacyOrders.CountNewOrdersByDateAsync(pharmacyId, today);
                var pendingOrders = await _unitOfWork.PharmacyOrders.CountPendingOrdersAsync(pharmacyId);
                var completedOrders = await _unitOfWork.PharmacyOrders.CountCompletedOrdersAsync(pharmacyId);
                var todayRevenue = await _unitOfWork.PharmacyOrders.CalculateRevenueByDateAsync(pharmacyId, today);
                var monthlyOrders = await _unitOfWork.PharmacyOrders.CountOrdersByMonthAsync(pharmacyId, currentYear, currentMonth);
                var monthlyRevenue = await _unitOfWork.PharmacyOrders.CalculateRevenueByMonthAsync(pharmacyId, currentYear, currentMonth);

                var response = new PharmacyStatisticsResponse
                {
                    NewOrdersToday = newOrdersToday,
                    PendingOrders = pendingOrders,
                    CompletedOrders = completedOrders,
                    TodayRevenue = todayRevenue,
                    MonthlyOrders = monthlyOrders,
                    MonthlyRevenue = monthlyRevenue
                };

                _logger.LogInformation("Retrieved statistics for pharmacy {PharmacyId}: NewToday={NewToday}, Pending={Pending}, Completed={Completed}",
                    pharmacyId, response.NewOrdersToday, response.PendingOrders, response.CompletedOrders);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        #endregion

        #region New Order Operations

        public async Task<PharmacyOrdersListOptimizedResponse> GetOptimizedOrdersAsync(Guid pharmacyId, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting optimized orders list for pharmacy {PharmacyId}, Page: {Page}, Size: {Size}",
                    pharmacyId, pageNumber, pageSize);

                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                var (orders, totalCount) = await _unitOfWork.PharmacyOrders
                    .GetOptimizedOrdersForPharmacyAsync(pharmacyId, pageNumber, pageSize);

                var ordersList = orders.Select(o => new PharmacyOrderListItemResponse
                {
                    OrderId = o.Id,
                    PrescriptionNumber = o.Prescription?.PrescriptionNumber ?? "N/A",
                    PatientName = $"{o.Patient?.FirstName} {o.Patient?.LastName}".Trim(),
                    OrderDate = o.CreatedAt,
                    TotalCost = o.TotalCost,
                    Status = o.Status
                }).ToList();

                var response = new PharmacyOrdersListOptimizedResponse
                {
                    Orders = ordersList,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieved {Count} orders out of {Total} for pharmacy {PharmacyId}",
                    ordersList.Count, totalCount, pharmacyId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimized orders for pharmacy {PharmacyId}", pharmacyId);
                throw;
            }
        }

        public async Task<PharmacyOrderDetailResponse> GetOrderDetailAsync(Guid pharmacyId, Guid orderId)
        {
            try
            {
                _logger.LogInformation("Getting order detail for order {OrderId}, pharmacy {PharmacyId}",
                    orderId, pharmacyId);

                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(pharmacyId);
                if (pharmacy == null)
                {
                    throw new KeyNotFoundException($"Pharmacy with ID {pharmacyId} not found");
                }

                var order = await _unitOfWork.PharmacyOrders.GetOrderDetailForPharmacyAsync(orderId, pharmacyId);
                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {orderId} not found for pharmacy {pharmacyId}");
                }

                var medications = new List<OrderMedicationDetailResponse>();

                // Get medication details from PrescribedMedications (which has dosage, frequency, duration)
                if (order.Prescription?.PrescribedMedications != null && order.Prescription.PrescribedMedications.Any())
                {
                    // Create a dictionary of medication prices from OrderItems
                    var medicationPrices = new Dictionary<Guid, decimal>();
                    if (order.OrderItems != null && order.OrderItems.Any())
                    {
                        foreach (var orderItem in order.OrderItems)
                        {
                            medicationPrices[orderItem.RequestedMedicationId] = orderItem.UnitPrice ?? 0;
                        }
                    }

                    medications = order.Prescription.PrescribedMedications.Select(pm => new OrderMedicationDetailResponse
                    {
                        MedicationName = pm.Medication?.BrandName ?? "N/A",
                        Price = medicationPrices.ContainsKey(pm.MedicationId) ? medicationPrices[pm.MedicationId] : 0,
                        Dosage = pm.Dosage ?? "N/A",
                        Frequency = pm.Frequency ?? "N/A",
                        Duration = pm.DurationDays > 0 ? $"{pm.DurationDays} أيام" : "N/A"
                    }).ToList();
                }

                var patientAddress = order.Patient?.Address != null
                    ? $"{order.Patient.Address.City}, {order.Patient.Address.Street}"
                    : "غير متوفر";

                var response = new PharmacyOrderDetailResponse
                {
                    OrderId = order.Id,
                    PrescriptionNumber = order.Prescription?.PrescriptionNumber ?? "N/A",
                    OrderDate = order.CreatedAt,
                    Status = order.Status,
                    Medications = medications,
                    DeliveryFee = order.DeliveryFee,
                    TotalCost = order.TotalCost,
                    DeliveryInfo = new OrderDeliveryInfoResponse
                    {
                        PatientName = $"{order.Patient?.FirstName} {order.Patient?.LastName}".Trim(),
                        PatientPhone = order.Patient?.PhoneNumber ?? "N/A",
                        Address = patientAddress
                    }
                };

                _logger.LogInformation("Retrieved order detail for order {OrderId}", orderId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order detail for order {OrderId}, pharmacy {PharmacyId}",
                    orderId, pharmacyId);
                throw;
            }
        }

        #endregion
    }
}
