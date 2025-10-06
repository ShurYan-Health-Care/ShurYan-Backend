using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Infrastructure.Data.Configurations.BaseConfigurations
{
	public abstract class AuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : AuditableEntity
	{
		public virtual void Configure(EntityTypeBuilder<TEntity> builder)
		{
			builder.HasKey(e => e.Id);
			builder.Property(e => e.Id).ValueGeneratedOnAdd();
			builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
			builder.Property(e => e.CreatedBy).IsRequired(false);
			builder.Property(e => e.UpdatedAt).IsRequired(false);
			builder.Property(e => e.UpdatedBy).IsRequired(false);
		}
	}
}
