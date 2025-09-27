using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
	public enum ClinicServiceType
	{
		[Description("كشف")]
		Examination = 1,

		[Description("سونار")]
		Ultrasound = 2,

		[Description("رسم قلب")]
		ECG = 3,

		[Description("تحاليل")]
		LabTests = 4,

		[Description("أشعة")]
		Radiology = 5,

		[Description("علاج طبيعي")]
		PhysicalTherapy = 6,

		[Description("تطعيمات")]
		Vaccinations = 7
	}
}
