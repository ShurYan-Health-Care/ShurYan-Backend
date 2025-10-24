using AutoMapper;
using Shuryan.Application.DTOs.Requests.Review;
using Shuryan.Application.DTOs.Responses.Review;
using Shuryan.Core.Entities.System.Review;

namespace Shuryan.Application.Mappers
{
    public class ReviewMappingProfile : Profile
    {
        public ReviewMappingProfile()
        {
            #region Doctor Review Mappings
            CreateMap<DoctorReview, DoctorReviewResponse>();
            CreateMap<CreateDoctorReviewRequest, DoctorReview>();
            CreateMap<UpdateDoctorReviewRequest, DoctorReview>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Pharmacy Review Mappings
            CreateMap<PharmacyReview, PharmacyReviewResponse>();
            CreateMap<CreatePharmacyReviewRequest, PharmacyReview>();
            #endregion

            #region Laboratory Review Mappings
            CreateMap<LaboratoryReview, LaboratoryReviewResponse>();
            CreateMap<CreateLaboratoryReviewRequest, LaboratoryReview>();
            #endregion
        }
    }
}
