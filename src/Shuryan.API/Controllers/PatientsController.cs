using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Application.Interfaces;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // ==================== BASIC CRUD ====================

        /// <summary>
        /// Get patient by ID with full details
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientResponse>> GetPatient(Guid id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
                return NotFound(new { Message = $"Patient with ID {id} not found" });

            return Ok(patient);
        }

        /// <summary>
        /// Create a new patient
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PatientResponse>> CreatePatient([FromBody] CreatePatientRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var patient = await _patientService.CreatePatientAsync(request);
                return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the patient", Details = ex.Message });
            }
        }

        /// <summary>
        /// Update patient information (including address)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PatientResponse>> UpdatePatient(Guid id, [FromBody] UpdatePatientRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var patient = await _patientService.UpdatePatientAsync(id, request);
                return Ok(patient);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the patient", Details = ex.Message });
            }
        }

        /// <summary>
        /// Delete patient (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePatient(Guid id)
        {
            var result = await _patientService.DeletePatientAsync(id);
            if (!result)
                return NotFound(new { Message = $"Patient with ID {id} not found" });

            return NoContent();
        }

        /// <summary>
        /// Restore a soft-deleted patient
        /// </summary>
        [HttpPost("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RestorePatient(Guid id)
        {
            var result = await _patientService.RestorePatientAsync(id);
            if (!result)
                return NotFound(new { Message = $"Patient with ID {id} not found or not deleted" });

            return Ok(new { Message = "Patient restored successfully", PatientId = id });
        }

        // ==================== QUERY ENDPOINTS ====================

        /// <summary>
        /// Get patient by email
        /// </summary>
        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientResponse>> GetPatientByEmail(string email)
        {
            var patient = await _patientService.GetPatientByEmailAsync(email);
            if (patient == null)
                return NotFound(new { Message = $"Patient with email {email} not found" });

            return Ok(patient);
        }

        /// <summary>
        /// Get all patients
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PatientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAllPatients([FromQuery] bool includeDeleted = false)
        {
            var patients = await _patientService.GetAllPatientsAsync(includeDeleted);
            return Ok(patients);
        }

        /// <summary>
        /// Get paginated patients
        /// </summary>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResponse<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<PatientResponse>>> GetPaginatedPatients([FromQuery] PaginationParams request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _patientService.GetPaginatedPatientsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Search patients with advanced filtering
        /// </summary>
        [HttpPost("search")]
        [ProducesResponseType(typeof(PaginatedResponse<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<PatientResponse>>> SearchPatients([FromBody] SearchTermPatientsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _patientService.SearchPatientsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get patients with medical history
        /// </summary>
        [HttpGet("with-medical-history")]
        [ProducesResponseType(typeof(IEnumerable<PatientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PatientResponse>>> GetPatientsWithMedicalHistory()
        {
            var patients = await _patientService.GetPatientsWithMedicalHistoryAsync();
            return Ok(patients);
        }

        /// <summary>
        /// Check if email is unique
        /// </summary>
        [HttpGet("check-email/{email}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> CheckEmailUnique(string email)
        {
            var isUnique = await _patientService.IsEmailUniqueAsync(email);
            return Ok(new { Email = email, IsUnique = isUnique });
        }

        /// <summary>
        /// Get total patients count
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetTotalPatientsCount([FromQuery] bool includeDeleted = false)
        {
            var count = await _patientService.GetTotalPatientsCountAsync(includeDeleted);
            return Ok(new { TotalCount = count, IncludeDeleted = includeDeleted });
        }

        /// <summary>
        /// Get current patient (for authenticated user)
        /// </summary>
        [HttpGet("current/{userId}")]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientResponse>> GetCurrentPatient(Guid userId)
        {
            var patient = await _patientService.GetCurrentPatientAsync(userId);
            if (patient == null)
                return NotFound(new { Message = $"Patient with ID {userId} not found" });

            return Ok(patient);
        }

        // ==================== MEDICAL HISTORY ENDPOINTS ====================

        /// <summary>
        /// Get patient medical history
        /// </summary>
        [HttpGet("{patientId}/medical-history")]
        [ProducesResponseType(typeof(IEnumerable<MedicalHistoryItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MedicalHistoryItemResponse>>> GetPatientMedicalHistory(Guid patientId)
        {
            try
            {
                var medicalHistory = await _patientService.GetPatientMedicalHistoryAsync(patientId);
                return Ok(medicalHistory);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Add medical history item
        /// </summary>
        [HttpPost("{patientId}/medical-history")]
        [ProducesResponseType(typeof(MedicalHistoryItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicalHistoryItemResponse>> AddMedicalHistoryItem(
            Guid patientId,
            [FromBody] CreateMedicalHistoryItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var item = await _patientService.AddMedicalHistoryItemAsync(patientId, request);
                return CreatedAtAction(
                    nameof(GetPatientMedicalHistory),
                    new { patientId },
                    item);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Update medical history item
        /// </summary>
        [HttpPut("{patientId}/medical-history/{itemId}")]
        [ProducesResponseType(typeof(MedicalHistoryItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicalHistoryItemResponse>> UpdateMedicalHistoryItem(
            Guid patientId,
            Guid itemId,
            [FromBody] UpdateMedicalHistoryItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var item = await _patientService.UpdateMedicalHistoryItemAsync(patientId, itemId, request);
                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Delete medical history item
        /// </summary>
        [HttpDelete("{patientId}/medical-history/{itemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteMedicalHistoryItem(Guid patientId, Guid itemId)
        {
            var result = await _patientService.DeleteMedicalHistoryItemAsync(patientId, itemId);
            if (!result)
                return NotFound(new { Message = "Medical history item not found" });

            return NoContent();
        }

        // ==================== APPOINTMENTS ENDPOINTS ====================

        /// <summary>
        /// Get all appointments for a patient
        /// </summary>
        [HttpGet("{patientId}/appointments")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetPatientAppointments(Guid patientId)
        {
            try
            {
                var appointments = await _patientService.GetPatientAppointmentsAsync(patientId);
                return Ok(appointments);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get upcoming appointments for a patient
        /// </summary>
        [HttpGet("{patientId}/appointments/upcoming")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetUpcomingAppointments(Guid patientId)
        {
            try
            {
                var appointments = await _patientService.GetUpcomingAppointmentsAsync(patientId);
                return Ok(appointments);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get past appointments for a patient
        /// </summary>
        [HttpGet("{patientId}/appointments/past")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetPastAppointments(Guid patientId)
        {
            try
            {
                var appointments = await _patientService.GetPastAppointmentsAsync(patientId);
                return Ok(appointments);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get next upcoming appointment for a patient
        /// </summary>
        [HttpGet("{patientId}/appointments/next")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppointmentResponse>> GetNextAppointment(Guid patientId)
        {
            try
            {
                var appointment = await _patientService.GetNextAppointmentAsync(patientId);
                if (appointment == null)
                    return NotFound(new { Message = "No upcoming appointments found" });

                return Ok(appointment);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get appointments count for a patient
        /// </summary>
        [HttpGet("{patientId}/appointments/count")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetAppointmentsCount(Guid patientId)
        {
            try
            {
                var count = await _patientService.GetAppointmentsCountAsync(patientId);
                return Ok(new { PatientId = patientId, AppointmentsCount = count });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // ==================== PRESCRIPTIONS ENDPOINTS ====================

        /// <summary>
        /// Get all prescriptions for a patient
        /// </summary>
        [HttpGet("{patientId}/prescriptions")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponse>>> GetPatientPrescriptions(Guid patientId)
        {
            try
            {
                var prescriptions = await _patientService.GetPatientPrescriptionsAsync(patientId);
                return Ok(prescriptions);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get active prescriptions for a patient
        /// </summary>
        [HttpGet("{patientId}/prescriptions/active")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponse>>> GetActivePrescriptions(Guid patientId)
        {
            try
            {
                var prescriptions = await _patientService.GetActivePrescriptionsAsync(patientId);
                return Ok(prescriptions);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific prescription by ID
        /// </summary>
        [HttpGet("{patientId}/prescriptions/{prescriptionId}")]
        [ProducesResponseType(typeof(PrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionResponse>> GetPrescriptionById(Guid patientId, Guid prescriptionId)
        {
            try
            {
                var prescription = await _patientService.GetPrescriptionByIdAsync(patientId, prescriptionId);
                if (prescription == null)
                    return NotFound(new { Message = $"Prescription with ID {prescriptionId} not found" });

                return Ok(prescription);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // ==================== LAB ORDERS ENDPOINTS ====================

        /// <summary>
        /// Get all lab orders for a patient
        /// </summary>
        [HttpGet("{patientId}/lab-orders")]
        [ProducesResponseType(typeof(IEnumerable<LabOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LabOrderResponse>>> GetPatientLabOrders(Guid patientId)
        {
            try
            {
                var labOrders = await _patientService.GetPatientLabOrdersAsync(patientId);
                return Ok(labOrders);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get pending lab orders for a patient
        /// </summary>
        [HttpGet("{patientId}/lab-orders/pending")]
        [ProducesResponseType(typeof(IEnumerable<LabOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LabOrderResponse>>> GetPendingLabOrders(Guid patientId)
        {
            try
            {
                var labOrders = await _patientService.GetPendingLabOrdersAsync(patientId);
                return Ok(labOrders);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific lab order by ID
        /// </summary>
        [HttpGet("{patientId}/lab-orders/{orderId}")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabOrderResponse>> GetLabOrderById(Guid patientId, Guid orderId)
        {
            try
            {
                var labOrder = await _patientService.GetLabOrderByIdAsync(patientId, orderId);
                if (labOrder == null)
                    return NotFound(new { Message = $"Lab order with ID {orderId} not found" });

                return Ok(labOrder);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // ==================== ADDRESS ENDPOINTS ====================

        /// <summary>
        /// Get patient address
        /// </summary>
        [HttpGet("{patientId}/address")]
        [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AddressResponse>> GetPatientAddress(Guid patientId)
        {
            try
            {
                var address = await _patientService.GetPatientAddressAsync(patientId);
                if (address == null)
                    return NotFound(new { Message = "Patient does not have an address" });

                return Ok(address);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Update patient address
        /// </summary>
        [HttpPut("{patientId}/address")]
        [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AddressResponse>> UpdatePatientAddress(
            Guid patientId,
            [FromBody] UpdateAddressRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var address = await _patientService.UpdatePatientAddressAsync(patientId, request);
                return Ok(address);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Create patient address
        /// </summary>
        [HttpPost("address")]
        [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AddressResponse>> CreatePatientAddress([FromBody] CreateAddressRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var address = await _patientService.CreatePatientAddressAsync(request);
                return CreatedAtAction(nameof(CreatePatientAddress), address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the address", Details = ex.Message });
            }
        }

        // ==================== PROFILE ENDPOINTS ====================

        /// <summary>
        /// Update patient profile image
        /// </summary>
        [HttpPut("{patientId}/profile-image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateProfileImage(Guid patientId, [FromBody] UpdateProfileImageRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _patientService.UpdateProfileImageAsync(patientId, request.ImageUrl);
                if (!result)
                    return NotFound(new { Message = $"Patient with ID {patientId} not found" });

                return Ok(new { Message = "Profile image updated successfully", PatientId = patientId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Remove patient profile image
        /// </summary>
        [HttpDelete("{patientId}/profile-image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemoveProfileImage(Guid patientId)
        {
            var result = await _patientService.RemoveProfileImageAsync(patientId);
            if (!result)
                return NotFound(new { Message = $"Patient with ID {patientId} not found" });

            return Ok(new { Message = "Profile image removed successfully", PatientId = patientId });
        }
    }

    // ==================== REQUEST MODELS ====================

    /// <summary>
    /// Request model for updating profile image
    /// </summary>
    public class UpdateProfileImageRequest
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}
