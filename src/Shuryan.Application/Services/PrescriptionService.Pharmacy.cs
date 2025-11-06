//using Microsoft.Extensions.Logging;
//using Shuryan.Application.DTOs.Requests.Prescription;
//using Shuryan.Application.DTOs.Responses.Prescription;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Shuryan.Application.Services
//{
//    public partial class PrescriptionService
//    {
//        #region Pharmacy Operations
//        public async Task<PrescriptionVerificationResponse> VerifyPrescriptionAsync(Guid id, string? verificationCode)
//        {
//            try
//            {
//                var prescription = await _unitOfWork.Prescriptions.GetPrescriptionWithDetailsAsync(id);
//                if (prescription == null)
//                    throw new ArgumentException($"Prescription with ID {id} not found");

//                // Check prescription status
//                var isCancelled = prescription.Status == Core.Enums.PrescriptionStatus.Cancelled;
//                var isExpired = prescription.Status == Core.Enums.PrescriptionStatus.Expired;
//                var isDispensed = prescription.Status == Core.Enums.PrescriptionStatus.Dispensed;

//                var signatureValid = !string.IsNullOrEmpty(prescription.DigitalSignature);
//                var isValid = !isCancelled && !isExpired && signatureValid;

//                string verificationStatus = "Valid";
//                string? reasonCannotBeDispensed = null;

//                if (isCancelled)
//                {
//                    verificationStatus = "Cancelled";
//                    reasonCannotBeDispensed = $"Prescription has been cancelled. Reason: {prescription.CancellationReason}";
//                }
//                else if (isExpired)
//                {
//                    verificationStatus = "Expired";
//                    reasonCannotBeDispensed = "Prescription has expired";
//                }
//                else if (isDispensed)
//                {
//                    verificationStatus = "Dispensed";
//                    reasonCannotBeDispensed = "Prescription has already been dispensed";
//                }

//                return new PrescriptionVerificationResponse
//                {
//                    IsValid = isValid,
//                    VerificationStatus = verificationStatus,
//                    Prescription = _mapper.Map<PrescriptionResponse>(prescription),
//                    VerifiedAt = DateTime.UtcNow,
//                    CanBeDispensed = isValid && !isDispensed,
//                    ReasonCannotBeDispensed = reasonCannotBeDispensed,
//                    DigitalSignatureStatus = signatureValid ? "Valid" : "Invalid",
//                    DoctorLicenseActive = true, // TODO: Check doctor license
//                    PatientEligible = true // TODO: Check patient eligibility
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error verifying prescription {PrescriptionId}", id);
//                throw;
//            }
//        }

//        public async Task<DispenseResult> DispensePrescriptionAsync(Guid id, DispensePrescriptionRequest request)
//        {
//            try
//            {
//                var prescription = await _unitOfWork.Prescriptions.GetPrescriptionWithDetailsAsync(id);
//                if (prescription == null)
//                    throw new ArgumentException($"Prescription with ID {id} not found");

//                // Check if already dispensed
//                if (prescription.Status == Core.Enums.PrescriptionStatus.Dispensed)
//                    throw new InvalidOperationException("Prescription has already been dispensed");

//                // Check if cancelled or expired
//                if (prescription.Status == Core.Enums.PrescriptionStatus.Cancelled)
//                    throw new InvalidOperationException("Cannot dispense a cancelled prescription");

//                if (prescription.Status == Core.Enums.PrescriptionStatus.Expired)
//                    throw new InvalidOperationException("Cannot dispense an expired prescription");

//                // Update prescription status
//                prescription.Status = Core.Enums.PrescriptionStatus.Dispensed;
//                _unitOfWork.Prescriptions.Update(prescription);

//                // Create dispensing record
//                var receiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMddHHmmss}";
//                var dispensingRecord = new Core.Entities.External.Pharmacies.DispensingRecord
//                {
//                    PrescriptionId = id,
//                    PharmacyId = request.PharmacyId,
//                    PharmacistId = request.PharmacistId,
//                    DispensedAt = DateTime.UtcNow,
//                    ReceiptNumber = receiptNumber,
//                    TotalCost = request.TotalCost,
//                    PaymentMethod = request.PaymentMethod,
//                    PharmacistNotes = null,
//                    PatientSignatureConfirmed = true
//                };

