using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Clinic
{
    public enum ClinicServiceType
    {
        [Description("كشف")]
        Examination = 1,

        [Description("سونار")]
        Ultrasound = 2,

        [Description("رسم قلب")]
        ECG = 3,

        [Description("تحاليل")]
        LabTests = 4,

        [Description("أشعة")]
        Radiology = 5,

        [Description("علاج طبيعي")]
        PhysicalTherapy = 6,

        [Description("تطعيمات")]
        Vaccinations = 7,

        [Description("عمليات جراحية صغرى")]
        MinorSurgery = 8,

        [Description("تضميد جروح")]
        WoundDressing = 9,

        [Description("قياس ضغط")]
        BloodPressureCheck = 10,

        [Description("قياس سكر")]
        BloodSugarCheck = 11,

        [Description("حقن")]
        Injections = 12,

        [Description("خياطة جروح")]
        Suturing = 13,

        [Description("إزالة غرز")]
        StitchRemoval = 14,

        [Description("جبائر")]
        Splinting = 15,

        [Description("استنشاق (بخار)")]
        Nebulization = 16,

        [Description("غسيل أذن")]
        EarWashing = 17,

        [Description("كي")]
        Cauterization = 18,

        [Description("ختان")]
        Circumcision = 19,

        [Description("تركيب قسطرة")]
        CatheterInsertion = 20,

        [Description("سحب عينات")]
        SampleCollection = 21,

        [Description("فحص نظر")]
        VisionTest = 22,

        [Description("قياس سمع")]
        HearingTest = 23,

        [Description("علاج تنفسي")]
        RespiratoryTherapy = 24,

        [Description("علاج بالأكسجين")]
        OxygenTherapy = 25,

        [Description("تخطيط عضلات")]
        EMG = 26,

        [Description("تخطيط أعصاب")]
        NerveConduction = 27
    }
}
