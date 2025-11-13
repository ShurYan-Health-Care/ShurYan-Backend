using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class LabTestSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.LabTests.AnyAsync())
            {
                Console.WriteLine("Lab Tests already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Lab Tests...");

            var labTests = new List<LabTest>
            {
                // Complete Blood Count Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "صورة دم كاملة",
                    Code = "CBC",
                    Category = LabTestCategory.CompleteBloodCount,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "تحليل هيموجلوبين",
                    Code = "HGB",
                    Category = LabTestCategory.CompleteBloodCount,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "عد الصفائح الدموية",
                    Code = "PLT",
                    Category = LabTestCategory.CompleteBloodCount,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Liver Function Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "وظائف الكبد الكاملة",
                    Code = "LFT",
                    Category = LabTestCategory.LiverFunction,
                    SpecialInstructions = "يفضل الصيام 8 ساعات قبل التحليل",
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "إنزيم ALT",
                    Code = "ALT",
                    Category = LabTestCategory.LiverFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "إنزيم AST",
                    Code = "AST",
                    Category = LabTestCategory.LiverFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "البيليروبين الكلي",
                    Code = "TBIL",
                    Category = LabTestCategory.LiverFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Kidney Function Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "وظائف الكلى",
                    Code = "RFT",
                    Category = LabTestCategory.KidneyFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "الكرياتينين",
                    Code = "CREAT",
                    Category = LabTestCategory.KidneyFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "اليوريا",
                    Code = "UREA",
                    Category = LabTestCategory.KidneyFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "حمض اليوريك",
                    Code = "URIC",
                    Category = LabTestCategory.KidneyFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Blood Sugar Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "سكر صائم",
                    Code = "FBS",
                    Category = LabTestCategory.BloodSugar,
                    SpecialInstructions = "صيام 8-12 ساعة قبل التحليل",
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "سكر فاطر",
                    Code = "RBS",
                    Category = LabTestCategory.BloodSugar,
                    SpecialInstructions = "بعد ساعتين من تناول الطعام",
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "سكر تراكمي",
                    Code = "HBA1C",
                    Category = LabTestCategory.BloodSugar,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Lipid Profile Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "دهون كاملة",
                    Code = "LIPID",
                    Category = LabTestCategory.LipidProfile,
                    SpecialInstructions = "صيام 12 ساعة قبل التحليل",
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "الكوليسترول الكلي",
                    Code = "CHOL",
                    Category = LabTestCategory.LipidProfile,
                    SpecialInstructions = "صيام 12 ساعة قبل التحليل",
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "الدهون الثلاثية",
                    Code = "TG",
                    Category = LabTestCategory.LipidProfile,
                    SpecialInstructions = "صيام 12 ساعة قبل التحليل",
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "الكوليسترول الضار",
                    Code = "LDL",
                    Category = LabTestCategory.LipidProfile,
                    SpecialInstructions = "صيام 12 ساعة قبل التحليل",
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "الكوليسترول النافع",
                    Code = "HDL",
                    Category = LabTestCategory.LipidProfile,
                    SpecialInstructions = "صيام 12 ساعة قبل التحليل",
                    CreatedAt = DateTime.UtcNow
                },

                // Thyroid Function Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "وظائف الغدة الدرقية",
                    Code = "TFT",
                    Category = LabTestCategory.ThyroidFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "هرمون TSH",
                    Code = "TSH",
                    Category = LabTestCategory.ThyroidFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "هرمون T3",
                    Code = "T3",
                    Category = LabTestCategory.ThyroidFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "هرمون T4",
                    Code = "T4",
                    Category = LabTestCategory.ThyroidFunction,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Hormone Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "هرمون التستوستيرون",
                    Code = "TEST",
                    Category = LabTestCategory.Hormones,
                    SpecialInstructions = "يفضل أخذ العينة في الصباح الباكر",
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "هرمون الاستروجين",
                    Code = "E2",
                    Category = LabTestCategory.Hormones,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "هرمون البرولاكتين",
                    Code = "PRL",
                    Category = LabTestCategory.Hormones,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Vitamins and Minerals Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "فيتامين د",
                    Code = "VITD",
                    Category = LabTestCategory.VitaminsAndMinerals,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "فيتامين ب12",
                    Code = "VITB12",
                    Category = LabTestCategory.VitaminsAndMinerals,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "الحديد",
                    Code = "FE",
                    Category = LabTestCategory.VitaminsAndMinerals,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "الكالسيوم",
                    Code = "CA",
                    Category = LabTestCategory.VitaminsAndMinerals,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "المغنيسيوم",
                    Code = "MG",
                    Category = LabTestCategory.VitaminsAndMinerals,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Immunology Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "تحليل فيروس سي",
                    Code = "HCV",
                    Category = LabTestCategory.Immunology,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "تحليل فيروس بي",
                    Code = "HBV",
                    Category = LabTestCategory.Immunology,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "تحليل الإيدز",
                    Code = "HIV",
                    Category = LabTestCategory.Immunology,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Urinalysis Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "تحليل بول كامل",
                    Code = "URINE",
                    Category = LabTestCategory.Urinalysis,
                    SpecialInstructions = "عينة بول الصباح الأولى",
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "مزرعة بول",
                    Code = "UC",
                    Category = LabTestCategory.Urinalysis,
                    SpecialInstructions = "عينة نظيفة من منتصف البول",
                    CreatedAt = DateTime.UtcNow
                },

                // Stool Analysis Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "تحليل براز كامل",
                    Code = "STOOL",
                    Category = LabTestCategory.StoolAnalysis,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "مزرعة براز",
                    Code = "SC",
                    Category = LabTestCategory.StoolAnalysis,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Microbiology Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "مزرعة دم",
                    Code = "BC",
                    Category = LabTestCategory.Microbiology,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "مزرعة حلق",
                    Code = "TC",
                    Category = LabTestCategory.Microbiology,
                    SpecialInstructions = "قبل تناول المضادات الحيوية",
                    CreatedAt = DateTime.UtcNow
                },

                // Tumor Markers Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "علامة ورم البروستاتا PSA",
                    Code = "PSA",
                    Category = LabTestCategory.TumorMarkers,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "علامة ورم CA 19-9",
                    Code = "CA199",
                    Category = LabTestCategory.TumorMarkers,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "علامة ورم CA 15-3",
                    Code = "CA153",
                    Category = LabTestCategory.TumorMarkers,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },

                // Pregnancy and Fertility Tests
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "تحليل حمل رقمي",
                    Code = "BHCG",
                    Category = LabTestCategory.PregnancyAndFertility,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "تحليل مخزون المبيض",
                    Code = "AMH",
                    Category = LabTestCategory.PregnancyAndFertility,
                    SpecialInstructions = null,
                    CreatedAt = DateTime.UtcNow
                },
                new LabTest
                {
                    Id = Guid.NewGuid(),
                    Name = "تحليل الحيوانات المنوية",
                    Code = "SA",
                    Category = LabTestCategory.PregnancyAndFertility,
                    SpecialInstructions = "امتناع عن العلاقة الزوجية لمدة 2-5 أيام",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.LabTests.AddRangeAsync(labTests);
            await context.SaveChangesAsync();

            Console.WriteLine($"{labTests.Count} Lab Tests seeded successfully!");
        }
    }
}
