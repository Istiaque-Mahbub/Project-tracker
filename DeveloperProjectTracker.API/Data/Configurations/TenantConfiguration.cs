using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
      .ValueGeneratedOnAdd();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(x => x.Slug)
                .IsUnique();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();
        }
    }
}
