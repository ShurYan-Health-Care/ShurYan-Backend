using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Requests.Prescription;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Pharmacies;
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

            // Prescription Mappings
            CreateMap<Prescription, PrescriptionResponse>();
            CreateMap<CreatePrescriptionRequest, Prescription>();
            CreateMap<UpdatePrescriptionRequest, Prescription>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Prescribed Medication Mappings
            CreateMap<PrescribedMedication, PrescribedMedicationResponse>();
            CreateMap<CreatePrescribedMedicationRequest, PrescribedMedication>();

            // Medication Mappings
            CreateMap<Medication, MedicationResponse>();
            CreateMap<CreateMedicationRequest, Medication>();

            // Doctor Mappings (for Prescription)
            CreateMap<Doctor, DTOs.Responses.Doctor.DoctorBasicResponse>();
            
            // Patient Mappings (for Prescription)
            CreateMap<Patient, DTOs.Responses.Patient.PatientBasicResponse>();
        }
    }
}
