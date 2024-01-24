using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BotApi.Database
{
    public partial class YotubeCommentsExtractorBotContext : DbContext
    {
        public YotubeCommentsExtractorBotContext()
        {
        }

        public YotubeCommentsExtractorBotContext(DbContextOptions<YotubeCommentsExtractorBotContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DownloadFromVideoTask> DownloadFromVideoTasks { get; set; } = null!;
        public virtual DbSet<TgUser> TgUsers { get; set; } = null!;
        public virtual DbSet<YoutubeApiKey> YoutubeApiKeys { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DownloadFromVideoTask>(entity =>
            {
                entity.ToTable("download_from_video_tasks");

                entity.HasIndex(e => e.UidTask, "download_from_channel_tasks_uid_task_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.Id, "download_tasks_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.UidTask, "download_tasks_uid_task_uindex")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('download_tasks_id_seq'::regclass)");

                entity.Property(e => e.BeginDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("begin_date");

                entity.Property(e => e.ChannelTitle)
                    .HasMaxLength(500)
                    .HasColumnName("channel_title");

                entity.Property(e => e.ChatId).HasColumnName("chat_id");

                entity.Property(e => e.CompleteDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("complete_date");

                entity.Property(e => e.Completed).HasColumnName("completed");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_date");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.ErrorText)
                    .HasMaxLength(500)
                    .HasColumnName("error_text");

                entity.Property(e => e.Failed).HasColumnName("failed");

                entity.Property(e => e.TotalCoast).HasColumnName("total_coast");

                entity.Property(e => e.TotalComments).HasColumnName("total_comments");

                entity.Property(e => e.TotalDownloaded).HasColumnName("total_downloaded");

                entity.Property(e => e.UidTask).HasColumnName("uid_task");

                entity.Property(e => e.VideoId)
                    .HasMaxLength(100)
                    .HasColumnName("video_id");

                entity.Property(e => e.VideoTitle)
                    .HasMaxLength(500)
                    .HasColumnName("video_title");

                entity.Property(e => e.VideoUrl)
                    .HasMaxLength(500)
                    .HasColumnName("video_url");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.DownloadFromVideoTasks)
                    .HasForeignKey(d => d.ChatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("download_tasks_tg_users_chat_id_fk");
            });

            modelBuilder.Entity<TgUser>(entity =>
            {
                entity.HasKey(e => e.ChatId)
                    .HasName("tg_users_pk");

                entity.ToTable("tg_users");

                entity.HasIndex(e => e.ChatId, "tg_users_chat_id_uindex")
                    .IsUnique();

                entity.Property(e => e.ChatId)
                    .ValueGeneratedNever()
                    .HasColumnName("chat_id");

                entity.Property(e => e.CreateData)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_data");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(500)
                    .HasColumnName("first_name");

                entity.Property(e => e.LanguageCode)
                    .HasMaxLength(10)
                    .HasColumnName("language_code");

                entity.Property(e => e.LastName)
                    .HasMaxLength(500)
                    .HasColumnName("last_name");

                entity.Property(e => e.UserName)
                    .HasMaxLength(100)
                    .HasColumnName("user_name");
            });

            modelBuilder.Entity<YoutubeApiKey>(entity =>
            {
                entity.ToTable("youtube_api_keys");

                entity.HasIndex(e => e.Id, "youtube_api_keys_id_uindex")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ApiKey)
                    .HasMaxLength(100)
                    .HasColumnName("api_key");

                entity.Property(e => e.Comments)
                    .HasMaxLength(500)
                    .HasColumnName("comments");

                entity.Property(e => e.DateAdd).HasColumnName("date_add");

                entity.Property(e => e.Deleted)
                    .HasColumnName("deleted")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.UnblockingDate).HasColumnName("unblocking_date");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
