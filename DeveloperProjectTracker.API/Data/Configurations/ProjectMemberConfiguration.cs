using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
    {
        public void Configure(EntityTypeBuilder<ProjectMember> builder)
        {
            builder.ToTable("ProjectMembers");

            //primary key
            builder.HasKey(pm => pm.Id);
            builder.Property(pm => pm.Id).
                ValueGeneratedNever();

            //projectId
            builder.Property(pm => pm.ProjectId).
                IsRequired();

            //User id
            builder.Property(pm => pm.UserId).
                IsRequired();

            //Role
            builder.Property(pm => pm.Role).
                HasConversion<string>().
                IsRequired().
                HasMaxLength(50);

            //JoinedAt
            builder.Property(pm => pm.JoinedAt)
           .HasColumnType("timestamp with time zone")
           .IsRequired();

            //project Relationships
            builder.HasOne(pm => pm.Project).
                WithMany(p => p.Members).
                HasForeignKey(pm => pm.ProjectId).
                OnDelete(DeleteBehavior.Cascade);

            //User Relationship
            builder.HasOne( pm => pm.User).
                WithMany(u => u.ProjectMemberships).
                HasForeignKey(pm => pm.ProjectId).
                OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pm => new
            {
                pm.ProjectId,
                pm.UserId
            })
           .IsUnique();

        }
    }
}
