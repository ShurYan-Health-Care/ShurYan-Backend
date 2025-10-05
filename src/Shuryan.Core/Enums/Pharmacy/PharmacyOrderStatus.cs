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
        [Description("في انتظار الموافقة والدفع من المريض")]
        PendingPayment = 1,

        [Description("تم الدفع - في انتظار تأكيد الصيدله")]
        PaidPendingLabConfirmation = 2,

        [Description("تم استلام الطلب")]
        Received = 3,

        [Description("جاري التحقق من التوفر")]
        VerificationInProgress = 4,

        [Description("تم تأكيد الطلب")]
        Confirmed = 3,

        [Description("جاري تحضير الطلب")]
        PreparationInProgress = 5,

        [Description("خرج للتوصيل")]
        OutForDelivery = 6,

        [Description("جاهز للاستلام")]
        ReadyForPickup = 7,

        [Description("تم التسليم")]
        Delivered = 8,

        [Description("ملغي")]
        Cancelled = 9
    }
}
