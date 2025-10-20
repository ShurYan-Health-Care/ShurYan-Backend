using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;

namespace Shuryan.Application.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Patient Mappings
            CreateMap<Patient, PatientResponse>();
            CreateMap<Patient, PatientBasicResponse>();
            CreateMap<CreatePatientRequest, Patient>();
            CreateMap<UpdatePatientRequest, Patient>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Address Mappings
            CreateMap<Address, AddressResponse>();
            CreateMap<CreateAddressRequest, Address>();
            CreateMap<UpdateAddressRequest, Address>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Medical History Mappings
            CreateMap<MedicalHistoryItem, MedicalHistoryItemResponse>();
            CreateMap<CreateMedicalHistoryItemRequest, MedicalHistoryItem>();
            CreateMap<UpdateMedicalHistoryItemRequest, MedicalHistoryItem>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
