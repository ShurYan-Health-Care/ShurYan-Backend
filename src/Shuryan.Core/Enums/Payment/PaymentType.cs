using System.ComponentModel;

namespace Shuryan.Core.Enums.Payment
{
    /// <summary>
    /// نوع طريقة الدفع الإلكتروني (Card أو Mobile Wallet)
    /// </summary>
    public enum PaymentType
    {
        [Description("بطاقة ائتمان/خصم")]
        Card = 1,

        [Description("محفظة إلكترونية - فودافون كاش")]
        MobileWallet = 2
    }
}