//                // Add dispensed medications
//                foreach (var med in request.DispensedMedications)
//                {
//                    dispensingRecord.DispensedMedications.Add(new Core.Entities.External.Pharmacies.DispensedMedicationItem
//                    {
//                        MedicationId = med.MedicationId,
//                        QuantityDispensed = med.QuantityDispensed,
//                        UnitPrice = med.UnitPrice
//                    });
//                }

//                await _unitOfWork.DispensingRecords.AddAsync(dispensingRecord);
//                await _unitOfWork.SaveChangesAsync();

//                // Get pharmacy name
//                var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(request.PharmacyId);

//                // Get pharmacist name (pharmacist is usually the pharmacy owner/staff)
//                string? pharmacistName = null;
//                if (pharmacy != null)
//                {
//                    pharmacistName = pharmacy.Name; // Use pharmacy name as fallback
//                }

//                // Map dispensed items for response
//                var dispensedItems = new List<DTOs.Responses.Prescription.DispensedItem>();
//                foreach (var med in request.DispensedMedications)
//                {
//                    var medication = await _unitOfWork.Medications.GetByIdAsync(med.MedicationId);
//                    dispensedItems.Add(new DTOs.Responses.Prescription.DispensedItem
//                    {
//                        MedicationId = med.MedicationId,
//                        MedicationName = medication?.BrandName ?? "Unknown",
//                        Quantity = med.QuantityDispensed,
//                        UnitPrice = med.UnitPrice,
//                        TotalPrice = med.QuantityDispensed * med.UnitPrice
//                    });
//                }

//                _logger.LogInformation("Prescription {PrescriptionId} dispensed at pharmacy {PharmacyId}",
//                    id, request.PharmacyId);

//                return new DispenseResult
//                {
//                    PrescriptionId = id,
//                    DispensingRecordId = dispensingRecord.Id,
//                    Success = true,
//                    ReceiptNumber = receiptNumber,
//                    DispensedAt = DateTime.UtcNow,
//                    PharmacyId = request.PharmacyId,
//                    PharmacyName = pharmacy?.Name,
//                    PharmacistId = request.PharmacistId,
//                    PharmacistName = pharmacistName,
//                    TotalCost = request.TotalCost,
//                    TotalItemsDispensed = request.DispensedMedications.Count,
//                    DispensedItems = dispensedItems,
//                    Message = "Prescription dispensed successfully"
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error dispensing prescription {PrescriptionId}", id);
//                throw;
//            }
//        }

//        public async Task<SharePrescriptionResult> SharePrescriptionAsync(Guid id, SharePrescriptionRequest request)
//        {
//            try
//            {
//                var prescription = await _unitOfWork.Prescriptions.GetByIdAsync(id);
//                if (prescription == null)
//                    throw new ArgumentException($"Prescription with ID {id} not found");

//                // Generate share code
//                var shareCode = new Random().Next(100000, 999999).ToString();
//                var shareUrl = $"https://app.shuryan.com/prescriptions/shared/{shareCode}";
//                var sharedAt = DateTime.UtcNow;

//                prescription.IsDigitallyShared = true;
//                prescription.SharedAt = sharedAt;
//                _unitOfWork.Prescriptions.Update(prescription);

//                // Create prescription share record
//                var prescriptionShare = new Core.Entities.External.Pharmacies.PrescriptionShare
//                {
//                    PrescriptionId = id,
//                    ShareCode = shareCode,
//                    ShareUrl = shareUrl,
//                    SharedAt = sharedAt,
//                    ExpiresAt = sharedAt.AddHours(request.ExpirationHours),
//                    PharmacyId = request.PharmacyId,
//                    AllowMultipleViews = request.AllowMultipleViews,
//                    IsActive = true,
//                    IsRevoked = false,
//                    MessageToPharmacy = request.MessageToPharmacy,
//                    AccessCount = 0
//                };

//                await _unitOfWork.PrescriptionShares.AddAsync(prescriptionShare);
//                await _unitOfWork.SaveChangesAsync();

//                // Get pharmacy name if provided
//                string? pharmacyName = null;
//                if (request.PharmacyId.HasValue)
//                {
//                    var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(request.PharmacyId.Value);
//                    pharmacyName = pharmacy?.Name;
//                }

//                _logger.LogInformation("Prescription {PrescriptionId} shared with code {ShareCode}",
//                    id, shareCode);

