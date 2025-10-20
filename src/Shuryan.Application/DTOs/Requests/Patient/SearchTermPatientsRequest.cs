using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Application.DTOs.Requests.Patient
{
    public class SearchTermPatientsRequest : PaginationParams
    {
        [StringLength(100)]
        public string? SearchTerm { get; set; }

        public Gender? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDateTo { get; set; }

        public bool? HasMedicalHistory { get; set; }

        public bool? HasUpcomingAppointments { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        public PatientSortBy SortBy { get; set; } = PatientSortBy.CreatedAt;

        public bool SortDescending { get; set; } = true;

        // For filtering by registration date
        public DateTime? RegisteredFrom { get; set; }
        public DateTime? RegisteredTo { get; set; }

        // For filtering by activity
        public bool? IsActive { get; set; }
        public DateTime? LastActivityFrom { get; set; }
        public DateTime? LastActivityTo { get; set; }
    }

    public enum PatientSortBy
    {
        FirstName,
        LastName,
        Email,
        BirthDate,
        CreatedAt,
        LastAppointment
    }
}