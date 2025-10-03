using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
    public enum PharmacyPaymentMethod
    {
        [Description("الدفع عند الاستلام")]
        CashOnDelivery = 1,

        [Description("الدفع أونلاين بالبطاقة")]
        OnlineCreditCard = 2, // (للمستقبل)

        [Description("الدفع من خلال التأمين")]
        Insurance = 3 // (للمستقبل)
    }
}
