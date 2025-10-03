using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
    public enum PharmacyOrderStatus
    {
        [Description("تم استلام الطلب")]
        Received = 1,

        [Description("جاري التحقق من التوفر")]
        VerificationInProgress = 2,

        [Description("تم تأكيد الطلب")]
        Confirmed = 3,

        [Description("جاري تحضير الطلب")]
        PreparationInProgress = 4,

        [Description("خرج للتوصيل")]
        OutForDelivery = 5,

        [Description("جاهز للاستلام")]
        ReadyForPickup = 6,

        [Description("تم التسليم")]
        Delivered = 7,

        [Description("ملغي")]
        Cancelled = 8
    }
}
