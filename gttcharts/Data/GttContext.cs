using System;
using gttcharts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace gttcharts.Data
{
    public partial class GttContext : DbContext
    {
        public GttContext()
        {
        }

        public GttContext(DbContextOptions<GttContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Issue> Issues { get; set; }
        public virtual DbSet<Record> Records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"DataSource=data.db;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issue>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("issue");

                entity.Property(e => e.Iid).HasColumnName("iid");

                entity.Property(e => e.Labels).HasColumnName("labels");

                entity.Property(e => e.Milestone).HasColumnName("milestone");

                entity.Property(e => e.Spent).HasColumnName("spent");

                entity.Property(e => e.Title).HasColumnName("title");

                entity.Property(e => e.TotalEstimate).HasColumnName("total_estimate");

                entity.Property(e => e.State).HasColumnName("state");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Closed).HasColumnName("closed").HasConversion(
                    v => v.ToString(),
                    v => v == "true");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Record>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("record");

                entity.Property(e => e.Date).HasColumnName("date");

                entity.Property(e => e.Iid).HasColumnName("iid");

                entity.Property(e => e.Time).HasColumnName("time");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.User).HasColumnName("user");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
