using System.ComponentModel;

namespace Shuryan.Core.Enums.Payment
{
    public enum PaymentStatus
    {
        [Description("في انتظار الدفع")]
        Pending = 1,

        [Description("جاري المعالجة")]
        Processing = 2,

        [Description("تم الدفع بنجاح")]
        Completed = 3,

        [Description("فشل الدفع")]
        Failed = 4,

        [Description("تم الإلغاء")]
        Cancelled = 5,

        [Description("تم الاسترجاع")]
        Refunded = 6,

        [Description("استرجاع جزئي")]
        PartiallyRefunded = 7
    }
}
