using System.ComponentModel;

namespace Shuryan.Core.Enums.Payment
{
    public enum PaymentMethod
    {
        [Description("كاش عند الاستلام")]
        CashOnDelivery = 1,

        [Description("بطاقة ائتمان/خصم")]
        CreditCard = 2,

        [Description("فودافون كاش")]
        VodafoneCash = 3,

        [Description("محفظة إلكترونية")]
        Wallet = 4,

        [Description("تحويل بنكي")]
        BankTransfer = 5
    }
}
