using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Laboratory
{
    public enum LabOrderStatus
    {
        [Description("في انتظار الموافقة والدفع من المريض")]
        PendingPayment = 1,

        [Description("تم الدفع - في انتظار تأكيد المعمل")]
        PaidPendingLabConfirmation = 2,

        [Description("تم التأكيد من المعمل")]
        ConfirmedByLab = 3,

        [Description("قيد التنفيذ في المعمل")]
        InProgress = 4,

        [Description("النتائج جاهزة")]
        ResultsReady = 5,

        [Description("تم الاستلام")]
        Completed = 6,

        [Description("ملغي من المريض")]
        CancelledByPatient = 7,

        [Description("ملغي من المعمل")]
        CancelledByLab = 8,
    }
}
