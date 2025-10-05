using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Doctor
{
    public enum MedicalSpecialty
    {
        [Description("طب عام")]
        GeneralMedicine = 1,

        [Description("طب الأطفال")]
        Pediatrics = 2,

        [Description("أمراض النساء والتوليد")]
        ObstetricsGynecology = 3,

        [Description("جراحة عامة")]
        GeneralSurgery = 4,

        [Description("طب القلب")]
        Cardiology = 5,

        [Description("طب الأعصاب")]
        Neurology = 6,

        [Description("طب العظام")]
        Orthopedics = 7,

        [Description("طب الجلدية")]
        Dermatology = 8,

        [Description("طب العيون")]
        Ophthalmology = 9,

        [Description("أنف وأذن وحنجرة")]
        ENT = 10,

        [Description("طب نفسي")]
        Psychiatry = 11,

        [Description("طب المسالك البولية")]
        Urology = 12,

        [Description("طب الصدر")]
        Pulmonology = 13,

        [Description("طب الكلى")]
        Nephrology = 14,

        [Description("طب الجهاز الهضمي")]
        Gastroenterology = 15,

        [Description("طب الغدد الصماء")]
        Endocrinology = 16,

        [Description("طب الأورام")]
        Oncology = 17,

        [Description("طب الأشعة")]
        Radiology = 18,

        [Description("طب التخدير")]
        Anesthesiology = 19,

        [Description("طب الطوارئ")]
        EmergencyMedicine = 20
    }
}
