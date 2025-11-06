using System.ComponentModel;

namespace Shuryan.Core.Enums.Pharmacy
{
    public enum PharmacyItemStatus
    {
        [Description("متاح")]
        Available = 1,

        [Description("غير متاح")]
        NotAvailable = 2,

        [Description("يوجد بديل")]
        AlternativeOffered = 3
    }
}
