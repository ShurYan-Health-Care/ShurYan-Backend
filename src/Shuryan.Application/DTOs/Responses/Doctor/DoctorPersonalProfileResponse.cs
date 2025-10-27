using Shuryan.Core.Enums.Identity;
using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// Response DTO for doctor's personal profile information
    /// معلومات البروفايل الشخصي للدكتور
    /// </summary>
    public class DoctorPersonalProfileResponse
    {
        public Guid Id { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public string? Biography { get; set; }
    }
}
