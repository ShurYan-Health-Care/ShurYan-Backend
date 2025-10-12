using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class CreateLabOrderDto
    {
        public Guid LabPrescriptionId { get; set; }
        public Guid LaboratoryId { get; set; }
        public SampleCollectionType SampleCollectionType { get; set; }
    }

}
