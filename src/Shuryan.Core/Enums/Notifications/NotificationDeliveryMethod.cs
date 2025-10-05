using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Notifications
{
	public enum NotificationDeliveryMethod
	{
		[Description("داخل التطبيق")]
		InApp = 1,

		[Description("بريد إلكتروني")]
		Email = 2,

		[Description("رسالة نصية")]
		SMS = 3,

		[Description("إشعار فوري")]
		PushNotification = 4
	}
}
