using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class ProjectMemberConfiguration
        : IEntityTypeConfiguration<ProjectMember>
    {
        public void Configure(
            EntityTypeBuilder<ProjectMember> builder)
        {
            builder.ToTable("ProjectMembers");

            // Primary Key
            builder.HasKey(pm => pm.Id);

            builder.Property(pm => pm.Id)
                .ValueGeneratedNever();

            // ProjectId
            builder.Property(pm => pm.ProjectId)
                .IsRequired();

            // UserId
            builder.Property(pm => pm.UserId)
                .IsRequired();

            // Role
            builder.Property(pm => pm.Role)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            // JoinedAt
            builder.Property(pm => pm.JoinedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Project Relationship
            builder.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // User Relationship
            builder.HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One user can belong to a project only once
            builder.HasIndex(pm => new
            {
                pm.ProjectId,
                pm.UserId
            })
            .IsUnique();
        }
    }
}