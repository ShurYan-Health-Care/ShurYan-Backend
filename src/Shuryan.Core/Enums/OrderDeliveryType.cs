using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums
{
    public enum OrderDeliveryType
    {
        [Description("توصيل سريع")]
        ExpressDelivery = 1,

        [Description("توصيل مُجدوَل")]
        ScheduledDelivery = 2,

        [Description("استلام من الصيدلية")]
        PharmacyPickup = 3
    }
}
