using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Pharmacy
{
    public enum DosageForm
    {
        [Description("أقراص")]
        Tablet = 1,

        [Description("كبسولة")]
        Capsule = 2,

        [Description("شراب")]
        Syrup = 3,

        [Description("حقنة")]
        Injection = 4,

        [Description("كريم")]
        Cream = 5,

        [Description("مرهم")]
        Ointment = 6,

        [Description("أخرى")]
        Other = 99
    }
}
