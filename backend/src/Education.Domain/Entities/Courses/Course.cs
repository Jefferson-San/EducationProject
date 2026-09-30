using Education.Domain.Common;

namespace Education.Domain.Entities.Courses;

/// <summary>Agregado Curso: as aulas só são criadas através dele, garantindo a regra de dono.</summary>
public class Course : AggregateRoot
{
    private readonly List<Lesson> _lessons = [];

    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid TeacherId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Lesson> Lessons => _lessons.AsReadOnly();

    private Course() { }

    public static Course Create(Guid teacherId, string title, string? description) => new()
    {
        TeacherId = teacherId,
        Title = title.Trim(),
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
        CreatedAt = DateTime.UtcNow
    };

    public bool IsOwnedBy(Guid userId) => TeacherId == userId;

    /// <param name="order">Posição da aula; sem valor, vai para o final do curso.</param>
    public Result<Lesson> AddLesson(Guid requesterId, string title, string? description, int? order)
    {
        if (!IsOwnedBy(requesterId))
            return Result.Failure<Lesson>(Error.Forbidden(
                "Course.NotOwner", "Somente o professor dono do curso pode criar aulas."));

        var lesson = Lesson.Create(Id, title, description, order ?? NextOrder());
        _lessons.Add(lesson);
        return Result.Success(lesson);
    }

    private int NextOrder() => _lessons.Count == 0 ? 1 : _lessons.Max(l => l.Order) + 1;
}
