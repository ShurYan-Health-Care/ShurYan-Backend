using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Pharmacy
{
    public enum OrderDeliveryType
    {
        [Description("توصيل")]
        Delivery = 1,

        [Description("استلام من الصيدلية")]
        PharmacyPickup = 2
    }
}
