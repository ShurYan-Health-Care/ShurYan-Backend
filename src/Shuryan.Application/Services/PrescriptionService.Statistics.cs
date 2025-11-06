//using Microsoft.Extensions.Logging;
//using Shuryan.Application.DTOs.Responses.Prescription;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Shuryan.Application.Services
//{
//    public partial class PrescriptionService
//    {
//        #region Statistics & Analytics
//        public async Task<IEnumerable<DTOs.Responses.Prescription.PrescriptionStatusHistory>> GetStatusHistoryAsync(Guid prescriptionId)
//        {
//            try
//            {
//                // Check if prescription exists
//                var prescription = await _unitOfWork.Prescriptions.GetPrescriptionWithDetailsAsync(prescriptionId);
//                if (prescription == null)
//                    throw new ArgumentException($"Prescription with ID {prescriptionId} not found");

//                // Get doctor name
//                var doctorName = prescription.Doctor != null
//                    ? $"د. {prescription.Doctor.FirstName} {prescription.Doctor.LastName}"
//                    : null;

//                // For now, return basic history based on prescription data
//                var history = new List<DTOs.Responses.Prescription.PrescriptionStatusHistory>();

//                // Add creation event
//                history.Add(new DTOs.Responses.Prescription.PrescriptionStatusHistory
//                {
//                    Id = Guid.NewGuid(),
//                    PrescriptionId = prescriptionId,
//                    PreviousStatus = null,
//                    NewStatus = "Active",
//                    ChangedAt = prescription.CreatedAt,
//                    ChangedBy = prescription.DoctorId,
//                    ChangedByName = doctorName,
//                    ChangedByRole = "Doctor",
//                    Reason = "Prescription created",
//                    Notes = "Initial prescription creation",
//                    IpAddress = null,
//                    UserAgent = null
//                });

//                // Add shared event if applicable
//                if (prescription.IsDigitallyShared && prescription.SharedAt.HasValue)
//                {
//                    history.Add(new DTOs.Responses.Prescription.PrescriptionStatusHistory
//                    {
//                        Id = Guid.NewGuid(),
//                        PrescriptionId = prescriptionId,
//                        PreviousStatus = "Active",
//                        NewStatus = "Shared",
//                        ChangedAt = prescription.SharedAt.Value,
//                        ChangedBy = prescription.DoctorId,
//                        ChangedByName = doctorName,
//                        ChangedByRole = "Doctor",
//                        Reason = "Shared with pharmacy",
//                        Notes = "Prescription shared digitally",
//                        IpAddress = null,
//                        UserAgent = null
//                    });
//                }

//                // Add cancelled event if applicable
//                if (prescription.Status == Core.Enums.PrescriptionStatus.Cancelled && prescription.CancelledAt.HasValue)
//                {
//                    history.Add(new DTOs.Responses.Prescription.PrescriptionStatusHistory
//                    {
//                        Id = Guid.NewGuid(),
//                        PrescriptionId = prescriptionId,
//                        PreviousStatus = prescription.IsDigitallyShared ? "Shared" : "Active",
//                        NewStatus = "Cancelled",
//                        ChangedAt = prescription.CancelledAt.Value,
//                        ChangedBy = prescription.DoctorId,
//                        ChangedByName = doctorName,
//                        ChangedByRole = "Doctor",
//                        Reason = prescription.CancellationReason ?? "Prescription cancelled",
//                        Notes = "Prescription cancelled",
//                        IpAddress = null,
//                        UserAgent = null
//                    });
//                }

//                // Add dispensed event if applicable
//                if (prescription.Status == Core.Enums.PrescriptionStatus.Dispensed && prescription.UpdatedAt.HasValue)
//                {
//                    // Try to get pharmacist info from dispensing record
//                    var dispensingRecords = await _unitOfWork.DispensingRecords.GetByPrescriptionIdAsync(prescriptionId);
//                    var latestDispensing = dispensingRecords.OrderByDescending(d => d.DispensedAt).FirstOrDefault();

//                    var pharmacyName = latestDispensing != null
//                        ? (await _unitOfWork.Pharmacies.GetByIdAsync(latestDispensing.PharmacyId))?.Name
//                        : null;

//                    history.Add(new DTOs.Responses.Prescription.PrescriptionStatusHistory
//                    {
//                        Id = Guid.NewGuid(),
//                        PrescriptionId = prescriptionId,
//                        PreviousStatus = "Shared",
//                        NewStatus = "Dispensed",
//                        ChangedAt = latestDispensing?.DispensedAt ?? prescription.UpdatedAt.Value,
//                        ChangedBy = latestDispensing?.PharmacistId,
//                        ChangedByName = pharmacyName,
//                        ChangedByRole = "Pharmacist",
//                        Reason = "Medication dispensed",
//                        Notes = $"All medications dispensed{(pharmacyName != null ? $" at {pharmacyName}" : "")}",
//                        IpAddress = null,
//                        UserAgent = null
//                    });
//                }

//                _logger.LogInformation("Retrieved {Count} status history entries for prescription {PrescriptionId}",
//                    history.Count, prescriptionId);

//                return history.OrderBy(h => h.ChangedAt);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting status history for prescription {PrescriptionId}", prescriptionId);
//                throw;
//            }
//        } 
//        #endregion
//    }
//}
