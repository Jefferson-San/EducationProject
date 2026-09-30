using Education.Domain.Entities.Videos;
using Education.Domain.Interfaces.Videos;

namespace Education.Application.Videos;

/// <param name="StreamingUrl">
/// URL absoluta do master.m3u8 no serviço de mídia (ex.: "http://localhost:8081/processed/{id}/master.m3u8").
/// Só é preenchida quando o status é Processed.
/// </param>
public sealed record VideoDto(
    Guid Id,
    VideoStatus Status,
    string? StreamingUrl,
    double? DurationSeconds,
    string? FailureReason,
    DateTime CreatedAt);

internal static class VideoMappings
{
    public static VideoDto ToDto(this Video video, IFileStorage storage) => new(
        video.Id,
        video.Status,
        video is { Status: VideoStatus.Processed, StreamingPath: not null }
            ? storage.GetPublicUrl(video.StreamingPath)
            : null,
        video.Duration?.TotalSeconds,
        video.FailureReason,
        video.CreatedAt);
}
