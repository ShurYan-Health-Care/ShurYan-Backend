using Shuryan.Core.Entities.System.Review;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ReviewRepositories
{
    public interface IDoctorReviewRepository : IGenericRepository<DoctorReview>
    {
        Task<IEnumerable<DoctorReview>> GetReviewsByDoctorAsync(Guid doctorId);
        Task<IEnumerable<DoctorReview>> GetReviewsByPatientAsync(Guid patientId);
        Task<DoctorReview?> GetReviewByAppointmentAsync(Guid appointmentId);
        Task<double> GetAverageRatingForDoctorAsync(Guid doctorId);
        Task<int> GetReviewCountForDoctorAsync(Guid doctorId);
    }
}

