namespace Education.Domain.Entities.Courses;

/// <summary>Assunto do curso. Lista fixa: evita categorias duplicadas por grafia ("Matemática" x "matemática").</summary>
public enum CourseCategory
{
    Matematica,
    Ciencias,
    Portugues,
    Historia,
    Geografia,
    Artes,
    Outros
}
