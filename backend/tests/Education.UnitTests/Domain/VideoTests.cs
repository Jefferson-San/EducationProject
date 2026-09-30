using Education.Domain.Common;
using Education.Domain.Entities.Videos;
using Education.Domain.Events;
using FluentAssertions;

namespace Education.UnitTests.Domain;

public class VideoTests
{
    private static Video NewVideo()
    {
        var id = Guid.NewGuid();
        return Video.Create(id, Guid.NewGuid(), $"original/{id}/original.mp4");
    }

    private static Video VideoIn(VideoStatus status)
    {
        var video = NewVideo();
        if (status == VideoStatus.Uploaded) return video;

        video.StartProcessing(redelivered: false);
        if (status == VideoStatus.Processed) video.Complete("processed/x/master.m3u8", TimeSpan.FromSeconds(10));
        if (status == VideoStatus.Failed) video.Fail("erro");
        return video;
    }

    [Fact]
    public void Create_ShouldStartAsUploaded_AndRaiseVideoUploadedEvent()
    {
        var video = NewVideo();

        video.Status.Should().Be(VideoStatus.Uploaded);
        video.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<VideoUploadedEvent>()
            .Which.VideoId.Should().Be(video.Id);
    }

    [Theory]
    [InlineData(VideoStatus.Uploaded, false, true)]
    [InlineData(VideoStatus.Failed, false, true)]
    [InlineData(VideoStatus.Processing, false, false)] // mensagem duplicada
    [InlineData(VideoStatus.Processing, true, true)]   // reentrega: o Worker anterior caiu no meio
    [InlineData(VideoStatus.Processed, false, false)]
    [InlineData(VideoStatus.Processed, true, false)]
    public void StartProcessing_ShouldFollowTheStateMachine(VideoStatus from, bool redelivered, bool expectedSuccess)
    {
        var video = VideoIn(from);

        var result = video.StartProcessing(redelivered);

        result.IsSuccess.Should().Be(expectedSuccess);
        video.Status.Should().Be(expectedSuccess ? VideoStatus.Processing : from);
        if (!expectedSuccess) result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Complete_ShouldRequireProcessing()
    {
        var video = NewVideo();

        video.Complete("processed/x/master.m3u8", null).IsFailure.Should().BeTrue();

        video.StartProcessing(redelivered: false);
        video.Complete("processed/x/master.m3u8", TimeSpan.FromSeconds(42)).IsSuccess.Should().BeTrue();
        video.Status.Should().Be(VideoStatus.Processed);
        video.StreamingPath.Should().Be("processed/x/master.m3u8");
        video.Duration.Should().Be(TimeSpan.FromSeconds(42));
    }

    [Fact]
    public void Fail_ShouldTruncateLongReasons()
    {
        var video = VideoIn(VideoStatus.Processing);

        video.Fail(new string('x', 5000)).IsSuccess.Should().BeTrue();

        video.Status.Should().Be(VideoStatus.Failed);
        video.FailureReason.Should().HaveLength(1000);
    }

    [Fact]
    public void FailToEnqueue_ShouldOnlyApplyBeforeProcessing()
    {
        VideoIn(VideoStatus.Uploaded).FailToEnqueue("fila fora do ar").IsSuccess.Should().BeTrue();
        VideoIn(VideoStatus.Processing).FailToEnqueue("fila fora do ar").IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(VideoStatus.Uploaded, true)]
    [InlineData(VideoStatus.Processing, false)]
    [InlineData(VideoStatus.Processed, true)]
    [InlineData(VideoStatus.Failed, true)]
    public void CanBeReplaced_ShouldBeFalseOnlyWhileProcessing(VideoStatus status, bool expected) =>
        VideoIn(status).CanBeReplaced.Should().Be(expected);
}
