using Shuryan.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Review
{
    public class ReviewFilterDto : PaginationParams
    {
        public Guid? EntityId { get; set; }
        public double? MinRating { get; set; }
        public double? MaxRating { get; set; }
    }
}
