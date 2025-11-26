using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
        public class LaboratoryProfileService : ILaboratoryProfileService
        {
                private readonly IUnitOfWork _unitOfWork;
                private readonly IMapper _mapper;
                private readonly ILogger<LaboratoryProfileService> _logger;
                private readonly IFileUploadService _fileUploadService;

                public LaboratoryProfileService(
                    IUnitOfWork unitOfWork,
                    IMapper mapper,
                    ILogger<LaboratoryProfileService> logger,
                    IFileUploadService fileUploadService)
                {
                        _unitOfWork = unitOfWork;
                        _mapper = mapper;
                        _logger = logger;
                        _fileUploadService = fileUploadService;
                }

                #region Basic Info Operations

                public async Task<LaboratoryBasicInfoResponse?> GetBasicInfoAsync(Guid laboratoryId)
                {
                        try
                        {
                                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
                                if (laboratory == null || laboratory.IsDeleted)
                                {
                                        _logger.LogWarning("Laboratory with ID {LaboratoryId} not found", laboratoryId);
                                        return null;
                                }

                                return new LaboratoryBasicInfoResponse
                                {
                                        Name = laboratory.Name,
                                        Description = laboratory.Description,
                                        Email = laboratory.Email,
                                        PhoneNumber = laboratory.PhoneNumber,
                                        WhatsAppNumber = laboratory.WhatsAppNumber,
                                        Website = laboratory.Website,
                                        ProfileImageUrl = laboratory.ProfilePictureUrl
                                };
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving basic info for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                public async Task<LaboratoryBasicInfoResponse> UpdateBasicInfoAsync(Guid laboratoryId, UpdateLaboratoryBasicInfoRequest request)
                {
                        try
                        {
                                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
                                if (laboratory == null || laboratory.IsDeleted)
                                {
                                        throw new KeyNotFoundException($"Laboratory with ID {laboratoryId} not found");
                                }

                                // تحديث الحقول اللي اتبعتت بس (Partial Update)
                                if (!string.IsNullOrWhiteSpace(request.Name))
                                {
                                        laboratory.Name = request.Name;
                                }

                                if (request.Description != null)
                                {
                                        laboratory.Description = request.Description;
                                }

                                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                                {
                                        laboratory.PhoneNumber = request.PhoneNumber;
                                        laboratory.PhoneNumberConfirmed = false; // Reset confirmation
                                }

                                if (request.WhatsAppNumber != null)
                                {
                                        laboratory.WhatsAppNumber = request.WhatsAppNumber;
                                }

                                if (request.Website != null)
                                {
                                        laboratory.Website = request.Website;
                                }

                                laboratory.UpdatedAt = DateTime.UtcNow;
                                await _unitOfWork.SaveChangesAsync();

                                _logger.LogInformation("Updated basic info for laboratory {LaboratoryId}", laboratoryId);

                                return new LaboratoryBasicInfoResponse
                                {
                                        Name = laboratory.Name,
                                        Description = laboratory.Description,
                                        Email = laboratory.Email,
                                        PhoneNumber = laboratory.PhoneNumber,
                                        WhatsAppNumber = laboratory.WhatsAppNumber,
                                        Website = laboratory.Website,
                                        ProfileImageUrl = laboratory.ProfilePictureUrl
                                };
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating basic info for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                public async Task<string> UpdateProfileImageAsync(Guid laboratoryId, IFormFile image)
                {
                        try
                        {
                                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
                                if (laboratory == null || laboratory.IsDeleted)
                                {
                                        throw new KeyNotFoundException($"Laboratory with ID {laboratoryId} not found");
                                }

                                // حذف الصورة القديمة لو موجودة
                                if (!string.IsNullOrEmpty(laboratory.ProfilePictureUrl))
                                {
                                        await _fileUploadService.DeleteFileAsync(laboratory.ProfilePictureUrl);
                                }

                                // رفع الصورة على Cloudinary
                                var uploadResult = await _fileUploadService.UploadProfileImageAsync(image, laboratoryId.ToString());
                                laboratory.ProfilePictureUrl = uploadResult.FileUrl;
                                laboratory.UpdatedAt = DateTime.UtcNow;
                                await _unitOfWork.SaveChangesAsync();

                                _logger.LogInformation("Updated profile image for laboratory {LaboratoryId}", laboratoryId);

                                return uploadResult.FileUrl;
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating profile image for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                #endregion

                #region Address Operations

                public async Task<LaboratoryAddressResponse?> GetAddressAsync(Guid laboratoryId)
                {
                        try
                        {
                                var laboratory = await _unitOfWork.Laboratories.GetLaboratoryWithDetailsAsync(laboratoryId);
                                if (laboratory == null || laboratory.IsDeleted)
                                {
                                        _logger.LogWarning("Laboratory with ID {LaboratoryId} not found", laboratoryId);
                                        return null;
                                }

                                if (laboratory.Address == null)
                                {
                                        return new LaboratoryAddressResponse();
                                }

                                return new LaboratoryAddressResponse
                                {
                                        Street = laboratory.Address.Street,
                                        City = laboratory.Address.City,
                                        Governorate = laboratory.Address.Governorate.ToString(),
                                        BuildingNumber = laboratory.Address.BuildingNumber,
                                        Latitude = laboratory.Address.Latitude,
                                        Longitude = laboratory.Address.Longitude
                                };
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving address for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                public async Task<LaboratoryAddressResponse> UpdateAddressAsync(Guid laboratoryId, UpdateLaboratoryAddressRequest request)
                {
                        try
                        {
                                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
                                if (laboratory == null || laboratory.IsDeleted)
                                {
                                        throw new KeyNotFoundException($"Laboratory with ID {laboratoryId} not found");
                                }

                                var addressRepo = _unitOfWork.Repository<Address>();
                                Address address;
                                bool isNewAddress = false;

                                // لو مفيش عنوان، نعمل واحد جديد
                                if (laboratory.AddressId == null)
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

                                        // Update laboratory's AddressId
                                        laboratory.AddressId = address.Id;
                                        _unitOfWork.Laboratories.Update(laboratory);

                                        isNewAddress = true;
                                }
                                else
                                {
                                        // Get existing address
                                        address = await addressRepo.GetByIdAsync(laboratory.AddressId.Value);
                                        if (address == null)
                                        {
                                                throw new KeyNotFoundException($"Address with ID {laboratory.AddressId.Value} not found");
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

                                _logger.LogInformation("Updated address for laboratory {LaboratoryId}, IsNew: {IsNew}", laboratoryId, isNewAddress);

                                return new LaboratoryAddressResponse
                                {
                                        Street = address.Street,
                                        City = address.City,
                                        Governorate = address.Governorate.ToString(),
                                        BuildingNumber = address.BuildingNumber,
                                        Latitude = address.Latitude,
                                        Longitude = address.Longitude
                                };
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating address for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                #endregion

                #region Working Hours Operations

                public async Task<LaboratoryWorkingHoursResponse> GetWorkingHoursAsync(Guid laboratoryId)
                {
                        try
                        {
                                var workingHours = await _unitOfWork.LabWorkingHours.GetWorkingHoursByLaboratoryAsync(laboratoryId);

                                var response = new LaboratoryWorkingHoursResponse();

                                // تحويل كل يوم من الأسبوع
                                foreach (var wh in workingHours)
                                {
                                        var daySchedule = ConvertToDayScheduleResponse(wh);

                                        switch (wh.Day)
                                        {
                                                case DayOfWeek.Saturday:
                                                        response.Saturday = daySchedule;
                                                        break;
                                                case DayOfWeek.Sunday:
                                                        response.Sunday = daySchedule;
                                                        break;
                                                case DayOfWeek.Monday:
                                                        response.Monday = daySchedule;
                                                        break;
                                                case DayOfWeek.Tuesday:
                                                        response.Tuesday = daySchedule;
                                                        break;
                                                case DayOfWeek.Wednesday:
                                                        response.Wednesday = daySchedule;
                                                        break;
                                                case DayOfWeek.Thursday:
                                                        response.Thursday = daySchedule;
                                                        break;
                                                case DayOfWeek.Friday:
                                                        response.Friday = daySchedule;
                                                        break;
                                        }
                                }

                                return response;
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving working hours for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                public async Task<LaboratoryWorkingHoursResponse> UpdateWorkingHoursAsync(Guid laboratoryId, UpdateLaboratoryWorkingHoursRequest request)
                {
                        try
                        {
                                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
                                if (laboratory == null || laboratory.IsDeleted)
                                {
                                        throw new KeyNotFoundException($"Laboratory with ID {laboratoryId} not found");
                                }

                                var schedule = request.WeeklySchedule;

                                // تحديث كل يوم لوحده (Partial Update)
                                await UpdateDaySchedule(laboratoryId, DayOfWeek.Saturday, schedule.Saturday);
                                await UpdateDaySchedule(laboratoryId, DayOfWeek.Sunday, schedule.Sunday);
                                await UpdateDaySchedule(laboratoryId, DayOfWeek.Monday, schedule.Monday);
                                await UpdateDaySchedule(laboratoryId, DayOfWeek.Tuesday, schedule.Tuesday);
                                await UpdateDaySchedule(laboratoryId, DayOfWeek.Wednesday, schedule.Wednesday);
                                await UpdateDaySchedule(laboratoryId, DayOfWeek.Thursday, schedule.Thursday);
                                await UpdateDaySchedule(laboratoryId, DayOfWeek.Friday, schedule.Friday);

                                await _unitOfWork.SaveChangesAsync();

                                _logger.LogInformation("Updated working hours for laboratory {LaboratoryId}", laboratoryId);

                                // جلب البيانات المحدثة
                                return await GetWorkingHoursAsync(laboratoryId);
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating working hours for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                #endregion

                #region Home Sample Collection Operations

                public async Task<LaboratoryHomeSampleCollectionResponse> GetHomeSampleCollectionSettingsAsync(Guid laboratoryId)
                {
                        try
                        {
                                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
                                if (laboratory == null || laboratory.IsDeleted)
                                {
                                        throw new KeyNotFoundException($"Laboratory with ID {laboratoryId} not found");
                                }

                                return new LaboratoryHomeSampleCollectionResponse
                                {
                                        OffersHomeSampleCollection = laboratory.OffersHomeSampleCollection,
                                        HomeSampleCollectionFee = laboratory.HomeSampleCollectionFee
                                };
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving home sample collection settings for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                public async Task<LaboratoryHomeSampleCollectionResponse> UpdateHomeSampleCollectionSettingsAsync(Guid laboratoryId, UpdateLaboratoryHomeSampleCollectionRequest request)
                {
                        try
                        {
                                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
                                if (laboratory == null || laboratory.IsDeleted)
                                {
                                        throw new KeyNotFoundException($"Laboratory with ID {laboratoryId} not found");
                                }

                                // Validation: لو الخدمة متاحة، لازم يكون في سعر
                                if (request.OffersHomeSampleCollection && !request.HomeSampleCollectionFee.HasValue)
                                {
                                        throw new InvalidOperationException("يجب تحديد رسوم خدمة سحب العينة من البيت عند تفعيل الخدمة");
                                }

                                laboratory.OffersHomeSampleCollection = request.OffersHomeSampleCollection;
                                laboratory.HomeSampleCollectionFee = request.OffersHomeSampleCollection ? request.HomeSampleCollectionFee : null;
                                laboratory.UpdatedAt = DateTime.UtcNow;

                                await _unitOfWork.SaveChangesAsync();

                                _logger.LogInformation("Updated home sample collection settings for laboratory {LaboratoryId}", laboratoryId);

                                return new LaboratoryHomeSampleCollectionResponse
                                {
                                        OffersHomeSampleCollection = laboratory.OffersHomeSampleCollection,
                                        HomeSampleCollectionFee = laboratory.HomeSampleCollectionFee
                                };
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating home sample collection settings for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                #endregion

                #region Helper Methods

                private LaboratoryDayScheduleResponse ConvertToDayScheduleResponse(LabWorkingHours workingHours)
                {
                        var (fromTime, fromPeriod) = ConvertTo12HourFormat(workingHours.StartTime);
                        var (toTime, toPeriod) = ConvertTo12HourFormat(workingHours.EndTime);

                        return new LaboratoryDayScheduleResponse
                        {
                                Enabled = workingHours.IsActive,
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

                private async Task UpdateDaySchedule(Guid laboratoryId, DayOfWeek dayOfWeek, LaboratoryDayScheduleDto? daySchedule)
                {
                        // لو اليوم مش موجود في الـ request، متعملش حاجة
                        if (daySchedule == null)
                                return;

                        // جلب ساعات العمل الحالية لليوم ده
                        var existingSchedules = await _unitOfWork.LabWorkingHours.GetWorkingHoursByLaboratoryAsync(laboratoryId);
                        var existingSchedule = existingSchedules.FirstOrDefault(wh => wh.Day == dayOfWeek);

                        // لو اليوم disabled، امسح الساعات لو موجودة
                        if (!daySchedule.Enabled)
                        {
                                if (existingSchedule != null)
                                {
                                        _unitOfWork.LabWorkingHours.Delete(existingSchedule);
                                        _logger.LogInformation("Disabled day {Day} for laboratory {LaboratoryId}", dayOfWeek, laboratoryId);
                                }
                                return;
                        }

                        // لو اليوم enabled، لازم يكون فيه أوقات
                        if (string.IsNullOrWhiteSpace(daySchedule.FromTime) ||
                            string.IsNullOrWhiteSpace(daySchedule.ToTime) ||
                            string.IsNullOrWhiteSpace(daySchedule.FromPeriod) ||
                            string.IsNullOrWhiteSpace(daySchedule.ToPeriod))
                        {
                                throw new InvalidOperationException($"يجب تحديد أوقات العمل لليوم {dayOfWeek}");
                        }

                        // تحويل الأوقات من 12 ساعة إلى 24 ساعة
                        var startTime = ConvertTo24HourFormat(daySchedule.FromTime, daySchedule.FromPeriod);
                        var endTime = ConvertTo24HourFormat(daySchedule.ToTime, daySchedule.ToPeriod);

                        if (existingSchedule != null)
                        {
                                // تحديث الساعات الموجودة
                                existingSchedule.StartTime = startTime;
                                existingSchedule.EndTime = endTime;
                                existingSchedule.IsActive = true;
                                existingSchedule.UpdatedAt = DateTime.UtcNow;
                                _unitOfWork.LabWorkingHours.Update(existingSchedule);
                        }
                        else
                        {
                                // إضافة ساعات جديدة
                                var newSchedule = new LabWorkingHours
                                {
                                        Id = Guid.NewGuid(),
                                        LaboratoryId = laboratoryId,
                                        Day = dayOfWeek,
                                        StartTime = startTime,
                                        EndTime = endTime,
                                        IsActive = true,
                                        CreatedAt = DateTime.UtcNow
                                };
                                await _unitOfWork.LabWorkingHours.AddAsync(newSchedule);
                        }

                        _logger.LogInformation("Updated working hours for day {Day} for laboratory {LaboratoryId}", dayOfWeek, laboratoryId);
                }

                private TimeOnly ConvertTo24HourFormat(string time, string period)
                {
                        if (!int.TryParse(time, out var hour))
                        {
                                throw new InvalidOperationException($"صيغة الساعة غير صحيحة: {time}");
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

                #region Lab Services Operations

                public async Task<IEnumerable<LabServiceDetailResponse>> GetLabServicesAsync(Guid laboratoryId)
                {
                        try
                        {
                                var services = await _unitOfWork.LabServices.GetLabServicesAsync(laboratoryId);

                                return services.Select(s => MapToLabServiceDetailResponse(s));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving lab services for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                public async Task<LabServiceDetailResponse?> GetLabServiceByIdAsync(Guid laboratoryId, Guid serviceId)
                {
                        try
                        {
                                var service = await _unitOfWork.LabServices.GetByIdAsync(serviceId);
                                if (service == null || service.LaboratoryId != laboratoryId)
                                {
                                        return null;
                                }

                                // Load LabTest if not loaded
                                if (service.LabTest == null)
                                {
                                        service.LabTest = await _unitOfWork.LabTests.GetByIdAsync(service.LabTestId);
                                }

                                return MapToLabServiceDetailResponse(service);
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving lab service {ServiceId} for laboratory {LaboratoryId}", serviceId, laboratoryId);
                                throw;
                        }
                }

                public async Task<LabServiceDetailResponse> AddLabServiceAsync(Guid laboratoryId, AddLabServiceRequest request)
                {
                        try
                        {
                                // Check if lab test exists
                                var labTest = await _unitOfWork.LabTests.GetByIdAsync(request.LabTestId);
                                if (labTest == null)
                                {
                                        throw new KeyNotFoundException($"التحليل غير موجود");
                                }

                                // Check if service already exists for this lab
                                var existingService = await _unitOfWork.LabServices.GetLabServiceByTestAsync(laboratoryId, request.LabTestId);
                                if (existingService != null)
                                {
                                        throw new InvalidOperationException("هذا التحليل موجود بالفعل في قائمة خدمات المعمل");
                                }

                                var newService = new LabService
                                {
                                        Id = Guid.NewGuid(),
                                        LaboratoryId = laboratoryId,
                                        LabTestId = request.LabTestId,
                                        Price = request.Price,
                                        IsAvailable = request.IsAvailable,
                                        LabSpecificNotes = request.LabSpecificNotes,
                                        CreatedAt = DateTime.UtcNow
                                };

                                await _unitOfWork.LabServices.AddAsync(newService);
                                await _unitOfWork.SaveChangesAsync();

                                newService.LabTest = labTest;

                                _logger.LogInformation("Added lab service {ServiceId} for laboratory {LaboratoryId}", newService.Id, laboratoryId);

                                return MapToLabServiceDetailResponse(newService);
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error adding lab service for laboratory {LaboratoryId}", laboratoryId);
                                throw;
                        }
                }

                public async Task<LabServiceDetailResponse> UpdateLabServiceAsync(Guid laboratoryId, Guid serviceId, UpdateLabServiceRequest request)
                {
                        try
                        {
                                var service = await _unitOfWork.LabServices.GetByIdAsync(serviceId);
                                if (service == null || service.LaboratoryId != laboratoryId)
                                {
                                        throw new KeyNotFoundException("الخدمة غير موجودة");
                                }

                                // Partial update
                                if (request.Price.HasValue)
                                {
                                        service.Price = request.Price.Value;
                                }

                                if (request.LabSpecificNotes != null)
                                {
                                        service.LabSpecificNotes = request.LabSpecificNotes;
                                }

                                service.UpdatedAt = DateTime.UtcNow;
                                _unitOfWork.LabServices.Update(service);
                                await _unitOfWork.SaveChangesAsync();

                                // Load LabTest if not loaded
                                if (service.LabTest == null)
                                {
                                        service.LabTest = await _unitOfWork.LabTests.GetByIdAsync(service.LabTestId);
                                }

                                _logger.LogInformation("Updated lab service {ServiceId} for laboratory {LaboratoryId}", serviceId, laboratoryId);

                                return MapToLabServiceDetailResponse(service);
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating lab service {ServiceId} for laboratory {LaboratoryId}", serviceId, laboratoryId);
                                throw;
                        }
                }

                public async Task<bool> DeleteLabServiceAsync(Guid laboratoryId, Guid serviceId)
                {
                        try
                        {
                                var service = await _unitOfWork.LabServices.GetByIdAsync(serviceId);
                                if (service == null || service.LaboratoryId != laboratoryId)
                                {
                                        return false;
                                }

                                _unitOfWork.LabServices.Delete(service);
                                await _unitOfWork.SaveChangesAsync();

                                _logger.LogInformation("Deleted lab service {ServiceId} for laboratory {LaboratoryId}", serviceId, laboratoryId);

                                return true;
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error deleting lab service {ServiceId} for laboratory {LaboratoryId}", serviceId, laboratoryId);
                                throw;
                        }
                }

                public async Task<LabServiceDetailResponse> UpdateLabServiceAvailabilityAsync(Guid laboratoryId, Guid serviceId, UpdateLabServiceAvailabilityRequest request)
                {
                        try
                        {
                                var service = await _unitOfWork.LabServices.GetByIdAsync(serviceId);
                                if (service == null || service.LaboratoryId != laboratoryId)
                                {
                                        throw new KeyNotFoundException("الخدمة غير موجودة");
                                }

                                service.IsAvailable = request.IsAvailable;
                                service.UpdatedAt = DateTime.UtcNow;
                                _unitOfWork.LabServices.Update(service);
                                await _unitOfWork.SaveChangesAsync();

                                // Load LabTest if not loaded
                                if (service.LabTest == null)
                                {
                                        service.LabTest = await _unitOfWork.LabTests.GetByIdAsync(service.LabTestId);
                                }

                                _logger.LogInformation("Updated availability for lab service {ServiceId} to {IsAvailable} for laboratory {LaboratoryId}",
                                        serviceId, request.IsAvailable, laboratoryId);

                                return MapToLabServiceDetailResponse(service);
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating availability for lab service {ServiceId} for laboratory {LaboratoryId}", serviceId, laboratoryId);
                                throw;
                        }
                }

                private LabServiceDetailResponse MapToLabServiceDetailResponse(LabService service)
                {
                        return new LabServiceDetailResponse
                        {
                                Id = service.Id,
                                LabTestId = service.LabTestId,
                                LabTestName = service.LabTest?.Name ?? string.Empty,
                                LabTestCode = service.LabTest?.Code ?? string.Empty,
                                LabTestCategory = service.LabTest?.Category.ToString() ?? string.Empty,
                                SpecialInstructions = service.LabTest?.SpecialInstructions,
                                Price = service.Price,
                                IsAvailable = service.IsAvailable,
                                LabSpecificNotes = service.LabSpecificNotes,
                                CreatedAt = service.CreatedAt,
                                UpdatedAt = service.UpdatedAt
                        };
                }

                #endregion

                #region Available Lab Tests

                public async Task<IEnumerable<LabTestListResponse>> GetAvailableLabTestsAsync(string? searchTerm = null, string? category = null)
                {
                        try
                        {
                                IEnumerable<LabTest> tests;

                                if (!string.IsNullOrWhiteSpace(searchTerm))
                                {
                                        tests = await _unitOfWork.LabTests.SearchTestsAsync(searchTerm);
                                }
                                else if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<Core.Enums.Laboratory.LabTestCategory>(category, true, out var categoryEnum))
                                {
                                        tests = await _unitOfWork.LabTests.GetTestsByCategoryAsync(categoryEnum);
                                }
                                else
                                {
                                        tests = await _unitOfWork.LabTests.GetAllAsync();
                                }

                                return tests.Select(t => new LabTestListResponse
                                {
                                        Id = t.Id,
                                        Name = t.Name,
                                        Code = t.Code,
                                        Category = t.Category.ToString(),
                                        SpecialInstructions = t.SpecialInstructions
                                });
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving available lab tests");
                                throw;
                        }
                }

                #endregion
        }
}
