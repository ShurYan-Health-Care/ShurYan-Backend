using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Appointments
{
    public enum AppointmentStatus
    {
        [Description("محجوز ومؤكد")]
        Confirmed = 1,

        [Description("وصل المريض")]
        CheckedIn = 2,

        [Description("الكشف جاري")]
        InProgress = 3,

        [Description("انتهى الموعد")]
        Completed = 4,

        [Description("لم يحضر المريض")]
        NoShow = 5,

        [Description("ملغي")]
        Cancelled = 6,

        [Description("الدفع المعلق")]
        PendingPayment = 7,
    }
}
