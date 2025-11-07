//using AutoMapper;
//using Microsoft.Extensions.Logging;
//using Shuryan.Application.DTOs.Requests.Laboratory;
//using Shuryan.Application.DTOs.Responses.Laboratory;
//using Shuryan.Application.Interfaces;
//using Shuryan.Core.Entities.Identity;
//using Shuryan.Core.Interfaces.UnitOfWork;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Shuryan.Application.Services
//{
//    public class LaboratoryService : ILaboratoryService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IMapper _mapper;
//        private readonly ILogger<LaboratoryService> _logger;

//        public LaboratoryService(
//            IUnitOfWork unitOfWork,
//            IMapper mapper,
//            ILogger<LaboratoryService> logger)
//        {
//            _unitOfWork = unitOfWork;
//            _mapper = mapper;
//            _logger = logger;
//        }
        
//        #region CRUD Operations
//        public async Task<IEnumerable<LaboratoryResponse>> GetAllLaboratoriesAsync(
//          string? searchTerm = null,
//          bool? offersHomeSampleCollection = null,
//          bool includeInactive = false)
//        {
//            try
//            {
//                var laboratories = (await _unitOfWork.Laboratories.GetAllAsync()).ToList();

//                // Apply filters
//                if (!includeInactive)
//                    laboratories = laboratories.Where(l => l.LaboratoryStatus == Core.Enums.Identity.Status.Active).ToList();

//                if (!string.IsNullOrWhiteSpace(searchTerm))
//                {
//                    var searchLower = searchTerm.ToLower();
//                    laboratories = laboratories.Where(l =>
//                        l.Name.ToLower().Contains(searchLower) ||
//                        (l.Description != null && l.Description.ToLower().Contains(searchLower))
//                    ).ToList();
//                }

//                if (offersHomeSampleCollection.HasValue)
//                    laboratories = laboratories.Where(l => l.OffersHomeSampleCollection == offersHomeSampleCollection.Value).ToList();

//                // Map to response (simple mapping first to avoid errors)
//                var responses = _mapper.Map<IEnumerable<LaboratoryResponse>>(laboratories).ToList();

//                // Load related data for each laboratory
//                foreach (var response in responses)
//                {
//                    try
//                    {
//                        var lab = laboratories.FirstOrDefault(l => l.Id == response.Id);
//                        if (lab == null) continue;

//                        // Load Address
//                        if (lab.AddressId.HasValue && lab.AddressId.Value != Guid.Empty)
//                        {
//                            try
//                            {
//                                var address = await _unitOfWork.Addresses.GetByIdAsync(lab.AddressId.Value);
//                                if (address != null)
//                                    response.Address = _mapper.Map<DTOs.Common.Address.AddressResponse>(address);
//                            }
//                            catch (Exception ex)
//                            {
//                                _logger.LogWarning(ex, "Error loading address for laboratory {LaboratoryId}", lab.Id);
//                            }
//                        }

//                        // Load Working Hours
//                        try
//                        {
//                            var workingHours = await _unitOfWork.LabWorkingHours.GetAllAsync();
//                            var labWorkingHours = workingHours.Where(w => w.LaboratoryId == lab.Id).ToList();
//                            response.WorkingHours = _mapper.Map<IEnumerable<LabWorkingHoursResponse>>(labWorkingHours);
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogWarning(ex, "Error loading working hours for laboratory {LaboratoryId}", lab.Id);
//                            response.WorkingHours = new List<LabWorkingHoursResponse>();
//                        }

//                        // Load Lab Services with Test Names
//                        try
//                        {
//                            var labServices = await _unitOfWork.LabServices.GetAllAsync();
//                            var services = labServices.Where(s => s.LaboratoryId == lab.Id && s.IsAvailable).ToList();
//                            var serviceResponses = new List<LabServiceResponse>();

//                            foreach (var service in services)
//                            {
//                                var serviceResponse = _mapper.Map<LabServiceResponse>(service);

