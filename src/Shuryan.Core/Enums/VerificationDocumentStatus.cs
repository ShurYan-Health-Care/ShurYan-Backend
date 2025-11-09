using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
    public enum VerificationDocumentStatus
    {
        [Description("مسودة")]
        Draft = 0,

        [Description("تحت المراجعة")]
        UnderReview = 1,

        [Description("مقبول")]
        Approved = 2,

        [Description("مرفوض")]
        Rejected = 3,

        [Description("منتهي الصلاحية")]
        Expired = 4
    }
}
