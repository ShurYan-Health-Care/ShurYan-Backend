using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
	public enum ClinicStatus
	{
		[Description("نشط")]
		Active,
		[Description("غير نشط")]
		Inactive,
	}
}
