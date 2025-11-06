using System.ComponentModel;

namespace Shuryan.Core.Enums.Payment
{
    public enum OrderType
    {
        [Description("طلب صيدلية")]
        PharmacyOrder = 1,

        [Description("طلب معمل")]
        LabOrder = 2,

        [Description("حجز استشارة")]
        ConsultationBooking = 3
    }
}
