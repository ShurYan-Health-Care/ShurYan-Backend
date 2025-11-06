using System.ComponentModel;

namespace Shuryan.Core.Enums.Payment
{
    public enum PaymentProvider
    {
        [Description("نظام داخلي")]
        Internal = 1,

        [Description("باي موب")]
        Paymob = 2,

        [Description("فوري")]
        Fawry = 3,

        [Description("أوبي")]
        Opay = 4,

        [Description("فودافون كاش")]
        VodafoneCash = 5
    }
}
