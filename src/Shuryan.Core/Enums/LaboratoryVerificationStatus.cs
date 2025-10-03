using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
	public enum LaboratoryVerificationStatus
	{
		[Description("غير مؤكد - لم يتم التحقق من المستندات")]
		Unverified = 0,

		[Description("تحت المراجعة - سيتم التحقق خلال 3-5 أيام")]
		UnderReview = 1,

		[Description("معمل معتمد ومتحقق من المستندات")]
		Verified = 2,

		[Description("مرفوض")]
		Rejected = 3,

		[Description("معلق")]
		Suspended = 4
	}
}
