using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Doctor
{
    public enum DoctorDocumentType
    {
        // Important for Verification
        [Description("البطاقة الشخصية")]
        NationalId,
        [Description("رخصة مزاولة المهنة")]
        MedicalPracticeLicense,
        [Description("عضوية النقابة")]
        SyndicateMembershipCard,
        [Description("شهادة التخرج من كلية الطب")]
        MedicalGraduationCertificate,
        [Description("شهادة التخصص")]
        SpecialtyCertificate,

        // Optional Professional Information
        [Description("شهادات مهنية اضافية")]
        AdditionalCertificates,
        [Description("جوائز وتقديرات")]
        AwardsAndRecognitions,
        [Description("الأبحاث المنشورة")]
        PublishedResearch,
        [Description("العضويات المهنية")]
        ProfessionalMemberships
    }
}