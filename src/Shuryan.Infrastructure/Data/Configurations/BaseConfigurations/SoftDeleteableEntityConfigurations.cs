using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Infrastructure.Data.Configurations.BaseConfigurations
{
	public abstract class SoftDeletableEntityConfiguration<TEntity> : AuditableEntityConfiguration<TEntity> where TEntity : SoftDeletableEntity
	{
		public override void Configure(EntityTypeBuilder<TEntity> builder)
		{
			base.Configure(builder);

			builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

			builder.Property(e => e.DeletedAt).IsRequired(false);

			builder.Property(e => e.DeletedBy).IsRequired(false);
		}
	}
}
