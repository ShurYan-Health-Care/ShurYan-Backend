using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.Interfaces;
using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorApplicationService _doctorApplicationService;

        public DoctorsController(IDoctorApplicationService doctorApplicationService)
        {
            _doctorApplicationService = doctorApplicationService;
        }

        // ==================== BASIC CRUD ====================
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorResponse>> GetDoctor(Guid id)
        {
            var doctor = await _doctorApplicationService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound(new { Message = $"Doctor with ID {id} not found" });

            return Ok(doctor);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DoctorResponse>> CreateDoctor([FromBody] CreateDoctorRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {

                var doctor = await _doctorApplicationService.CreateDoctorAsync(request);
                return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctor);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the doctor", Details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DoctorResponse>> UpdateDoctor(Guid id, [FromBody] UpdateDoctorRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var doctor = await _doctorApplicationService.UpdateDoctorAsync(id, request);
                return Ok(doctor);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the doctor", Details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteDoctor(Guid id)
        {
            var result = await _doctorApplicationService.DeleteDoctorAsync(id);
            if (!result)
                return NotFound(new { Message = $"Doctor with ID {id} not found" });

            return NoContent();
        }

        // ==================== QUERY ENDPOINTS ====================

        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorResponse>> GetDoctorByEmail(string email)
        {
            var doctor = await _doctorApplicationService.GetDoctorByEmailAsync(email);
            if (doctor == null)
                return NotFound(new { Message = $"Doctor with email {email} not found" });

            return Ok(doctor);
        }

        [HttpGet("specialty/{specialty}")]
        [ProducesResponseType(typeof(IEnumerable<DoctorResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DoctorResponse>>> GetDoctorsBySpecialty(MedicalSpecialty specialty)
        {
            var doctors = await _doctorApplicationService.GetDoctorsBySpecialtyAsync(specialty);
            return Ok(doctors);
        }

        [HttpGet("verified")]
        [ProducesResponseType(typeof(IEnumerable<DoctorResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DoctorResponse>>> GetVerifiedDoctors()
        {
            var doctors = await _doctorApplicationService.GetVerifiedDoctorsAsync();
            return Ok(doctors);
        }

        [HttpGet("governorate/{governorate}")]
        [ProducesResponseType(typeof(IEnumerable<DoctorResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DoctorResponse>>> GetDoctorsByGovernorate(Governorate governorate)
        {
            var doctors = await _doctorApplicationService.GetDoctorsByGovernorateAsync(governorate);
            return Ok(doctors);
        }

        [HttpPost("search")]
        [ProducesResponseType(typeof(PaginatedResponse<DoctorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<DoctorResponse>>> SearchDoctors([FromBody] SearchDoctorsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _doctorApplicationService.SearchDoctorsAsync(request);
            return Ok(result);
        }

        // ==================== AVAILABILITY ENDPOINTS ====================

        [HttpGet("{id}/availability")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CheckDoctorAvailability(Guid id, [FromQuery] DateTime dateTime)
        {
            var doctor = await _doctorApplicationService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound(new { Message = $"Doctor with ID {id} not found" });

            var isAvailable = await _doctorApplicationService.IsDoctorAvailableAtAsync(id, dateTime);
            return Ok(new
            {
                DoctorId = id,
                DateTime = dateTime,
                IsAvailable = isAvailable
            });
        }

        [HttpGet("{id}/availabilities")]
        [ProducesResponseType(typeof(IEnumerable<DoctorAvailabilityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DoctorAvailabilityResponse>>> GetDoctorAvailabilities(Guid id)
        {
            var doctor = await _doctorApplicationService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound(new { Message = $"Doctor with ID {id} not found" });

            var availabilities = await _doctorApplicationService.GetDoctorAvailabilitiesAsync(id);
            return Ok(availabilities);
        }

        [HttpPost("{id}/availabilities")]
        [ProducesResponseType(typeof(DoctorAvailabilityResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorAvailabilityResponse>> AddDoctorAvailability(
            Guid id,
            [FromBody] CreateDoctorAvailabilityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var availability = await _doctorApplicationService.AddDoctorAvailabilityAsync(id, request);
                return CreatedAtAction(
                    nameof(GetDoctorAvailabilities),
                    new { id },
                    availability);
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

      
        [HttpPut("{doctorId}/availabilities/{availabilityId}")]
        [ProducesResponseType(typeof(DoctorAvailabilityResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorAvailabilityResponse>> UpdateDoctorAvailability(
            Guid doctorId,
            Guid availabilityId,
            [FromBody] UpdateDoctorAvailabilityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var availability = await _doctorApplicationService.UpdateDoctorAvailabilityAsync(
                    doctorId,
                    availabilityId,
                    request);
                return Ok(availability);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpDelete("{doctorId}/availabilities/{availabilityId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteDoctorAvailability(Guid doctorId, Guid availabilityId)
        {
            var result = await _doctorApplicationService.DeleteDoctorAvailabilityAsync(doctorId, availabilityId);
            if (!result)
                return NotFound(new { Message = "Availability not found" });

            return NoContent();
        }

        // ==================== CONSULTATION ENDPOINTS ====================

        [HttpGet("{id}/consultations")]
        [ProducesResponseType(typeof(IEnumerable<DoctorConsultationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DoctorConsultationResponse>>> GetDoctorConsultations(Guid id)
        {
            var doctor = await _doctorApplicationService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound(new { Message = $"Doctor with ID {id} not found" });

            var consultations = await _doctorApplicationService.GetDoctorConsultationsAsync(id);
            return Ok(consultations);
        }

        [HttpPost("{id}/consultations")]
        [ProducesResponseType(typeof(DoctorConsultationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DoctorConsultationResponse>> AddDoctorConsultation(
            Guid id,
            [FromBody] CreateDoctorConsultationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var consultation = await _doctorApplicationService.AddDoctorConsultationAsync(id, request);
                return CreatedAtAction(
                    nameof(GetDoctorConsultations),
                    new { id },
                    consultation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        
        [HttpPut("{doctorId}/consultations/{consultationId}")]
        [ProducesResponseType(typeof(DoctorConsultationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorConsultationResponse>> UpdateDoctorConsultation(
            Guid doctorId,
            Guid consultationId,
            [FromBody] UpdateDoctorConsultationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var consultation = await _doctorApplicationService.UpdateDoctorConsultationAsync(
                    doctorId,
                    consultationId,
                    request);
                return Ok(consultation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        
        [HttpDelete("{doctorId}/consultations/{consultationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteDoctorConsultation(Guid doctorId, Guid consultationId)
        {
            var result = await _doctorApplicationService.DeleteDoctorConsultationAsync(doctorId, consultationId);
            if (!result)
                return NotFound(new { Message = "Consultation not found" });

            return NoContent();
        }

        // ==================== DOCUMENT ENDPOINTS ====================

        [HttpGet("{id}/documents")]
        [ProducesResponseType(typeof(IEnumerable<DoctorDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DoctorDocumentResponse>>> GetDoctorDocuments(Guid id)
        {
            var doctor = await _doctorApplicationService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound(new { Message = $"Doctor with ID {id} not found" });

            var documents = await _doctorApplicationService.GetDoctorDocumentsAsync(id);
            return Ok(documents);
        }

        [HttpPost("{id}/documents")]
        [ProducesResponseType(typeof(DoctorDocumentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DoctorDocumentResponse>> UploadDoctorDocument(
            Guid id,
            [FromBody] CreateDoctorDocumentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var document = await _doctorApplicationService.AddDoctorDocumentAsync(id, request);
                return CreatedAtAction(
                    nameof(GetDoctorDocuments),
                    new { id },
                    document);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // ==================== OVERRIDE ENDPOINTS ====================

        [HttpGet("{id}/overrides")]
        [ProducesResponseType(typeof(IEnumerable<DoctorOverrideResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DoctorOverrideResponse>>> GetDoctorOverrides(Guid id)
        {
            var doctor = await _doctorApplicationService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound(new { Message = $"Doctor with ID {id} not found" });

            var overrides = await _doctorApplicationService.GetDoctorOverridesAsync(id);
            return Ok(overrides);
        }

        [HttpPost("{id}/overrides")]
        [ProducesResponseType(typeof(DoctorOverrideResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DoctorOverrideResponse>> AddDoctorOverride(
            Guid id,
            [FromBody] CreateDoctorOverrideRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var overrideSchedule = await _doctorApplicationService.AddDoctorOverrideAsync(id, request);
                return CreatedAtAction(
                    nameof(GetDoctorOverrides),
                    new { id },
                    overrideSchedule);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // ==================== VERIFICATION ENDPOINT ====================

        [HttpPost("{id}/verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> VerifyDoctor(Guid id, [FromBody] VerifyDoctorRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _doctorApplicationService.VerifyDoctorAsync(id, request);
            if (!result)
                return NotFound(new { Message = $"Doctor with ID {id} not found" });

            return Ok(new
            {
                Message = "Doctor verification updated successfully",
                DoctorId = id,
                IsVerified = request.IsVerified
            });
        }
    }
}