using Education.Domain.Entities.Auth;
using Education.Domain.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Education.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(4000);
        builder.Property(x => x.TeacherId).HasColumnName("teacher_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.TeacherId).HasDatabaseName("ix_courses_teacher_id");

        builder.HasOne<User>().WithMany().HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);

        // Aulas pertencem ao agregado Curso (acesso pelo campo _lessons).
        builder.HasMany(x => x.Lessons).WithOne().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lessons).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