//                                // Load Lab Test details
//                                try
//                                {
//                                    var labTest = await _unitOfWork.LabTests.GetByIdAsync(service.LabTestId);
//                                    if (labTest != null)
//                                    {
//                                        serviceResponse.LabTestName = labTest.Name;
//                                        serviceResponse.LabTestCategory = labTest.Category.ToString();
//                                    }
//                                }
//                                catch (Exception testEx)
//                                {
//                                    _logger.LogWarning(testEx, "Error loading lab test {LabTestId}", service.LabTestId);
//                                }

//                                serviceResponses.Add(serviceResponse);
//                            }

//                            response.LabServices = serviceResponses;
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogWarning(ex, "Error loading services for laboratory {LaboratoryId}", lab.Id);
//                            response.LabServices = new List<LabServiceResponse>();
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogWarning(ex, "Error loading details for laboratory {LaboratoryId}", response.Id);
//                    }
//                }

//                _logger.LogInformation("Retrieved {Count} laboratories", responses.Count);
//                return responses;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting all laboratories");
//                throw;
//            }
//        }

//        public async Task<LaboratoryResponse?> GetLaboratoryByIdAsync(Guid id)
//        {
//            try
//            {
//                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(id);
//                if (laboratory == null)
//                {
//                    _logger.LogWarning("Laboratory with ID {LaboratoryId} not found", id);
//                    return null;
//                }

//                var response = _mapper.Map<LaboratoryResponse>(laboratory);

//                // Load Address
//                if (laboratory.AddressId.HasValue && laboratory.AddressId.Value != Guid.Empty)
//                {
//                    try
//                    {
//                        var address = await _unitOfWork.Addresses.GetByIdAsync(laboratory.AddressId.Value);
//                        if (address != null)
//                            response.Address = _mapper.Map<DTOs.Common.Address.AddressResponse>(address);
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogWarning(ex, "Error loading address for laboratory {LaboratoryId}", id);
//                    }
//                }

//                // Load Working Hours
//                try
//                {
//                    var workingHours = await _unitOfWork.LabWorkingHours.GetAllAsync();
//                    var labWorkingHours = workingHours.Where(w => w.LaboratoryId == id).ToList();
//                    response.WorkingHours = _mapper.Map<IEnumerable<LabWorkingHoursResponse>>(labWorkingHours);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogWarning(ex, "Error loading working hours for laboratory {LaboratoryId}", id);
//                    response.WorkingHours = new List<LabWorkingHoursResponse>();
//                }

//                // Load Lab Services with Test Names
//                try
//                {
//                    var labServices = await _unitOfWork.LabServices.GetAllAsync();
//                    var services = labServices.Where(s => s.LaboratoryId == id && s.IsAvailable).ToList();
//                    var serviceResponses = new List<LabServiceResponse>();

//                    foreach (var service in services)
//                    {
//                        var serviceResponse = _mapper.Map<LabServiceResponse>(service);

//                        // Load Lab Test details
//                        try
//                        {
//                            var labTest = await _unitOfWork.LabTests.GetByIdAsync(service.LabTestId);
//                            if (labTest != null)
//                            {
//                                serviceResponse.LabTestName = labTest.Name;
//                                serviceResponse.LabTestCategory = labTest.Category.ToString();
//                            }
//                        }
//                        catch (Exception testEx)
//                        {
//                            _logger.LogWarning(testEx, "Error loading lab test {LabTestId}", service.LabTestId);
//                        }

//                        serviceResponses.Add(serviceResponse);
//                    }

//                    response.LabServices = serviceResponses;
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogWarning(ex, "Error loading services for laboratory {LaboratoryId}", id);
//                    response.LabServices = new List<LabServiceResponse>();
//                }

//                _logger.LogInformation("Retrieved laboratory {LaboratoryId}", id);
//                return response;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting laboratory {LaboratoryId}", id);
//                throw;
//            }
//        }

//        public async Task<LaboratoryBasicResponse?> GetLaboratoryBasicInfoAsync(Guid id)
//        {
//            try
//            {
//                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(id);
//                if (laboratory == null)
//                    return null;

