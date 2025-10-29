using System.ComponentModel;

namespace Shuryan.Core.Enums.Medical
{
    /// <summary>
    /// أنواع الشركاء اللي الدكتور ممكن يقترحهم
    /// </summary>
    public enum PartnerType
    {
        [Description("صيدلية")]
        Pharmacy = 1,

        [Description("معمل تحاليل")]
        Laboratory = 2
    }
}
