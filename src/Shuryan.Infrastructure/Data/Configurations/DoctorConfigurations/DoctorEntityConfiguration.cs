using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Partners;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Infrastructure.Data.Configurations.DoctorConfigurations
{
    public class DoctorEntityConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            // Table Mapping
            builder.ToTable("Doctors");

            // Properties
            builder.Property(d => d.MedicalSpecialty).IsRequired().HasConversion<int>();
            builder.Property(d => d.YearsOfExperience).IsRequired();
            builder.Property(d => d.Biography).IsRequired(false).HasMaxLength(1000);
            builder.Property(d => d.VerificationStatus).IsRequired().HasConversion<int>().HasDefaultValue(VerificationStatus.Unverified);
            builder.Property(d => d.VerifiedAt).IsRequired(false);
            builder.Property(d => d.VerifierId).IsRequired(false);

            // Ignore NotMapped Properties
            builder.Ignore(d => d.AverageRating);
            builder.Ignore(d => d.TotalReviewsCount);

            // Verifier Relationship (Many-to-One, Optional)
            builder.HasOne(d => d.Verifier)
                .WithMany(v => v.VerifiedDoctors)
                .HasForeignKey(d => d.VerifierId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Clinic Relationship (One-to-One, Optional)
            builder.HasOne(d => d.Clinic)
                .WithOne(c => c.Doctor)
                .HasForeignKey<Clinic>(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Partner Suggestion Relationship (One-to-One, Optional)
            builder.HasOne(d => d.PartnerSuggestion)
                .WithOne(ps => ps.Doctor)
                .HasForeignKey<DoctorPartnerSuggestion>(ps => ps.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Consultations Relationship (One-to-Many)
            builder.HasMany(d => d.Consultations)
                .WithOne(ds => ds.Doctor)
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Overrides Relationship (One-to-Many)
            builder.HasMany(d => d.Overrides)
                .WithOne(do_override => do_override.Doctor)
                .HasForeignKey(do_override => do_override.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Availabilities Relationship (One-to-Many)
            builder.HasMany(d => d.Availabilities)
                .WithOne(da => da.Doctor)
                .HasForeignKey(da => da.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Verification Documents Relationship (One-to-Many)
            builder.HasMany(d => d.VerificationDocuments)
                .WithOne(vd => vd.Doctor)
                .HasForeignKey(vd => vd.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointments Relationship (One-to-Many)
            builder.HasMany(d => d.Appointments)
                .WithOne(a => a.Doctor)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescriptions Relationship (One-to-Many)
            builder.HasMany(d => d.Prescriptions)
                .WithOne(pr => pr.Doctor)
                .HasForeignKey(pr => pr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lab Prescriptions Relationship (One-to-Many)
            builder.HasMany(d => d.LabPrescriptions)
                .WithOne(lp => lp.Doctor)
                .HasForeignKey(lp => lp.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor Reviews Relationship (One-to-Many)
            builder.HasMany(d => d.DoctorReviews)
                .WithOne(dr => dr.Doctor)
                .HasForeignKey(dr => dr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(d => d.MedicalSpecialty)
                .HasDatabaseName("IX_Doctor_MedicalSpecialty");

            builder.HasIndex(d => d.VerificationStatus)
                .HasDatabaseName("IX_Doctor_VerificationStatus");

            builder.HasIndex(d => new { d.VerificationStatus, d.MedicalSpecialty })
                .HasDatabaseName("IX_Doctor_Verification_Specialty");

            // Check Constraint
            builder.HasCheckConstraint("CK_Doctor_YearsOfExperience", "[YearsOfExperience] >= 0 AND [YearsOfExperience] <= 60");
        }
    }
}
