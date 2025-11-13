using AutoMapper;
using Shuryan.Application.DTOs.Requests.Prescription;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Core.Entities.External.Pharmacies;

namespace Shuryan.Application.Mappers
{
    public class PrescriptionMappingProfile : Profile
    {
        public PrescriptionMappingProfile()
        {
            #region Prescription Mappings
            //CreateMap<Prescription, PrescriptionResponse>();
            //CreateMap<Prescription, PrescriptionDetailsResponse>()
            //    .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.FirstName : string.Empty))
            //    .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient != null ? src.Patient.FirstName : string.Empty))
            //    .ForMember(dest => dest.Medications, opt => opt.MapFrom(src => src.PrescribedMedications));
            //CreateMap<CreatePrescriptionRequest, Prescription>();
            //CreateMap<UpdatePrescriptionRequest, Prescription>()
            //    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Prescribed Medication Mappings
            CreateMap<PrescribedMedication, PrescribedMedicationResponse>();
            CreateMap<CreatePrescribedMedicationRequest, PrescribedMedication>();
            #endregion

            #region Medication Mappings
            CreateMap<Medication, MedicationResponse>();
            CreateMap<CreateMedicationRequest, Medication>();
            #endregion
        }
    }
}
