using Education.Domain.Entities.Courses;
using Education.Domain.Entities.Videos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Education.Infrastructure.Persistence.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("videos");
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.CanBeReplaced);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.LessonId).HasColumnName("lesson_id");
        builder.Property(x => x.OriginalPath).HasColumnName("original_path").HasMaxLength(500).IsRequired();
        builder.Property(x => x.StreamingPath).HasColumnName("streaming_path").HasMaxLength(500);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(1000);
        builder.Property(x => x.Duration).HasColumnName("duration");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        // Concorrência otimista (xmin): duas entregas da mesma mensagem não iniciam o processamento juntas.
        builder.Property(x => x.Version).IsRowVersion();

        // 1:1 — cada aula tem no máximo um vídeo.
        builder.HasIndex(x => x.LessonId).IsUnique().HasDatabaseName("ux_videos_lesson_id");
        builder.HasOne<Lesson>().WithOne().HasForeignKey<Video>(x => x.LessonId).OnDelete(DeleteBehavior.Cascade);
    }
}
