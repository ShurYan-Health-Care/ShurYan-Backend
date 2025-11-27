using System.ComponentModel.DataAnnotations;
using Shuryan.Core.Enums.Laboratory;

namespace Shuryan.Application.DTOs.Requests.Patient
{
        /// <summary>
        /// إنشاء طلب تحاليل جديد
        /// </summary>
        public class CreatePatientLabOrderRequest
        {
                [Required(ErrorMessage = "معرف روشتة التحاليل مطلوب")]
                public Guid LabPrescriptionId { get; set; }

                [Required(ErrorMessage = "معرف المعمل مطلوب")]
                public Guid LaboratoryId { get; set; }

                public SampleCollectionType SampleCollectionType { get; set; } = SampleCollectionType.LabVisit;
        }
}
