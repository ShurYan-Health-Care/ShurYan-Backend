using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Partners;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class DoctorPartnerService : IDoctorPartnerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DoctorPartnerService> _logger;

        public DoctorPartnerService(
            IUnitOfWork unitOfWork,
            ILogger<DoctorPartnerService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Current Suggested Partner

        public async Task<SuggestedPartnerResponse> GetSuggestedPartnerAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting suggested partners for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var suggestion = await _unitOfWork.DoctorPartnerSuggestions.GetByDoctorIdAsync(doctorId);

                if (suggestion == null)
                {
                    _logger.LogInformation("No partner suggestions found for doctor {DoctorId}", doctorId);
                    return new SuggestedPartnerResponse { Pharmacy = null, Laboratory = null };
                }

                PartnerResponse? pharmacy = null;
                PartnerResponse? laboratory = null;

                // Bring the suggested pharmacy if it exists
                if (suggestion.SuggestedPharmacyId.HasValue)
                {
                    var pharmacyEntity = await _unitOfWork.Pharmacies.GetByIdAsync(suggestion.SuggestedPharmacyId.Value);
                    if (pharmacyEntity != null)
                    {
                        pharmacy = MapPharmacyToPartnerResponse(pharmacyEntity);
                    }
                }

                // Bring the proposed lab if it exists
                if (suggestion.SuggestedLaboratoryId.HasValue)
                {
                    var laboratoryEntity = await _unitOfWork.Laboratories.GetByIdAsync(suggestion.SuggestedLaboratoryId.Value);
                    if (laboratoryEntity != null)
                    {
                        laboratory = MapLaboratoryToPartnerResponse(laboratoryEntity);
                    }
                }

                _logger.LogInformation("Successfully retrieved suggested partners for doctor {DoctorId}", doctorId);
                return new SuggestedPartnerResponse { Pharmacy = pharmacy, Laboratory = laboratory };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggested partners for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<SuggestedPartnerResponse> SuggestPartnerAsync(Guid doctorId, SuggestPartnerRequest request)
        {
            try
            {
                _logger.LogInformation("Suggesting partners for doctor {DoctorId}. Pharmacy: {PharmacyId}, Laboratory: {LaboratoryId}",
                    doctorId, request.PharmacyId, request.LaboratoryId);

                // Verify the presence of the 
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                if (!request.PharmacyId.HasValue && !request.LaboratoryId.HasValue)
                    throw new ArgumentException("At least one pharmacy or laboratory must be designated");

                if (request.PharmacyId.HasValue)
                {
                    var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(request.PharmacyId.Value);
                    if (pharmacy == null)
                        throw new ArgumentException($"Pharmacy with ID {request.PharmacyId} not found");

                    if (pharmacy.PharmacyStatus != Status.Active)
                        throw new InvalidOperationException("An inactive pharmacy cannot be proposed");
                }

                if (request.LaboratoryId.HasValue)
                {
                    var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(request.LaboratoryId.Value);
                    if (laboratory == null)
                        throw new ArgumentException($"Laboratory with ID {request.LaboratoryId} not found");

                    if (laboratory.LaboratoryStatus != Status.Active)
                        throw new InvalidOperationException("An inactive laboratory cannot be proposed");
                }

                // Check for a previous suggestion
                var existingSuggestion = await _unitOfWork.DoctorPartnerSuggestions.GetByDoctorIdAsync(doctorId);

                if (existingSuggestion != null)
                {
                    _logger.LogInformation("Updating existing partner suggestions for doctor {DoctorId}", doctorId);

                    // Pharmacy update
                    if (request.PharmacyId.HasValue)
                    {
                        existingSuggestion.SuggestedPharmacyId = request.PharmacyId.Value;
                        existingSuggestion.PharmacySuggestedAt = DateTime.UtcNow;
                    }

                    // Lab Update
                    if (request.LaboratoryId.HasValue)
                    {
                        existingSuggestion.SuggestedLaboratoryId = request.LaboratoryId.Value;
                        existingSuggestion.LaboratorySuggestedAt = DateTime.UtcNow;
                    }

                    existingSuggestion.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create a new suggest
                    _logger.LogInformation("Creating new partner suggestions for doctor {DoctorId}", doctorId);

                    var newSuggestion = new DoctorPartnerSuggestion
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = doctorId,
                        SuggestedPharmacyId = request.PharmacyId,
                        PharmacySuggestedAt = request.PharmacyId.HasValue ? DateTime.UtcNow : null,
                        SuggestedLaboratoryId = request.LaboratoryId,
                        LaboratorySuggestedAt = request.LaboratoryId.HasValue ? DateTime.UtcNow : null,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.DoctorPartnerSuggestions.AddAsync(newSuggestion);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully suggested partners for doctor {DoctorId}", doctorId);
                return await GetSuggestedPartnerAsync(doctorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suggesting partners for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<bool> RemoveSuggestedPartnerAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Removing suggested partner for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var result = await _unitOfWork.DoctorPartnerSuggestions.DeleteByDoctorIdAsync(doctorId);

                if (!result)
                {
                    _logger.LogWarning("No partner suggestion found for doctor {DoctorId}", doctorId);
                    return false;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully removed suggested partner for doctor {DoctorId}", doctorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing suggested partner for doctor {DoctorId}", doctorId);
                throw;
            }
        }
        #endregion

        #region Available Partners

        public async Task<PaginatedResponse<PartnerResponse>> GetAvailablePharmaciesAsync(PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting available pharmacies with pagination. Page: {Page}, Size: {Size}",
                    paginationParams.PageNumber, paginationParams.PageSize);

                var allPharmacies = await _unitOfWork.Pharmacies.GetAllAsync();
                var activePharmacies = allPharmacies
                    .Where(p => p.PharmacyStatus == Status.Active && !p.IsDeleted)
                    .ToList();

                var totalCount = activePharmacies.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

                var paginatedPharmacies = activePharmacies
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .Select(p => MapPharmacyToPartnerResponse(p))
                    .ToList();

                _logger.LogInformation("Successfully retrieved {Count} pharmacies out of {Total}",
                    paginatedPharmacies.Count, totalCount);

                return new PaginatedResponse<PartnerResponse>
                {
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    HasNextPage = paginationParams.PageNumber < totalPages,
                    Data = paginatedPharmacies
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available pharmacies");
                throw;
            }
        }

        public async Task<PaginatedResponse<PartnerResponse>> GetAvailableLaboratoriesAsync(PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting available laboratories with pagination. Page: {Page}, Size: {Size}",
                    paginationParams.PageNumber, paginationParams.PageSize);

                var allLaboratories = await _unitOfWork.Laboratories.GetAllAsync();
                var activeLaboratories = allLaboratories
                    .Where(l => l.LaboratoryStatus == Status.Active && !l.IsDeleted)
                    .ToList();

                var totalCount = activeLaboratories.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

                var paginatedLaboratories = activeLaboratories
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .Select(l => MapLaboratoryToPartnerResponse(l))
                    .ToList();

                _logger.LogInformation("Successfully retrieved {Count} laboratories out of {Total}",
                    paginatedLaboratories.Count, totalCount);

                return new PaginatedResponse<PartnerResponse>
                {
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    HasNextPage = paginationParams.PageNumber < totalPages,
                    Data = paginatedLaboratories
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available laboratories");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private PartnerResponse MapPharmacyToPartnerResponse(Pharmacy pharmacy)
        {
            var address = pharmacy.Address != null
                ? $"{pharmacy.Address.Street}, {pharmacy.Address.City}, {pharmacy.Address.Governorate}"
                : "No address available";

            return new PartnerResponse
            {
                Id = pharmacy.Id,
                Name = pharmacy.Name,
                PartnerType = "pharmacy",
                OwnerName = $"{pharmacy.FirstName} {pharmacy.LastName}",
                Address = address,
                Phone = pharmacy.PhoneNumber ?? "",
                Description = pharmacy.Description
            };
        }

        private PartnerResponse MapLaboratoryToPartnerResponse(Laboratory laboratory)
        {
            var address = laboratory.Address != null
                ? $"{laboratory.Address.Street}, {laboratory.Address.City}, {laboratory.Address.Governorate}"
                : "No address available";

            return new PartnerResponse
            {
                Id = laboratory.Id,
                Name = laboratory.Name,
                PartnerType = "laboratory",
                OwnerName = $"{laboratory.FirstName} {laboratory.LastName}",
                Address = address,
                Phone = laboratory.PhoneNumber ?? "",
                Description = laboratory.Description
            };
        }

        #endregion
    }
}
