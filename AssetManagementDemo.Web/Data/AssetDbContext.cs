using AssetManagementDemo.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementDemo.Web.Data
{
    public class AssetDbContext : DbContext
    {
        public AssetDbContext(DbContextOptions<AssetDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee Configurations
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.EmployeeId);
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.HasIndex(e => e.EmployeeName);
                entity.HasIndex(e => e.Department);
                entity.HasIndex(e => e.Location);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedDate);
                entity.Property(e => e.EmployeeCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.EmployeeName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Department).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            });

            // Asset Configurations
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("Assets");
                entity.HasKey(a => a.AssetId);
                entity.HasIndex(a => a.AssetCode).IsUnique();
                entity.HasIndex(a => a.AssetName);
                entity.HasIndex(a => a.Category);
                entity.HasIndex(a => a.Brand);
                entity.HasIndex(a => a.SerialNumber);
                entity.HasIndex(a => a.Status);
                entity.HasIndex(a => a.CreatedDate);
                entity.Property(a => a.AssetCode).HasMaxLength(20).IsRequired();
                entity.Property(a => a.AssetName).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Status).HasMaxLength(20).HasDefaultValue("Available");
                entity.Property(a => a.PurchasePrice).HasColumnType("decimal(18, 2)");
            });

            // AssetAssignment Configurations
            modelBuilder.Entity<AssetAssignment>(entity =>
            {
                entity.ToTable("AssetAssignments");
                entity.HasKey(aa => aa.AssignmentId);
                entity.HasIndex(aa => aa.EmployeeId);
                entity.HasIndex(aa => aa.AssetId);
                entity.HasIndex(aa => aa.IsActive);
                entity.HasIndex(aa => aa.AssignedDate);
                entity.HasIndex(aa => aa.ReturnDate);

                entity.HasOne(aa => aa.Employee)
                      .WithMany(e => e.AssetAssignments)
                      .HasForeignKey(aa => aa.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Assignment_Employee");

                entity.HasOne(aa => aa.Asset)
                      .WithMany(a => a.AssetAssignments)
                      .HasForeignKey(aa => aa.AssetId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Assignment_Asset");
            });
        }
    }
}
