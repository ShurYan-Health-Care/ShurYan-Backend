using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
        public class PatientLabService : IPatientLabService
        {
                private readonly IUnitOfWork _unitOfWork;
                private readonly ILogger<PatientLabService> _logger;

                public PatientLabService(IUnitOfWork unitOfWork, ILogger<PatientLabService> logger)
                {
                        _unitOfWork = unitOfWork;
                        _logger = logger;
                }

                #region Laboratory Search

                /// <summary>
                /// البحث عن أقرب 3 معامل للمريض بناءً على الإحداثيات
                /// </summary>
                public async Task<FindNearbyLaboratoriesResponse> FindNearbyLaboratoriesAsync(FindNearbyLaboratoriesRequest request)
                {
                        try
                        {
                                _logger.LogInformation("Searching for nearby laboratories at coordinates: {Latitude}, {Longitude}", 
                                    request.Latitude, request.Longitude);

                                // جلب كل المعامل المفعلة مع تفاصيلها
                                var laboratories = await _unitOfWork.Laboratories.GetAllActiveLaboratoriesWithDetailsAsync();

                                if (!laboratories.Any())
                                {
                                        _logger.LogWarning("No active laboratories found in the system");
                                        return new FindNearbyLaboratoriesResponse
                                        {
                                                NearbyLaboratories = new List<NearbyLaboratorySimpleResponse>(),
                                                TotalFound = 0,
                                                SearchRadiusKm = 0
                                        };
                                }

                                // حساب المسافة لكل معمل
                                var laboratoriesWithDistance = new List<(Core.Entities.Identity.Laboratory laboratory, double distance)>();

                                foreach (var laboratory in laboratories)
                                {
                                        if (laboratory.Address?.Latitude.HasValue == true && laboratory.Address?.Longitude.HasValue == true)
                                        {
                                                var distance = CalculateDistance(
                                                    request.Latitude, request.Longitude,
                                                    laboratory.Address.Latitude.Value, laboratory.Address.Longitude.Value);

                                                laboratoriesWithDistance.Add((laboratory, distance));
                                        }
                                }

                                // ترتيب حسب المسافة وأخذ أقرب 3
                                var nearestLaboratories = laboratoriesWithDistance
                                    .OrderBy(x => x.distance)
                                    .Take(3)
                                    .ToList();

                                // تحويل إلى Response DTOs - بس المعلومات المطلوبة
                                var nearbyLaboratoryResponses = nearestLaboratories.Select(x => new NearbyLaboratorySimpleResponse
                                {
                                        Id = x.laboratory.Id,
                                        Name = x.laboratory.Name,
                                        DistanceInKm = Math.Round(x.distance, 2),
                                        OffersHomeSampleCollection = x.laboratory.OffersHomeSampleCollection,
                                        HomeSampleCollectionFee = x.laboratory.HomeSampleCollectionFee,
                                        ProfileImageUrl = x.laboratory.ProfilePictureUrl,
                                        PhoneNumber = x.laboratory.PhoneNumber ?? string.Empty
                                }).ToList();

                                var maxDistance = nearestLaboratories.Any() ? nearestLaboratories.Max(x => x.distance) : 0;

                                _logger.LogInformation("Found {Count} nearby laboratories within {MaxDistance}km", 
                                    nearbyLaboratoryResponses.Count, Math.Round(maxDistance, 2));

                                return new FindNearbyLaboratoriesResponse
                                {
                                        NearbyLaboratories = nearbyLaboratoryResponses,
                                        TotalFound = nearbyLaboratoryResponses.Count,
                                        SearchRadiusKm = Math.Round(maxDistance, 2)
                                };
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error finding nearby laboratories for coordinates: {Latitude}, {Longitude}", 
                                    request.Latitude, request.Longitude);
                                throw;
                        }
                }

                /// <summary>
                /// البحث عن أقرب 3 معامل للمريض بناءً على عنوانه المسجل
                /// </summary>
                public async Task<FindNearbyLaboratoriesResponse> FindNearbyLaboratoriesForPatientAsync(Guid patientId)
                {
                        try
                        {
                                _logger.LogInformation("Finding nearby laboratories for patient: {PatientId}", patientId);

                                // جلب المريض مع العنوان
                                var patient = await _unitOfWork.Patients.GetPatientWithAddressAsync(patientId);
                                if (patient == null)
                                {
                                        _logger.LogWarning("Patient not found: {PatientId}", patientId);
                                        throw new KeyNotFoundException($"Patient with ID {patientId} not found");
                                }

                                // التحقق من وجود عنوان مع إحداثيات
                                if (patient.Address == null)
                                {
                                        _logger.LogWarning("Patient {PatientId} does not have a registered address", patientId);
                                        throw new KeyNotFoundException("Patient does not have a registered address");
                                }

                                if (!patient.Address.Latitude.HasValue || !patient.Address.Longitude.HasValue)
                                {
                                        _logger.LogWarning("Patient {PatientId} address does not have coordinates", patientId);
                                        throw new KeyNotFoundException("Patient address does not have coordinates (latitude/longitude)");
                                }

                                // استخدام عنوان المريض للبحث
                                var request = new FindNearbyLaboratoriesRequest
                                {
                                        Latitude = patient.Address.Latitude.Value,
                                        Longitude = patient.Address.Longitude.Value
                                };

                                _logger.LogInformation("Using patient address coordinates: {Latitude}, {Longitude}", 
                                    request.Latitude, request.Longitude);

                                return await FindNearbyLaboratoriesAsync(request);
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error finding nearby laboratories for patient: {PatientId}", patientId);
                                throw;
                        }
                }

                public async Task<IEnumerable<NearbyLaboratoryResponse>> GetNearbyLaboratoriesAsync(
                    Guid patientId,
                    double latitude,
                    double longitude,
                    double radiusInKm = 10,
                    bool? offersHomeSampleCollection = null,
                    string? searchQuery = null,
                    int pageNumber = 1,
                    int pageSize = 20)
                {
                        var laboratories = await _unitOfWork.Laboratories.GetAllAsync();
                        var result = new List<NearbyLaboratoryResponse>();

                        foreach (var lab in laboratories)
                        {
                                if (lab.Address == null) continue;
                                if (!lab.Address.Latitude.HasValue || !lab.Address.Longitude.HasValue) continue;

                                var distance = CalculateDistance(latitude, longitude, lab.Address.Latitude.Value, lab.Address.Longitude.Value);
                                if (distance > radiusInKm) continue;

                                if (offersHomeSampleCollection.HasValue && lab.OffersHomeSampleCollection != offersHomeSampleCollection.Value)
                                        continue;

                                if (!string.IsNullOrWhiteSpace(searchQuery) &&
                                    !lab.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                                        continue;

                                var servicesCount = (await _unitOfWork.LabServices.GetLabServicesAsync(lab.Id)).Count();
                                var avgRating = await _unitOfWork.LaboratoryReviews.GetAverageRatingForLaboratoryAsync(lab.Id);
                                var reviewsCount = await _unitOfWork.LaboratoryReviews.GetReviewCountForLaboratoryAsync(lab.Id);
                                var workingHours = await _unitOfWork.LabWorkingHours.GetWorkingHoursByLaboratoryAsync(lab.Id);
                                var isOpenNow = IsLaboratoryOpenNow(workingHours);
                                var todayHours = GetTodayWorkingHours(workingHours);

                                result.Add(new NearbyLaboratoryResponse
                                {
                                        Id = lab.Id,
                                        Name = lab.Name,
                                        Description = lab.Description,
                                        ProfileImageUrl = lab.ProfilePictureUrl,
                                        PhoneNumber = lab.PhoneNumber ?? string.Empty,
                                        WhatsAppNumber = lab.WhatsAppNumber,
                                        DistanceInKm = Math.Round(distance, 2),
                                        Address = $"{lab.Address?.Street}, {lab.Address?.City}",
                                        Latitude = lab.Address?.Latitude,
                                        Longitude = lab.Address?.Longitude,
                                        OffersHomeSampleCollection = lab.OffersHomeSampleCollection,
                                        HomeSampleCollectionFee = lab.HomeSampleCollectionFee,
                                        AvailableServicesCount = servicesCount,
                                        AverageRating = avgRating > 0 ? avgRating : null,
                                        TotalReviewsCount = reviewsCount,
                                        IsOpenNow = isOpenNow,
                                        TodayWorkingHours = todayHours
                                });
                        }

                        return result
                            .OrderBy(l => l.DistanceInKm)
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize);
                }

                public async Task<LaboratoryDetailResponse?> GetLaboratoryDetailsAsync(
                    Guid patientId,
                    Guid laboratoryId,
                    double? latitude = null,
                    double? longitude = null)
                {
                        var lab = await _unitOfWork.Laboratories.GetLaboratoryWithDetailsAsync(laboratoryId);
                        if (lab == null) return null;

                        var avgRating = await _unitOfWork.LaboratoryReviews.GetAverageRatingForLaboratoryAsync(laboratoryId);
                        var reviewsCount = await _unitOfWork.LaboratoryReviews.GetReviewCountForLaboratoryAsync(laboratoryId);
                        var reviews = await _unitOfWork.LaboratoryReviews.GetReviewsByLaboratoryAsync(laboratoryId);
                        var workingHours = await _unitOfWork.LabWorkingHours.GetWorkingHoursByLaboratoryAsync(laboratoryId);

                        double? distance = null;
                        if (latitude.HasValue && longitude.HasValue && lab.Address?.Latitude != null && lab.Address?.Longitude != null)
                        {
                                distance = CalculateDistance(latitude.Value, longitude.Value, lab.Address.Latitude.Value, lab.Address.Longitude.Value);
                        }

                        var ratingBreakdown = CalculateRatingBreakdown(reviews);

                        return new LaboratoryDetailResponse
                        {
                                Id = lab.Id,
                                Name = lab.Name,
                                Description = lab.Description,
                                ProfileImageUrl = lab.ProfilePictureUrl,
                                PhoneNumber = lab.PhoneNumber ?? string.Empty,
                                WhatsAppNumber = lab.WhatsAppNumber,
                                Website = lab.Website,
                                Address = $"{lab.Address?.Street}, {lab.Address?.City}",
                                City = lab.Address?.City,
                                Area = lab.Address?.Governorate.ToString(),
                                Latitude = lab.Address?.Latitude,
                                Longitude = lab.Address?.Longitude,
                                DistanceInKm = distance.HasValue ? Math.Round(distance.Value, 2) : null,
                                OffersHomeSampleCollection = lab.OffersHomeSampleCollection,
                                HomeSampleCollectionFee = lab.HomeSampleCollectionFee,
                                AverageRating = avgRating > 0 ? avgRating : null,
                                TotalReviewsCount = reviewsCount,
                                RatingBreakdown = ratingBreakdown,
                                IsOpenNow = IsLaboratoryOpenNow(workingHours),
                                WorkingHours = MapWorkingHours(workingHours)
                        };
                }

                public async Task<IEnumerable<LaboratoryServiceResponse>> GetLaboratoryServicesAsync(
                    Guid laboratoryId,
                    string? category = null)
                {
                        var services = await _unitOfWork.LabServices.GetLabServicesAsync(laboratoryId);
                        var result = new List<LaboratoryServiceResponse>();

                        foreach (var service in services)
                        {
                                var labTest = await _unitOfWork.LabTests.GetByIdAsync(service.LabTestId);
                                if (labTest == null) continue;

                                if (!string.IsNullOrWhiteSpace(category) &&
                                    !labTest.Category.ToString().Equals(category, StringComparison.OrdinalIgnoreCase))
                                        continue;

                                result.Add(new LaboratoryServiceResponse
                                {
                                        Id = service.Id,
                                        LabTestId = service.LabTestId,
                                        TestName = labTest.Name,
                                        TestCode = labTest.Code,
                                        Category = labTest.Category.ToString(),
                                        CategoryArabic = GetCategoryArabicName(labTest.Category),
                                        Price = service.Price,
                                        IsAvailable = service.IsAvailable,
                                        SpecialInstructions = labTest.SpecialInstructions,
                                        LaboratoryNotes = service.LabSpecificNotes
                                });
                        }

                        return result.OrderBy(s => s.Category).ThenBy(s => s.TestName);
                }

                public async Task<IEnumerable<LaboratoryReviewResponse>> GetLaboratoryReviewsAsync(
                    Guid laboratoryId,
                    int pageNumber = 1,
                    int pageSize = 20)
                {
                        var reviews = await _unitOfWork.LaboratoryReviews.GetReviewsByLaboratoryAsync(laboratoryId);
                        var result = new List<LaboratoryReviewResponse>();

                        foreach (var review in reviews)
                        {
                                var patient = await _unitOfWork.Patients.GetByIdAsync(review.PatientId);
                                result.Add(new LaboratoryReviewResponse
                                {
                                        Id = review.Id,
                                        PatientName = patient != null ? $"{patient.FirstName} {patient.LastName[0]}." : "مريض",
                                        PatientProfileImage = patient?.ProfilePictureUrl,
                                        OverallSatisfaction = review.OverallSatisfaction,
                                        ResultAccuracy = review.ResultAccuracy,
                                        DeliverySpeed = review.DeliverySpeed,
                                        ServiceQuality = review.ServiceQuality,
                                        ValueForMoney = review.ValueForMoney,
                                        AverageRating = review.AverageRating,
                                        CreatedAt = review.CreatedAt,
                                        IsEdited = review.IsEdited
                                });
                        }

                        return result
                            .OrderByDescending(r => r.CreatedAt)
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize);
                }

                #endregion

                #region Lab Prescriptions

                public async Task<IEnumerable<PatientLabPrescriptionResponse>> GetPatientLabPrescriptionsAsync(Guid patientId)
                {
                        var prescriptions = await _unitOfWork.LabPrescriptions.GetPagedPrescriptionsForPatientAsync(patientId, 1, 100);
                        var result = new List<PatientLabPrescriptionResponse>();

                        foreach (var prescription in prescriptions)
                        {
                                result.Add(await MapPrescriptionToResponse(prescription));
                        }

                        return result.OrderByDescending(p => p.CreatedAt);
                }

                public async Task<PatientLabPrescriptionResponse?> GetLabPrescriptionDetailsAsync(Guid patientId, Guid prescriptionId)
                {
                        var prescription = await _unitOfWork.LabPrescriptions.GetPrescriptionWithDetailsAsync(prescriptionId);
                        if (prescription == null || prescription.PatientId != patientId)
                                return null;

                        return await MapPrescriptionToResponse(prescription);
                }

                #endregion

                #region Lab Orders

                public async Task<PatientLabOrderResponse> CreateLabOrderAsync(Guid patientId, CreatePatientLabOrderRequest request)
                {
                        var prescription = await _unitOfWork.LabPrescriptions.GetPrescriptionWithDetailsAsync(request.LabPrescriptionId);
                        if (prescription == null || prescription.PatientId != patientId)
                                throw new ArgumentException("روشتة التحاليل غير موجودة");

                        var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(request.LaboratoryId);
                        if (laboratory == null)
                                throw new ArgumentException("المعمل غير موجود");

                        // Check if order already exists
                        var existingOrders = await _unitOfWork.LabOrders.GetAllAsync();
                        if (existingOrders.Any(o => o.LabPrescriptionId == request.LabPrescriptionId && o.LaboratoryId == request.LaboratoryId))
                                throw new InvalidOperationException("يوجد طلب مسبق لهذه الروشتة في هذا المعمل");

                        // Calculate total cost
                        decimal totalCost = 0;
                        foreach (var item in prescription.Items)
                        {
                                var service = await _unitOfWork.LabServices.GetLabServiceByTestAsync(request.LaboratoryId, item.LabTestId);
                                if (service != null)
                                        totalCost += service.Price;
                        }

                        var order = new LabOrder
                        {
                                Id = Guid.NewGuid(),
                                LabPrescriptionId = request.LabPrescriptionId,
                                LaboratoryId = request.LaboratoryId,
                                PatientId = patientId,
                                Status = LabOrderStatus.NewRequest,
                                SampleCollectionType = request.SampleCollectionType,
                                TestsTotalCost = totalCost,
                                SampleCollectionDeliveryCost = request.SampleCollectionType == SampleCollectionType.HomeSampleCollection
                                ? laboratory.HomeSampleCollectionFee ?? 0
                                : 0,
                                CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.LabOrders.AddAsync(order);
                        await _unitOfWork.SaveChangesAsync();

                        _logger.LogInformation("Created lab order {OrderId} for patient {PatientId}", order.Id, patientId);

                        return await MapOrderToResponse(order);
                }

                public async Task<IEnumerable<PatientLabOrderResponse>> GetPatientLabOrdersAsync(Guid patientId, LabOrderStatus? status = null)
                {
                        var orders = await _unitOfWork.LabOrders.GetPagedOrdersForPatientAsync(patientId, 1, 100);

                        if (status.HasValue)
                                orders = orders.Where(o => o.Status == status.Value);

                        var result = new List<PatientLabOrderResponse>();
                        foreach (var order in orders)
                        {
                                result.Add(await MapOrderToResponse(order));
                        }

                        return result.OrderByDescending(o => o.CreatedAt);
                }

                public async Task<PatientLabOrderResponse?> GetLabOrderDetailsAsync(Guid patientId, Guid orderId)
                {
                        var order = await _unitOfWork.LabOrders.GetOrderWithDetailsAsync(orderId);
                        if (order == null || order.PatientId != patientId)
                                return null;

                        return await MapOrderToResponse(order);
                }

                public async Task<PatientLabOrderResponse> RequestHomeSampleCollectionAsync(
                    Guid patientId,
                    Guid orderId,
                    RequestHomeSampleCollectionRequest request)
                {
                        var order = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                        if (order == null || order.PatientId != patientId)
                                throw new ArgumentException("الطلب غير موجود");

                        if (order.SampleCollectionType == SampleCollectionType.HomeSampleCollection)
                                throw new InvalidOperationException("الطلب بالفعل لخدمة سحب من البيت");

                        var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(order.LaboratoryId);
                        if (laboratory == null || !laboratory.OffersHomeSampleCollection)
                                throw new InvalidOperationException("المعمل لا يوفر خدمة سحب العينة من البيت");

                        order.SampleCollectionType = SampleCollectionType.HomeSampleCollection;
                        order.SampleCollectionDeliveryCost = laboratory.HomeSampleCollectionFee ?? 0;
                        order.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.SaveChangesAsync();

                        _logger.LogInformation("Updated order {OrderId} to home sample collection", orderId);

                        return await MapOrderToResponse(order);
                }

                public async Task<PatientLabOrderResponse> CancelLabOrderAsync(Guid patientId, Guid orderId, string reason)
                {
                        var order = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                        if (order == null || order.PatientId != patientId)
                                throw new ArgumentException("الطلب غير موجود");

                        if (order.Status == LabOrderStatus.Completed ||
                            order.Status == LabOrderStatus.CancelledByPatient ||
                            order.Status == LabOrderStatus.RejectedByLab)
                                throw new InvalidOperationException("لا يمكن إلغاء هذا الطلب");

                        order.Status = LabOrderStatus.CancelledByPatient;
                        order.CancellationReason = reason;
                        order.CancelledAt = DateTime.UtcNow;
                        order.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.SaveChangesAsync();

                        _logger.LogInformation("Patient {PatientId} cancelled order {OrderId}", patientId, orderId);

                        return await MapOrderToResponse(order);
                }

                #endregion

                #region Lab Results

                public async Task<IEnumerable<PatientLabResultResponse>> GetLabOrderResultsAsync(Guid patientId, Guid orderId)
                {
                        var order = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                        if (order == null || order.PatientId != patientId)
                                throw new ArgumentException("الطلب غير موجود");

                        var results = await _unitOfWork.LabResults.GetResultsByLabOrderAsync(orderId);
                        var response = new List<PatientLabResultResponse>();

                        foreach (var result in results)
                        {
                                var labTest = await _unitOfWork.LabTests.GetByIdAsync(result.LabTestId);
                                response.Add(new PatientLabResultResponse
                                {
                                        Id = result.Id,
                                        LabOrderId = result.LabOrderId,
                                        LabTestId = result.LabTestId,
                                        TestName = labTest?.Name ?? string.Empty,
                                        TestCode = labTest?.Code ?? string.Empty,
                                        Category = labTest?.Category.ToString() ?? string.Empty,
                                        ResultValue = result.ResultValue,
                                        ReferenceRange = result.ReferenceRange,
                                        Unit = result.Unit,
                                        Notes = result.Notes,
                                        AttachmentUrl = result.AttachmentUrl,
                                        CreatedAt = result.CreatedAt
                                });
                        }

                        return response;
                }

                #endregion

                #region Reviews

                public async Task<LaboratoryReviewResponse> CreateLaboratoryReviewAsync(
                    Guid patientId,
                    Guid orderId,
                    CreateLaboratoryReviewRequest request)
                {
                        var order = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                        if (order == null || order.PatientId != patientId)
                                throw new ArgumentException("الطلب غير موجود");

                        if (order.Status != LabOrderStatus.Completed)
                                throw new InvalidOperationException("لا يمكن تقييم طلب غير مكتمل");

                        var existingReview = await _unitOfWork.LaboratoryReviews.GetReviewByLabOrderAsync(orderId);
                        if (existingReview != null)
                                throw new InvalidOperationException("تم تقييم هذا الطلب مسبقاً");

                        var review = new LaboratoryReview
                        {
                                Id = Guid.NewGuid(),
                                LabOrderId = orderId,
                                PatientId = patientId,
                                LaboratoryId = order.LaboratoryId,
                                OverallSatisfaction = request.OverallSatisfaction,
                                ResultAccuracy = request.ResultAccuracy,
                                DeliverySpeed = request.DeliverySpeed,
                                ServiceQuality = request.ServiceQuality,
                                ValueForMoney = request.ValueForMoney,
                                CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.LaboratoryReviews.AddAsync(review);
                        await _unitOfWork.SaveChangesAsync();

                        var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);

                        _logger.LogInformation("Patient {PatientId} reviewed laboratory {LaboratoryId}", patientId, order.LaboratoryId);

                        return new LaboratoryReviewResponse
                        {
                                Id = review.Id,
                                PatientName = patient != null ? $"{patient.FirstName} {patient.LastName[0]}." : "مريض",
                                PatientProfileImage = patient?.ProfileImageUrl,
                                OverallSatisfaction = review.OverallSatisfaction,
                                ResultAccuracy = review.ResultAccuracy,
                                DeliverySpeed = review.DeliverySpeed,
                                ServiceQuality = review.ServiceQuality,
                                ValueForMoney = review.ValueForMoney,
                                AverageRating = review.AverageRating,
                                CreatedAt = review.CreatedAt,
                                IsEdited = false
                        };
                }

                #endregion

                #region Private Helper Methods

                private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
                {
                        const double R = 6371; // Earth's radius in km
                        var dLat = ToRadians(lat2 - lat1);
                        var dLon = ToRadians(lon2 - lon1);
                        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                        return R * c;
                }

                private static double ToRadians(double degrees) => degrees * Math.PI / 180;

                private static bool IsLaboratoryOpenNow(IEnumerable<LabWorkingHours> workingHours)
                {
                        var now = DateTime.Now;
                        var todayHours = workingHours.FirstOrDefault(w => w.Day == now.DayOfWeek);

                        if (todayHours == null || !todayHours.IsActive) return false;

                        var currentTime = TimeOnly.FromDateTime(now);
                        return currentTime >= todayHours.StartTime && currentTime <= todayHours.EndTime;
                }

                private static string? GetTodayWorkingHours(IEnumerable<LabWorkingHours> workingHours)
                {
                        var todayHours = workingHours.FirstOrDefault(w => w.Day == DateTime.Now.DayOfWeek);
                        if (todayHours == null || !todayHours.IsActive) return "مغلق اليوم";
                        return $"{todayHours.StartTime:HH\\:mm} - {todayHours.EndTime:HH\\:mm}";
                }

                private static LaboratoryRatingBreakdown? CalculateRatingBreakdown(IEnumerable<LaboratoryReview> reviews)
                {
                        var reviewList = reviews.ToList();
                        if (!reviewList.Any()) return null;

                        return new LaboratoryRatingBreakdown
                        {
                                OverallSatisfaction = reviewList.Average(r => r.OverallSatisfaction),
                                ResultAccuracy = reviewList.Average(r => r.ResultAccuracy),
                                DeliverySpeed = reviewList.Average(r => r.DeliverySpeed),
                                ServiceQuality = reviewList.Average(r => r.ServiceQuality),
                                ValueForMoney = reviewList.Average(r => r.ValueForMoney)
                        };
                }

                private static List<LaboratoryWorkingHoursResponse> MapWorkingHours(IEnumerable<LabWorkingHours> workingHours)
                {
                        var dayNames = new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Saturday, "السبت" },
                { DayOfWeek.Sunday, "الأحد" },
                { DayOfWeek.Monday, "الإثنين" },
                { DayOfWeek.Tuesday, "الثلاثاء" },
                { DayOfWeek.Wednesday, "الأربعاء" },
                { DayOfWeek.Thursday, "الخميس" },
                { DayOfWeek.Friday, "الجمعة" }
            };

                        return workingHours.Select(w => new LaboratoryWorkingHoursResponse
                        {
                                DayOfWeek = w.Day.ToString(),
                                DayOfWeekArabic = dayNames.GetValueOrDefault(w.Day, w.Day.ToString()),
                                IsClosed = !w.IsActive,
                                OpenTime = !w.IsActive ? null : w.StartTime.ToString(@"HH\:mm"),
                                CloseTime = !w.IsActive ? null : w.EndTime.ToString(@"HH\:mm")
                        }).ToList();
                }

                private static string GetCategoryArabicName(LabTestCategory category)
                {
                        return category switch
                        {
                                LabTestCategory.CompleteBloodCount => "تحاليل الدم الكاملة",
                                LabTestCategory.LiverFunction => "وظائف الكبد",
                                LabTestCategory.KidneyFunction => "وظائف الكلى",
                                LabTestCategory.BloodSugar => "تحاليل السكر",
                                LabTestCategory.LipidProfile => "تحاليل الدهون",
                                LabTestCategory.ThyroidFunction => "الغدة الدرقية",
                                LabTestCategory.Hormones => "الهرمونات",
                                LabTestCategory.VitaminsAndMinerals => "الفيتامينات والمعادن",
                                LabTestCategory.Immunology => "المناعة",
                                LabTestCategory.Urinalysis => "تحليل البول",
                                LabTestCategory.StoolAnalysis => "تحليل البراز",
                                LabTestCategory.Microbiology => "الميكروبيولوجي",
                                LabTestCategory.TumorMarkers => "علامات الأورام",
                                LabTestCategory.PregnancyAndFertility => "الحمل والخصوبة",
                                LabTestCategory.Other => "أخرى",
                                _ => category.ToString()
                        };
                }

                private static string GetStatusArabicName(LabOrderStatus status)
                {
                        return status switch
                        {
                                LabOrderStatus.NewRequest => "طلب جديد",
                                LabOrderStatus.AwaitingLabReview => "في انتظار مراجعة المعمل",
                                LabOrderStatus.ConfirmedByLab => "تم التأكيد من المعمل",
                                LabOrderStatus.AwaitingPayment => "في انتظار الدفع",
                                LabOrderStatus.Paid => "تم الدفع",
                                LabOrderStatus.AwaitingSamples => "في انتظار العينات",
                                LabOrderStatus.InProgressAtLab => "قيد التنفيذ في المعمل",
                                LabOrderStatus.ResultsReady => "النتائج جاهزة",
                                LabOrderStatus.Completed => "تم الاستلام",
                                LabOrderStatus.CancelledByPatient => "ملغي من المريض",
                                LabOrderStatus.RejectedByLab => "مرفوض من المعمل",
                                _ => status.ToString()
                        };
                }

                private static string GetSampleCollectionTypeArabic(SampleCollectionType type)
                {
                        return type switch
                        {
                                SampleCollectionType.LabVisit => "زيارة المعمل",
                                SampleCollectionType.HomeSampleCollection => "سحب من البيت",
                                _ => type.ToString()
                        };
                }

                private async Task<PatientLabPrescriptionResponse> MapPrescriptionToResponse(LabPrescription prescription)
                {
                        var doctor = await _unitOfWork.Doctors.GetByIdAsync(prescription.DoctorId);
                        var items = prescription.Items.ToList();
                        var tests = new List<PatientLabPrescriptionItemResponse>();

                        foreach (var item in items)
                        {
                                var labTest = await _unitOfWork.LabTests.GetByIdAsync(item.LabTestId);
                                if (labTest != null)
                                {
                                        tests.Add(new PatientLabPrescriptionItemResponse
                                        {
                                                Id = item.Id,
                                                LabTestId = item.LabTestId,
                                                TestName = labTest.Name,
                                                TestCode = labTest.Code,
                                                Category = labTest.Category.ToString(),
                                                SpecialInstructions = labTest.SpecialInstructions,
                                                DoctorNotes = item.DoctorNotes
                                        });
                                }
                        }

                        var order = prescription.LabOrder;

                        return new PatientLabPrescriptionResponse
                        {
                                Id = prescription.Id,
                                AppointmentId = prescription.AppointmentId,
                                DoctorId = prescription.DoctorId,
                                DoctorName = doctor != null ? $"د. {doctor.FirstName} {doctor.LastName}" : string.Empty,
                                DoctorSpecialty = doctor?.MedicalSpecialty.ToString(),
                                DoctorProfileImage = doctor?.ProfilePictureUrl,
                                GeneralNotes = prescription.GeneralNotes,
                                CreatedAt = prescription.CreatedAt,
                                Tests = tests,
                                HasOrder = order != null,
                                LabOrderId = order?.Id,
                                OrderStatus = order != null ? GetStatusArabicName(order.Status) : null
                        };
                }

                private async Task<PatientLabOrderResponse> MapOrderToResponse(LabOrder order)
                {
                        var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(order.LaboratoryId);
                        var avgRating = await _unitOfWork.LaboratoryReviews.GetAverageRatingForLaboratoryAsync(order.LaboratoryId);
                        var prescription = await _unitOfWork.LabPrescriptions.GetByIdAsync(order.LabPrescriptionId);
                        var results = await _unitOfWork.LabResults.GetResultsByLabOrderAsync(order.Id);

                        var tests = new List<PatientLabOrderTestResponse>();
                        if (prescription != null)
                        {
                                foreach (var item in prescription.Items)
                                {
                                        var labTest = await _unitOfWork.LabTests.GetByIdAsync(item.LabTestId);
                                        var service = await _unitOfWork.LabServices.GetLabServiceByTestAsync(order.LaboratoryId, item.LabTestId);
                                        var hasResult = results.Any(r => r.LabTestId == item.LabTestId);

                                        if (labTest != null)
                                        {
                                                tests.Add(new PatientLabOrderTestResponse
                                                {
                                                        LabTestId = item.LabTestId,
                                                        TestName = labTest.Name,
                                                        TestCode = labTest.Code,
                                                        Category = labTest.Category.ToString(),
                                                        Price = service?.Price ?? 0,
                                                        HasResult = hasResult
                                                });
                                        }
                                }
                        }

                        return new PatientLabOrderResponse
                        {
                                Id = order.Id,
                                LabPrescriptionId = order.LabPrescriptionId,
                                LaboratoryId = order.LaboratoryId,
                                LaboratoryName = laboratory?.Name ?? string.Empty,
                                LaboratoryProfileImage = laboratory?.ProfilePictureUrl,
                                LaboratoryPhone = laboratory?.PhoneNumber,
                                LaboratoryRating = avgRating > 0 ? avgRating : null,
                                Status = order.Status.ToString(),
                                StatusArabic = GetStatusArabicName(order.Status),
                                SampleCollectionType = order.SampleCollectionType.ToString(),
                                SampleCollectionTypeArabic = GetSampleCollectionTypeArabic(order.SampleCollectionType),
                                TestsTotalCost = order.TestsTotalCost,
                                SampleCollectionDeliveryCost = order.SampleCollectionDeliveryCost,
                                CreatedAt = order.CreatedAt,
                                ConfirmedByLabAt = order.ConfirmedByLabAt,
                                CancelledAt = order.CancelledAt,
                                CancellationReason = order.CancellationReason,
                                Tests = tests,
                                HasResults = results.Any(),
                                ResultsCount = results.Count()
                        };
                }

                #endregion
        }
}
