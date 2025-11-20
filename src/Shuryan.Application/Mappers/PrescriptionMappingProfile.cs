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
            CreateMap<Prescription, PrescriptionResponse>()
                .ForMember(dest => dest.Doctor, opt => opt.MapFrom(src => src.Doctor))
                .ForMember(dest => dest.Patient, opt => opt.MapFrom(src => src.Patient))
                .ForMember(dest => dest.PrescribedMedications, opt => opt.MapFrom(src => src.PrescribedMedications));
            
            CreateMap<CreatePrescriptionRequest, Prescription>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.PrescribedMedications, opt => opt.Ignore());
            
            CreateMap<UpdatePrescriptionRequest, Prescription>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
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
