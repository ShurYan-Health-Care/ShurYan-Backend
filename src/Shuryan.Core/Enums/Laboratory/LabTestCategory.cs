using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Laboratory
{
    public enum LabTestCategory
    {
        [Description("تحاليل الدم الكاملة")]
        CompleteBloodCount = 1,

        [Description("تحاليل وظائف الكبد")]
        LiverFunction = 2,

        [Description("تحاليل وظائف الكلى")]
        KidneyFunction = 3,

        [Description("تحاليل السكر")]
        BloodSugar = 4,

        [Description("تحاليل الدهون")]
        LipidProfile = 5,

        [Description("تحاليل الغدة الدرقية")]
        ThyroidFunction = 6,

        [Description("تحاليل الهرمونات")]
        Hormones = 7,

        [Description("تحاليل الفيتامينات والمعادن")]
        VitaminsAndMinerals = 8,

        [Description("تحاليل المناعة")]
        Immunology = 9,

        [Description("تحاليل البول")]
        Urinalysis = 10,

        [Description("تحاليل البراز")]
        StoolAnalysis = 11,

        [Description("تحاليل الميكروبات")]
        Microbiology = 12,

        [Description("تحاليل الأورام")]
        TumorMarkers = 13,

        [Description("تحاليل الحمل والخصوبة")]
        PregnancyAndFertility = 14,

        [Description("تحاليل أخرى")]
        Other = 99
    }
}