//                var response = _mapper.Map<LaboratoryBasicResponse>(laboratory);
//                return response;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting laboratory basic info {LaboratoryId}", id);
//                throw;
//            }
//        }

//        public async Task<LaboratoryResponse> CreateLaboratoryAsync(CreateLaboratoryRequest request)
//        {
//            try
//            {
//                // Check for duplicate laboratory name
//                var allLabs = await _unitOfWork.Laboratories.GetAllAsync();
//                var existingLabByName = allLabs.FirstOrDefault(l =>
//                    l.Name.Trim().ToLower() == request.Name.Trim().ToLower());

//                if (existingLabByName != null)
//                {
//                    throw new InvalidOperationException($"معمل بنفس الاسم '{request.Name}' موجود بالفعل");
//                }

//                // Check for duplicate WhatsApp number
//                if (!string.IsNullOrWhiteSpace(request.WhatsAppNumber))
//                {
//                    var existingLabByWhatsApp = allLabs.FirstOrDefault(l =>
//                        l.WhatsAppNumber == request.WhatsAppNumber);

//                    if (existingLabByWhatsApp != null)
//                    {
//                        throw new InvalidOperationException($"رقم الواتساب '{request.WhatsAppNumber}' مستخدم بالفعل");
//                    }
//                }

//                // Create Address first
//                var address = _mapper.Map<Core.Entities.Shared.Address>(request.Address);
//                address.Id = Guid.NewGuid();
//                address.CreatedAt = DateTime.UtcNow;
//                await _unitOfWork.Addresses.AddAsync(address);

//                // Create Laboratory
//                var laboratory = _mapper.Map<Laboratory>(request);
//                laboratory.Id = Guid.NewGuid();
//                laboratory.AddressId = address.Id;
//                laboratory.CreatedAt = DateTime.UtcNow;
//                laboratory.LaboratoryStatus = Core.Enums.Identity.Status.Active;
//                laboratory.VerificationStatus = Core.Enums.Identity.VerificationStatus.UnderReview;

//                // Set required IdentityUser fields
//                laboratory.UserName = $"lab_{laboratory.Id}"; // Unique username
//                laboratory.Email = $"lab_{laboratory.Id}@temp.local"; // Temporary email (will be updated later)
//                laboratory.EmailConfirmed = false;
//                laboratory.SecurityStamp = Guid.NewGuid().ToString();

//                await _unitOfWork.Laboratories.AddAsync(laboratory);

//                // Add working hours if provided
//                if (request.WorkingHours != null && request.WorkingHours.Any())
//                {
//                    foreach (var hoursRequest in request.WorkingHours)
//                    {
//                        var workingHours = _mapper.Map<Core.Entities.External.Laboratories.LabWorkingHours>(hoursRequest);
//                        workingHours.Id = Guid.NewGuid();
//                        workingHours.LaboratoryId = laboratory.Id;
//                        workingHours.CreatedAt = DateTime.UtcNow;
//                        await _unitOfWork.LabWorkingHours.AddAsync(workingHours);
//                    }
//                }

//                await _unitOfWork.SaveChangesAsync();

//                _logger.LogInformation("Created laboratory {LaboratoryId} with address {AddressId}", laboratory.Id, address.Id);

//                return await GetLaboratoryByIdAsync(laboratory.Id)
//                    ?? throw new InvalidOperationException("Failed to retrieve created laboratory");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating laboratory");
//                throw;
//            }
//        }

//        public async Task<LaboratoryResponse> UpdateLaboratoryAsync(Guid id, UpdateLaboratoryRequest request)
//        {
//            try
//            {
//                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(id);
//                if (laboratory == null)
//                    throw new ArgumentException($"Laboratory with ID {id} not found");

