using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Clinic;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class ClinicService : IClinicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClinicService> _logger;
        private readonly IFileUploadService _fileUploadService;

        public ClinicService(
            IUnitOfWork unitOfWork,
            ILogger<ClinicService> logger,
            IFileUploadService fileUploadService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        #region Clinic Info

        public async Task<ClinicInfoResponse?> GetClinicInfoAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting clinic info for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    return null;
                }

                // Get clinic separately
                var allClinics = await _unitOfWork.Clinics.GetAllAsync();
                var clinic = allClinics.FirstOrDefault(c => c.DoctorId == doctorId);
                if (clinic == null)
                {
                    _logger.LogInformation("No clinic found for doctor {DoctorId}, returning empty response", doctorId);
                    return new ClinicInfoResponse();
                }

                // Get phone numbers
                var allPhoneNumbers = await _unitOfWork.Repository<ClinicPhoneNumber>().GetAllAsync();
                var clinicPhones = allPhoneNumbers.Where(p => p.ClinicId == clinic.Id).ToList();

                var phoneNumbers = clinicPhones.Select(p => new PhoneNumberResponse
                {
                    Number = p.Number,
                    Type = (int)p.Type
                }).ToList();

                // Get services
                var allServices = await _unitOfWork.Repository<Core.Entities.External.Clinic.ClinicService>().GetAllAsync();
                var clinicServices = allServices.Where(s => s.ClinicId == clinic.Id).ToList();

                var services = clinicServices.Select(s => new ServiceItemResponse
                {
                    Id = (int)s.ServiceType,
                    Label = s.ServiceType.ToString(),
                    Value = s.ServiceType.ToString()
                }).ToList();

                var response = new ClinicInfoResponse
                {
                    ClinicName = clinic.Name,
                    PhoneNumbers = phoneNumbers,
                    Services = services
                };

                _logger.LogInformation("Successfully retrieved clinic info for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clinic info for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<ClinicInfoResponse> UpdateClinicInfoAsync(Guid doctorId, UpdateClinicInfoRequest request)
        {
            try
            {
                _logger.LogInformation("=== START: Updating clinic info for doctor {DoctorId} ===", doctorId);
                _logger.LogInformation("Request: ClinicName={ClinicName}, PhoneCount={PhoneCount}, ServiceCount={ServiceCount}", 
                    request.ClinicName ?? "null", request.PhoneNumbers?.Count ?? 0, request.Services?.Count ?? 0);

                _logger.LogInformation("Getting doctor...");
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogError("Doctor {DoctorId} not found!", doctorId);
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }
                _logger.LogInformation("Doctor found: {DoctorId}", doctor.Id);

                // Get clinic separately to ensure it's loaded
                _logger.LogInformation("Getting clinic...");
                var allClinics = await _unitOfWork.Clinics.GetAllAsync();
                _logger.LogInformation("Total clinics in DB: {Count}", allClinics.Count());
                var clinic = allClinics.FirstOrDefault(c => c.DoctorId == doctorId);

                if (clinic == null)
                {
                    _logger.LogInformation("No clinic found. Creating new clinic for doctor {DoctorId}", doctorId);

                    // Create address first
                    _logger.LogInformation("Creating address...");
                    var address = new Address
                    {
                        Id = Guid.NewGuid(),
                        Street = "",
                        City = "",
                        Governorate = Governorate.Cairo,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<Address>().AddAsync(address);
                    _logger.LogInformation("Address created: {AddressId}", address.Id);

                    _logger.LogInformation("Creating clinic...");
                    clinic = new Core.Entities.External.Clinic.Clinic
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = doctorId,
                        Name = request.ClinicName,
                        AddressId = address.Id,
                        ClinicStatus = Status.Active,
                        FacilityVideoUrl = "",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<Core.Entities.External.Clinic.Clinic>().AddAsync(clinic);
                    _logger.LogInformation("Clinic created: {ClinicId}", clinic.Id);
                }
                else
                {
                    _logger.LogInformation("Clinic found: {ClinicId}. Updating...", clinic.Id);
                    
                    // Update clinic name only if provided
                    if (!string.IsNullOrWhiteSpace(request.ClinicName))
                    {
                        clinic.Name = request.ClinicName;
                        clinic.UpdatedAt = DateTime.UtcNow;
                        _logger.LogInformation("Clinic name updated to: {ClinicName}", clinic.Name);
                    }
                    else
                    {
                        _logger.LogInformation("Clinic name not provided, keeping existing: {ClinicName}", clinic.Name);
                    }
                }

                // Update phone numbers - REPLACE (Clear old + Add new)
                if (request.PhoneNumbers != null)
                {
                    _logger.LogInformation("Step 4: Replacing phone numbers...");
                    var phoneRepo = _unitOfWork.Repository<ClinicPhoneNumber>();
                    
                    // Validate max 3 phones
                    if (request.PhoneNumbers.Count > 3)
                    {
                        _logger.LogError("Too many phone numbers: {Count}. Maximum is 3.", request.PhoneNumbers.Count);
                        throw new InvalidOperationException("الحد الأقصى لأرقام الهاتف هو 3 أرقام فقط");
                    }
                    
                    // Get all existing phones for this clinic
                    var allPhones = await phoneRepo.GetAllAsync();
                    var existingPhones = allPhones.Where(p => p.ClinicId == clinic.Id).ToList();
                    
                    _logger.LogInformation("Found {Count} existing phones. Deleting all...", existingPhones.Count);
                    
                    // Delete ALL old phones
                    foreach (var phone in existingPhones)
                    {
                        phoneRepo.Delete(phone);
                    }
                    
                    _logger.LogInformation("Adding {Count} new phones...", request.PhoneNumbers.Count);
                    
                    // Add ALL new phones
                    foreach (var phoneDto in request.PhoneNumbers)
                    {
                        _logger.LogInformation("Adding phone: Number={Number}, Type={Type}", phoneDto.Number, phoneDto.Type);
                        
                        // Validate enum value
                        if (!Enum.IsDefined(typeof(ClinicPhoneNumberType), phoneDto.Type))
                        {
                            _logger.LogError("Invalid phone type: {Type}", phoneDto.Type);
                            throw new ArgumentException($"Invalid phone type: {phoneDto.Type}. Must be 0 (Landline), 1 (WhatsApp), or 2 (Mobile)");
                        }

                        // Add new phone
                        var clinicPhone = new ClinicPhoneNumber
                        {
                            Id = Guid.NewGuid(),
                            ClinicId = clinic.Id,
                            Number = phoneDto.Number,
                            Type = (ClinicPhoneNumberType)phoneDto.Type,
                            CreatedAt = DateTime.UtcNow
                        };
                        await phoneRepo.AddAsync(clinicPhone);
                        _logger.LogInformation("Phone added: {PhoneId}", clinicPhone.Id);
                    }
                    
                    _logger.LogInformation("Phones replaced successfully. Total new phones: {Count}", request.PhoneNumbers.Count);
                }
                else
                {
                    _logger.LogInformation("Step 4: No phone numbers provided, keeping existing phones...");
                }

                // Update services - REPLACE (Clear old + Add new)
                if (request.Services != null)
                {
                    _logger.LogInformation("Step 5: Replacing services...");
                    var serviceRepo = _unitOfWork.Repository<Core.Entities.External.Clinic.ClinicService>();
                    
                    // Get all existing services for this clinic
                    var allServices = await serviceRepo.GetAllAsync();
                    var existingServices = allServices.Where(s => s.ClinicId == clinic.Id).ToList();
                    
                    _logger.LogInformation("Found {Count} existing services. Deleting all...", existingServices.Count);
                    
                    // Delete ALL old services
                    foreach (var service in existingServices)
                    {
                        serviceRepo.Delete(service);
                    }
                    
                    _logger.LogInformation("Adding {Count} new services...", request.Services.Count);
                    
                    // Add ALL new services
                    foreach (var serviceDto in request.Services)
                    {
                        _logger.LogInformation("Adding service: Id={Id}, Label={Label}", serviceDto.Id, serviceDto.Label);
                        
                        // Validate enum value
                        if (!Enum.IsDefined(typeof(ClinicServiceType), serviceDto.Id))
                        {
                            _logger.LogWarning("Invalid service type: {ServiceId} for doctor {DoctorId}. Skipping.", serviceDto.Id, doctorId);
                            continue; // Skip invalid services
                        }

                        // Add new service
                        var clinicService = new Core.Entities.External.Clinic.ClinicService
                        {
                            Id = Guid.NewGuid(),
                            ClinicId = clinic.Id,
                            ServiceType = (ClinicServiceType)serviceDto.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        await serviceRepo.AddAsync(clinicService);
                        _logger.LogInformation("Service added: {ServiceId}", clinicService.Id);
                    }
                    
                    _logger.LogInformation("Services replaced successfully. Total new services: {Count}", request.Services.Count);
                }
                else
                {
                    _logger.LogInformation("Step 5: No services provided, keeping existing services...");
                }

                _logger.LogInformation("Step 6: Saving changes to database...");
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Changes saved successfully!");

                _logger.LogInformation("Successfully updated clinic info for doctor {DoctorId}", doctorId);
                return await GetClinicInfoAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated clinic info");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ERROR: Failed to update clinic info for doctor {DoctorId} ===\nError Type: {ExceptionType}\nError Message: {Message}\nStack Trace: {StackTrace}", 
                    doctorId, ex.GetType().Name, ex.Message, ex.StackTrace);
                
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {InnerMessage}\nInner Stack: {InnerStack}", 
                        ex.InnerException.Message, ex.InnerException.StackTrace);
                }
                throw;
            }
        }

        #endregion

        #region Clinic Address

        public async Task<ClinicAddressResponse?> GetClinicAddressAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting clinic address for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    return null;
                }

                // Get clinic separately
                var allClinics = await _unitOfWork.Clinics.GetAllAsync();
                var clinic = allClinics.FirstOrDefault(c => c.DoctorId == doctorId);
                if (clinic == null)
                {
                    _logger.LogInformation("No clinic found for doctor {DoctorId}, returning empty response", doctorId);
                    return new ClinicAddressResponse();
                }

                // Get address separately
                var address = await _unitOfWork.Repository<Address>().GetByIdAsync(clinic.AddressId);
                if (address == null)
                {
                    _logger.LogInformation("No address found for clinic {ClinicId}, returning empty response", clinic.Id);
                    return new ClinicAddressResponse();
                }

                var response = new ClinicAddressResponse
                {
                    Governorate = address.Governorate.ToString(),
                    City = address.City,
                    Street = address.Street,
                    BuildingNumber = address.BuildingNumber ?? string.Empty,
                    Latitude = address.Latitude,
                    Longitude = address.Longitude
                };

                _logger.LogInformation("Successfully retrieved clinic address for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clinic address for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<ClinicAddressResponse> UpdateClinicAddressAsync(Guid doctorId, UpdateClinicAddressRequest request)
        {
            try
            {
                _logger.LogInformation("Updating clinic address for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get clinic separately
                var allClinics = await _unitOfWork.Clinics.GetAllAsync();
                var clinic = allClinics.FirstOrDefault(c => c.DoctorId == doctorId);
                if (clinic == null)
                    throw new InvalidOperationException($"No clinic found for doctor {doctorId}. Please create clinic info first.");

                // Get address separately
                var address = await _unitOfWork.Repository<Address>().GetByIdAsync(clinic.AddressId);
                if (address == null)
                {
                    // Create new address
                    address = new Address
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow
                    };
                    clinic.AddressId = address.Id;
                    await _unitOfWork.Repository<Address>().AddAsync(address);
                }

                // Update address
                // Parse governorate string to enum (supports both English name and Arabic description)
                var governorate = ParseGovernorate(request.Governorate);
                if (governorate.HasValue)
                {
                    address.Governorate = governorate.Value;
                }
                else
                {
                    _logger.LogWarning("Invalid governorate: {Governorate}. Using default (Cairo)", request.Governorate);
                    address.Governorate = Governorate.Cairo; // Default fallback
                }
                
                address.City = request.City;
                address.Street = request.Street;
                address.BuildingNumber = request.BuildingNumber ?? string.Empty;
                address.Latitude = request.Latitude;
                address.Longitude = request.Longitude;
                address.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated clinic address for doctor {DoctorId}", doctorId);
                return await GetClinicAddressAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated clinic address");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating clinic address for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Clinic Images

        public async Task<ClinicImagesListResponse> GetClinicImagesAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting clinic images for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get clinic separately
                var allClinics = await _unitOfWork.Clinics.GetAllAsync();
                var clinic = allClinics.FirstOrDefault(c => c.DoctorId == doctorId);
                if (clinic == null)
                {
                    _logger.LogInformation("No clinic found for doctor {DoctorId}, returning empty list", doctorId);
                    return new ClinicImagesListResponse();
                }

                var allPhotos = await _unitOfWork.Repository<ClinicPhoto>().GetAllAsync();
                var clinicPhotos = allPhotos
                    .Where(p => p.ClinicId == clinic.Id)
                    .OrderBy(p => p.DisplayOrder)
                    .ToList();

                var images = clinicPhotos.Select(p => new ClinicImageResponse
                {
                    Id = p.Id,
                    Url = p.PhotoUrl,
                    Order = p.DisplayOrder,
                    UploadedAt = p.CreatedAt
                }).ToList();

                _logger.LogInformation("Successfully retrieved {Count} clinic images for doctor {DoctorId}", images.Count, doctorId);
                return new ClinicImagesListResponse { Images = images };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clinic images for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<ClinicImageResponse> UploadClinicImageAsync(Guid doctorId, UploadClinicImageRequest request)
        {
            try
            {
                _logger.LogInformation("Uploading clinic image for doctor {DoctorId} with order {Order}", doctorId, request.Order);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get clinic separately
                var allClinics = await _unitOfWork.Clinics.GetAllAsync();
                var clinic = allClinics.FirstOrDefault(c => c.DoctorId == doctorId);
                if (clinic == null)
                    throw new InvalidOperationException($"No clinic found for doctor {doctorId}. Please create clinic info first.");

                // Check maximum images limit (6)
                var allPhotos = await _unitOfWork.Repository<ClinicPhoto>().GetAllAsync();
                var existingPhotos = allPhotos.Where(p => p.ClinicId == clinic.Id).ToList();

                if (existingPhotos.Count >= 6)
                    throw new InvalidOperationException($"Maximum of 6 images allowed. Current count: {existingPhotos.Count}");

                // Check if order is already used
                var orderExists = existingPhotos.Any(p => p.DisplayOrder == request.Order);
                if (orderExists)
                    throw new InvalidOperationException($"Display order {request.Order} is already used. Please choose a different order.");

                // Upload image
                var uploadResult = await _fileUploadService.UploadClinicImageAsync(request.Image, doctorId.ToString());

                var photo = new ClinicPhoto
                {
                    Id = Guid.NewGuid(),
                    ClinicId = clinic.Id,
                    PhotoUrl = uploadResult.FileUrl,
                    DisplayOrder = request.Order,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<ClinicPhoto>().AddAsync(photo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully uploaded clinic image {ImageId} for doctor {DoctorId}", photo.Id, doctorId);

                return new ClinicImageResponse
                {
                    Id = photo.Id,
                    Url = photo.PhotoUrl,
                    Order = photo.DisplayOrder,
                    UploadedAt = photo.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading clinic image for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<bool> DeleteClinicImageAsync(Guid doctorId, Guid imageId)
        {
            try
            {
                _logger.LogInformation("Deleting clinic image {ImageId} for doctor {DoctorId}", imageId, doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var photo = await _unitOfWork.Repository<ClinicPhoto>().GetByIdAsync(imageId);
                if (photo == null)
                    throw new ArgumentException($"Image with ID {imageId} not found");

                // Get clinic to verify ownership
                var allClinics = await _unitOfWork.Clinics.GetAllAsync();
                var clinic = allClinics.FirstOrDefault(c => c.DoctorId == doctorId);
                
                // Verify ownership
                if (clinic == null || photo.ClinicId != clinic.Id)
                    throw new InvalidOperationException("You don't have permission to delete this image");

                // Delete from cloud storage
                await _fileUploadService.DeleteFileAsync(photo.PhotoUrl);

                // Delete from database
                _unitOfWork.Repository<ClinicPhoto>().Delete(photo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted clinic image {ImageId} for doctor {DoctorId}", imageId, doctorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting clinic image {ImageId} for doctor {DoctorId}", imageId, doctorId);
                throw;
            }
        }

        public async Task<ClinicImagesListResponse> ReorderClinicImagesAsync(Guid doctorId, ReorderClinicImagesRequest request)
        {
            try
            {
                _logger.LogInformation("Reordering clinic images for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get clinic separately
                var allClinics = await _unitOfWork.Clinics.GetAllAsync();
                var clinic = allClinics.FirstOrDefault(c => c.DoctorId == doctorId);
                if (clinic == null)
                    throw new InvalidOperationException($"No clinic found for doctor {doctorId}");

                // Get all photos
                var allPhotos = await _unitOfWork.Repository<ClinicPhoto>().GetAllAsync();
                var clinicPhotos = allPhotos.Where(p => p.ClinicId == clinic.Id).ToList();

                // Update orders
                foreach (var imageOrder in request.Images)
                {
                    var photo = clinicPhotos.FirstOrDefault(p => p.Id == imageOrder.Id);
                    if (photo != null)
                    {
                        photo.DisplayOrder = imageOrder.Order;
                        photo.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully reordered clinic images for doctor {DoctorId}", doctorId);
                return await GetClinicImagesAsync(doctorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering clinic images for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Parse governorate string to enum. Supports both English name (Cairo) and Arabic description (القاهرة)
        /// </summary>
        private Governorate? ParseGovernorate(string governorateString)
        {
            if (string.IsNullOrWhiteSpace(governorateString))
                return null;

            // Try parse as enum name first (Cairo, Giza, etc.)
            if (Enum.TryParse<Governorate>(governorateString, true, out var governorate))
            {
                return governorate;
            }

            // Try parse by Arabic description (القاهرة, الجيزة, etc.)
            foreach (Governorate gov in Enum.GetValues(typeof(Governorate)))
            {
                var field = gov.GetType().GetField(gov.ToString());
                if (field != null)
                {
                    var attribute = (System.ComponentModel.DescriptionAttribute?)
                        Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute));
                    
                    if (attribute != null && attribute.Description == governorateString)
                    {
                        return gov;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
