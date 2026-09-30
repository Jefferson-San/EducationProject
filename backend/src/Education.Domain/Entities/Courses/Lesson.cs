using Education.Domain.Common;

namespace Education.Domain.Entities.Courses;

public class Lesson : Entity
{
    public Guid CourseId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public int Order { get; private set; }

    private Lesson() { }

    /// <summary>Criada somente pelo agregado <see cref="Course"/> (Course.AddLesson).</summary>
    internal static Lesson Create(Guid courseId, string title, string? description, int order) => new()
    {
        CourseId = courseId,
        Title = title.Trim(),
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
        Order = order
    };
}
