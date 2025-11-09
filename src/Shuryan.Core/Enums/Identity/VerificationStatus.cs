using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Identity
{
    public enum VerificationStatus
    {
        [Description("غير معتمد")]
        Unverified = 0,

        [Description("تحت المراجعة")]
        UnderReview = 1,

        [Description("معتمد")]
        Verified = 2,

        [Description("مرفوض")]
        Rejected = 3,

        [Description("معلق")]
        Suspended = 4,

        [Description("مُرسل")]
        Sent = 5
    }
}
