using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/rx")]
    public class PublicRxController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILabPrescriptionService _labPrescriptionService;

        public PublicRxController(
            IPrescriptionService prescriptionService,
            ILabPrescriptionService labPrescriptionService)
        {
            _prescriptionService = prescriptionService;
            _labPrescriptionService = labPrescriptionService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPrescription(Guid id)
        {
            // First try Pharmacy Prescription
            var pharmacyRx = await _prescriptionService.GetPrescriptionByIdAsync(id);
            if (pharmacyRx != null)
            {
                return Ok(ApiResponse<object>.Success(new { Type = "Pharmacy", Data = pharmacyRx }, "تم العثور على الروشتة"));
            }

            // Then try Lab Prescription (use detailed version to include test names & codes)
            var labRx = await _labPrescriptionService.GetLabPrescriptionDetailedAsync(id);
            if (labRx != null)
            {
                return Ok(ApiResponse<object>.Success(new { Type = "Lab", Data = labRx }, "تم العثور على الروشتة"));
            }

            return NotFound(ApiResponse<object>.Failure("لم يتم العثور على الروشتة", statusCode: 404));
        }
    }
}
