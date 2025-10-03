using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
    public enum OverrideType
    {
        [Description("متاح")]
        Available = 1,

        [Description("غير متاح")]
        Unavailable = 2
    }
}
