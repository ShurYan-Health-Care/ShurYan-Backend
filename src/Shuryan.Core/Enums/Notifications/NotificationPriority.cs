using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Notifications
{
	public enum NotificationPriority
	{
		[Description("منخفضة")]
		Low = 1,

		[Description("عادية")]
		Normal = 2,

		[Description("مرتفعة")]
		High = 3,

		[Description("عاجلة")]
		Urgent = 4
	}
}
