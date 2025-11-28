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
        [Description("طلب جديد")]
        NewRequest = 1,

        [Description("في انتظار مراجعة المعمل")]
        AwaitingLabReview = 2,

        [Description("تم التأكيد من المعمل")]
        ConfirmedByLab = 3,

        [Description("في انتظار الدفع")]
        AwaitingPayment = 4,

        [Description("تم الدفع")]
        Paid = 5,

        [Description("في انتظار العينات")]
        AwaitingSamples = 6,

        [Description("قيد التنفيذ في المعمل")]
        InProgressAtLab = 7,

        [Description("النتائج جاهزة")]
        ResultsReady = 8,

        [Description("تم الاستلام")]
        Completed = 9,

        [Description("ملغي من المريض")]
        CancelledByPatient = 10,

        [Description("مرفوض من المعمل")]
        RejectedByLab = 11,
    }
}
