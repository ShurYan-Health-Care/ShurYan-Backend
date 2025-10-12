using Shuryan.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LaboratoryFilterDto : PaginationParams
    {
        public string? SearchTerm { get; set; }
        public string? Governorate { get; set; }
        public bool? OffersHomeSampleCollection { get; set; }
        public double? MinRating { get; set; }
    }

}
