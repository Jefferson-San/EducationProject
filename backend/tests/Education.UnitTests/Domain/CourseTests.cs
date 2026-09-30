using Education.Domain.Common;
using Education.Domain.Entities.Courses;
using FluentAssertions;

namespace Education.UnitTests.Domain;

public class CourseTests
{
    private static readonly Guid Teacher = Guid.NewGuid();

    [Fact]
    public void AddLesson_ShouldBeForbidden_WhenRequesterIsNotTheOwner()
    {
        var course = Course.Create(Teacher, "Frações", null);

        var result = course.AddLesson(Guid.NewGuid(), "Aula intrusa", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
        course.Lessons.Should().BeEmpty();
    }

    [Fact]
    public void AddLesson_WithoutOrder_ShouldGoToTheEnd()
    {
        var course = Course.Create(Teacher, "Frações", null);

        course.AddLesson(Teacher, "Introdução", null, null).Value.Order.Should().Be(1);
        course.AddLesson(Teacher, "Avançado", null, 10).Value.Order.Should().Be(10);
        course.AddLesson(Teacher, "Revisão", null, null).Value.Order.Should().Be(11);
    }

    [Fact]
    public void Create_ShouldTrimTexts_AndTreatBlankDescriptionAsNull()
    {
        var course = Course.Create(Teacher, "  Matemática  ", "   ");

        course.Title.Should().Be("Matemática");
        course.Description.Should().BeNull();
    }
}
