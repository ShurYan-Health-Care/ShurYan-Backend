using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class ReorderClinicImagesRequest
    {
        [Required(ErrorMessage = "قائمة الصور مطلوبة")]
        [MinLength(1, ErrorMessage = "يجب أن تحتوي القائمة على صورة واحدة على الأقل")]
        public List<ImageOrderDto> Images { get; set; } = new List<ImageOrderDto>();
    }

    public class ImageOrderDto
    {
        [Required(ErrorMessage = "معرف الصورة مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "الترتيب مطلوب")]
        [Range(0, 5, ErrorMessage = "الترتيب يجب أن يكون بين 0 و 5")]
        public int Order { get; set; }
    }
}
