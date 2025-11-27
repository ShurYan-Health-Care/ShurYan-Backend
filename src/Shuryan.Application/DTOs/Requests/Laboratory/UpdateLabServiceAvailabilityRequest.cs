using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class UpdateLabServiceAvailabilityRequest
        {
                [Required(ErrorMessage = "حالة التوفر مطلوبة")]
                public bool IsAvailable { get; set; }
        }
}
