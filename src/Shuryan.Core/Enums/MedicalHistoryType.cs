using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
	public enum MedicalHistoryType
	{
		// Medical Information - Optional but Recommended
		[Description("الحساسية من الادوية")]
		DrugAllergy,
		[Description("الامراض المزمنة")]
		ChronicDisease,
		[Description("الادوية الحالية")]
		CurrentMedication,
		[Description("العمليات الجراحية السابقة")]
		PreviousSurgery
	}
}
