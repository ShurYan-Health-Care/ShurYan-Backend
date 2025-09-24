using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
	public enum VerificationStatus
	{
		[Description("غير مؤكد - لم يتم التحقق من المؤهلات")]
		Unverified = 0,

		[Description("تحت المراجعة - سيتم التحقق خلال 3-5 أيام")]
		UnderReview = 1,

		[Description("طبيب معتمد ومتحقق من المؤهلات")]
		Verified = 2,

		[Description("مرفوض")]
		Rejected = 3,

		[Description("معلق")]
		Suspended = 4
	}
}
