using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Infrastructure.Data.Configurations.Identity
{
    public class VerifierEntityConfiguration : IEntityTypeConfiguration<Verifier>
    {
        public void Configure(EntityTypeBuilder<Verifier> builder)
        {
            builder.ToTable("Verifiers");

            builder.Property(dv => dv.CreatedByAdminId)
                   .IsRequired();

            // Relationships
            builder.HasMany(dv => dv.VerifiedDoctors)
                   .WithOne(d => d.Verifier)
                   .HasForeignKey(d => d.VerifierId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Relationships
            builder.HasMany(dv => dv.VerifiedLabors)
                   .WithOne(d => d.Verifier)
                   .HasForeignKey(d => d.VerifierId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
