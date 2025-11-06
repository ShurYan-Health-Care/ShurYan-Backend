using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Medical.Partners;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class DoctorPartnerSuggestionConfiguration : AuditableEntityConfiguration<DoctorPartnerSuggestion>
    {
        public override void Configure(EntityTypeBuilder<DoctorPartnerSuggestion> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("DoctorPartnerSuggestions");

            // Properties
            builder.Property(s => s.DoctorId).IsRequired();
            builder.Property(s => s.SuggestedPharmacyId).IsRequired(false);
            builder.Property(s => s.PharmacySuggestedAt).IsRequired(false);
            builder.Property(s => s.SuggestedLaboratoryId).IsRequired(false);
            builder.Property(s => s.LaboratorySuggestedAt).IsRequired(false);

            // Indexes
            builder.HasIndex(s => s.DoctorId)
                .IsUnique()
                .HasDatabaseName("IX_DoctorPartnerSuggestion_DoctorId");

            builder.HasIndex(s => s.SuggestedPharmacyId)
                .HasDatabaseName("IX_DoctorPartnerSuggestion_PharmacyId");

            builder.HasIndex(s => s.SuggestedLaboratoryId)
                .HasDatabaseName("IX_DoctorPartnerSuggestion_LaboratoryId");

            // Relationships
            builder.HasOne(s => s.Doctor)
                .WithOne(d => d.PartnerSuggestion)
                .HasForeignKey<DoctorPartnerSuggestion>(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