//                // Update properties (only if provided)
//                if (!string.IsNullOrEmpty(request.Name))
//                    laboratory.Name = request.Name;
//                if (request.Description != null)
//                    laboratory.Description = request.Description;
//                if (request.WhatsAppNumber != null)
//                    laboratory.WhatsAppNumber = request.WhatsAppNumber;
//                if (request.Website != null)
//                    laboratory.Website = request.Website;
//                if (request.OffersHomeSampleCollection.HasValue)
//                    laboratory.OffersHomeSampleCollection = request.OffersHomeSampleCollection.Value;
//                if (request.HomeSampleCollectionFee.HasValue)
//                    laboratory.HomeSampleCollectionFee = request.HomeSampleCollectionFee;

//                // Update working hours if provided
//                if (request.WorkingHours != null && request.WorkingHours.Any())
//                {
//                    // Remove existing working hours
//                    var existingHours = await _unitOfWork.LabWorkingHours.GetAllAsync();
//                    var labExistingHours = existingHours.Where(w => w.LaboratoryId == id).ToList();

//                    foreach (var existingHour in labExistingHours)
//                    {
//                        _unitOfWork.LabWorkingHours.Delete(existingHour);
//                    }

//                    // Add new working hours
//                    foreach (var hoursRequest in request.WorkingHours)
//                    {
//                        var workingHours = _mapper.Map<Core.Entities.External.Laboratories.LabWorkingHours>(hoursRequest);
//                        workingHours.Id = Guid.NewGuid();
//                        workingHours.LaboratoryId = id;
//                        workingHours.CreatedAt = DateTime.UtcNow;
//                        await _unitOfWork.LabWorkingHours.AddAsync(workingHours);
//                    }
//                }

//                laboratory.UpdatedAt = DateTime.UtcNow;
//                await _unitOfWork.SaveChangesAsync();

//                _logger.LogInformation("Updated laboratory {LaboratoryId}", id);

//                return await GetLaboratoryByIdAsync(id)
//                    ?? throw new InvalidOperationException("Failed to retrieve updated laboratory");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating laboratory {LaboratoryId}", id);
//                throw;
//            }
//        }

//        public async Task<bool> DeleteLaboratoryAsync(Guid id)
//        {
//            try
//            {
//                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(id);
//                if (laboratory == null)
//                    return false;

//                // Hard delete - remove from database
//                _unitOfWork.Laboratories.Delete(laboratory);
//                await _unitOfWork.SaveChangesAsync();

//                _logger.LogInformation("Permanently deleted laboratory {LaboratoryId}", id);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error deleting laboratory {LaboratoryId}", id);
//                throw;
//            }
//        }

//        #endregion

//        #region Lab Services Management
//        public async Task<IEnumerable<LabServiceResponse>> GetLaboratoryServicesAsync(Guid laboratoryId)
//        {
//            try
//            {
//                var services = await _unitOfWork.LabServices.GetAllAsync();
//                var labServices = services.Where(s => s.LaboratoryId == laboratoryId).ToList();

//                var responses = new List<LabServiceResponse>();

//                foreach (var service in labServices)
//                {
//                    var response = _mapper.Map<LabServiceResponse>(service);

//                    // Load Lab Test details
//                    try
//                    {
//                        var labTest = await _unitOfWork.LabTests.GetByIdAsync(service.LabTestId);
//                        if (labTest != null)
//                        {
//                            response.LabTestName = labTest.Name;
//                            response.LabTestCategory = labTest.Category.ToString();
//                        }
//                    }
//                    catch (Exception testEx)
//                    {
//                        _logger.LogWarning(testEx, "Error loading lab test {LabTestId}", service.LabTestId);
//                    }

//                    responses.Add(response);
//                }

//                return responses;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting laboratory services for {LaboratoryId}", laboratoryId);
//                throw;
//            }
//        }

//        public async Task<LabServiceResponse> AddLaboratoryServiceAsync(Guid laboratoryId, CreateLabServiceRequest request)
//        {
//            try
//            {
//                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
//                if (laboratory == null)
//                    throw new ArgumentException($"Laboratory with ID {laboratoryId} not found");

//                var labTest = await _unitOfWork.LabTests.GetByIdAsync(request.LabTestId);
//                if (labTest == null)
//                    throw new ArgumentException($"Lab test with ID {request.LabTestId} not found");

