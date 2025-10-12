using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LabOrderFilterDto : PaginationParams
    {
        public Guid? PatientId { get; set; }
        public Guid? LaboratoryId { get; set; }
        public Status? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
