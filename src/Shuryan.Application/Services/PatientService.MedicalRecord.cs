using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;

namespace Shuryan.Application.Services
{
    public partial class PatientService
    {
        #region Medical Record Operations (Profile)

        public async Task<MedicalRecordResponse?> GetPatientMedicalRecordAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found", patientId);
                    return null;
                }

                var medicalHistory = patient.MedicalHistory.ToList();
                
                if (!medicalHistory.Any())
                {
                    _logger.LogInformation("Patient {PatientId} has no medical history", patientId);
                    return null;
                }

                var response = new MedicalRecordResponse
                {
                    PatientId = patient.Id,
                    PatientFullName = $"{patient.FirstName} {patient.LastName}",
                    LastUpdatedAt = medicalHistory.Max(m => m.UpdatedAt ?? m.CreatedAt),
                    DrugAllergies = new List<DrugAllergyItem>(),
                    ChronicDiseases = new List<ChronicDiseaseItem>(),
                    CurrentMedications = new List<CurrentMedicationItem>(),
                    PreviousSurgeries = new List<PreviousSurgeryItem>()
                };

                // Parse medical history items by type
                foreach (var item in medicalHistory)
                {
                    var parts = item.Text.Split('|').Select(p => p.Trim()).ToArray();

                    switch (item.Type)
                    {
                        case MedicalHistoryType.DrugAllergy:
                            response.DrugAllergies.Add(new DrugAllergyItem
                            {
                                Id = item.Id,
                                DrugName = parts.Length > 0 ? parts[0] : string.Empty,
                                Reaction = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : null,
                                CreatedAt = item.CreatedAt
                            });
                            break;

                        case MedicalHistoryType.ChronicDisease:
                            response.ChronicDiseases.Add(new ChronicDiseaseItem
                            {
                                Id = item.Id,
                                DiseaseName = parts.Length > 0 ? parts[0] : string.Empty,
                                CreatedAt = item.CreatedAt
                            });
                            break;

                        case MedicalHistoryType.CurrentMedication:
                            response.CurrentMedications.Add(new CurrentMedicationItem
                            {
                                Id = item.Id,
                                MedicationName = parts.Length > 0 ? parts[0] : string.Empty,
                                Dosage = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : null,
                                Frequency = parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]) ? parts[2] : null,
                                StartDate = parts.Length > 3 && DateTime.TryParse(parts[3], out var startDate) ? startDate : null,
                                Reason = parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]) ? parts[4] : null
                            });
                            break;

                        case MedicalHistoryType.PreviousSurgery:
                            response.PreviousSurgeries.Add(new PreviousSurgeryItem
                            {
                                Id = item.Id,
                                SurgeryName = parts.Length > 0 ? parts[0] : string.Empty,
                                SurgeryDate = parts.Length > 1 && DateTime.TryParse(parts[1], out var surgeryDate) ? surgeryDate : null,
                                CreatedAt = item.CreatedAt
                            });
                            break;
                    }
                }

                _logger.LogInformation("Retrieved medical record for patient {PatientId}", patientId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical record for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<MedicalRecordResponse> UpdatePatientMedicalRecordAsync(Guid patientId, UpdateMedicalRecordRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var existingItems = patient.MedicalHistory.ToList();

                // Process Drug Allergies
                if (request.DrugAllergies != null)
                {
                    await ProcessMedicalHistorySection(
                        patientId,
                        MedicalHistoryType.DrugAllergy,
                        existingItems,
                        request.DrugAllergies.Select(d => new MedicalHistoryUpdate
                        {
                            Id = d.Id,
                            Text = string.IsNullOrWhiteSpace(d.Reaction) 
                                ? d.DrugName 
                                : $"{d.DrugName} | {d.Reaction}"
                        }).ToList()
                    );
                }

                // Process Chronic Diseases
                if (request.ChronicDiseases != null)
                {
                    await ProcessMedicalHistorySection(
                        patientId,
                        MedicalHistoryType.ChronicDisease,
                        existingItems,
                        request.ChronicDiseases.Select(d => new MedicalHistoryUpdate
                        {
                            Id = d.Id,
                            Text = d.DiseaseName
                        }).ToList()
                    );
                }

                // Process Current Medications
                if (request.CurrentMedications != null)
                {
                    await ProcessMedicalHistorySection(
                        patientId,
                        MedicalHistoryType.CurrentMedication,
                        existingItems,
                        request.CurrentMedications.Select(m => new MedicalHistoryUpdate
                        {
                            Id = m.Id,
                            Text = BuildMedicationText(m)
                        }).ToList()
                    );
                }

                // Process Previous Surgeries
                if (request.PreviousSurgeries != null)
                {
                    await ProcessMedicalHistorySection(
                        patientId,
                        MedicalHistoryType.PreviousSurgery,
                        existingItems,
                        request.PreviousSurgeries.Select(s => new MedicalHistoryUpdate
                        {
                            Id = s.Id,
                            Text = s.SurgeryDate.HasValue 
                                ? $"{s.SurgeryName} | {s.SurgeryDate.Value:yyyy-MM-dd}" 
                                : s.SurgeryName
                        }).ToList()
                    );
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated medical record for patient {PatientId}", patientId);

                // Return updated medical record
                var updatedRecord = await GetPatientMedicalRecordAsync(patientId);
                return updatedRecord ?? new MedicalRecordResponse
                {
                    PatientId = patientId,
                    PatientFullName = $"{patient.FirstName} {patient.LastName}",
                    LastUpdatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical record for patient {PatientId}", patientId);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods for Medical Record
        private async Task ProcessMedicalHistorySection(
            Guid patientId,
            MedicalHistoryType type,
            List<MedicalHistoryItem> existingItems,
            List<MedicalHistoryUpdate> updates)
        {
            var existingItemsOfType = existingItems.Where(i => i.Type == type).ToList();
            var updateIds = updates.Where(u => u.Id.HasValue).Select(u => u.Id.Value).ToHashSet();

            // Delete items not in the update request
            foreach (var itemToDelete in existingItemsOfType.Where(i => !updateIds.Contains(i.Id)))
            {
                _unitOfWork.MedicalHistoryItems.Delete(itemToDelete);
                _logger.LogInformation("Deleted medical history item {ItemId} for patient {PatientId}", itemToDelete.Id, patientId);
            }

            // Update existing and add new items
            foreach (var update in updates)
            {
                if (update.Id.HasValue)
                {
                    // Update existing item
                    var existingItem = existingItemsOfType.FirstOrDefault(i => i.Id == update.Id.Value);
                    if (existingItem != null)
                    {
                        existingItem.Text = update.Text;
                        existingItem.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.MedicalHistoryItems.Update(existingItem);
                        _logger.LogInformation("Updated medical history item {ItemId} for patient {PatientId}", existingItem.Id, patientId);
                    }
                }
                else
                {
                    // Add new item
                    var newItem = new MedicalHistoryItem
                    {
                        Id = Guid.NewGuid(),
                        PatientId = patientId,
                        Type = type,
                        Text = update.Text,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = patientId
                    };
                    var addedItem = await _unitOfWork.MedicalHistoryItems.AddAsync(newItem);
                    _logger.LogInformation("Added new medical history item {ItemId} for patient {PatientId}", addedItem.Id, patientId);
                }
            }
        }

        private string BuildMedicationText(CurrentMedicationItemRequest medication)
        {
            var parts = new List<string> { medication.MedicationName };

            if (!string.IsNullOrWhiteSpace(medication.Dosage))
                parts.Add(medication.Dosage);
            else
                parts.Add(string.Empty);

            if (!string.IsNullOrWhiteSpace(medication.Frequency))
                parts.Add(medication.Frequency);
            else
                parts.Add(string.Empty);

            if (medication.StartDate.HasValue)
                parts.Add(medication.StartDate.Value.ToString("yyyy-MM-dd"));
            else
                parts.Add(string.Empty);

            if (!string.IsNullOrWhiteSpace(medication.Reason))
                parts.Add(medication.Reason);

            return string.Join(" | ", parts).TrimEnd(' ', '|');
        }

        private class MedicalHistoryUpdate
        {
            public Guid? Id { get; set; }
            public string Text { get; set; } = string.Empty;
        }
        #endregion
    }
}
