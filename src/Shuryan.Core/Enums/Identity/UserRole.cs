using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Identity
{
    public enum UserRole
    {
        [Description("مريض")]
        Patient = 1,

        [Description("طبيب")]
        Doctor = 2,

        [Description("مدير النظام")]
        Admin = 3,

        [Description("فاحص الطبيب")]
        Verifier = 4,

        [Description("معمل تحاليل")]
        Laboratory = 5,

        [Description("صيدلية")]
        Pharmacy = 6
    }
}
