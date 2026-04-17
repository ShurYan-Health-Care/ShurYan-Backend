using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Telemedicine
{
    public class CreateSessionRequest
    {
        [Required]
        public Guid AppointmentId { get; set; }
    }
}