//                var labService = _mapper.Map<Core.Entities.External.Laboratories.LabService>(request);
//                labService.Id = Guid.NewGuid();
//                labService.LaboratoryId = laboratoryId;
//                labService.CreatedAt = DateTime.UtcNow;

//                await _unitOfWork.LabServices.AddAsync(labService);
//                await _unitOfWork.SaveChangesAsync();

//                _logger.LogInformation("Added service to laboratory {LaboratoryId}", laboratoryId);

//                var response = _mapper.Map<LabServiceResponse>(labService);

//                // Load Lab Test details
//                response.LabTestName = labTest.Name;
//                response.LabTestCategory = labTest.Category.ToString();

//                return response;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error adding service to laboratory {LaboratoryId}", laboratoryId);
//                throw;
//            }
//        }

//        public async Task<LabServiceResponse> UpdateLaboratoryServiceAsync(Guid serviceId, CreateLabServiceRequest request)
//        {
//            try
//            {
//                var service = await _unitOfWork.LabServices.GetByIdAsync(serviceId);
//                if (service == null)
//                    throw new ArgumentException($"Lab service with ID {serviceId} not found");

//                // Check if LabTest exists
//                var labTest = await _unitOfWork.LabTests.GetByIdAsync(request.LabTestId);
//                if (labTest == null)
//                    throw new ArgumentException($"Lab test with ID {request.LabTestId} not found");

//                service.LabTestId = request.LabTestId;
//                service.Price = request.Price;
//                service.IsAvailable = request.IsAvailable;
//                service.LabSpecificNotes = request.LabSpecificNotes;
//                service.UpdatedAt = DateTime.UtcNow;
//                await _unitOfWork.SaveChangesAsync();

//                _logger.LogInformation("Updated lab service {ServiceId}", serviceId);

//                var response = _mapper.Map<LabServiceResponse>(service);

//                // Load Lab Test details
//                response.LabTestName = labTest.Name;
//                response.LabTestCategory = labTest.Category.ToString();

//                return response;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating lab service {ServiceId}", serviceId);
//                throw;
//            }
//        }

//        public async Task<bool> RemoveLaboratoryServiceAsync(Guid serviceId)
//        {
//            try
//            {
//                var service = await _unitOfWork.LabServices.GetByIdAsync(serviceId);
//                if (service == null)
//                    return false;

//                service.IsAvailable = false;
//                service.UpdatedAt = DateTime.UtcNow;
//                await _unitOfWork.SaveChangesAsync();

//                _logger.LogInformation("Removed lab service {ServiceId}", serviceId);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error removing lab service {ServiceId}", serviceId);
//                throw;
//            }
//        }

//        #endregion

//        #region Working Hours
//        public async Task<IEnumerable<LabWorkingHoursResponse>> GetLaboratoryWorkingHoursAsync(Guid laboratoryId)
//        {
//            try
//            {
//                var workingHours = await _unitOfWork.LabWorkingHours.GetAllAsync();
//                var labWorkingHours = workingHours.Where(w => w.LaboratoryId == laboratoryId).ToList();

//                var responses = _mapper.Map<IEnumerable<LabWorkingHoursResponse>>(labWorkingHours);
//                return responses;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting working hours for laboratory {LaboratoryId}", laboratoryId);
//                throw;
//            }
//        }

//        public async Task SetLaboratoryWorkingHoursAsync(Guid laboratoryId, IEnumerable<CreateLabWorkingHoursRequest> request)
//        {
//            try
//            {
//                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
//                if (laboratory == null)
//                    throw new ArgumentException($"Laboratory with ID {laboratoryId} not found");

//                // Note: Removing existing working hours would require a Delete method
//                // For now, we'll just add new ones

//                // Add new working hours
//                foreach (var hoursRequest in request)
//                {
//                    var workingHours = _mapper.Map<Core.Entities.External.Laboratories.LabWorkingHours>(hoursRequest);
//                    workingHours.Id = Guid.NewGuid();
//                    workingHours.LaboratoryId = laboratoryId;
//                    workingHours.CreatedAt = DateTime.UtcNow;

