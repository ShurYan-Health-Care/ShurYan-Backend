using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Pharmacy
{
    public enum PharmacyOrderStatus
    {
        [Description("في انتظار رد الصيدلية")]
        PendingPharmacyResponse = 1,

        [Description("في انتظار تأكيد المريض")]
        WaitingForPatientConfirmation = 2,

        [Description("في انتظار الدفع")]
        PendingPayment = 3,

        [Description("تم تأكيد الطلب")]
        Confirmed = 5,

        [Description("جاري تحضير الطلب")]
        PreparationInProgress = 6,

        [Description("خرج للتوصيل")]
        OutForDelivery = 7,

        [Description("جاهز للاستلام")]
        ReadyForPickup = 8,

        [Description("تم التسليم")]
        Delivered = 9,

        [Description("ملغي")]
        Cancelled = 10
    }
}
