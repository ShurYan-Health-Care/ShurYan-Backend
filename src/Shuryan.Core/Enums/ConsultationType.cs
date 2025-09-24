using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
	public enum ConsultationType
	{
		[Description("كشف عادي")]
		Regular = 1,

		[Description("إعادة")]
		FollowUp = 2,

		//[Description("استشارة")]
		//Consultation = 3,
	}
}
