using AutoMapper;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;

namespace Shuryan.Application.Mappers
{
    public class PatientMappingProfile : Profile
    {
        public PatientMappingProfile()
        {
            #region Patient Mappings
            CreateMap<Patient, PatientResponse>();
            CreateMap<Patient, PatientBasicResponse>();
            CreateMap<CreatePatientRequest, Patient>();
            CreateMap<UpdatePatientRequest, Patient>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Medical History Mappings
            CreateMap<MedicalHistoryItem, MedicalHistoryItemResponse>();
            CreateMap<CreateMedicalHistoryItemRequest, MedicalHistoryItem>();
            CreateMap<UpdateMedicalHistoryItemRequest, MedicalHistoryItem>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
        }
    }
}
