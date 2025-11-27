using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorDetailsWithClinicResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"د. {FirstName} {LastName}";
        
        public MedicalSpecialty MedicalSpecialty { get; set; }
        public string MedicalSpecialtyName { get; set; } = string.Empty;
        
        public Gender? Gender { get; set; }
        public string? GenderName { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        public string? ProfileImageUrl { get; set; }
        
        public string? Biography { get; set; }
        
        public int YearsOfExperience { get; set; }
        
        public ClinicDetailsResponse? Clinic { get; set; }
        
        public PartnerSuggestionsResponse? PartnerSuggestions { get; set; }
        
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
    
    public class ClinicDetailsResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public IEnumerable<ClinicPhoneNumberResponse> PhoneNumbers { get; set; } = new List<ClinicPhoneNumberResponse>();
        
        // الخدمات المتاحة
        public IEnumerable<ClinicServiceResponse> OfferedServices { get; set; } = new List<ClinicServiceResponse>();
        
        public ClinicAddressResponse? Address { get; set; }
        
        public IEnumerable<ClinicPhotoResponse> Photos { get; set; } = new List<ClinicPhotoResponse>();
    }
    
    public class PartnerSuggestionsResponse
    {
        public PartnerBasicInfoResponse? SuggestedPharmacy { get; set; }
        public PartnerBasicInfoResponse? SuggestedLaboratory { get; set; }
    }
    
    public class PartnerBasicInfoResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
