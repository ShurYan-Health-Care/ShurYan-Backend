using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Enums.Pharmacy;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class MedicationSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.Medications.AnyAsync())
            {
                Console.WriteLine("Medications already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Medications...");

            var medications = new List<Medication>
            {
                // Pain Relief - Fixed GUIDs for testing
                new Medication { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), BrandName = "بانادول", GenericName = "باراسيتامول", Strength = "500 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), BrandName = "بروفين", GenericName = "إيبوبروفين", Strength = "400 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), BrandName = "كتافلام", GenericName = "ديكلوفيناك البوتاسيوم", Strength = "50 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), BrandName = "فولتارين", GenericName = "ديكلوفيناك الصوديوم", Strength = "75 مجم", DosageForm = DosageForm.Injection, CreatedAt = DateTime.UtcNow },

                // Antibiotics
                new Medication { Id = Guid.NewGuid(), BrandName = "أوجمنتين", GenericName = "أموكسيسيلين + حمض الكلافولانيك", Strength = "1 جم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "فلوموكس", GenericName = "أموكسيسيلين", Strength = "500 مجم", DosageForm = DosageForm.Capsule, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "سيبروفلوكساسين", GenericName = "سيبروفلوكساسين", Strength = "500 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "زيثروماكس", GenericName = "أزيثرومايسين", Strength = "500 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },

                // Diabetes
                new Medication { Id = Guid.NewGuid(), BrandName = "جلوكوفاج", GenericName = "ميتفورمين", Strength = "500 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "أماريل", GenericName = "جليميبرايد", Strength = "2 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "نوفورابيد", GenericName = "أنسولين أسبارت", Strength = "100 وحدة/مل", DosageForm = DosageForm.Injection, CreatedAt = DateTime.UtcNow },

                // Blood Pressure
                new Medication { Id = Guid.NewGuid(), BrandName = "كونكور", GenericName = "بيسوبرولول", Strength = "5 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "نورفاسك", GenericName = "أملوديبين", Strength = "5 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "كوفرسيل", GenericName = "بيريندوبريل", Strength = "5 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },

                // Cholesterol
                new Medication { Id = Guid.NewGuid(), BrandName = "ليبيتور", GenericName = "أتورفاستاتين", Strength = "20 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "كريستور", GenericName = "روزوفاستاتين", Strength = "10 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },

                // Stomach & Digestive
                new Medication { Id = Guid.NewGuid(), BrandName = "نيكسيوم", GenericName = "إيزوميبرازول", Strength = "40 مجم", DosageForm = DosageForm.Capsule, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "كونترولوك", GenericName = "بانتوبرازول", Strength = "40 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "موتيليوم", GenericName = "دومبيريدون", Strength = "10 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "أنتينال", GenericName = "نيفوروكسازيد", Strength = "200 مجم", DosageForm = DosageForm.Capsule, CreatedAt = DateTime.UtcNow },

                // Allergy
                new Medication { Id = Guid.NewGuid(), BrandName = "تلفاست", GenericName = "فيكسوفينادين", Strength = "180 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "كلاريتين", GenericName = "لوراتادين", Strength = "10 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "زيرتك", GenericName = "سيتريزين", Strength = "10 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },

                // Vitamins & Supplements
                new Medication { Id = Guid.NewGuid(), BrandName = "فيتامين د", GenericName = "كوليكالسيفيرول", Strength = "5000 وحدة دولية", DosageForm = DosageForm.Capsule, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "فيروجلوبين", GenericName = "حديد + حمض الفوليك + فيتامين ب12", Strength = "متعدد", DosageForm = DosageForm.Capsule, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "كالسيوم د", GenericName = "كربونات الكالسيوم + فيتامين د", Strength = "600 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },

                // Cough & Cold
                new Medication { Id = Guid.NewGuid(), BrandName = "توسكان", GenericName = "ديكستروميثورفان", Strength = "15 مجم/5 مل", DosageForm = DosageForm.Syrup, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "كافوسيد", GenericName = "جوايفينيسين + ديكستروميثورفان", Strength = "متعدد", DosageForm = DosageForm.Syrup, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "كونجستال", GenericName = "باراسيتامول + كلورفينرامين", Strength = "متعدد", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },

                // Topical Medications
                new Medication { Id = Guid.NewGuid(), BrandName = "فيوسيدين", GenericName = "حمض الفوسيديك", Strength = "2%", DosageForm = DosageForm.Cream, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "بيتاديرم", GenericName = "بيتاميثازون", Strength = "0.1%", DosageForm = DosageForm.Cream, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "فولتارين جل", GenericName = "ديكلوفيناك", Strength = "1%", DosageForm = DosageForm.Ointment, CreatedAt = DateTime.UtcNow },

                // Pediatric Medications
                new Medication { Id = Guid.NewGuid(), BrandName = "بنادول شراب للأطفال", GenericName = "باراسيتامول", Strength = "120 مجم/5 مل", DosageForm = DosageForm.Syrup, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "بروفين شراب للأطفال", GenericName = "إيبوبروفين", Strength = "100 مجم/5 مل", DosageForm = DosageForm.Syrup, CreatedAt = DateTime.UtcNow },

                // Respiratory
                new Medication { Id = Guid.NewGuid(), BrandName = "فنتولين", GenericName = "سالبوتامول", Strength = "100 ميكروجرام", DosageForm = DosageForm.Other, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "سيريتايد", GenericName = "فلوتيكازون + سالميتيرول", Strength = "متعدد", DosageForm = DosageForm.Other, CreatedAt = DateTime.UtcNow },

                // Anticoagulants
                new Medication { Id = Guid.NewGuid(), BrandName = "أسبرين", GenericName = "حمض أسيتيل ساليسيليك", Strength = "75 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },
                new Medication { Id = Guid.NewGuid(), BrandName = "بلافيكس", GenericName = "كلوبيدوجريل", Strength = "75 مجم", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow },

                // Thyroid
                new Medication { Id = Guid.NewGuid(), BrandName = "إلتروكسين", GenericName = "ليفوثيروكسين", Strength = "100 ميكروجرام", DosageForm = DosageForm.Tablet, CreatedAt = DateTime.UtcNow }
            };

            await context.Medications.AddRangeAsync(medications);
            await context.SaveChangesAsync();

            Console.WriteLine($"{medications.Count} Medications seeded successfully!");
        }
    }
}
