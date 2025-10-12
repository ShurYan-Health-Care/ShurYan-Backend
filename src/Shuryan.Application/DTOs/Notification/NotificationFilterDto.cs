using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Notification
{
    public class NotificationFilterDto : PaginationParams
    {
        public Guid? UserId { get; set; }
        public bool? IsRead { get; set; }
        public NotificationType? Type { get; set; }
        public NotificationPriority? Priority { get; set; }
    }
}
