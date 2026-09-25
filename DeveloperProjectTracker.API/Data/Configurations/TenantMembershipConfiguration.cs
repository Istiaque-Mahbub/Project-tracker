using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class TenantMembershipConfiguration : IEntityTypeConfiguration<TenantMembership>
    {
        public void Configure(EntityTypeBuilder<TenantMembership> builder)
        {
            builder.ToTable("TenantMemberships");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Role)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.JoinedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Tenant -> Memberships
            builder.HasOne(x => x.Tenant)
                .WithMany(t => t.Memberships)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Memberships
            builder.HasOne(x => x.User)
                .WithMany(u => u.TenantMemberships)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One user can belong to a tenant only once
            builder.HasIndex(x => new
            {
                x.TenantId,
                x.UserId
            })
            .IsUnique();
        }
    }
}
