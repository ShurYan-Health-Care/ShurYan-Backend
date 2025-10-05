using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Laboratory
{
    public enum SampleCollectionType
    {
        [Description("زيارة المعمل")]
        LabVisit = 1,

        [Description("جمع عينة من المنزل")]
        HomeSampleCollection = 2
    }
}
