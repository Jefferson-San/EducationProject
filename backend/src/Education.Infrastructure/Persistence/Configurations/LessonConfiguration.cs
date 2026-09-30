using Education.Domain.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Education.Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        // Id gerado no domínio: sem isso o EF trataria a aula nova do agregado como "existente" (UPDATE).
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.CourseId).HasColumnName("course_id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(4000);
        builder.Property(x => x.Order).HasColumnName("order");

        builder.HasIndex(x => x.CourseId).HasDatabaseName("ix_lessons_course_id");
    }
}
