// Contrato duplicado em Education.Application/Videos/VideoStorageKeys.cs: qualquer mudança aqui deve ser feita lá também.
namespace VideoProcessing.Contracts.Storage;

/// <summary>
/// Chaves relativas ao Storage, usadas pela API e pelo Worker.
/// </summary>
public static class StorageKeys
{
    public static string OriginalFolder(Guid videoId) => $"original/{videoId}";
    public static string OriginalFile(Guid videoId) => $"{OriginalFolder(videoId)}/original.mp4";

    public static string ProcessedFolder(Guid videoId) => $"processed/{videoId}";
    public static string MasterPlaylist(Guid videoId) => $"{ProcessedFolder(videoId)}/master.m3u8";
}
