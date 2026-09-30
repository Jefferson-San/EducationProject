using Education.Application.Videos.Commands;
using FluentAssertions;

namespace Education.UnitTests.Videos;

public class UploadVideoCommandValidatorTests
{
    private readonly UploadVideoCommandValidator _validator = new();

    private static UploadVideoCommand Command(string fileName, long length) =>
        new(Guid.NewGuid(), Guid.NewGuid(), fileName, length, Stream.Null);

    [Theory]
    [InlineData("aula.mp4")]
    [InlineData("AULA.MP4")]
    public void Mp4WithinLimit_ShouldBeValid(string fileName) =>
        _validator.Validate(Command(fileName, 1024)).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("aula.txt")]
    [InlineData("aula.mov")]
    [InlineData("aula")]
    public void NonMp4_ShouldBeInvalid(string fileName) =>
        _validator.Validate(Command(fileName, 1024)).IsValid.Should().BeFalse();

    [Theory]
    [InlineData(0)]
    [InlineData(UploadVideoCommand.MaxFileSizeBytes + 1)]
    public void SizeOutOfRange_ShouldBeInvalid(long length) =>
        _validator.Validate(Command("aula.mp4", length)).IsValid.Should().BeFalse();
}
