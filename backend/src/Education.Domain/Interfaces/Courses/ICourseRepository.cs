using Education.Domain.Entities.Courses;

namespace Education.Domain.Interfaces.Courses;

public interface ICourseRepository : IRepository<Course>
{
    /// <summary>Curso com as aulas carregadas (necessário para adicionar aulas ao agregado).</summary>
    Task<Course?> GetWithLessonsAsync(Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>Curso dono da aula informada (usado para checar o dono antes do upload).</summary>
    Task<Course?> GetByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default);
}
