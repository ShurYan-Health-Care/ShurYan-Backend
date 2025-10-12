using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Notification
{
    public class MarkNotificationAsReadRequest
    {
        [Required(ErrorMessage = "Notification ID is required")]
        public Guid NotificationId { get; set; }
    }
}

