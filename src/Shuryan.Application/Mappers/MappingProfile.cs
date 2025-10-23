using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Requests.Prescription;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.External.Laboratories;
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
            CreateMap<Address, AddressResponse>()
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate.ToString()));
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

            // ==================== Laboratory Mappings ====================
            
            // Laboratory
            CreateMap<Laboratory, LaboratoryResponse>();
            CreateMap<Laboratory, LaboratoryBasicResponse>();
            CreateMap<CreateLaboratoryRequest, Laboratory>();
            CreateMap<UpdateLaboratoryRequest, Laboratory>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Lab Order
            CreateMap<LabOrder, LabOrderResponse>();
            CreateMap<CreateLabOrderRequest, LabOrder>();

            // Lab Service
            CreateMap<LabService, LabServiceResponse>();
            CreateMap<CreateLabServiceRequest, LabService>();

            // Lab Working Hours
            CreateMap<LabWorkingHours, LabWorkingHoursResponse>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToTimeSpan()))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToTimeSpan()));
            CreateMap<CreateLabWorkingHoursRequest, LabWorkingHours>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.StartTime)))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.EndTime)));

            // Lab Test
            CreateMap<LabTest, LabTestResponse>();

            // Lab Result
            CreateMap<LabResult, LabResultResponse>();
            CreateMap<CreateLabResultRequest, LabResult>();

            // Laboratory Document
            CreateMap<LaboratoryDocument, LaboratoryDocumentResponse>();
            CreateMap<CreateLaboratoryDocumentRequest, LaboratoryDocument>();

            // Lab Prescription
            CreateMap<LabPrescription, LabPrescriptionResponse>();
            CreateMap<CreateLabPrescriptionRequest, LabPrescription>();

            // Lab Prescription Item
            CreateMap<LabPrescriptionItem, LabPrescriptionItemResponse>()
                .ForMember(dest => dest.SpecialInstructions, opt => opt.MapFrom(src => src.DoctorNotes));
            CreateMap<CreateLabPrescriptionItemRequest, LabPrescriptionItem>()
                .ForMember(dest => dest.DoctorNotes, opt => opt.MapFrom(src => src.SpecialInstructions));
        }
    }
}
