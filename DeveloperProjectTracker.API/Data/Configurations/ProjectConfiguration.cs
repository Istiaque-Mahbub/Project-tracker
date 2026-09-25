using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperProjectTracker.API.Data.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id).
                ValueGeneratedNever();

            //Name
            builder.Property(p => p.Name).
                IsRequired().
                HasMaxLength(300);

            //Description
            builder.Property(p => p.Description).
                HasMaxLength(3000);

            //Owner
            builder.Property(p => p.OwnerId).
                IsRequired();

            // Tenant
            builder.Property(p => p.TenantId)
                .IsRequired();

            //Dates
            builder.Property(p => p.CreateAt).
                HasColumnType("timestamp with time zone").
                IsRequired();

            builder.Property(p => p.UpdateAt).
                HasColumnType("timestamp with time zone");

            //owner relationship
            builder.HasOne(p => p.Owner).
                WithMany(u => u.OwnedProjects).
                HasForeignKey(p => p.OwnerId).
                OnDelete(DeleteBehavior.Restrict);

            // Tenant relationship
            builder.HasOne(p => p.Tenant)
                .WithMany(t => t.Projects)
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            //members relationship
            builder.HasMany(p => p.Members).
                WithOne(pm => pm.Project).
                HasForeignKey(pm => pm.ProjectId).
                OnDelete(DeleteBehavior.Cascade);

            // Project -> Tasks
            builder.HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> Milestones
            builder.HasMany(p => p.Milestones)
                .WithOne(m => m.Project)
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> Labels
            builder.HasMany(p => p.Labels)
                .WithOne(l => l.Project)
                .HasForeignKey(l => l.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> Activities
            builder.HasMany(p => p.Activities)
                .WithOne(a => a.Project)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
    
}
