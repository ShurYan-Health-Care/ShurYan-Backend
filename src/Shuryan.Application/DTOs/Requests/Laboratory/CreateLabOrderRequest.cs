using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class CreateLabOrderRequest
    {
        [Required(ErrorMessage = "Patient ID is required")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "Lab prescription ID is required")]
        public Guid LabPrescriptionId { get; set; }

        [Required(ErrorMessage = "Laboratory ID is required")]
        public Guid LaboratoryId { get; set; }

        [Required(ErrorMessage = "Sample collection type is required")]
        public SampleCollectionType SampleCollectionType { get; set; } = SampleCollectionType.LabVisit;
    }
}
