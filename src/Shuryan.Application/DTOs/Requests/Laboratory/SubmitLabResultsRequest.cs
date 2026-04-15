using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class SubmitLabResultsRequest
    {
        [Required(ErrorMessage = "Results are required")]
        [MinLength(1, ErrorMessage = "At least one result is required")]
        public List<LabResultSubmissionDto> Results { get; set; } = new();
    }
}