//                return new SharePrescriptionResult
//                {
//                    PrescriptionId = id,
//                    Success = true,
//                    ShareCode = shareCode,
//                    ShareUrl = shareUrl,
//                    SharedAt = sharedAt,
//                    ExpiresAt = sharedAt.AddHours(request.ExpirationHours),
//                    PharmacyId = request.PharmacyId,
//                    PharmacyName = pharmacyName,
//                    QrCodeImage = null, // TODO: Generate QR code
//                    Message = "Prescription shared successfully"
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error sharing prescription {PrescriptionId}", id);
//                throw;
//            }
//        }

//        public async Task<IEnumerable<DTOs.Responses.Prescription.DispensingRecord>> GetDispensingHistoryAsync(Guid prescriptionId)
//        {
//            try
//            {
//                var records = await _unitOfWork.DispensingRecords.GetByPrescriptionIdAsync(prescriptionId);

//                var result = new List<DTOs.Responses.Prescription.DispensingRecord>();

//                foreach (var record in records)
//                {
//                    var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(record.PharmacyId);

//                    // Map dispensed medications
//                    var medications = new List<DTOs.Responses.Prescription.DispensedMedicationDetail>();
//                    if (record.DispensedMedications != null)
//                    {
//                        foreach (var med in record.DispensedMedications)
//                        {
//                            medications.Add(new DTOs.Responses.Prescription.DispensedMedicationDetail
//                            {
//                                MedicationId = med.MedicationId,
//                                MedicationName = med.Medication?.BrandName ?? "Unknown",
//                                QuantityDispensed = med.QuantityDispensed,
//                                UnitPrice = med.UnitPrice,
//                                TotalPrice = med.QuantityDispensed * med.UnitPrice,
//                                BatchNumber = med.BatchNumber,
//                                ExpiryDate = med.ExpiryDate
//                            });
//                        }
//                    }

//                    result.Add(new DTOs.Responses.Prescription.DispensingRecord
//                    {
//                        Id = record.Id,
//                        PrescriptionId = record.PrescriptionId,
//                        PharmacyId = record.PharmacyId,
//                        PharmacyName = pharmacy?.Name,
//                        PharmacistId = record.PharmacistId,
//                        DispensedAt = record.DispensedAt,
//                        ReceiptNumber = record.ReceiptNumber,
//                        TotalCost = record.TotalCost,
//                        PaymentMethod = record.PaymentMethod,
//                        Medications = medications,
//                        PharmacistNotes = record.PharmacistNotes,
//                        PatientSignatureConfirmed = record.PatientSignatureConfirmed
//                    });
//                }

//                _logger.LogInformation("Retrieved {Count} dispensing records for prescription {PrescriptionId}",
//                    result.Count, prescriptionId);

//                return result;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting dispensing history for prescription {PrescriptionId}", prescriptionId);
//                throw;
//            }
//        }

//        public async Task AcceptPrescriptionDeliveryAsync(Guid id, AcceptDeliveryRequest request)
//        {
//            try
//            {
//                var prescription = await _unitOfWork.Prescriptions.GetPrescriptionWithDetailsAsync(id);
//                if (prescription == null)
//                    throw new ArgumentException($"Prescription with ID {id} not found");

//                // Check if prescription is in valid state
//                if (prescription.Status == Core.Enums.PrescriptionStatus.Cancelled)
//                    throw new InvalidOperationException("Cannot accept delivery for cancelled prescription");

//                if (prescription.Status == Core.Enums.PrescriptionStatus.Dispensed)
//                    throw new InvalidOperationException("Prescription has already been dispensed");

//                // Update prescription with delivery info (if you have these fields)
//                // prescription.DeliveryAcceptedAt = DateTime.UtcNow;
//                // prescription.EstimatedDeliveryTime = request.EstimatedDeliveryTime;
//                // prescription.DeliveryPharmacyId = request.PharmacyId;

//                _unitOfWork.Prescriptions.Update(prescription);
//                await _unitOfWork.SaveChangesAsync();

//                _logger.LogInformation("Delivery accepted for prescription {PrescriptionId} by pharmacy {PharmacyId}",
//                    id, request.PharmacyId);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error accepting delivery for prescription {PrescriptionId}", id);
//                throw;
//            }
//        } 
//        #endregion
//    }
//}
