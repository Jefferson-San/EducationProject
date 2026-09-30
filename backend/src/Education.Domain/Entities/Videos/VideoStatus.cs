namespace Education.Domain.Entities.Videos;

/// <summary>UPLOADED → PROCESSING → PROCESSED, ou PROCESSING → FAILED (ver Video).</summary>
public enum VideoStatus
{
    Uploaded,
    Processing,
    Processed,
    Failed
}