//                    await _unitOfWork.LabWorkingHours.AddAsync(workingHours);
//                }

//                await _unitOfWork.SaveChangesAsync();

//                _logger.LogInformation("Set working hours for laboratory {LaboratoryId}", laboratoryId);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error setting working hours for laboratory {LaboratoryId}", laboratoryId);
//                throw;
//            }
//        } 
//        #endregion

//        #region Search & Filter
//        public async Task<IEnumerable<LaboratoryResponse>> SearchLaboratoriesByLocationAsync(
//            double latitude,
//            double longitude,
//            double radiusKm)
//        {
//            try
//            {
//                // This would require spatial queries - simplified version
//                var laboratories = await _unitOfWork.Laboratories.GetAllAsync();
//                var responses = _mapper.Map<IEnumerable<LaboratoryResponse>>(laboratories);

//                _logger.LogInformation("Searched laboratories by location");
//                return responses;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error searching laboratories by location");
//                throw;
//            }
//        }

//        public async Task<IEnumerable<LaboratoryResponse>> GetLaboratoriesOfferingTestAsync(Guid labTestId)
//        {
//            try
//            {
//                var services = await _unitOfWork.LabServices.GetAllAsync();
//                var labServices = services.Where(s => s.LabTestId == labTestId && s.IsAvailable).ToList();

//                var laboratoryIds = labServices.Select(s => s.LaboratoryId).Distinct().ToList();
//                var laboratories = new List<Laboratory>();

//                foreach (var labId in laboratoryIds)
//                {
//                    var lab = await _unitOfWork.Laboratories.GetLaboratoryWithDetailsAsync(labId);
//                    if (lab != null)
//                        laboratories.Add(lab);
//                }

//                var responses = _mapper.Map<IEnumerable<LaboratoryResponse>>(laboratories);
//                return responses;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting laboratories offering test {LabTestId}", labTestId);
//                throw;
//            }
//        } 
//        #endregion

//        #region Statistics

//        public async Task<LaboratoryStatistics> GetLaboratoryStatisticsAsync(Guid laboratoryId)
//        {
//            try
//            {
//                var laboratory = await _unitOfWork.Laboratories.GetLaboratoryWithDetailsAsync(laboratoryId);
//                if (laboratory == null)
//                    throw new ArgumentException($"Laboratory with ID {laboratoryId} not found");

//                var orders = await _unitOfWork.LabOrders.GetAllAsync();
//                var labOrders = orders.Where(o => o.LaboratoryId == laboratoryId).ToList();

//                var statistics = new LaboratoryStatistics
//                {
//                    TotalOrders = labOrders.Count,
//                    CompletedOrders = labOrders.Count(o => o.Status == Core.Enums.Laboratory.LabOrderStatus.Completed),
//                    PendingOrders = labOrders.Count(o => o.Status == Core.Enums.Laboratory.LabOrderStatus.PendingPayment ||
//                                                         o.Status == Core.Enums.Laboratory.LabOrderStatus.ConfirmedByLab),
//                    CancelledOrders = labOrders.Count(o => o.Status == Core.Enums.Laboratory.LabOrderStatus.CancelledByPatient ||
//                                                           o.Status == Core.Enums.Laboratory.LabOrderStatus.CancelledByLab),
//                    TotalRevenue = labOrders.Where(o => o.Status == Core.Enums.Laboratory.LabOrderStatus.Completed)
//                                            .Sum(o => o.TestsTotalCost + o.SampleCollectionDeliveryCost),
//                    AverageRating = laboratory.AverageRating,
//                    TotalReviews = laboratory.TotalReviewsCount,
//                    TotalServices = laboratory.LabServices?.Count ?? 0
//                };

//                return statistics;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting statistics for laboratory {LaboratoryId}", laboratoryId);
//                throw;
//            }
//        } 
//        #endregion
//    }
//}
