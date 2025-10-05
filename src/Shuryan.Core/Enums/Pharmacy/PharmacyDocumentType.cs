using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Pharmacy
{
    public enum PharmacyDocumentType
    {
        [Description("الترخيص التجاري")]
        CommercialLicense = 1,

        [Description("ترخيص وزارة الصحة")]
        HealthMinistryLicense = 2,

        [Description("شهادة الجودة ISO")]
        ISOCertificate = 3,

        [Description("عقد الإيجار أو ملكية المكان")]
        PropertyDocument = 4,

        [Description("البطاقة الضريبية")]
        TaxCard = 5,

        [Description("مستندات إضافية")]
        AdditionalDocuments = 6

    }
}
