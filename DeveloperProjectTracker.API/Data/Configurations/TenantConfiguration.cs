using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class TenantConfiguration
        : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(
            EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("Tenants");

            // Primary Key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            // Tenant Name
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            // Tenant Slug
            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(150);

            // Slug must be unique
            builder.HasIndex(x => x.Slug)
                .IsUnique();

            // CreatedAt
            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Active status
            builder.Property(x => x.IsActive)
                .IsRequired();
        }
    }
}