using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AdvertisementService.Models.DBModels
{
    public partial class advertisementserviceContext : DbContext
    {
        public advertisementserviceContext()
        {
        }

        public advertisementserviceContext(DbContextOptions<advertisementserviceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Advertisements> Advertisements { get; set; }
        public virtual DbSet<Advertisementscampaigns> Advertisementscampaigns { get; set; }
        public virtual DbSet<Advertisementsintervals> Advertisementsintervals { get; set; }
        public virtual DbSet<Campaigns> Campaigns { get; set; }
        public virtual DbSet<Intervals> Intervals { get; set; }
        public virtual DbSet<Mediametadata> Mediametadata { get; set; }
        public virtual DbSet<Medias> Medias { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=localhost;port=3306;user=root;password=MySQL@123;database=advertisementservice", x => x.ServerVersion("8.0.20-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Advertisements>(entity =>
            {
                entity.HasKey(e => e.AdvertisementId)
                    .HasName("PRIMARY");

                entity.ToTable("advertisements");

                entity.HasIndex(e => e.MediaId)
                    .HasName("mediaId");

                entity.Property(e => e.AdvertisementId).HasColumnName("advertisementId");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.InstitutionId).HasColumnName("institutionId");

                entity.Property(e => e.MediaId).HasColumnName("mediaId");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.Advertisements)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("advertisements_ibfk_1");
            });

            modelBuilder.Entity<Advertisementscampaigns>(entity =>
            {
                entity.ToTable("advertisementscampaigns");

                entity.HasIndex(e => e.AdvertisementId)
                    .HasName("advertisementId");

                entity.HasIndex(e => e.CampaignId)
                    .HasName("campaignId");

                entity.Property(e => e.AdvertisementsCampaignsId).HasColumnName("advertisementsCampaignsId");

                entity.Property(e => e.AdvertisementId).HasColumnName("advertisementId");

                entity.Property(e => e.CampaignId).HasColumnName("campaignId");

                entity.HasOne(d => d.Advertisement)
                    .WithMany(p => p.Advertisementscampaigns)
                    .HasForeignKey(d => d.AdvertisementId)
                    .HasConstraintName("advertisementscampaigns_ibfk_2");

                entity.HasOne(d => d.Campaign)
                    .WithMany(p => p.Advertisementscampaigns)
                    .HasForeignKey(d => d.CampaignId)
                    .HasConstraintName("advertisementscampaigns_ibfk_1");
            });

            modelBuilder.Entity<Advertisementsintervals>(entity =>
            {
                entity.ToTable("advertisementsintervals");

                entity.HasIndex(e => e.AdvertisementId)
                    .HasName("advertisementId");

                entity.HasIndex(e => e.IntervalId)
                    .HasName("intervalId");

                entity.Property(e => e.AdvertisementsIntervalsId).HasColumnName("advertisementsIntervalsId");

                entity.Property(e => e.AdvertisementId).HasColumnName("advertisementId");

                entity.Property(e => e.IntervalId).HasColumnName("intervalId");

                entity.HasOne(d => d.Advertisement)
                    .WithMany(p => p.Advertisementsintervals)
                    .HasForeignKey(d => d.AdvertisementId)
                    .HasConstraintName("advertisementsintervals_ibfk_2");

                entity.HasOne(d => d.Interval)
                    .WithMany(p => p.Advertisementsintervals)
                    .HasForeignKey(d => d.IntervalId)
                    .HasConstraintName("advertisementsintervals_ibfk_1");
            });

            modelBuilder.Entity<Campaigns>(entity =>
            {
                entity.HasKey(e => e.CampaignId)
                    .HasName("PRIMARY");

                entity.ToTable("campaigns");

                entity.Property(e => e.CampaignId).HasColumnName("campaignId");

                entity.Property(e => e.EndAt)
                    .HasColumnName("endAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.StartAt)
                    .HasColumnName("startAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("enum('active','inactive')")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(500)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<Intervals>(entity =>
            {
                entity.HasKey(e => e.IntervalId)
                    .HasName("PRIMARY");

                entity.ToTable("intervals");

                entity.Property(e => e.IntervalId).HasColumnName("intervalId");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Mediametadata>(entity =>
            {
                entity.ToTable("mediametadata");

                entity.Property(e => e.MediaMetadataId).HasColumnName("mediaMetadataId");

                entity.Property(e => e.Duration).HasColumnName("duration");

                entity.Property(e => e.Size).HasColumnName("size");
            });

            modelBuilder.Entity<Medias>(entity =>
            {
                entity.HasKey(e => e.MediaId)
                    .HasName("PRIMARY");

                entity.ToTable("medias");

                entity.HasIndex(e => e.MediaMetadataId)
                    .HasName("mediaMetadataId");

                entity.Property(e => e.MediaId).HasColumnName("mediaId");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.MediaMetadataId).HasColumnName("mediaMetadataId");

                entity.Property(e => e.MediaType)
                    .HasColumnName("mediaType")
                    .HasColumnType("enum('video','image')")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.HasOne(d => d.MediaMetadata)
                    .WithMany(p => p.Medias)
                    .HasForeignKey(d => d.MediaMetadataId)
                    .HasConstraintName("medias_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
