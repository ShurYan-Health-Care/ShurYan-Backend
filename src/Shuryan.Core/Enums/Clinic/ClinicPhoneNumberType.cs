using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Clinic
{
    public enum ClinicPhoneNumberType
    {
        [Description("خط ارضي")]
        Landline,
        [Description("واتساب")]
        WhatsApp,
        [Description("موبايل")]
        Mobile
    }
}
