using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Medical.Partners;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class DoctorPartnerSuggestionConfiguration : IEntityTypeConfiguration<DoctorPartnerSuggestion>
    {
        public void Configure(EntityTypeBuilder<DoctorPartnerSuggestion> builder)
        {
            builder.ToTable("DoctorPartnerSuggestions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.DoctorId)
                .IsRequired();

            // Pharmacy fields (nullable)
            builder.Property(s => s.SuggestedPharmacyId)
                .IsRequired(false);

            builder.Property(s => s.PharmacySuggestedAt)
                .IsRequired(false);

            // Laboratory fields (nullable)
            builder.Property(s => s.SuggestedLaboratoryId)
                .IsRequired(false);

            builder.Property(s => s.LaboratorySuggestedAt)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(s => s.DoctorId)
                .IsUnique()
                .HasDatabaseName("IX_DoctorPartnerSuggestions_DoctorId");

            builder.HasIndex(s => s.SuggestedPharmacyId)
                .HasDatabaseName("IX_DoctorPartnerSuggestions_PharmacyId");

            builder.HasIndex(s => s.SuggestedLaboratoryId)
                .HasDatabaseName("IX_DoctorPartnerSuggestions_LaboratoryId");

            // Relationships
            builder.HasOne(s => s.Doctor)
                .WithOne(d => d.PartnerSuggestion)
                .HasForeignKey<DoctorPartnerSuggestion>(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit fields
            builder.Property(s => s.CreatedAt)
                .IsRequired();

            builder.Property(s => s.UpdatedAt)
                .IsRequired(false);
        }
    }
}
